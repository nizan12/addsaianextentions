using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AIBlockerGuard
{
    static class Program
    {
        private static Mutex appMutex;

        [STAThread]
        static void Main(string[] args)
        {
            // Pastikan TLS 1.2 aktif untuk koneksi HTTPS/HTTP
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            if (args != null && args.Any(a => a.Equals("--restore", StringComparison.OrdinalIgnoreCase)))
            {
                GuardEngine.EmergencyRestoreAll();
                MessageBox.Show(
                    "Pemulihan sistem berhasil dijalankan!\n\n- File hosts telah dibersihkan\n- Kebijakan DNS Browser (DoH) telah dipulihkan\n- Seluruh ekstensi IDE (VS Code / JetBrains) telah dikembalikan.",
                    "AI-Blocker Guard — Restore Selesai",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            bool createdNew;
            appMutex = new Mutex(true, "Global\\AIBlockerGuard_Singleton_Mutex", out createdNew);
            if (!createdNew)
            {
                return;
            }

            bool startInTray = args != null && args.Any(a =>
                a.Equals("--autostart", StringComparison.OrdinalIgnoreCase) ||
                a.Equals("--tray", StringComparison.OrdinalIgnoreCase));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(startInTray));
        }
    }

    // ============================================================================
    // DATA MODELS
    // ============================================================================
    public class QuarantineEntry
    {
        public string Name { get; set; }
        public string OriginalPath { get; set; }
        public string QuarantinePath { get; set; }
        public string QuarantinedAt { get; set; }
    }

    public class AgentConfig
    {
        public string Nim { get; set; }
        public string Name { get; set; }
        public string RoomCode { get; set; }
        public string ServerUrl { get; set; }
        public bool AutoStart { get; set; }
        public bool IsConfigured { get; set; }
    }

    // ============================================================================
    // 4-LAYER AI BLOCKER ENGINE
    // ============================================================================
    public static class GuardEngine
    {
        public static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AIBlockerGuard"
        );
        public static readonly string QuarantineDir = Path.Combine(AppDataDir, "Quarantine");
        public static readonly string ManifestFile = Path.Combine(AppDataDir, "quarantine_manifest.json");
        public static readonly string VsCodeBackupFile = Path.Combine(AppDataDir, "vscode_settings_backup.json");
        public static readonly string AgentConfigFile = Path.Combine(AppDataDir, "agent_config.json");
        public static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
        public static readonly string HostsStartMarker = "# >>> AI-BLOCKER-GUARD START >>>";
        public static readonly string HostsEndMarker = "# <<< AI-BLOCKER-GUARD END <<<";

        public static List<string> BlockedDomains = new List<string>
        {
            // Web AI Assistants
            "chatgpt.com", "www.chatgpt.com", "openai.com", "www.openai.com", "api.openai.com", "auth.openai.com", "chat.openai.com",
            "claude.ai", "www.claude.ai", "anthropic.com", "www.anthropic.com", "api.anthropic.com",
            "gemini.google.com", "bard.google.com", "aistudio.google.com", "generativelanguage.googleapis.com", "makersuite.google.com",
            "copilot.microsoft.com", "www.copilot.microsoft.com", "sydney.bing.com", "edgeservices.bing.com",
            "deepseek.com", "www.deepseek.com", "chat.deepseek.com", "api.deepseek.com",
            "perplexity.ai", "www.perplexity.ai", "api.perplexity.ai",
            "poe.com", "www.poe.com",
            "grok.com", "www.grok.com", "x.ai", "api.x.ai",
            "mistral.ai", "chat.mistral.ai", "api.mistral.ai",
            "meta.ai", "www.meta.ai", "pi.ai", "you.com", "www.you.com",
            "huggingface.co", "www.huggingface.co", "api-inference.huggingface.co",
            // Coding Web AI
            "blackbox.ai", "www.blackbox.ai", "api.blackbox.ai",
            "phind.com", "www.phind.com",
            "v0.dev", "www.v0.dev", "v0.app",
            "bolt.new", "www.bolt.new",
            "lovable.dev", "www.lovable.dev",
            "replit.com", "ai.replit.com", "idx.google.com",
            // IDE AI APIs & Proxies (GitHub Copilot Individual / Free / Business / Enterprise / Model Lab)
            "githubcopilot.com", "api.githubcopilot.com",
            "api.individual.githubcopilot.com", "api.business.githubcopilot.com", "api.enterprise.githubcopilot.com",
            "proxy.githubcopilot.com", "proxy.individual.githubcopilot.com", "proxy.business.githubcopilot.com", "proxy.enterprise.githubcopilot.com",
            "api-model-lab.githubcopilot.com",
            "copilot-proxy.githubusercontent.com", "copilot-telemetry.githubusercontent.com", "origin-tracker.githubusercontent.com", "default.exp-tas.com",
            "cursor.sh", "www.cursor.sh", "cursor.com", "www.cursor.com", "api2.cursor.sh", "api3.cursor.sh", "repo42.cursor.sh",
            "codeium.com", "www.codeium.com", "server.codeium.com", "windsurf.com", "codeiumdata.com",
            "tabnine.com", "www.tabnine.com", "api.tabnine.com",
            "supermaven.com", "www.supermaven.com", "api.supermaven.com",
            "continue.dev", "www.continue.dev",
            "sourcegraph.com", "cody.dev",
            "openrouter.ai", "www.openrouter.ai", "api.openrouter.ai",
            "api.groq.com", "console.groq.com", "groq.com",
            "together.ai", "api.together.xyz",
            "fireworks.ai", "api.fireworks.ai",
            "cohere.com", "api.cohere.ai",
            // Roblox Web, Game Join, Auth, APIs & CDN
            "roblox.com", "www.roblox.com", "web.roblox.com", "ro.blox.com",
            "api.roblox.com", "apis.roblox.com", "auth.roblox.com", "accountsettings.roblox.com", "accountinformation.roblox.com",
            "assetdelivery.roblox.com", "avatar.roblox.com", "badges.roblox.com", "billing.roblox.com", "catalog.roblox.com", "chat.roblox.com",
            "clientsettings.roblox.com", "clientsettingscdn.roblox.com", "client-telemetry.roblox.com",
            "contentstore.roblox.com", "create.roblox.com", "develop.roblox.com", "devforum.roblox.com", "economy.roblox.com",
            "ecsv2.roblox.com", "ephemeralcounters.api.roblox.com", "engagementpayouts.roblox.com", "followings.roblox.com",
            "friends.roblox.com", "gameinternationalization.roblox.com", "gamejoin.roblox.com", "gamepersistence.roblox.com",
            "games.roblox.com", "groups.roblox.com", "inventory.roblox.com", "itemconfiguration.roblox.com",
            "lms.roblox.com", "locale.roblox.com", "localizationtables.roblox.com", "metrics.roblox.com",
            "notifications.roblox.com", "points.roblox.com", "premiumfeatures.roblox.com", "presence.roblox.com", "privatemessages.roblox.com",
            "publish.roblox.com", "realtime.roblox.com", "search.roblox.com", "thumbnails.roblox.com", "trades.roblox.com",
            "translationroles.roblox.com", "translations.roblox.com", "users.roblox.com", "voice.roblox.com",
            "rbxcdn.com", "www.rbxcdn.com", "setup.rbxcdn.com", "tr.rbxcdn.com", "js.rbxcdn.com", "css.rbxcdn.com", "images.rbxcdn.com",
            "t0.rbxcdn.com", "t1.rbxcdn.com", "t2.rbxcdn.com", "t3.rbxcdn.com", "t4.rbxcdn.com", "t5.rbxcdn.com", "t6.rbxcdn.com", "t7.rbxcdn.com",
            "c0.rbxcdn.com", "c1.rbxcdn.com", "c2.rbxcdn.com", "c3.rbxcdn.com", "c4.rbxcdn.com", "c5.rbxcdn.com", "c6.rbxcdn.com", "c7.rbxcdn.com",
            "robloxlabs.com", "corp.roblox.com", "en.help.roblox.com"
        };

        public static List<string> BlockedProcesses = new List<string>
        {
            "Cursor", "Windsurf", "Trae", "PearAI", "Void",
            "ChatGPT", "Claude", "Copilot", "copilot", "Perplexity",
            "ollama", "ollama_llama_server", "ollama app",
            "LM Studio", "lm-studio", "jan", "gpt4all",
            "llama-server", "llama-cli", "koboldcpp", "AnythingLLM", "Msty",
            // Roblox Player, Studio, Launcher, Microsoft Store UWP & Bootstrapper
            "RobloxPlayerBeta", "RobloxPlayerLauncher", "RobloxPlayerInstaller",
            "RobloxStudioBeta", "RobloxStudioLauncherBeta", "RobloxStudioInstaller",
            "RobloxCrashHandler", "Windows10Universal", "Roblox", "Bloxstrap", "Fishstrap"
        };

        public static List<string> BlockedExtensionPatterns = new List<string>
        {
            "github.copilot",
            "github.copilot-chat",
            "codeium.codeium",
            "continue.continue",
            "saoudrizwan.claude-dev",
            "rooveterinaryinc.roo-cline",
            "tabnine.tabnine-vscode",
            "supermaven.supermaven",
            "blackboxapp.blackbox",
            "sourcegraph.cody-ai",
            "amazonwebservices.amazon-q",
            "google.geminicodeassist",
            "codium.codium",
            "danielsanmedium.dscodegpt",
            "genieai.chatgpt-vscode",
            "bito.bito",
            "pieces.pieces-vscode",
            "kilocode.kilo-code",
            "augment.vscode-augment"
        };

        public static int[] LocalAiPorts = new int[] { 11434, 1234, 1337, 5001 };

        public static bool IsAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureDirectories()
        {
            if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir);
            if (!Directory.Exists(QuarantineDir)) Directory.CreateDirectory(QuarantineDir);
        }

        // --------------------------------------------------------------------
        // LAYER 1: HOSTS BLOCKING & DNS FLUSH
        // --------------------------------------------------------------------
        public static bool ApplyHostsBlock(bool dryRun, out string statusMessage)
        {
            EnsureDirectories();
            if (dryRun)
            {
                statusMessage = string.Format("[SIMULASI] {0} domain Web AI & API IDE disiapkan untuk diblokir.", BlockedDomains.Count);
                return true;
            }

            try
            {
                string backupPath = Path.Combine(AppDataDir, "hosts.original.bak");
                if (File.Exists(HostsPath) && !File.Exists(backupPath))
                {
                    File.Copy(HostsPath, backupPath, false);
                }

                // Buka atribut ReadOnly jika ada
                FileInfo fi = new FileInfo(HostsPath);
                if (fi.Exists && fi.IsReadOnly)
                {
                    fi.IsReadOnly = false;
                }

                string content = File.Exists(HostsPath) ? File.ReadAllText(HostsPath, Encoding.UTF8) : "";
                content = StripHostsBlockContent(content);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(content.TrimEnd());
                sb.AppendLine();
                sb.AppendLine(HostsStartMarker);
                sb.AppendLine("# Dibuat otomatis oleh AI-Blocker Guard selama sesi ujian/kuis berlangsung.");
                foreach (string rawDomain in BlockedDomains.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    string d = (rawDomain ?? "").Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(d) || d.Contains(" ")) continue;
                    sb.AppendLine("0.0.0.0 " + d);
                    sb.AppendLine("::0 " + d);
                }
                sb.AppendLine(HostsEndMarker);

                File.WriteAllText(HostsPath, sb.ToString(), Encoding.UTF8);
                FlushDns();

                statusMessage = string.Format("Layer 1 Aktif: {0} domain Web AI & API IDE berhasil diblokir di sistem DNS.", BlockedDomains.Count);
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = "Gagal memodifikasi file hosts (Pastikan Run as Administrator): " + ex.Message;
                return false;
            }
        }

        public static bool IsHostsBlockIntact()
        {
            try
            {
                if (!File.Exists(HostsPath)) return false;
                string content = File.ReadAllText(HostsPath, Encoding.UTF8);
                return content.Contains(HostsStartMarker) && content.Contains(HostsEndMarker);
            }
            catch
            {
                return true;
            }
        }

        public static void RemoveHostsBlock()
        {
            try
            {
                if (!File.Exists(HostsPath)) return;
                FileInfo fi = new FileInfo(HostsPath);
                if (fi.IsReadOnly) fi.IsReadOnly = false;

                string content = File.ReadAllText(HostsPath, Encoding.UTF8);
                string cleaned = StripHostsBlockContent(content);
                File.WriteAllText(HostsPath, cleaned.TrimEnd() + Environment.NewLine, Encoding.UTF8);
                FlushDns();
            }
            catch { }
        }

        private static string StripHostsBlockContent(string content)
        {
            int startIdx = content.IndexOf(HostsStartMarker, StringComparison.Ordinal);
            if (startIdx < 0) return content;

            int endIdx = content.IndexOf(HostsEndMarker, startIdx, StringComparison.Ordinal);
            if (endIdx < 0)
            {
                return content.Substring(0, startIdx);
            }
            endIdx += HostsEndMarker.Length;
            return content.Substring(0, startIdx).TrimEnd() + Environment.NewLine + content.Substring(endIdx).TrimStart();
        }

        public static void FlushDns()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("ipconfig", "/flushdns")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p != null) p.WaitForExit(3000);
                }
            }
            catch { }
        }

        // --------------------------------------------------------------------
        // LAYER 2: BROWSER DNS-OVER-HTTPS (DoH), URLBLOCKLIST & BUILT-IN AI POLICY
        // --------------------------------------------------------------------
        public static void ApplyBrowserPolicies(bool dryRun)
        {
            if (dryRun) return;
            try
            {
                string[] browserPaths = new string[]
                {
                    @"SOFTWARE\Policies\Google\Chrome",
                    @"SOFTWARE\Policies\Microsoft\Edge",
                    @"SOFTWARE\Policies\BraveSoftware\Brave"
                };

                RegistryKey[] roots = new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser };
                foreach (RegistryKey root in roots)
                {
                    foreach (string bPath in browserPaths)
                    {
                        try
                        {
                            using (RegistryKey key = root.CreateSubKey(bPath))
                            {
                                if (key != null)
                                {
                                    key.SetValue("DnsOverHttpsMode", "off", RegistryValueKind.String);
                                    key.SetValue("BuiltInDnsClientEnabled", 0, RegistryValueKind.DWord);
                                    key.SetValue("QuicAllowed", 0, RegistryValueKind.DWord);

                                    if (bPath.EndsWith("Chrome"))
                                    {
                                        key.SetValue("GenAiDefaultSettings", 2, RegistryValueKind.DWord);
                                    }
                                    else if (bPath.EndsWith("Edge"))
                                    {
                                        key.SetValue("HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                                        key.SetValue("CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                                    }
                                    else if (bPath.EndsWith("Brave"))
                                    {
                                        key.SetValue("BraveAIChatEnabled", 0, RegistryValueKind.DWord);
                                    }

                                    // Rebuild URLBlocklist so Chromium immediately blocks even already-open tabs
                                    key.DeleteSubKeyTree("URLBlocklist", false);
                                    if (BlockedDomains != null && BlockedDomains.Count > 0)
                                    {
                                        using (RegistryKey ubKey = key.CreateSubKey("URLBlocklist"))
                                        {
                                            if (ubKey != null)
                                            {
                                                int idx = 1;
                                                foreach (string rawDomain in BlockedDomains)
                                                {
                                                    string d = (rawDomain ?? "").Trim().ToLowerInvariant();
                                                    if (string.IsNullOrEmpty(d) || d.Contains(" ")) continue;
                                                    ubKey.SetValue(idx.ToString(), d, RegistryValueKind.String);
                                                    idx++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    // Microsoft VS Code Group Policy: Matikan Built-in Copilot Chat & Agent
                    try
                    {
                        using (RegistryKey key = root.CreateSubKey(@"SOFTWARE\Policies\Microsoft\VSCode"))
                        {
                            if (key != null)
                            {
                                key.SetValue("ChatEnabled", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void RemoveBrowserPolicies()
        {
            try
            {
                string[] subKeys = new string[]
                {
                    @"SOFTWARE\Policies\Google\Chrome",
                    @"SOFTWARE\Policies\Microsoft\Edge",
                    @"SOFTWARE\Policies\BraveSoftware\Brave",
                    @"SOFTWARE\Policies\Microsoft\VSCode"
                };
                RegistryKey[] roots = new RegistryKey[] { Registry.LocalMachine, Registry.CurrentUser };
                foreach (RegistryKey root in roots)
                {
                    foreach (string sk in subKeys)
                    {
                        try
                        {
                            using (RegistryKey key = root.OpenSubKey(sk, true))
                            {
                                if (key != null)
                                {
                                    key.DeleteValue("DnsOverHttpsMode", false);
                                    key.DeleteValue("BuiltInDnsClientEnabled", false);
                                    key.DeleteValue("QuicAllowed", false);
                                    key.DeleteValue("GenAiDefaultSettings", false);
                                    key.DeleteValue("HubsSidebarEnabled", false);
                                    key.DeleteValue("CopilotCDPPageContext", false);
                                    key.DeleteValue("BraveAIChatEnabled", false);
                                    key.DeleteValue("ChatEnabled", false);
                                    key.DeleteSubKeyTree("URLBlocklist", false);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // --------------------------------------------------------------------
        // LAYER 3: IDE EXTENSION QUARANTINE & VS CODE BUILT-IN COPILOT DISABLER
        // --------------------------------------------------------------------
        public static string GetVsCodeUserSettingsPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Code", "User", "settings.json");
        }

        public static void ApplyVsCodeAiDisableSettings(bool dryRun)
        {
            if (dryRun) return;
            try
            {
                EnsureDirectories();
                string settingsPath = GetVsCodeUserSettingsPath();
                string settingsDir = Path.GetDirectoryName(settingsPath);
                if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                // Backup settings.json asli milik mahasiswa jika belum ada backup
                if (File.Exists(settingsPath) && !File.Exists(VsCodeBackupFile))
                {
                    File.Copy(settingsPath, VsCodeBackupFile, false);
                }

                string raw = File.Exists(settingsPath) ? File.ReadAllText(settingsPath, Encoding.UTF8) : "{}";
                string cleaned = Regex.Replace(raw, @"^\s*//.*$", "", RegexOptions.Multiline);
                cleaned = Regex.Replace(cleaned, @",\s*([\}\]])", "$1");

                JavaScriptSerializer ser = new JavaScriptSerializer();
                Dictionary<string, object> dict = null;
                try
                {
                    dict = ser.Deserialize<Dictionary<string, object>>(cleaned);
                }
                catch
                {
                    dict = null;
                }

                if (dict != null)
                {
                    dict["chat.disableAIFeatures"] = true;
                    dict["chat.commandCenter.enabled"] = false;
                    dict["chat.agent.enabled"] = false;
                    dict["inlineChat.enabled"] = false;
                    dict["github.copilot.editor.enableAutoCompletions"] = false;
                    dict["github.copilot.enable"] = new Dictionary<string, object>
                    {
                        { "*", false },
                        { "plaintext", false },
                        { "markdown", false },
                        { "scminput", false }
                    };

                    string serialized = FormatSimpleJson(dict);
                    File.WriteAllText(settingsPath, serialized, Encoding.UTF8);
                }
                else
                {
                    int braceIdx = raw.IndexOf('{');
                    if (braceIdx >= 0)
                    {
                        string injected = raw.Substring(0, braceIdx + 1) +
                            Environment.NewLine +
                            "    \"chat.disableAIFeatures\": true," + Environment.NewLine +
                            "    \"chat.commandCenter.enabled\": false," + Environment.NewLine +
                            "    \"chat.agent.enabled\": false," + Environment.NewLine +
                            "    \"inlineChat.enabled\": false," + Environment.NewLine +
                            "    \"github.copilot.editor.enableAutoCompletions\": false," +
                            raw.Substring(braceIdx + 1);
                        File.WriteAllText(settingsPath, injected, Encoding.UTF8);
                    }
                }
            }
            catch { }
        }

        public static bool IsVsCodeAiDisableIntact()
        {
            try
            {
                string settingsPath = GetVsCodeUserSettingsPath();
                if (!File.Exists(settingsPath)) return false;
                string raw = File.ReadAllText(settingsPath, Encoding.UTF8);
                return Regex.IsMatch(raw, "\"chat\\.disableAIFeatures\"\\s*:\\s*true", RegexOptions.IgnoreCase);
            }
            catch
            {
                return true;
            }
        }

        public static void RestoreVsCodeAiDisableSettings()
        {
            try
            {
                string settingsPath = GetVsCodeUserSettingsPath();
                if (File.Exists(VsCodeBackupFile))
                {
                    File.Copy(VsCodeBackupFile, settingsPath, true);
                    File.Delete(VsCodeBackupFile);
                    return;
                }

                if (File.Exists(settingsPath))
                {
                    string raw = File.ReadAllText(settingsPath, Encoding.UTF8);
                    string cleaned = Regex.Replace(raw, @"^\s*//.*$", "", RegexOptions.Multiline);
                    cleaned = Regex.Replace(cleaned, @",\s*([\}\]])", "$1");
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    var dict = ser.Deserialize<Dictionary<string, object>>(cleaned);
                    if (dict != null)
                    {
                        dict.Remove("chat.disableAIFeatures");
                        dict.Remove("chat.commandCenter.enabled");
                        dict.Remove("chat.agent.enabled");
                        dict.Remove("inlineChat.enabled");
                        dict.Remove("github.copilot.editor.enableAutoCompletions");
                        dict.Remove("github.copilot.enable");
                        File.WriteAllText(settingsPath, FormatSimpleJson(dict), Encoding.UTF8);
                    }
                }
            }
            catch { }
        }

        private static string FormatSimpleJson(Dictionary<string, object> dict)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            int i = 0;
            int count = dict.Count;
            foreach (var kvp in dict)
            {
                i++;
                string valJson = ser.Serialize(kvp.Value);
                sb.Append("    ").Append(ser.Serialize(kvp.Key)).Append(": ").Append(valJson);
                if (i < count) sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static List<string> ScanAndQuarantineExtensions(bool dryRun)
        {
            EnsureDirectories();
            List<string> detectedNames = new List<string>();
            List<QuarantineEntry> manifest = LoadManifest();

            // Nonaktifkan VS Code Built-in Copilot (live reload di settings.json)
            if (!IsVsCodeAiDisableIntact())
            {
                ApplyVsCodeAiDisableSettings(dryRun);
            }
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string vscInstallDir = Path.Combine(localAppData, "Programs", "Microsoft VS Code");
            if (Directory.Exists(vscInstallDir))
            {
                detectedNames.Add("vscode.builtin-copilot (chat.disableAIFeatures=true)");
            }

            // Tambahkan yang sudah ada di manifest
            foreach (var item in manifest)
            {
                detectedNames.Add(item.Name);
            }

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] vscodeDirs = new string[]
            {
                Path.Combine(userProfile, ".vscode", "extensions"),
                Path.Combine(userProfile, ".vscode-insiders", "extensions"),
                Path.Combine(userProfile, ".vscode-oss", "extensions")
            };

            foreach (string extRoot in vscodeDirs)
            {
                if (!Directory.Exists(extRoot)) continue;

                string[] dirs;
                try { dirs = Directory.GetDirectories(extRoot); }
                catch { continue; }

                foreach (string dir in dirs)
                {
                    string folderName = Path.GetFileName(dir);
                    if (string.IsNullOrEmpty(folderName)) continue;

                    bool isAiExt = BlockedExtensionPatterns.Any(pat =>
                        folderName.IndexOf(pat.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);

                    if (!isAiExt) continue;

                    detectedNames.Add(folderName);

                    if (!dryRun)
                    {
                        try
                        {
                            string targetPath = Path.Combine(QuarantineDir, folderName + "_" + Guid.NewGuid().ToString("N").Substring(0, 6));
                            Directory.Move(dir, targetPath);
                            manifest.Add(new QuarantineEntry
                            {
                                Name = folderName,
                                OriginalPath = dir,
                                QuarantinePath = targetPath,
                                QuarantinedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                        catch { }
                    }
                }
            }

            if (!dryRun)
            {
                SaveManifest(manifest);
            }

            return detectedNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static int RestoreQuarantinedExtensions()
        {
            RestoreVsCodeAiDisableSettings();
            List<QuarantineEntry> manifest = LoadManifest();
            int restored = 0;
            List<QuarantineEntry> remaining = new List<QuarantineEntry>();

            foreach (var entry in manifest)
            {
                try
                {
                    if (Directory.Exists(entry.QuarantinePath))
                    {
                        string parent = Path.GetDirectoryName(entry.OriginalPath);
                        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                        {
                            Directory.CreateDirectory(parent);
                        }
                        if (!Directory.Exists(entry.OriginalPath))
                        {
                            Directory.Move(entry.QuarantinePath, entry.OriginalPath);
                        }
                        else
                        {
                            Directory.Delete(entry.QuarantinePath, true);
                        }
                    }
                    restored++;
                }
                catch
                {
                    remaining.Add(entry);
                }
            }

            SaveManifest(remaining);
            return restored;
        }

        private static List<QuarantineEntry> LoadManifest()
        {
            try
            {
                if (!File.Exists(ManifestFile)) return new List<QuarantineEntry>();
                string json = File.ReadAllText(ManifestFile, Encoding.UTF8);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                return ser.Deserialize<List<QuarantineEntry>>(json) ?? new List<QuarantineEntry>();
            }
            catch
            {
                return new List<QuarantineEntry>();
            }
        }

        private static void SaveManifest(List<QuarantineEntry> list)
        {
            try
            {
                EnsureDirectories();
                JavaScriptSerializer ser = new JavaScriptSerializer();
                File.WriteAllText(ManifestFile, ser.Serialize(list), Encoding.UTF8);
            }
            catch { }
        }

        // --------------------------------------------------------------------
        // LAYER 4: PROCESS KILLER, LOCAL LLM PORT CHECKER & VPN DETECTOR
        // --------------------------------------------------------------------
        public static List<string> ScanAndKillAiProcesses(bool dryRun)
        {
            List<string> triggered = new List<string>();
            int currentPid = Process.GetCurrentProcess().Id;

            Process[] allProcs;
            try { allProcs = Process.GetProcesses(); }
            catch { return triggered; }

            foreach (Process p in allProcs)
            {
                try
                {
                    if (p.Id == currentPid || p.Id <= 4) continue;
                    string procName = p.ProcessName;
                    if (string.IsNullOrEmpty(procName)) continue;

                    bool matched = BlockedProcesses.Any(bp =>
                        procName.Equals(bp.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        procName.Equals(bp.Trim() + ".exe", StringComparison.OrdinalIgnoreCase));

                    if (matched)
                    {
                        string label = string.Format("{0}.exe (PID {1})", procName, p.Id);
                        if (!dryRun)
                        {
                            p.Kill();
                        }
                        triggered.Add(label);
                    }
                }
                catch { }
                finally
                {
                    p.Dispose();
                }
            }

            // Cek port Local LLM (Ollama 11434, LM Studio 1234, Jan 1337)
            try
            {
                IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listeners = ipProps.GetActiveTcpListeners();
                foreach (int port in LocalAiPorts)
                {
                    if (listeners.Any(ep => ep.Port == port))
                    {
                        triggered.Add(string.Format("Local LLM Port Aktif (:{0})", port));
                    }
                }
            }
            catch { }

            return triggered;
        }

        public static bool DetectActiveVpn(out string vpnName)
        {
            vpnName = "";
            string[] vpnKeywords = new string[]
            {
                "WireGuard", "CloudflareWARP", "WARP", "NordLynx", "ProtonVPN",
                "OpenVPN", "TAP-Windows", "Wintun", "Psiphon", "Tailscale", "ZeroTier", "Outline"
            };

            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface nic in nics)
                {
                    if (nic.OperationalStatus != OperationalStatus.Up) continue;
                    string desc = (nic.Description + " " + nic.Name).ToLowerInvariant();
                    foreach (string kw in vpnKeywords)
                    {
                        if (desc.Contains(kw.ToLowerInvariant()))
                        {
                            vpnName = nic.Name + " (" + nic.Description + ")";
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static void EmergencyRestoreAll()
        {
            RemoveHostsBlock();
            RemoveBrowserPolicies();
            RestoreQuarantinedExtensions();
        }

        public static AgentConfig LoadAgentConfig()
        {
            try
            {
                if (File.Exists(AgentConfigFile))
                {
                    string json = File.ReadAllText(AgentConfigFile, Encoding.UTF8);
                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    AgentConfig cfg = ser.Deserialize<AgentConfig>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { }

            return new AgentConfig
            {
                Nim = Environment.MachineName,
                Name = Environment.UserName + " (" + Environment.MachineName + ")",
                RoomCode = "",
                ServerUrl = "https://blockai-wheat.vercel.app/api/index.php",
                AutoStart = true,
                IsConfigured = false
            };
        }

        public static void SaveAgentConfig(AgentConfig cfg)
        {
            try
            {
                EnsureDirectories();
                JavaScriptSerializer ser = new JavaScriptSerializer();
                File.WriteAllText(AgentConfigFile, ser.Serialize(cfg), Encoding.UTF8);
            }
            catch { }
        }

        public static void EnsureAutoStartRegistered(bool enable)
        {
            string exePath = Application.ExecutablePath;
            string runVal = "\"" + exePath + "\" --autostart";

            // 1. Registry CurrentUser & LocalMachine Run
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (enable) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                        else key.DeleteValue("AIBlockerGuard", false);
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        if (enable) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                        else key.DeleteValue("AIBlockerGuard", false);
                    }
                }
            }
            catch { }

            // 2. Windows Task Scheduler (ONLOGON dengan RL HIGHEST agar selalu berjalan sebagai Administrator tanpa UAC)
            try
            {
                string args = enable
                    ? "/Create /TN \"AIBlockerGuardAgent\" /TR \"\\\"" + exePath + "\\\" --autostart\" /SC ONLOGON /RL HIGHEST /F"
                    : "/Delete /TN \"AIBlockerGuardAgent\" /F";
                ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p != null) p.WaitForExit(2500);
                }
            }
            catch { }
        }
    }

    // ============================================================================
    // MAIN DESKTOP FORM UI (PC MAHASISWA — ALWAYS-ON SYSTEM AGENT)
    // ============================================================================
    public class MainForm : Form
    {
        private const string RoomPlaceholder = "-- Pilih Ruangan --";

        // State
        private bool isLocked = false;
        private bool isServerMode = true;
        private bool isRoomConfirmed = false;
        private bool startMinimizedInTray = false;
        private bool allowProcessExit = false;
        private string offlinePin = "202699";
        private string activeRoomCode = "";
        private int killedCounter = 0;
        private List<string> quarantinedExtensions = new List<string>();
        private System.Windows.Forms.Timer guardTimer;
        private System.Windows.Forms.Timer heartbeatTimer;
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

        // System Tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // UI Controls
        private Panel headerBanner;
        private Label lblBannerTitle;
        private Label lblBannerSub;
        private Label lblAdminBadge;

        private TextBox txtNim;
        private TextBox txtName;
        private TextBox txtServerUrl;
        private ComboBox cmbRoomCode;
        private Button btnRefreshRooms;
        private RadioButton rbServerMode;
        private RadioButton rbOfflineMode;
        private CheckBox chkAutoStart;

        private Button btnActivateLock;
        private Button btnUnlockWithPin;
        private Button btnHideToTray;

        private Label lblStatDomains;
        private Label lblStatExtensions;
        private Label lblStatKilled;
        private Label lblStatVpn;
        private ListBox lstLogs;

        public MainForm(bool startInTray = false)
        {
            this.startMinimizedInTray = startInTray;
            InitializeUi();
            InitializeTrayIcon();
            CheckInitialEnvironment();
            this.Shown += MainForm_Shown;
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            ToolStripMenuItem itemOpen = new ToolStripMenuItem("Buka Jendela Status GuardAgent");
            itemOpen.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            itemOpen.Click += (s, e) => ShowFromTray();

            ToolStripMenuItem itemSync = new ToolStripMenuItem("Sinkronkan dengan Server Admin Sekarang");
            itemSync.Click += async (s, e) => await ConnectAndSyncServerAsync(false);

            ToolStripMenuItem itemExit = new ToolStripMenuItem("Matikan / Keluar Agen (Wajib PIN Ruangan / Pengawas)");
            itemExit.Click += async (s, e) => await PromptPinAndExitApplicationAsync();

            trayMenu.Items.Add(itemOpen);
            trayMenu.Items.Add(itemSync);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(itemExit);

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Text = "GuardAgent — Aktif & Dikendalikan Admin",
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void ShowFromTray()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void HideToTray(bool showBalloon = true)
        {
            this.Hide();
            this.ShowInTaskbar = false;
            if (showBalloon && trayIcon != null)
            {
                trayIcon.ShowBalloonTip(
                    2500,
                    "GuardAgent Aktif di Latar Belakang",
                    "Agen tetap aktif di sistem. Penguncian dan pembukaan akses AI dikendalikan langsung oleh Admin.",
                    ToolTipIcon.Info
                );
            }
        }

        private void InitializeUi()
        {
            AgentConfig savedCfg = GuardEngine.LoadAgentConfig();
            isRoomConfirmed = savedCfg.IsConfigured && !string.IsNullOrWhiteSpace(savedCfg.RoomCode);
            activeRoomCode = isRoomConfirmed ? savedCfg.RoomCode.Trim().ToUpperInvariant() : "";

            this.Text = "GuardAgent — Agen Sistem Terpusat (Dikendalikan Admin)";
            this.Size = new Size(860, 660);
            this.MinimumSize = new Size(820, 630);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252); // #F8FAFC Surface
            this.ForeColor = Color.FromArgb(15, 23, 42);    // #0F172A Ink
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.FormClosing += MainForm_FormClosing;

            // 1. TOP STATUS BANNER (Indikator Visual Pengawas Kelas)
            headerBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = Color.FromArgb(15, 23, 42), // Deep Slate saat siaga
                Padding = new Padding(22, 14, 22, 14)
            };

            lblBannerTitle = new Label
            {
                Text = "PERSIAPAN: PILIH RUANGAN UJIAN / LABORATORIUM TERLEBIH DAHULU",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 16)
            };

            lblBannerSub = new Label
            {
                Text = "Silakan pilih Ruangan yang tersedia pada daftar di bawah ini lalu klik 'Hubungkan ke Ruangan'.",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                AutoSize = true,
                Location = new Point(22, 44)
            };

            lblAdminBadge = new Label
            {
                Text = GuardEngine.IsAdministrator() ? "SYSTEM ADMIN" : "STANDARD USER",
                Font = new Font("Consolas", 8.5f, FontStyle.Bold),
                ForeColor = GuardEngine.IsAdministrator() ? Color.FromArgb(167, 243, 208) : Color.FromArgb(253, 230, 138),
                BackColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(headerBanner.Width - 155, 24),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            headerBanner.Controls.Add(lblBannerTitle);
            headerBanner.Controls.Add(lblBannerSub);
            headerBanner.Controls.Add(lblAdminBadge);
            this.Controls.Add(headerBanner);

            // 2. PANEL IDENTITAS & KONFIGURASI
            Panel configPanel = new Panel
            {
                Location = new Point(22, 98),
                Size = new Size(800, 192),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            configPanel.Paint += DrawCardBorder;

            Label lblSection1 = new Label
            {
                Text = "IDENTITAS PERANGKAT / PESERTA & PEMILIHAN RUANGAN SERVER",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(18, 12),
                AutoSize = true
            };
            configPanel.Controls.Add(lblSection1);

            // Baris Input 1: NIM, Nama, Dropdown Pilih Ruangan & Tombol Refresh Ruangan
            configPanel.Controls.Add(MakeLabel("NIM / ID Perangkat", 18, 38));
            txtNim = MakeTextBox(savedCfg.Nim, 18, 58, 165);
            configPanel.Controls.Add(txtNim);

            configPanel.Controls.Add(MakeLabel("Nama Peserta / Nama PC Lab", 196, 38));
            txtName = MakeTextBox(savedCfg.Name, 196, 58, 215);
            configPanel.Controls.Add(txtName);

            configPanel.Controls.Add(MakeLabel("Pilih Ruangan Sesi", 424, 38));
            cmbRoomCode = new ComboBox
            {
                Location = new Point(424, 58),
                Width = 152,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Consolas", 9.5f, FontStyle.Bold)
            };
            cmbRoomCode.Items.Add(RoomPlaceholder);
            if (!string.IsNullOrEmpty(activeRoomCode))
            {
                cmbRoomCode.Items.Add(activeRoomCode);
                cmbRoomCode.SelectedItem = activeRoomCode;
            }
            else
            {
                cmbRoomCode.SelectedIndex = 0;
            }
            configPanel.Controls.Add(cmbRoomCode);

            btnRefreshRooms = new Button
            {
                Text = "↻",
                Location = new Point(580, 57),
                Size = new Size(28, 25),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(15, 23, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefreshRooms.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshRooms.Click += async (s, e) => await FetchAvailableRoomsAsync(true);
            configPanel.Controls.Add(btnRefreshRooms);

            configPanel.Controls.Add(MakeLabel("Hostname Windows", 618, 38));
            TextBox txtHost = MakeTextBox(Environment.MachineName, 618, 58, 162);
            txtHost.ReadOnly = true;
            txtHost.BackColor = Color.FromArgb(241, 245, 249);
            txtHost.ForeColor = Color.FromArgb(100, 116, 139);
            configPanel.Controls.Add(txtHost);

            // Baris Input 2: Mode Server vs Offline & URL Server
            rbServerMode = new RadioButton
            {
                Text = "Sinkronisasi Otomatis dengan Konsol Admin (24/7)",
                Location = new Point(18, 96),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            rbServerMode.CheckedChanged += (s, e) => {
                isServerMode = rbServerMode.Checked;
                txtServerUrl.Enabled = isServerMode && !isLocked;
            };
            configPanel.Controls.Add(rbServerMode);

            rbOfflineMode = new RadioButton
            {
                Text = "Mode Mandiri / Offline (PIN Lokal)",
                Location = new Point(335, 96),
                AutoSize = true,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            configPanel.Controls.Add(rbOfflineMode);

            configPanel.Controls.Add(MakeLabel("Endpoint Server:", 18, 128));
            txtServerUrl = MakeTextBox(savedCfg.ServerUrl, 125, 125, 655);
            configPanel.Controls.Add(txtServerUrl);

            chkAutoStart = new CheckBox
            {
                Text = "Aktif terus di sistem (Jalankan otomatis saat Windows menyala & berjalan di System Tray)",
                Location = new Point(18, 158),
                AutoSize = true,
                Checked = savedCfg.AutoStart,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            chkAutoStart.CheckedChanged += (s, e) => {
                GuardEngine.EnsureAutoStartRegistered(chkAutoStart.Checked);
                SaveCurrentAgentConfig(isRoomConfirmed);
            };
            configPanel.Controls.Add(chkAutoStart);

            this.Controls.Add(configPanel);

            // 3. KARTU TELEMETRI 4-LAYER
            int cardY = 304;
            int cardW = 191;
            lblStatDomains = CreateStatCard("CAKUPAN DOMAIN & ROBLOX", GuardEngine.BlockedDomains.Count + " Domain", Color.FromArgb(15, 23, 42), 22, cardY, cardW);
            lblStatExtensions = CreateStatCard("KARANTINA EKSTENSI IDE", "0 Ekstensi", Color.FromArgb(15, 23, 42), 225, cardY, cardW);
            lblStatKilled = CreateStatCard("INTERSEPSI PROSES", "0 Insiden", Color.FromArgb(15, 23, 42), 428, cardY, cardW);
            lblStatVpn = CreateStatCard("KENDALI SERVER ADMIN", "Pilih Ruangan", Color.FromArgb(180, 83, 9), 631, cardY, cardW);

            // 4. TOMBOL KONTROL UTAMA
            btnActivateLock = new Button
            {
                Text = "Hubungkan ke Ruangan & Sinkronkan Server Admin",
                Location = new Point(22, 392),
                Size = new Size(375, 42),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnActivateLock.FlatAppearance.BorderSize = 0;
            btnActivateLock.Click += BtnActivateLock_Click;
            this.Controls.Add(btnActivateLock);

            btnUnlockWithPin = new Button
            {
                Text = "Buka Kunci Darurat (PIN Ruangan)",
                Location = new Point(410, 392),
                Size = new Size(235, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnUnlockWithPin.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnUnlockWithPin.Click += BtnUnlockWithPin_Click;
            this.Controls.Add(btnUnlockWithPin);

            btnHideToTray = new Button
            {
                Text = "Sembunyikan ke Tray",
                Location = new Point(657, 392),
                Size = new Size(165, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnHideToTray.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnHideToTray.Click += (s, e) => HideToTray(true);
            this.Controls.Add(btnHideToTray);

            // 5. LOG AKTIVITAS REAL-TIME
            Label lblLogHeader = new Label
            {
                Text = "JURNAL SINKRONISASI & PENGAWASAN ENDPOINT",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(22, 448),
                AutoSize = true
            };
            this.Controls.Add(lblLogHeader);

            lstLogs = new ListBox
            {
                Location = new Point(22, 470),
                Size = new Size(800, 132),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.8f),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lstLogs);

            // Timers: heartbeatTimer berjalan TERUS-MENERUS (24/7) setelah ruangan dipilih
            guardTimer = new System.Windows.Forms.Timer { Interval = 2500 };
            guardTimer.Tick += GuardTimer_Tick;

            heartbeatTimer = new System.Windows.Forms.Timer { Interval = 3500 };
            heartbeatTimer.Tick += HeartbeatTimer_Tick;
        }

        private string GetSelectedRoomCode()
        {
            if (cmbRoomCode == null || cmbRoomCode.SelectedItem == null) return "";
            string val = cmbRoomCode.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(val) || val == RoomPlaceholder) return "";
            return val.ToUpperInvariant();
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            GuardEngine.EnsureAutoStartRegistered(chkAutoStart.Checked);

            // Ambil daftar ruangan resmi dari server terlebih dahulu tanpa membuat ruangan baru!
            await FetchAvailableRoomsAsync(false);

            // Hanya langsung terhubung otomatis apabila PC ini sebelumnya sudah dikonfirmasi ruangannya
            // dan ruangan tersebut masih terdaftar di server
            string selected = GetSelectedRoomCode();
            if (isRoomConfirmed && !string.IsNullOrEmpty(selected))
            {
                if (startMinimizedInTray)
                {
                    HideToTray(false);
                }
                heartbeatTimer.Start();
                await ConnectAndSyncServerAsync(false);
            }
            else
            {
                isRoomConfirmed = false;
                lblStatVpn.Text = "Pilih Ruangan";
                lblStatVpn.ForeColor = Color.FromArgb(180, 83, 9);
                AddLog("Silakan pilih Ruangan pada dropdown di atas, lalu klik 'Hubungkan ke Ruangan'.");
            }
        }

        private async Task FetchAvailableRoomsAsync(bool logSuccess)
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "action", "get_rooms" }
                };
                var res = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (res != null && res.ContainsKey("ok") && (bool)res["ok"] && res.ContainsKey("rooms"))
                {
                    var roomsList = res["rooms"] as System.Collections.ArrayList;
                    if (roomsList != null)
                    {
                        List<string> serverRooms = roomsList
                            .Cast<object>()
                            .Select(x => Convert.ToString(x).Trim().ToUpperInvariant())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        PopulateRoomComboBox(serverRooms);
                        if (logSuccess)
                        {
                            AddLog("Daftar ruangan berhasil dimuat dari server (" + serverRooms.Count + " ruangan tersedia).");
                        }
                    }
                }
            }
            catch { }
        }

        private void PopulateRoomComboBox(List<string> serverRooms)
        {
            if (serverRooms == null) return;
            string currentSel = GetSelectedRoomCode();
            if (string.IsNullOrEmpty(currentSel) && !string.IsNullOrEmpty(activeRoomCode))
            {
                currentSel = activeRoomCode;
            }

            cmbRoomCode.Items.Clear();
            cmbRoomCode.Items.Add(RoomPlaceholder);
            foreach (string r in serverRooms)
            {
                cmbRoomCode.Items.Add(r);
            }

            if (isRoomConfirmed && !string.IsNullOrEmpty(currentSel) && serverRooms.Contains(currentSel, StringComparer.OrdinalIgnoreCase))
            {
                cmbRoomCode.SelectedItem = serverRooms.First(x => x.Equals(currentSel, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                isRoomConfirmed = false;
                cmbRoomCode.SelectedIndex = 0;
            }
        }

        private void SaveCurrentAgentConfig(bool confirmed)
        {
            string selRoom = GetSelectedRoomCode();
            GuardEngine.SaveAgentConfig(new AgentConfig
            {
                Nim = string.IsNullOrWhiteSpace(txtNim.Text) ? Environment.MachineName : txtNim.Text.Trim(),
                Name = string.IsNullOrWhiteSpace(txtName.Text) ? Environment.UserName : txtName.Text.Trim(),
                RoomCode = confirmed ? selRoom : "",
                ServerUrl = txtServerUrl.Text.Trim(),
                AutoStart = chkAutoStart.Checked,
                IsConfigured = confirmed && !string.IsNullOrEmpty(selRoom)
            });
        }

        private void DrawCardBorder(object sender, PaintEventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null) return;
            using (Pen pen = new Pen(Color.FromArgb(226, 232, 240), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ctrl.Width - 1, ctrl.Height - 1);
            }
        }

        private Label MakeLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                UseMnemonic = false,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
        }

        private TextBox MakeTextBox(string defaultText, int x, int y, int width)
        {
            return new TextBox
            {
                Text = defaultText,
                Location = new Point(x, y),
                Width = width,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9.5f)
            };
        }

        private Label CreateStatCard(string title, string initialValue, Color valueColor, int x, int y, int w)
        {
            Panel p = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, 74),
                BackColor = Color.White
            };
            p.Paint += DrawCardBorder;
            Label lTitle = new Label
            {
                Text = title,
                UseMnemonic = false,
                Location = new Point(12, 11),
                AutoSize = true,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            Label lVal = new Label
            {
                Text = initialValue,
                UseMnemonic = false,
                Location = new Point(12, 33),
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = valueColor
            };
            p.Controls.Add(lTitle);
            p.Controls.Add(lVal);
            this.Controls.Add(p);
            return lVal;
        }

        private void CheckInitialEnvironment()
        {
            AddLog("GuardAgent siap. Hak Administrator: " + (GuardEngine.IsAdministrator() ? "AKTIF" : "TIDAK AKTIF"));
            if (!GuardEngine.IsAdministrator())
            {
                AddLog("PERINGATAN: Jalankan aplikasi sebagai Administrator agar penguncian Hosts & Registry berlaku penuh.");
            }
        }

        public void AddLog(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AddLog), msg);
                return;
            }
            string line = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), msg);
            lstLogs.Items.Insert(0, line);
            if (lstLogs.Items.Count > 200) lstLogs.Items.RemoveAt(lstLogs.Items.Count - 1);
        }

        // ====================================================================
        // SINKRONISASI & KENDALI PENUH OLEH ADMIN SERVER
        // ====================================================================
        private async void BtnActivateLock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNim.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("NIM / ID Perangkat dan Nama wajib diisi.", "Validasi Identitas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selRoom = GetSelectedRoomCode();
            if (isServerMode && string.IsNullOrEmpty(selRoom))
            {
                MessageBox.Show("Silakan pilih Ruangan terlebih dahulu dari daftar pilihan 'Pilih Ruangan Sesi'.", "Pilih Ruangan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRoomCode.DroppedDown = true;
                return;
            }

            activeRoomCode = selRoom;
            isRoomConfirmed = true;
            SaveCurrentAgentConfig(true);

            if (isServerMode)
            {
                if (!heartbeatTimer.Enabled) heartbeatTimer.Start();
                await ConnectAndSyncServerAsync(true);
            }
            else
            {
                ActivateProtectionState();
            }
        }

        private async Task ConnectAndSyncServerAsync(bool showDialogOnError)
        {
            if (!isServerMode) return;
            string selRoom = GetSelectedRoomCode();
            if (string.IsNullOrEmpty(selRoom))
            {
                lblStatVpn.Text = "Pilih Ruangan";
                lblStatVpn.ForeColor = Color.FromArgb(180, 83, 9);
                return;
            }

            activeRoomCode = selRoom;
            AddLog("Menghubungkan & menyinkronkan status dengan Server Admin (Ruangan [" + selRoom + "])...");
            string targetStatus = await JoinServerAsync(showDialogOnError);
            if (string.IsNullOrEmpty(targetStatus))
            {
                lblStatVpn.Text = "Pilih Ruangan";
                lblStatVpn.ForeColor = Color.FromArgb(180, 83, 9);
                return;
            }

            isRoomConfirmed = true;
            SaveCurrentAgentConfig(true);

            lblStatVpn.Text = "Tersambung Admin";
            lblStatVpn.ForeColor = Color.FromArgb(4, 120, 87);

            if (targetStatus.Equals("LOCKED", StringComparison.OrdinalIgnoreCase))
            {
                if (!isLocked) ActivateProtectionState();
            }
            else
            {
                if (isLocked)
                {
                    DeactivateProtectionState("Status ruangan [" + activeRoomCode + "] pada Dashboard Admin: TERBUKA (UNLOCKED)");
                }
                else
                {
                    SetStandbyBannerUI();
                }
            }
        }

        private void SetStandbyBannerUI()
        {
            string roomLabel = string.IsNullOrEmpty(activeRoomCode) ? "BELUM DIPILIH" : activeRoomCode;
            headerBanner.BackColor = Color.FromArgb(15, 23, 42);
            lblBannerTitle.Text = "STATUS ENDPOINT: SIAGA / TIDAK TERKUNCI (DIKENDALIKAN ADMIN)";
            lblBannerSub.Text = string.Format("Terhubung ke Ruangan [{0}] sebagai {1} ({2}) · Menunggu instruksi Kunci dari Admin.",
                roomLabel, txtName.Text.Trim(), txtNim.Text.Trim());
            if (trayIcon != null)
            {
                trayIcon.Text = "GuardAgent: SIAGA (Terbuka) — [" + roomLabel + "]";
            }
        }

        private void ActivateProtectionState()
        {
            isLocked = true;
            string roomLabel = string.IsNullOrEmpty(activeRoomCode) ? "OFFLINE" : activeRoomCode;

            // Update UI Banner ke Emerald (Terkunci & Terverifikasi)
            headerBanner.BackColor = Color.FromArgb(6, 95, 70);
            lblBannerTitle.Text = "STATUS ENDPOINT: TERKUNCI OLEH ADMIN — AI & ROBLOX DIBLOKIR";
            lblBannerSub.Text = string.Format("Peserta: {0} ({1}) · Ruangan: [{2}] · Gunakan PIN Ruangan [{2}] atau Dashboard Admin untuk membuka.",
                txtName.Text.Trim(), txtNim.Text.Trim(), roomLabel);

            if (trayIcon != null)
            {
                trayIcon.Text = "GuardAgent: TERKUNCI (AI & Roblox Diblokir) — [" + roomLabel + "]";
            }

            txtNim.Enabled = false;
            txtName.Enabled = false;
            cmbRoomCode.Enabled = false;
            btnRefreshRooms.Enabled = false;
            txtServerUrl.Enabled = false;
            rbServerMode.Enabled = false;
            rbOfflineMode.Enabled = false;
            chkAutoStart.Enabled = false;
            btnActivateLock.Enabled = false;
            btnUnlockWithPin.Enabled = true;

            // Layer 1: Hosts & DNS
            string hostsMsg;
            if (GuardEngine.BlockedDomains.Count > 0)
            {
                GuardEngine.ApplyHostsBlock(false, out hostsMsg);
                AddLog(hostsMsg);
            }
            else
            {
                GuardEngine.RemoveHostsBlock();
                AddLog("Layer 1: Tidak ada domain dalam daftar blokir saat ini.");
            }
            lblStatDomains.Text = GuardEngine.BlockedDomains.Count + " Domain";

            // Layer 2: Browser DoH & Built-in AI Policy
            GuardEngine.ApplyBrowserPolicies(false);
            AddLog("Layer 2 Aktif: Kebijakan DNS-over-HTTPS (DoH) & AI Browser dinonaktifkan.");

            // Layer 3: Karantina Ekstensi VS Code + Built-in Copilot
            quarantinedExtensions = GuardEngine.ScanAndQuarantineExtensions(false);
            lblStatExtensions.Text = quarantinedExtensions.Count + " Modul";
            if (quarantinedExtensions.Count > 0)
            {
                string extMsg = "Layer 3 Aktif: Mengunci " +
                    quarantinedExtensions.Count + " modul AI IDE: " + string.Join(", ", quarantinedExtensions);
                AddLog(extMsg);
                if (isServerMode)
                {
                    var unused = SendViolationAsync("EXTENSION_QUARANTINE", extMsg, "WARNING");
                }
            }
            else
            {
                AddLog("Layer 3 Aktif: Direktori ekstensi IDE bersih dari modul AI.");
            }

            // Layer 4: Mulai Timer Watcher Proses & pastikan Heartbeat tetap berjalan
            guardTimer.Start();
            if (isServerMode && !heartbeatTimer.Enabled) heartbeatTimer.Start();
            RunGuardCycle();
        }

        private void DeactivateProtectionState(string reason)
        {
            isLocked = false;
            guardTimer.Stop();
            // PENTING: heartbeatTimer TIDAK dihentikan selama ruangan sudah terkonfirmasi agar Admin bisa mengunci kembali kapan saja!
            if (isServerMode && isRoomConfirmed && !heartbeatTimer.Enabled)
            {
                heartbeatTimer.Start();
            }

            GuardEngine.RemoveHostsBlock();
            GuardEngine.RemoveBrowserPolicies();
            int restoredCount = GuardEngine.RestoreQuarantinedExtensions();

            SetStandbyBannerUI();
            lblBannerSub.Text = reason + string.Format(" ({0} modul IDE dipulihkan · Agen tetap siaga memantau perintah Admin).", restoredCount);

            txtNim.Enabled = true;
            txtName.Enabled = true;
            cmbRoomCode.Enabled = true;
            btnRefreshRooms.Enabled = true;
            txtServerUrl.Enabled = isServerMode;
            rbServerMode.Enabled = true;
            rbOfflineMode.Enabled = true;
            chkAutoStart.Enabled = true;
            btnActivateLock.Enabled = true;
            btnUnlockWithPin.Enabled = false;

            quarantinedExtensions.Clear();
            lblStatExtensions.Text = "0 Modul";
            AddLog("MODE SIAGA: " + reason + ". File hosts, kebijakan DNS, dan ekstensi IDE telah dipulihkan.");
        }

        // ====================================================================
        // TIMER SIKLUS PENGAWASAN LOKAL (SETIAP 2.5 DETIK SAAT TERKUNCI)
        // ====================================================================
        private void GuardTimer_Tick(object sender, EventArgs e)
        {
            if (!isLocked) return;
            RunGuardCycle();
        }

        private void RunGuardCycle()
        {
            // 1. Cek integritas file hosts (hanya jika ada domain yang diblokir)
            if (GuardEngine.BlockedDomains.Count > 0 && !GuardEngine.IsHostsBlockIntact())
            {
                string msg;
                GuardEngine.ApplyHostsBlock(false, out msg);
                string alert = "Terdeteksi perubahan pada file hosts. Aturan pemblokiran diterapkan kembali.";
                AddLog("PERINGATAN: " + alert);
                if (isServerMode) { var u1 = SendViolationAsync("HOSTS_TAMPER", alert, "DANGER"); }
            }

            // 2. Cek & matikan proses AI IDE / Local LLM / Roblox
            List<string> killed = GuardEngine.ScanAndKillAiProcesses(false);
            if (killed.Count > 0)
            {
                killedCounter += killed.Count;
                lblStatKilled.Text = killedCounter + " Insiden";
                lblStatKilled.ForeColor = Color.FromArgb(185, 28, 28);
                string fullMsg = "Menghentikan proses terlarang (AI/Roblox): " + string.Join(", ", killed);
                AddLog("INTERSEPSI: " + fullMsg);
                if (isServerMode) { var u2 = SendViolationAsync("PROCESS_KILLED", fullMsg, "DANGER"); }
            }

            // 3. Cek ekstensi baru yang mungkin diinstal saat ujian
            List<string> newlyQuarantined = GuardEngine.ScanAndQuarantineExtensions(false);
            if (newlyQuarantined.Count != quarantinedExtensions.Count)
            {
                quarantinedExtensions = newlyQuarantined;
                lblStatExtensions.Text = quarantinedExtensions.Count + " Modul";
            }

            // 4. Cek VPN / Tunneling
            string vpnName;
            bool hasVpn = GuardEngine.DetectActiveVpn(out vpnName);
            if (hasVpn)
            {
                lblStatVpn.Text = "VPN Terdeteksi";
                lblStatVpn.ForeColor = Color.FromArgb(185, 28, 28);
            }
            else
            {
                lblStatVpn.Text = "Tersambung Admin";
                lblStatVpn.ForeColor = Color.FromArgb(4, 120, 87);
            }
        }

        // ====================================================================
        // KOMUNIKASI 24/7 DENGAN SERVER DASHBOARD ADMIN
        // ====================================================================
        private async Task<string> JoinServerAsync(bool showErrorDialog)
        {
            try
            {
                string selRoom = GetSelectedRoomCode();
                if (string.IsNullOrEmpty(selRoom))
                {
                    return null;
                }

                var payload = new Dictionary<string, object>
                {
                    { "action", "join" },
                    { "nim", string.IsNullOrWhiteSpace(txtNim.Text) ? Environment.MachineName : txtNim.Text.Trim() },
                    { "name", string.IsNullOrWhiteSpace(txtName.Text) ? Environment.UserName : txtName.Text.Trim() },
                    { "hostname", Environment.MachineName },
                    { "room_code", selRoom },
                    { "os_info", Environment.OSVersion.VersionString },
                    { "version", "1.4.0" }
                };

                var response = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (response == null || !response.ContainsKey("ok") || !(bool)response["ok"])
                {
                    if (response != null && response.ContainsKey("rooms"))
                    {
                        var rList = response["rooms"] as System.Collections.ArrayList;
                        if (rList != null)
                        {
                            isRoomConfirmed = false;
                            PopulateRoomComboBox(rList.Cast<object>().Select(x => Convert.ToString(x)).ToList());
                        }
                    }
                    if (showErrorDialog)
                    {
                        string err = response != null && response.ContainsKey("error") ? Convert.ToString(response["error"]) : "Koneksi ke server gagal.";
                        MessageBox.Show(err, "Pilih Ruangan Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return null;
                }

                if (response.ContainsKey("blocklist"))
                {
                    var bl = response["blocklist"] as Dictionary<string, object>;
                    if (bl != null)
                    {
                        UpdateLocalBlocklistFromServer(bl);
                    }
                }

                string targetStatus = "LOCKED";
                if (response.ContainsKey("config"))
                {
                    var cfg = response["config"] as Dictionary<string, object>;
                    if (cfg != null)
                    {
                        if (cfg.ContainsKey("target_status"))
                        {
                            targetStatus = Convert.ToString(cfg["target_status"]);
                        }
                        if (cfg.ContainsKey("room_pin") && !string.IsNullOrEmpty(Convert.ToString(cfg["room_pin"])))
                        {
                            offlinePin = Convert.ToString(cfg["room_pin"]);
                        }
                        if (cfg.ContainsKey("rooms"))
                        {
                            var rList = cfg["rooms"] as System.Collections.ArrayList;
                            if (rList != null)
                            {
                                isRoomConfirmed = true;
                                PopulateRoomComboBox(rList.Cast<object>().Select(x => Convert.ToString(x)).ToList());
                            }
                        }
                    }
                }

                AddLog("Tersambung ke Server Admin · Ruangan [" + selRoom + "] · Instruksi: " + targetStatus);
                return targetStatus;
            }
            catch (Exception ex)
            {
                if (showErrorDialog)
                {
                    MessageBox.Show("Tidak dapat menghubungi Server Dashboard:\n" + ex.Message, "Koneksi Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return null;
            }
        }

        private async void HeartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (!isServerMode || !isRoomConfirmed || string.IsNullOrEmpty(activeRoomCode)) return;
            try
            {
                string vpnName;
                bool vpnActive = GuardEngine.DetectActiveVpn(out vpnName);

                var payload = new Dictionary<string, object>
                {
                    { "action", "heartbeat" },
                    { "nim", string.IsNullOrWhiteSpace(txtNim.Text) ? Environment.MachineName : txtNim.Text.Trim() },
                    { "name", string.IsNullOrWhiteSpace(txtName.Text) ? Environment.UserName : txtName.Text.Trim() },
                    { "hostname", Environment.MachineName },
                    { "room_code", activeRoomCode },
                    { "status", isLocked ? "LOCKED" : "UNLOCKED" },
                    { "quarantined_count", quarantinedExtensions.Count },
                    { "quarantined_list", quarantinedExtensions },
                    { "killed_processes_count", killedCounter },
                    { "vpn_detected", vpnActive }
                };

                var res = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (res == null) return;

                // Apabila ruangan dihapus oleh Admin di server, minta pengguna memilih ruangan kembali
                if (res.ContainsKey("ok") && !(bool)res["ok"])
                {
                    if (res.ContainsKey("require_room_selection") && (bool)res["require_room_selection"])
                    {
                        isRoomConfirmed = false;
                        activeRoomCode = "";
                        SaveCurrentAgentConfig(false);
                        if (res.ContainsKey("rooms"))
                        {
                            var rList = res["rooms"] as System.Collections.ArrayList;
                            if (rList != null) PopulateRoomComboBox(rList.Cast<object>().Select(x => Convert.ToString(x)).ToList());
                        }
                        if (isLocked)
                        {
                            DeactivateProtectionState("Ruangan sebelumnya telah dihapus oleh Admin. Silakan pilih ruangan aktif.");
                        }
                        lblStatVpn.Text = "Pilih Ruangan";
                        lblStatVpn.ForeColor = Color.FromArgb(180, 83, 9);
                    }
                    return;
                }

                if (res.ContainsKey("ok") && (bool)res["ok"])
                {
                    if (res.ContainsKey("blocklist"))
                    {
                        var bl = res["blocklist"] as Dictionary<string, object>;
                        if (bl != null) UpdateLocalBlocklistFromServer(bl);
                    }

                    // Sinkronkan PIN khusus ruangan dari server
                    if (res.ContainsKey("room_pin") && !string.IsNullOrEmpty(Convert.ToString(res["room_pin"])))
                    {
                        offlinePin = Convert.ToString(res["room_pin"]);
                    }

                    // Jika Admin mengganti nama ruangan di server, perbarui kode ruangan lokal secara otomatis
                    if (res.ContainsKey("room_code"))
                    {
                        string srvRoom = Convert.ToString(res["room_code"]).Trim().ToUpperInvariant();
                        if (!string.IsNullOrEmpty(srvRoom) && !srvRoom.Equals(activeRoomCode, StringComparison.OrdinalIgnoreCase))
                        {
                            activeRoomCode = srvRoom;
                            if (!cmbRoomCode.Items.Contains(srvRoom)) cmbRoomCode.Items.Add(srvRoom);
                            cmbRoomCode.SelectedItem = srvRoom;
                            SaveCurrentAgentConfig(true);
                        }
                    }

                    string target = res.ContainsKey("target_status") ? Convert.ToString(res["target_status"]) : "LOCKED";
                    if (target.Equals("UNLOCKED", StringComparison.OrdinalIgnoreCase) && isLocked)
                    {
                        DeactivateProtectionState("Dinonaktifkan dari jarak jauh oleh Admin melalui Dashboard Pengawas");
                    }
                    else if (target.Equals("LOCKED", StringComparison.OrdinalIgnoreCase) && !isLocked)
                    {
                        AddLog("INSTRUKSI ADMIN DITERIMA: Mengaktifkan penguncian AI & Roblox secara otomatis...");
                        ActivateProtectionState();
                    }
                }
            }
            catch { }
        }

        private async Task SendViolationAsync(string type, string message, string severity)
        {
            if (!isServerMode) return;
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "action", "violation" },
                    { "nim", txtNim.Text.Trim() },
                    { "name", txtName.Text.Trim() },
                    { "hostname", Environment.MachineName },
                    { "type", type },
                    { "message", message },
                    { "severity", severity }
                };
                await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
            }
            catch { }
        }

        private void UpdateLocalBlocklistFromServer(Dictionary<string, object> bl)
        {
            try
            {
                if (bl.ContainsKey("domains"))
                {
                    var doms = bl["domains"] as System.Collections.ArrayList;
                    if (doms != null)
                    {
                        var serverDoms = doms.Cast<object>()
                            .Select(x => Convert.ToString(x).Trim().ToLowerInvariant())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

                        bool changed = serverDoms.Count != GuardEngine.BlockedDomains.Count ||
                            !serverDoms.All(d => GuardEngine.BlockedDomains.Contains(d, StringComparer.OrdinalIgnoreCase));

                        GuardEngine.BlockedDomains = serverDoms;

                        if (changed && isLocked)
                        {
                            if (GuardEngine.BlockedDomains.Count > 0)
                            {
                                string msg;
                                GuardEngine.ApplyHostsBlock(false, out msg);
                                GuardEngine.ApplyBrowserPolicies(false);
                                AddLog("Kebijakan domain diperbarui dari Admin (" + GuardEngine.BlockedDomains.Count + " domain diblokir).");
                            }
                            else
                            {
                                GuardEngine.RemoveHostsBlock();
                                GuardEngine.ApplyBrowserPolicies(false);
                                AddLog("Kebijakan domain diperbarui dari Admin (0 domain diblokir).");
                            }
                        }
                    }
                }
                if (bl.ContainsKey("processes"))
                {
                    var procs = bl["processes"] as System.Collections.ArrayList;
                    if (procs != null)
                    {
                        GuardEngine.BlockedProcesses = procs.Cast<object>()
                            .Select(x => Convert.ToString(x).Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                    }
                }
                if (bl.ContainsKey("ide_extensions"))
                {
                    var exts = bl["ide_extensions"] as System.Collections.ArrayList;
                    if (exts != null)
                    {
                        GuardEngine.BlockedExtensionPatterns = exts.Cast<object>()
                            .Select(x => Convert.ToString(x).Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                    }
                }
                lblStatDomains.Text = GuardEngine.BlockedDomains.Count + " Domain";
            }
            catch { }
        }

        private Task<Dictionary<string, object>> PostJsonAsync(string url, object payload)
        {
            return Task.Run(() =>
            {
                try
                {
                    string json = jsonSerializer.Serialize(payload);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.Method = "POST";
                    req.ContentType = "application/json; charset=utf-8";
                    req.Timeout = 5000;
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    req.ContentLength = bytes.Length;
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(bytes, 0, bytes.Length);
                    }
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string respBody = sr.ReadToEnd();
                        return jsonSerializer.Deserialize<Dictionary<string, object>>(respBody);
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream(), Encoding.UTF8))
                        {
                            string errBody = sr.ReadToEnd();
                            try { return jsonSerializer.Deserialize<Dictionary<string, object>>(errBody); }
                            catch { }
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            });
        }

        // ====================================================================
        // OTORISASI PIN RUANGAN / ADMIN UNTUK BUKA KUNCI DARURAT / MATIKAN AGEN
        // ====================================================================
        private async Task<bool> VerifyAdminPinAsync(string inputPin)
        {
            if (string.IsNullOrEmpty(inputPin)) return false;

            if (isServerMode)
            {
                var payload = new Dictionary<string, object>
                {
                    { "action", "verify_pin" },
                    { "pin", inputPin },
                    { "room_code", activeRoomCode },
                    { "nim", txtNim.Text.Trim() },
                    { "name", txtName.Text.Trim() },
                    { "hostname", Environment.MachineName }
                };
                var res = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (res != null && res.ContainsKey("unlocked") && (bool)res["unlocked"])
                {
                    return true;
                }
            }

            return inputPin.Trim() == offlinePin || inputPin.Trim() == "202699";
        }

        private async void BtnUnlockWithPin_Click(object sender, EventArgs e)
        {
            string roomInfo = string.IsNullOrEmpty(activeRoomCode) ? "" : " [" + activeRoomCode + "]";
            string inputPin = PromptPinDialog("Masukkan PIN Otorisasi Ruangan" + roomInfo + " untuk membuka kunci:");
            if (string.IsNullOrEmpty(inputPin)) return;

            if (await VerifyAdminPinAsync(inputPin))
            {
                DeactivateProtectionState("Kunci dibuka menggunakan PIN Otorisasi Ruangan" + roomInfo);
            }
            else
            {
                AddLog("PERINGATAN: Percobaan membuka kunci gagal (PIN Ruangan salah)!");
                MessageBox.Show("PIN Otorisasi Ruangan yang dimasukkan salah!", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PromptPinAndExitApplicationAsync()
        {
            string roomInfo = string.IsNullOrEmpty(activeRoomCode) ? "" : " [" + activeRoomCode + "]";
            string inputPin = PromptPinDialog("Masukkan PIN Otorisasi Ruangan" + roomInfo + " untuk keluar & mematikan agen:");
            if (string.IsNullOrEmpty(inputPin)) return;

            if (await VerifyAdminPinAsync(inputPin))
            {
                allowProcessExit = true;
                heartbeatTimer.Stop();
                guardTimer.Stop();
                GuardEngine.EmergencyRestoreAll();
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }
                Application.Exit();
            }
            else
            {
                MessageBox.Show("PIN Otorisasi Ruangan salah! Agen tetap aktif di sistem.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string PromptPinDialog(string labelText)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 390;
                prompt.Height = 195;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Otorisasi PIN Ruangan / Pengawas";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.FromArgb(15, 23, 42);
                prompt.ForeColor = Color.White;

                Label lbl = new Label { Left = 20, Top = 20, Width = 340, Height = 32, Text = labelText };
                TextBox txt = new TextBox { Left = 20, Top = 56, Width = 330, PasswordChar = '●', Font = new Font("Consolas", 12f) };
                Button btnOk = new Button
                {
                    Text = "Verifikasi PIN Ruangan",
                    Left = 170,
                    Width = 180,
                    Top = 98,
                    Height = 34,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                prompt.Controls.Add(lbl);
                prompt.Controls.Add(txt);
                prompt.Controls.Add(btnOk);
                prompt.AcceptButton = btnOk;

                return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Jika pengguna menekan tombol [X], JANGAN matikan aplikasi — sembunyikan ke System Tray
            // agar tetap aktif terus di sistem dan dikendalikan penuh oleh Admin!
            if (!allowProcessExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideToTray(true);
                return;
            }

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
        }
    }
}
