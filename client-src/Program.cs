using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
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
                    "AI-Blocker Guard - Restore Selesai",
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
    // NAMED PIPE CLIENT TO AIBlockerGuardService (SYSTEM SERVICE)
    // ============================================================================
    public static class ServicePipeClient
    {
        public const string PipeName = "AIBlockerGuardPipe";

        public static Dictionary<string, object> SendCommand(Dictionary<string, object> payload, int timeoutMs = 2000)
        {
            try
            {
                JavaScriptSerializer ser = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };
                using (NamedPipeClientStream client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                {
                    client.Connect(timeoutMs);
                    using (StreamWriter writer = new StreamWriter(client, Encoding.UTF8, 4096, true))
                    {
                        writer.WriteLine(ser.Serialize(payload));
                        writer.Flush();
                    }
                    using (StreamReader reader = new StreamReader(client, Encoding.UTF8, false, 4096, true))
                    {
                        string respLine = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(respLine))
                        {
                            return ser.Deserialize<Dictionary<string, object>>(respLine);
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static bool IsServiceAvailable()
        {
            var res = SendCommand(new Dictionary<string, object> { { "cmd", "PING" } }, 700);
            return res != null && res.ContainsKey("ok") && Convert.ToBoolean(res["ok"]);
        }
    }

    // ============================================================================
    // 4-LAYER AI BLOCKER ENGINE
    // ============================================================================
    public static class GuardEngine
    {
        public const string CurrentVersion = "1.6.0";
        public static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AIBlockerGuard"
        );
        public static readonly string UserDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIBlockerGuard"
        );
        public static readonly string QuarantineDir = Path.Combine(UserDataDir, "Quarantine");
        public static readonly string ManifestFile = Path.Combine(UserDataDir, "quarantine_manifest.json");
        public static readonly string VsCodeBackupFile = Path.Combine(UserDataDir, "vscode_settings_backup.json");
        public static readonly string AgentConfigFile = Path.Combine(AppDataDir, "agent_config.json");
        public static readonly string UserAgentConfigFile = Path.Combine(UserDataDir, "agent_config.json");
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
            try { if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir); } catch { }
            try { if (!Directory.Exists(UserDataDir)) Directory.CreateDirectory(UserDataDir); } catch { }
            try { if (!Directory.Exists(QuarantineDir)) Directory.CreateDirectory(QuarantineDir); } catch { }
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

            if (!IsAdministrator())
            {
                var res = ServicePipeClient.SendCommand(new Dictionary<string, object>
                {
                    { "cmd", "APPLY_SYSTEM_LOCK" },
                    { "domains", BlockedDomains },
                    { "block_doh", true }
                });
                if (res != null && res.ContainsKey("ok") && Convert.ToBoolean(res["ok"]))
                {
                    FlushDns();
                    statusMessage = res.ContainsKey("message") ? Convert.ToString(res["message"]) : "Layer 1 Aktif via System Service.";
                    return true;
                }
                statusMessage = "Mode Standard User (Service belum terpasang): Proteksi IDE & Proses tetap aktif.";
                return false;
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
            if (!IsAdministrator())
            {
                var res = ServicePipeClient.SendCommand(new Dictionary<string, object>
                {
                    { "cmd", "CHECK_HOSTS" },
                    { "domains", BlockedDomains }
                }, 1000);
                if (res != null && res.ContainsKey("intact"))
                {
                    return Convert.ToBoolean(res["intact"]);
                }
                return true;
            }

            try
            {
                if (BlockedDomains == null || BlockedDomains.Count == 0) return true;
                if (!File.Exists(HostsPath)) return false;
                string content = File.ReadAllText(HostsPath, Encoding.UTF8);
                int startIdx = content.IndexOf(HostsStartMarker, StringComparison.Ordinal);
                int endIdx = content.IndexOf(HostsEndMarker, StringComparison.Ordinal);
                if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return false;

                string blockSection = content.Substring(startIdx, endIdx - startIdx);
                int expectedCount = BlockedDomains
                    .Select(d => (d ?? "").Trim().ToLowerInvariant())
                    .Where(d => !string.IsNullOrEmpty(d) && !d.Contains(" "))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                int actualZeroCount = 0;
                string[] lines = blockSection.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (line.TrimStart().StartsWith("0.0.0.0 ", StringComparison.Ordinal))
                    {
                        actualZeroCount++;
                    }
                }
                return actualZeroCount == expectedCount;
            }
            catch
            {
                return true;
            }
        }

        public static void RemoveHostsBlock()
        {
            if (!IsAdministrator())
            {
                ServicePipeClient.SendCommand(new Dictionary<string, object> { { "cmd", "RESTORE_SYSTEM" } });
                FlushDns();
                return;
            }

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

            ResetBrowserNetworkSockets();
        }

        public static void ResetBrowserNetworkSockets()
        {
            // Browser modern (Chrome, Edge, Brave, Opera, Vivaldi, Firefox) mempertahankan koneksi HTTP/2 & QUIC (UDP 443)
            // serta cache DNS internal di dalam sub-proses "network.mojom.NetworkService" (atau "--type=socket" di Firefox).
            // Menghentikan sub-proses jaringan ini (tanpa menutup tab/jendela browser) memaksa browser me-reset soket
            // secara instan (< 0.5 detik) dan membaca ulang file hosts Windows!
            try
            {
                string wmiQuery = "SELECT ProcessId, Name, CommandLine FROM Win32_Process WHERE " +
                    "(Name='chrome.exe' OR Name='msedge.exe' OR Name='brave.exe' OR Name='opera.exe' OR Name='vivaldi.exe' OR Name='firefox.exe') " +
                    "AND (CommandLine LIKE '%network.mojom.NetworkService%' OR CommandLine LIKE '%--type=socket%')";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        try
                        {
                            int pid = Convert.ToInt32(obj["ProcessId"]);
                            using (Process p = Process.GetProcessById(pid))
                            {
                                p.Kill();
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        // --------------------------------------------------------------------
        // LAYER 2: BROWSER DNS-OVER-HTTPS (DoH), URLBLOCKLIST & BUILT-IN AI POLICY
        // --------------------------------------------------------------------
        private static readonly string[] SearchAiBlockPatterns = new string[]
        {
            "google.com/search",
            "google.co.id/search",
            "google.com/aimode",
            "google.co.id/aimode",
            "google.com/search?udm=50",
            "google.co.id/search?udm=50",
            "google.com/search?udm=14&udm=50",
            "google.co.id/search?udm=14&udm=50",
            "bing.com/chat",
            "bing.com/copilotsearch",
            "bing.com/search?showconv=1",
            "duck.ai",
            "duckduckgo.com/?ia=chat",
            "search.brave.com/ask",
            "search.brave.com/search?summary=1",
            "yandex.com/search/neuro",
            "search.yahoo.com/search?fr=ai"
        };

        public static void ApplyBrowserPolicies(bool dryRun)
        {
            if (dryRun) return;
            try
            {
                bool hasDomains = (BlockedDomains != null && BlockedDomains.Count > 0);
                bool googleComBlocked = false;
                bool googleCoIdBlocked = false;
                HashSet<string> uniqueEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (hasDomains)
                {
                    foreach (string rawDomain in BlockedDomains)
                    {
                        string d = (rawDomain ?? "").Trim().ToLowerInvariant();
                        if (string.IsNullOrEmpty(d) || d.Contains(" ")) continue;
                        if (d == "google.com" || d == "www.google.com") googleComBlocked = true;
                        if (d == "google.co.id" || d == "www.google.co.id") googleCoIdBlocked = true;
                        uniqueEntries.Add(d);
                    }
                    foreach (string p in SearchAiBlockPatterns)
                    {
                        uniqueEntries.Add(p);
                    }
                }

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
                                        key.SetValue("AIModeSettings", 1, RegistryValueKind.DWord);
                                        key.SetValue("GeminiSettings", 1, RegistryValueKind.DWord);
                                        key.SetValue("GenAILocalFoundationalModelSettings", 1, RegistryValueKind.DWord);
                                    }
                                    else if (bPath.EndsWith("Edge"))
                                    {
                                        key.SetValue("HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                                        key.SetValue("CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                                        key.SetValue("CopilotPageContext", 0, RegistryValueKind.DWord);
                                        key.SetValue("EdgeEntraCopilotPageContext", 0, RegistryValueKind.DWord);
                                        key.SetValue("DiscoverPageContextEnabled", 0, RegistryValueKind.DWord);
                                    }
                                    else if (bPath.EndsWith("Brave"))
                                    {
                                        key.SetValue("BraveAIChatEnabled", 0, RegistryValueKind.DWord);
                                    }

                                    if (hasDomains && !googleComBlocked)
                                    {
                                        key.SetValue("DefaultSearchProviderEnabled", 1, RegistryValueKind.DWord);
                                        key.SetValue("DefaultSearchProviderName", "Google Web (Mode Ujian)", RegistryValueKind.String);
                                        key.SetValue("DefaultSearchProviderKeyword", "g", RegistryValueKind.String);
                                        key.SetValue("DefaultSearchProviderSearchURL", "https://www.google.com/search?q={searchTerms}&udm=14", RegistryValueKind.String);
                                    }
                                    else
                                    {
                                        key.DeleteValue("DefaultSearchProviderEnabled", false);
                                        key.DeleteValue("DefaultSearchProviderName", false);
                                        key.DeleteValue("DefaultSearchProviderKeyword", false);
                                        key.DeleteValue("DefaultSearchProviderSearchURL", false);
                                    }

                                    // Rebuild URLBlocklist & URLAllowlist so Chromium immediately blocks even already-open tabs
                                    key.DeleteSubKeyTree("URLBlocklist", false);
                                    if (hasDomains && uniqueEntries.Count > 0)
                                    {
                                        using (RegistryKey ubKey = key.CreateSubKey("URLBlocklist"))
                                        {
                                            if (ubKey != null)
                                            {
                                                int idx = 1;
                                                foreach (string entry in uniqueEntries)
                                                {
                                                    ubKey.SetValue(idx.ToString(), entry, RegistryValueKind.String);
                                                    idx++;
                                                }
                                            }
                                        }
                                    }

                                    key.DeleteSubKeyTree("URLAllowlist", false);
                                    if (hasDomains && (!googleComBlocked || !googleCoIdBlocked))
                                    {
                                        using (RegistryKey uaKey = key.CreateSubKey("URLAllowlist"))
                                        {
                                            if (uaKey != null)
                                            {
                                                int aIdx = 1;
                                                if (!googleComBlocked)
                                                {
                                                    uaKey.SetValue(aIdx.ToString(), "google.com/search?udm=14", RegistryValueKind.String);
                                                    aIdx++;
                                                }
                                                if (!googleCoIdBlocked)
                                                {
                                                    uaKey.SetValue(aIdx.ToString(), "google.co.id/search?udm=14", RegistryValueKind.String);
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
                                    key.DeleteValue("AIModeSettings", false);
                                    key.DeleteValue("GeminiSettings", false);
                                    key.DeleteValue("GenAILocalFoundationalModelSettings", false);
                                    key.DeleteValue("HubsSidebarEnabled", false);
                                    key.DeleteValue("CopilotCDPPageContext", false);
                                    key.DeleteValue("CopilotPageContext", false);
                                    key.DeleteValue("EdgeEntraCopilotPageContext", false);
                                    key.DeleteValue("DiscoverPageContextEnabled", false);
                                    key.DeleteValue("BraveAIChatEnabled", false);
                                    key.DeleteValue("DefaultSearchProviderEnabled", false);
                                    key.DeleteValue("DefaultSearchProviderName", false);
                                    key.DeleteValue("DefaultSearchProviderKeyword", false);
                                    key.DeleteValue("DefaultSearchProviderSearchURL", false);
                                    key.DeleteValue("ChatEnabled", false);
                                    key.DeleteSubKeyTree("URLBlocklist", false);
                                    key.DeleteSubKeyTree("URLAllowlist", false);
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

            // Deteksi instalasi VS Code secara komprehensif (User Installer, System 64-bit, System 32-bit, maupun settings.json)
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string progFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string[] vscPossibleDirs = new string[]
            {
                Path.Combine(localAppData, "Programs", "Microsoft VS Code"),
                Path.Combine(progFiles, "Microsoft VS Code"),
                Path.Combine(progFilesX86, "Microsoft VS Code"),
                Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432") ?? @"C:\Program Files", "Microsoft VS Code")
            };

            bool vscInstalled = vscPossibleDirs.Any(d => Directory.Exists(d)) ||
                                File.Exists(GetVsCodeUserSettingsPath()) ||
                                Directory.Exists(Path.Combine(userProfile, ".vscode"));

            if (vscInstalled)
            {
                detectedNames.Add("vscode.builtin-copilot (chat.disableAIFeatures=true)");
            }

            // Tambahkan yang sudah ada di manifest
            foreach (var item in manifest)
            {
                detectedNames.Add(item.Name);
            }

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

        public static string GetLocalIPv4Address()
        {
            try
            {
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
                    if (endPoint != null && endPoint.Address != null)
                    {
                        return endPoint.Address.ToString();
                    }
                }
            }
            catch { }

            try
            {
                foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                        netInterface.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                    {
                        var ipProps = netInterface.GetIPProperties();
                        foreach (var addr in ipProps.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                                !System.Net.IPAddress.IsLoopback(addr.Address))
                            {
                                return addr.Address.ToString();
                            }
                        }
                    }
                }
            }
            catch { }

            return "127.0.0.1";
        }

        public static void EmergencyRestoreAll()
        {
            RemoveHostsBlock();
            RemoveBrowserPolicies();
            RestoreQuarantinedExtensions();
        }

        public const string DefaultServerUrl = "http://172.19.1.10/api/index.php";

        public static AgentConfig LoadAgentConfig()
        {
            string[] candidates = new string[] { AgentConfigFile, UserAgentConfigFile };
            foreach (string path in candidates)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path, Encoding.UTF8);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        AgentConfig cfg = ser.Deserialize<AgentConfig>(json);
                        if (cfg != null)
                        {
                            if (string.IsNullOrWhiteSpace(cfg.ServerUrl) ||
                                cfg.ServerUrl.IndexOf("blockai-wheat.vercel.app", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                cfg.ServerUrl = DefaultServerUrl;
                                SaveAgentConfig(cfg);
                            }
                            return cfg;
                        }
                    }
                }
                catch { }
            }

            return new AgentConfig
            {
                Nim = Environment.MachineName,
                Name = Environment.UserName + " (" + Environment.MachineName + ")",
                RoomCode = "",
                ServerUrl = DefaultServerUrl,
                AutoStart = true,
                IsConfigured = false
            };
        }

        public static void SaveAgentConfig(AgentConfig cfg)
        {
            EnsureDirectories();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = ser.Serialize(cfg);
            try { File.WriteAllText(AgentConfigFile, json, Encoding.UTF8); } catch { }
            try { File.WriteAllText(UserAgentConfigFile, json, Encoding.UTF8); } catch { }
        }

        public static void EnsureAutoStartRegistered(bool enable, string customExePath = null)
        {
            string exePath = !string.IsNullOrEmpty(customExePath) ? customExePath : Application.ExecutablePath;
            string runVal = "\"" + exePath + "\" --autostart";

            // 1. Registry CurrentUser & LocalMachine Run (berjalan langsung untuk semua user tanpa UAC karena asInvoker)
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

            // 2. Bersihkan Task Scheduler lama (jika ada) agar tidak memicu konflik/elevasi ganda
            if (IsAdministrator())
            {
                try
                {
                    ProcessStartInfo psiDel = new ProcessStartInfo("schtasks.exe", "/Delete /TN \"AIBlockerGuardAgent\" /F")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    using (Process p = Process.Start(psiDel))
                    {
                        if (p != null) p.WaitForExit(2000);
                    }
                }
                catch { }
            }

            // 3. Folder Startup Windows (CommonStartup & User Startup) via VBS background launcher (tanpa console & tanpa UAC)
            try
            {
                string[] startupDirs = new string[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
                };

                foreach (string dir in startupDirs)
                {
                    if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) continue;

                    string vbsPath = Path.Combine(dir, "GuardAgent-Startup.vbs");
                    string batPath = Path.Combine(dir, "GuardAgent-Startup.bat");
                    try { if (File.Exists(batPath)) File.Delete(batPath); } catch { }

                    if (enable)
                    {
                        string vbsCode =
                            "On Error Resume Next\r\n" +
                            "Set WshShell = CreateObject(\"WScript.Shell\")\r\n" +
                            "WshShell.Run \"\"\"" + exePath.Replace("\"", "\"\"") + "\"\" --autostart\", 0, False\r\n";

                        File.WriteAllText(vbsPath, vbsCode, Encoding.ASCII);
                    }
                    else
                    {
                        try { if (File.Exists(vbsPath)) File.Delete(vbsPath); } catch { }
                    }
                }
            }
            catch { }
        }
    }

    // ============================================================================
    // DESIGN TOKENS & CUSTOM CONTROLS (GUARDCONSOLE INSTITUTIONAL EDITORIAL)
    // ============================================================================
    internal enum BannerState
    {
        Setup,
        Standby,
        Locked
    }

    internal enum TelemetryTone
    {
        Neutral,
        Accent,
        Warning,
        Danger
    }

    internal sealed class AgentThemePalette
    {
        public bool IsDark;
        public Color Canvas;
        public Color Rail;
        public Color Surface;
        public Color SurfaceHover;
        public Color InputBg;
        public Color ReadonlyBg;
        public Color BorderSubtle;
        public Color BorderStrong;
        public Color InkPrimary;
        public Color InkSecondary;
        public Color InkMuted;
        public Color PrimaryBg;
        public Color PrimaryHover;
        public Color PrimaryPressed;
        public Color PrimaryInk;
        public Color SecondaryBg;
        public Color SecondaryHover;
        public Color SecondaryPressed;
        public Color SecondaryInk;
        public Color Accent;
        public Color AccentSoft;
        public Color AccentInk;
        public Color AccentBorder;
        public Color Danger;
        public Color DangerSoft;
        public Color DangerInk;
        public Color DangerBorder;
        public Color Warning;
        public Color WarningSoft;
        public Color WarningInk;
        public Color WarningBorder;
        public Color BadgeBg;
        public Color BadgeStroke;
        public Color LockedBannerBg;
        public Color LockedBannerSub;
        public Color Divider;

        public static readonly AgentThemePalette Light = CreateLight();
        public static readonly AgentThemePalette Dark = CreateDark();

        private static AgentThemePalette CreateLight()
        {
            AgentThemePalette p = new AgentThemePalette();
            p.IsDark = false;
            p.Canvas = Color.FromArgb(244, 242, 236);
            p.Rail = Color.FromArgb(236, 232, 223);
            p.Surface = Color.FromArgb(255, 255, 255);
            p.SurfaceHover = Color.FromArgb(250, 249, 245);
            p.InputBg = Color.FromArgb(255, 255, 255);
            p.ReadonlyBg = Color.FromArgb(236, 232, 223);
            p.BorderSubtle = Color.FromArgb(221, 215, 202);
            p.BorderStrong = Color.FromArgb(194, 187, 174);
            p.InkPrimary = Color.FromArgb(24, 26, 31);
            p.InkSecondary = Color.FromArgb(67, 72, 82);
            p.InkMuted = Color.FromArgb(82, 88, 100);
            p.PrimaryBg = Color.FromArgb(24, 26, 31);
            p.PrimaryHover = Color.FromArgb(44, 48, 58);
            p.PrimaryPressed = Color.FromArgb(17, 19, 22);
            p.PrimaryInk = Color.FromArgb(255, 255, 255);
            p.SecondaryBg = Color.FromArgb(255, 255, 255);
            p.SecondaryHover = Color.FromArgb(236, 232, 223);
            p.SecondaryPressed = Color.FromArgb(229, 224, 213);
            p.SecondaryInk = Color.FromArgb(24, 26, 31);
            p.Accent = Color.FromArgb(14, 98, 69);
            p.AccentSoft = Color.FromArgb(230, 243, 238);
            p.AccentInk = Color.FromArgb(10, 77, 54);
            p.AccentBorder = Color.FromArgb(179, 219, 199);
            p.Danger = Color.FromArgb(180, 35, 42);
            p.DangerSoft = Color.FromArgb(252, 236, 238);
            p.DangerInk = Color.FromArgb(142, 26, 32);
            p.DangerBorder = Color.FromArgb(240, 189, 192);
            p.Warning = Color.FromArgb(154, 91, 6);
            p.WarningSoft = Color.FromArgb(254, 245, 228);
            p.WarningInk = Color.FromArgb(122, 70, 3);
            p.WarningBorder = Color.FromArgb(234, 205, 151);
            p.BadgeBg = Color.FromArgb(24, 26, 31);
            p.BadgeStroke = Color.FromArgb(255, 255, 255);
            p.LockedBannerBg = Color.FromArgb(14, 98, 69);
            p.LockedBannerSub = Color.FromArgb(221, 244, 234);
            p.Divider = Color.FromArgb(230, 225, 215);
            return p;
        }

        private static AgentThemePalette CreateDark()
        {
            AgentThemePalette p = new AgentThemePalette();
            p.IsDark = true;
            p.Canvas = Color.FromArgb(17, 19, 22);
            p.Rail = Color.FromArgb(24, 27, 32);
            p.Surface = Color.FromArgb(24, 27, 32);
            p.SurfaceHover = Color.FromArgb(33, 37, 44);
            p.InputBg = Color.FromArgb(29, 33, 40);
            p.ReadonlyBg = Color.FromArgb(33, 37, 44);
            p.BorderSubtle = Color.FromArgb(44, 50, 59);
            p.BorderStrong = Color.FromArgb(64, 72, 86);
            p.InkPrimary = Color.FromArgb(242, 240, 234);
            p.InkSecondary = Color.FromArgb(197, 201, 210);
            p.InkMuted = Color.FromArgb(156, 163, 175);
            p.PrimaryBg = Color.FromArgb(242, 240, 234);
            p.PrimaryHover = Color.FromArgb(223, 220, 211);
            p.PrimaryPressed = Color.FromArgb(207, 202, 192);
            p.PrimaryInk = Color.FromArgb(17, 19, 22);
            p.SecondaryBg = Color.FromArgb(29, 33, 40);
            p.SecondaryHover = Color.FromArgb(38, 43, 52);
            p.SecondaryPressed = Color.FromArgb(45, 51, 62);
            p.SecondaryInk = Color.FromArgb(242, 240, 234);
            p.Accent = Color.FromArgb(22, 122, 86);
            p.AccentSoft = Color.FromArgb(21, 43, 36);
            p.AccentInk = Color.FromArgb(115, 227, 183);
            p.AccentBorder = Color.FromArgb(37, 91, 71);
            p.Danger = Color.FromArgb(224, 82, 89);
            p.DangerSoft = Color.FromArgb(46, 22, 24);
            p.DangerInk = Color.FromArgb(255, 156, 159);
            p.DangerBorder = Color.FromArgb(107, 39, 43);
            p.Warning = Color.FromArgb(217, 138, 26);
            p.WarningSoft = Color.FromArgb(46, 35, 18);
            p.WarningInk = Color.FromArgb(248, 198, 112);
            p.WarningBorder = Color.FromArgb(106, 75, 28);
            p.BadgeBg = Color.FromArgb(242, 240, 234);
            p.BadgeStroke = Color.FromArgb(17, 19, 22);
            p.LockedBannerBg = Color.FromArgb(17, 87, 62);
            p.LockedBannerSub = Color.FromArgb(213, 243, 230);
            p.Divider = Color.FromArgb(38, 43, 52);
            return p;
        }
    }

    internal static class AgentDrawing
    {
        public static FontFamily ResolveFontFamily(string[] preferred, FontFamily fallback)
        {
            FontFamily[] installed = FontFamily.Families;
            for (int i = 0; i < preferred.Length; i++)
            {
                string name = preferred[i];
                for (int j = 0; j < installed.Length; j++)
                {
                    if (string.Equals(installed[j].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return installed[j];
                    }
                }
            }
            return fallback;
        }

        public static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0f || rect.Width <= 0f || rect.Height <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }
            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void DrawShieldMark(Graphics g, Rectangle bounds, Color badgeBg, Color strokeColor, Color borderColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF plateRect = new RectangleF(bounds.X, bounds.Y, bounds.Width - 1f, bounds.Height - 1f);
            float r = Math.Max(4f, bounds.Width * 0.22f);
            using (GraphicsPath plate = CreateRoundedRectPath(plateRect, r))
            using (SolidBrush bgBrush = new SolidBrush(badgeBg))
            {
                g.FillPath(bgBrush, plate);
                if (borderColor != Color.Transparent)
                {
                    using (Pen bp = new Pen(borderColor, 1f))
                    {
                        g.DrawPath(bp, plate);
                    }
                }
            }

            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            float sx = bounds.Width * 0.25f;
            float topY = cy - bounds.Height * 0.27f;
            float midY = cy - bounds.Height * 0.16f;
            float lowY = cy + bounds.Height * 0.04f;
            float tipY = cy + bounds.Height * 0.29f;

            using (GraphicsPath shield = new GraphicsPath())
            {
                shield.StartFigure();
                shield.AddLine(cx, topY, cx + sx, midY);
                shield.AddLine(cx + sx, midY, cx + sx, lowY);
                shield.AddBezier(
                    cx + sx, lowY,
                    cx + sx, cy + bounds.Height * 0.19f,
                    cx + sx * 0.35f, cy + bounds.Height * 0.26f,
                    cx, tipY
                );
                shield.AddBezier(
                    cx, tipY,
                    cx - sx * 0.35f, cy + bounds.Height * 0.26f,
                    cx - sx, cy + bounds.Height * 0.19f,
                    cx - sx, lowY
                );
                shield.AddLine(cx - sx, lowY, cx - sx, midY);
                shield.CloseFigure();

                float strokeW = Math.Max(1.4f, bounds.Width * 0.052f);
                using (Pen pen = new Pen(strokeColor, strokeW))
                {
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, shield);
                }
            }
        }

        public static void DrawCheckMark(Graphics g, float x, float y, float size, Color color, float thickness)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, thickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                PointF p1 = new PointF(x + size * 0.22f, y + size * 0.53f);
                PointF p2 = new PointF(x + size * 0.43f, y + size * 0.74f);
                PointF p3 = new PointF(x + size * 0.79f, y + size * 0.29f);
                g.DrawLines(pen, new PointF[] { p1, p2, p3 });
            }
        }

        public static void DrawMoonOrSunGlyph(Graphics g, float cx, float cy, bool isDarkTheme, Color color, Color bgFill)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (!isDarkTheme)
            {
                using (SolidBrush fgBrush = new SolidBrush(color))
                using (SolidBrush cutBrush = new SolidBrush(bgFill))
                {
                    g.FillEllipse(fgBrush, cx - 5.2f, cy - 5.2f, 10.4f, 10.4f);
                    g.FillEllipse(cutBrush, cx - 1.5f, cy - 6.8f, 9.2f, 9.2f);
                }
            }
            else
            {
                using (Pen p = new Pen(color, 1.4f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    g.DrawEllipse(p, cx - 2.6f, cy - 2.6f, 5.2f, 5.2f);
                    for (int i = 0; i < 8; i++)
                    {
                        double a = (i * Math.PI) / 4.0;
                        float x1 = cx + (float)(Math.Cos(a) * 4.3);
                        float y1 = cy + (float)(Math.Sin(a) * 4.3);
                        float x2 = cx + (float)(Math.Cos(a) * 5.8);
                        float y2 = cy + (float)(Math.Sin(a) * 5.8);
                        g.DrawLine(p, x1, y1, x2, y2);
                    }
                }
            }
        }

        public static void DrawRefreshGlyph(Graphics g, float cx, float cy, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(color, 1.5f))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawArc(p, cx - 5.2f, cy - 5.2f, 10.4f, 10.4f, -35f, 295f);
                // Arrowhead at upper-right gap
                PointF a1 = new PointF(cx + 2.2f, cy - 5.2f);
                PointF tip = new PointF(cx + 5.2f, cy - 2.6f);
                PointF a2 = new PointF(cx + 5.6f, cy - 6.0f);
                g.DrawLine(p, a1, tip);
                g.DrawLine(p, a2, tip);
            }
        }
    }

    internal sealed class AgentButton : Button
    {
        public bool IsPrimary;
        public bool IsThemeToggle;
        public bool IsRefreshIcon;
        public bool IsBannerOverlay;
        public bool IsDangerSecondary;
        public AgentThemePalette Theme;
        private bool isHover;
        private bool isPressed;

        public AgentButton()
        {
            Theme = AgentThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled)
            {
                isHover = false;
                isPressed = false;
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled) { isHover = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (Enabled && mevent.Button == MouseButtons.Left) { isPressed = true; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            isPressed = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent != null ? Parent.BackColor : Theme.Canvas;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            Color fill;
            Color fg;
            Color border = Color.Transparent;

            if (IsBannerOverlay)
            {
                fg = Color.White;
                border = Color.FromArgb(148, 224, 192);
                fill = isPressed
                    ? Color.FromArgb(9, 52, 36)
                    : (isHover ? Color.FromArgb(11, 66, 46) : Color.FromArgb(18, 78, 56));
            }
            else if (IsPrimary)
            {
                if (!Enabled)
                {
                    fill = Theme.IsDark ? Color.FromArgb(52, 58, 69) : Color.FromArgb(196, 192, 184);
                    fg = Theme.IsDark ? Color.FromArgb(132, 139, 152) : Color.FromArgb(255, 255, 255);
                }
                else if (isPressed)
                {
                    fill = Theme.PrimaryPressed;
                    fg = Theme.PrimaryInk;
                }
                else if (isHover)
                {
                    fill = Theme.PrimaryHover;
                    fg = Theme.PrimaryInk;
                }
                else
                {
                    fill = Theme.PrimaryBg;
                    fg = Theme.PrimaryInk;
                }
            }
            else if (IsDangerSecondary && Enabled)
            {
                fill = isPressed
                    ? (Theme.IsDark ? Color.FromArgb(72, 30, 34) : Color.FromArgb(245, 214, 217))
                    : (isHover ? (Theme.IsDark ? Color.FromArgb(58, 26, 29) : Color.FromArgb(249, 226, 229)) : Theme.DangerSoft);
                fg = Theme.DangerInk;
                border = Theme.DangerBorder;
            }
            else
            {
                if (!Enabled)
                {
                    fill = Theme.SurfaceHover;
                    fg = Theme.InkMuted;
                    border = Theme.BorderSubtle;
                }
                else if (isPressed)
                {
                    fill = Theme.SecondaryPressed;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
                else if (isHover)
                {
                    fill = Theme.SecondaryHover;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
                else
                {
                    fill = Theme.SecondaryBg;
                    fg = Theme.SecondaryInk;
                    border = Theme.BorderStrong;
                }
            }

            RectangleF rect = new RectangleF(1f, 1f, Width - 3f, Height - 3f);
            using (GraphicsPath path = AgentDrawing.CreateRoundedRectPath(rect, 7f))
            using (SolidBrush bgBrush = new SolidBrush(fill))
            {
                g.FillPath(bgBrush, path);
                if (border != Color.Transparent)
                {
                    using (Pen pen = new Pen(border, 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            if (Focused && ShowFocusCues)
            {
                RectangleF focusRect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
                Color focusColor = IsBannerOverlay
                    ? Color.White
                    : (Theme.IsDark ? Theme.AccentInk : Theme.Accent);
                using (GraphicsPath focusPath = AgentDrawing.CreateRoundedRectPath(focusRect, 8f))
                using (Pen focusPen = new Pen(focusColor, 1.8f))
                {
                    g.DrawPath(focusPen, focusPath);
                }
            }

            if (IsRefreshIcon)
            {
                AgentDrawing.DrawRefreshGlyph(g, Width / 2f, Height / 2f, fg);
            }
            else if (IsThemeToggle)
            {
                bool showDarkGlyph = IsBannerOverlay ? true : Theme.IsDark;
                AgentDrawing.DrawMoonOrSunGlyph(g, 18f, Height / 2f, showDarkGlyph && Theme.IsDark, fg, fill);
                Rectangle textRect = new Rectangle(28, 0, Width - 34, Height);
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
                );
            }
            else
            {
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    new Rectangle(6, 0, Width - 12, Height),
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
        }
    }

    internal sealed class AgentCheckBox : CheckBox
    {
        public AgentThemePalette Theme;
        private bool isHover;

        public AgentCheckBox()
        {
            Theme = AgentThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            AutoSize = false;
            Height = 26;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled) isHover = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled) { isHover = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color bg = (isHover && Enabled) ? Theme.SurfaceHover : (Parent != null ? Parent.BackColor : Theme.Surface);
            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            float boxY = (Height - 17f) / 2f;
            RectangleF boxRect = new RectangleF(2f, boxY, 17f, 17f);
            using (GraphicsPath boxPath = AgentDrawing.CreateRoundedRectPath(boxRect, 4f))
            {
                if (Checked)
                {
                    Color fill = Enabled
                        ? Theme.Accent
                        : (Theme.IsDark ? Color.FromArgb(42, 82, 66) : Color.FromArgb(148, 184, 170));
                    using (SolidBrush boxBrush = new SolidBrush(fill))
                    {
                        g.FillPath(boxBrush, boxPath);
                    }
                    AgentDrawing.DrawCheckMark(g, 2f, boxY, 17f, Color.White, 1.9f);
                }
                else
                {
                    using (SolidBrush boxBrush = new SolidBrush(Theme.InputBg))
                    {
                        g.FillPath(boxBrush, boxPath);
                    }
                    Color boxBorder = Enabled ? Theme.BorderStrong : Theme.BorderSubtle;
                    using (Pen boxPen = new Pen(boxBorder, 1.3f))
                    {
                        g.DrawPath(boxPen, boxPath);
                    }
                }
            }

            Color ink = Enabled ? Theme.InkPrimary : Theme.InkMuted;
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(26, 0, Width - 30, Height),
                ink,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            if (Focused && ShowFocusCues)
            {
                Color focusColor = Theme.IsDark ? Theme.AccentInk : Theme.Accent;
                using (Pen fp = new Pen(focusColor, 1.2f))
                {
                    g.DrawRectangle(fp, 0, 1, Width - 2, Height - 3);
                }
            }
        }
    }

    internal sealed class AgentRadioButton : RadioButton
    {
        public AgentThemePalette Theme;
        private bool isHover;

        public AgentRadioButton()
        {
            Theme = AgentThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            AutoSize = false;
            Height = 24;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled) isHover = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled) { isHover = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color bg = (isHover && Enabled) ? Theme.SurfaceHover : (Parent != null ? Parent.BackColor : Theme.Surface);
            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            float cy = (Height - 16f) / 2f;
            RectangleF outer = new RectangleF(2f, cy, 16f, 16f);
            using (SolidBrush innerBg = new SolidBrush(Theme.InputBg))
            {
                g.FillEllipse(innerBg, outer);
            }

            Color ringColor = Checked
                ? (Enabled ? Theme.Accent : Theme.BorderStrong)
                : (Enabled ? Theme.BorderStrong : Theme.BorderSubtle);
            using (Pen ringPen = new Pen(ringColor, Checked ? 1.8f : 1.3f))
            {
                g.DrawEllipse(ringPen, outer);
            }

            if (Checked)
            {
                Color dotColor = Enabled ? Theme.Accent : Theme.InkMuted;
                using (SolidBrush dotBrush = new SolidBrush(dotColor))
                {
                    g.FillEllipse(dotBrush, 6f, cy + 4f, 8f, 8f);
                }
            }

            Color ink = Enabled ? Theme.InkPrimary : Theme.InkMuted;
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(24, 0, Width - 28, Height),
                ink,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            if (Focused && ShowFocusCues)
            {
                Color focusColor = Theme.IsDark ? Theme.AccentInk : Theme.Accent;
                using (Pen fp = new Pen(focusColor, 1.2f))
                {
                    g.DrawRectangle(fp, 0, 1, Width - 2, Height - 3);
                }
            }
        }
    }

    internal sealed class AgentCardPanel : Panel
    {
        public float CornerRadius;
        public Color FillColor;
        public Color BorderColor;
        public Color FocusedBorderColor;
        public Color DividerColor;
        public bool IsFocusedBorder;
        public int DividerY = -1;
        public int DividerPadX = 18;

        public AgentCardPanel()
        {
            CornerRadius = 10f;
            FillColor = Color.White;
            BorderColor = Color.LightGray;
            FocusedBorderColor = Color.SeaGreen;
            DividerColor = Color.LightGray;
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBg = Parent != null ? Parent.BackColor : FillColor;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath path = AgentDrawing.CreateRoundedRectPath(rect, CornerRadius))
            using (SolidBrush bgBrush = new SolidBrush(FillColor))
            {
                g.FillPath(bgBrush, path);
                Color bCol = IsFocusedBorder ? FocusedBorderColor : BorderColor;
                float bW = IsFocusedBorder ? 1.5f : 1f;
                using (Pen pen = new Pen(bCol, bW))
                {
                    g.DrawPath(pen, path);
                }
            }

            if (DividerY > 0 && DividerY < Height - 4)
            {
                using (Pen dp = new Pen(DividerColor, 1f))
                {
                    g.DrawLine(dp, DividerPadX, DividerY, Width - DividerPadX, DividerY);
                }
            }
        }
    }

    internal sealed class AgentComboBox : ComboBox
    {
        public AgentThemePalette Theme;
        private bool isHover;

        public AgentComboBox()
        {
            Theme = AgentThemePalette.Light;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 28;
            Cursor = Cursors.Hand;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            if (!Enabled) isHover = false;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled) { isHover = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHover = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = selected ? Theme.AccentSoft : Theme.Surface;
            Color fg = selected ? Theme.AccentInk : Theme.InkPrimary;

            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, e.Bounds);
            }

            string text = Items[e.Index] != null ? Items[e.Index].ToString() : string.Empty;
            Rectangle textRect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, Math.Max(10, e.Bounds.Width - 16), e.Bounds.Height);
            TextRenderer.DrawText(
                g,
                text,
                Font,
                textRect,
                fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );
        }

        private void PaintComboFrame(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent != null ? Parent.BackColor : Theme.Surface;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            Color fill = !Enabled ? Theme.ReadonlyBg : (isHover ? Theme.SurfaceHover : Theme.InputBg);
            Color border = Focused && Enabled ? Theme.Accent : Theme.BorderStrong;
            float borderW = Focused && Enabled ? 1.5f : 1f;

            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath path = AgentDrawing.CreateRoundedRectPath(rect, 7f))
            using (SolidBrush bgBrush = new SolidBrush(fill))
            using (Pen borderPen = new Pen(border, borderW))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
            }

            string displayText = SelectedItem != null ? SelectedItem.ToString() : Text;
            Color textColor = !Enabled ? Theme.InkMuted : Theme.InkPrimary;
            Rectangle textBounds = new Rectangle(10, 0, Math.Max(12, Width - 34), Height);
            TextRenderer.DrawText(
                g,
                displayText,
                Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
            );

            Color arrowColor = !Enabled ? Theme.InkMuted : Theme.InkSecondary;
            float cx = Width - 15f;
            float cy = Height / 2f + 0.5f;
            using (Pen ap = new Pen(arrowColor, 1.6f))
            {
                ap.StartCap = LineCap.Round;
                ap.EndCap = LineCap.Round;
                ap.LineJoin = LineJoin.Round;
                g.DrawLine(ap, cx - 4f, cy - 2f, cx, cy + 2.2f);
                g.DrawLine(ap, cx, cy + 2.2f, cx + 4f, cy - 2f);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            const int WM_PAINT = 0x000F;
            const int WM_PRINT = 0x0317;
            const int WM_PRINTCLIENT = 0x0318;
            if (m.Msg == WM_PAINT)
            {
                using (Graphics g = CreateGraphics())
                {
                    PaintComboFrame(g);
                }
            }
            else if ((m.Msg == WM_PRINT || m.Msg == WM_PRINTCLIENT) && m.WParam != IntPtr.Zero)
            {
                using (Graphics g = Graphics.FromHdc(m.WParam))
                {
                    PaintComboFrame(g);
                }
            }
        }
    }

    internal sealed class AgentTelemetryLedger : Control
    {
        public AgentThemePalette Theme;
        public Font LabelFont;
        public Font ValueFont;

        private readonly string[] titles = new string[]
        {
            "CAKUPAN DOMAIN & ROBLOX",
            "KARANTINA EKSTENSI IDE",
            "INTERSEPSI PROSES",
            "KENDALI SERVER ADMIN"
        };

        private readonly string[] values = new string[]
        {
            "0 Domain",
            "0 Modul",
            "0 Insiden",
            "Pilih Ruangan"
        };

        private readonly TelemetryTone[] tones = new TelemetryTone[]
        {
            TelemetryTone.Neutral,
            TelemetryTone.Neutral,
            TelemetryTone.Neutral,
            TelemetryTone.Warning
        };

        public AgentTelemetryLedger()
        {
            Theme = AgentThemePalette.Light;
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
            TabStop = false;
        }

        public void SetCell(int index, string valueText, TelemetryTone tone)
        {
            if (index < 0 || index >= 4) return;
            values[index] = valueText ?? string.Empty;
            tones[index] = tone;
            Invalidate();
        }

        public string GetCellValue(int index)
        {
            if (index < 0 || index >= 4) return string.Empty;
            return values[index];
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Color parentBg = Parent != null ? Parent.BackColor : Theme.Canvas;
            using (SolidBrush clearBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            RectangleF outerRect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using (GraphicsPath outerPath = AgentDrawing.CreateRoundedRectPath(outerRect, 10f))
            {
                using (SolidBrush bgBrush = new SolidBrush(Theme.Surface))
                {
                    g.FillPath(bgBrush, outerPath);
                }

                // Semantic tint on Cell 3 (KENDALI SERVER ADMIN) clipped cleanly inside rounded frame
                float cellW = (Width - 2f) / 4f;
                float cell3X = 0.5f + cellW * 3f;
                Color tintFill = Color.Transparent;
                if (tones[3] == TelemetryTone.Accent) tintFill = Theme.AccentSoft;
                else if (tones[3] == TelemetryTone.Warning) tintFill = Theme.WarningSoft;
                else if (tones[3] == TelemetryTone.Danger) tintFill = Theme.DangerSoft;

                if (tintFill != Color.Transparent)
                {
                    Region prevClip = g.Clip;
                    g.SetClip(outerPath);
                    using (SolidBrush tintBrush = new SolidBrush(tintFill))
                    {
                        g.FillRectangle(tintBrush, cell3X, 0f, Width - cell3X, Height);
                    }
                    g.Clip = prevClip;
                }

                // Vertical 1px hairline cell dividers
                using (Pen divPen = new Pen(Theme.BorderSubtle, 1f))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        float x = (float)Math.Round(0.5f + cellW * i);
                        g.DrawLine(divPen, x, 1f, x, Height - 2f);
                    }
                }

                // Outer frame border drawn last for crispness
                using (Pen borderPen = new Pen(Theme.BorderSubtle, 1f))
                {
                    g.DrawPath(borderPen, outerPath);
                }
            }

            Font lf = LabelFont != null ? LabelFont : Font;
            Font vf = ValueFont != null ? ValueFont : Font;
            float w = (Width - 2f) / 4f;

            for (int i = 0; i < 4; i++)
            {
                int x = (int)Math.Round(0.5f + w * i);
                int cw = (int)Math.Round(w);

                Color valInk = Theme.InkPrimary;
                Color dotColor = Color.Transparent;
                if (tones[i] == TelemetryTone.Accent)
                {
                    valInk = Theme.AccentInk;
                    dotColor = Theme.Accent;
                }
                else if (tones[i] == TelemetryTone.Warning)
                {
                    valInk = Theme.WarningInk;
                    dotColor = Theme.Warning;
                }
                else if (tones[i] == TelemetryTone.Danger)
                {
                    valInk = Theme.DangerInk;
                    dotColor = Theme.Danger;
                }

                TextRenderer.DrawText(
                    g,
                    titles[i],
                    lf,
                    new Rectangle(x + 14, 13, cw - 24, 16),
                    Theme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );

                if (i == 3 && dotColor != Color.Transparent)
                {
                    using (SolidBrush dotBrush = new SolidBrush(dotColor))
                    {
                        g.FillEllipse(dotBrush, x + 15f, 41f, 8f, 8f);
                    }
                    TextRenderer.DrawText(
                        g,
                        values[i],
                        vf,
                        new Rectangle(x + 29, 32, cw - 39, 28),
                        valInk,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                    );
                }
                else
                {
                    TextRenderer.DrawText(
                        g,
                        values[i],
                        vf,
                        new Rectangle(x + 14, 32, cw - 24, 28),
                        valInk,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                    );
                }
            }
        }
    }

    internal sealed class AgentDoubleBufferedPanel : Panel
    {
        public AgentDoubleBufferedPanel()
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );
        }
    }

    // ============================================================================
    // MAIN DESKTOP FORM UI (PC MAHASISWA - ALWAYS-ON SYSTEM AGENT)
    // ============================================================================
    public class MainForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private const string RoomPlaceholder = "-- Pilih Ruangan --";

        // State
        private bool isLocked = false;
        private bool isServerMode = true;
        private bool isRoomConfirmed = false;
        private bool startMinimizedInTray = false;
        private bool isPreviewMode = false;
        private bool allowProcessExit = false;
        private string offlinePin = "202699";
        private string offlinePinHash = "";
        private string activeRoomCode = "";
        private string lastPinVerifyError = "";
        private int killedCounter = 0;
        private List<string> quarantinedExtensions = new List<string>();
        private System.Windows.Forms.Timer guardTimer;
        private System.Windows.Forms.Timer heartbeatTimer;
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

        // System Tray
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        // Theme & Typography
        private AgentThemePalette currentTheme = AgentThemePalette.Light;
        private BannerState currentBannerState = BannerState.Setup;
        private string customBannerSub = null;
        private bool isAdminPrivilegeNow = false;
        private bool hasServicePrivilegeNow = false;

        private FontFamily uiFamily;
        private FontFamily semiboldFamily;
        private FontFamily monoFamily;

        private Font fBannerTitle;
        private Font fBannerSub;
        private Font fSectionLabel;
        private Font fLabel;
        private Font fSubtle;
        private Font fButton;
        private Font fMono;
        private Font fMonoBold;
        private Font fMonoSmall;
        private Font fStatValue;
        private Icon appIcon;

        // Header Banner
        private AgentDoubleBufferedPanel headerBanner;
        private AgentButton btnThemeToggle;

        // Config Card
        private AgentCardPanel configPanel;
        private Label lblSection1;
        private Label lblVersionTag;
        private Label lblNim;
        private Label lblName;
        private Label lblRoom;
        private Label lblHost;
        private Label lblServerUrl;

        private AgentCardPanel pnlNim;
        private AgentCardPanel pnlName;
        private AgentCardPanel pnlHost;
        private AgentCardPanel pnlServerUrl;

        private TextBox txtNim;
        private TextBox txtName;
        private TextBox txtHost;
        private TextBox txtServerUrl;
        private AgentComboBox cmbRoomCode;
        private AgentButton btnRefreshRooms;
        private AgentRadioButton rbServerMode;
        private AgentRadioButton rbOfflineMode;
        private AgentCheckBox chkAutoStart;

        // Unified 4-Cell Telemetry Ledger
        private AgentTelemetryLedger telemetryLedger;

        // Action Buttons
        private AgentButton btnActivateLock;
        private AgentButton btnUnlockWithPin;
        private AgentButton btnHideToTray;

        // Live Endpoint Activity Journal
        private Label lblLogHeader;
        private Label lblLogCount;
        private AgentButton btnUpdateApp;
        private AgentCardPanel pnlLogCard;
        private Label lblLogEmpty;
        private ListBox lstLogs;
        private Label lblCredit;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int RegisterWindowMessage(string lpString);

        private int msgTaskbarCreated = 0;
        private bool allowShowForm = true;

        protected override void SetVisibleCore(bool value)
        {
            if (!allowShowForm)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void WndProc(ref Message m)
        {
            if (msgTaskbarCreated != 0 && m.Msg == msgTaskbarCreated)
            {
                if (trayIcon != null && !isPreviewMode)
                {
                    trayIcon.Visible = false;
                    trayIcon.Visible = true;
                }
            }
            base.WndProc(ref m);
        }

        private bool lifecycleStarted = false;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!lifecycleStarted && !isPreviewMode)
            {
                this.BeginInvoke(new Action(StartAgentLifecycle));
            }
        }

        public MainForm(bool startInTray = false) : this(startInTray, false)
        {
        }

        public MainForm(bool startInTray, bool previewMode)
        {
            this.startMinimizedInTray = startInTray;
            this.isPreviewMode = previewMode;
            if (startInTray && !previewMode)
            {
                this.allowShowForm = false;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            InitializeFontsAndIcon();
            InitializeUi();
            InitializeTrayIcon();
            ApplyTheme(AgentThemePalette.Light);
            if (!previewMode)
            {
                CheckInitialEnvironment();
            }
            else
            {
                UpdateLogEmptyAndCount();
            }
        }

        private void InitializeFontsAndIcon()
        {
            uiFamily = AgentDrawing.ResolveFontFamily(new string[] { "Segoe UI", "Microsoft Sans Serif" }, FontFamily.GenericSansSerif);
            semiboldFamily = AgentDrawing.ResolveFontFamily(new string[] { "Segoe UI Semibold", "Segoe UI" }, uiFamily);
            monoFamily = AgentDrawing.ResolveFontFamily(new string[] { "Consolas", "Cascadia Mono", "Courier New" }, FontFamily.GenericMonospace);

            fBannerTitle = new Font(uiFamily, 12.5f, FontStyle.Bold);
            fBannerSub = new Font(uiFamily, 8.6f, FontStyle.Regular);
            fSectionLabel = new Font(semiboldFamily, 7.6f, FontStyle.Bold);
            fLabel = new Font(semiboldFamily, 8.4f, FontStyle.Bold);
            fSubtle = new Font(uiFamily, 8.4f, FontStyle.Regular);
            fButton = new Font(semiboldFamily, 9.0f, FontStyle.Bold);
            fMono = new Font(monoFamily, 9.5f, FontStyle.Regular);
            fMonoBold = new Font(monoFamily, 9.5f, FontStyle.Bold);
            fMonoSmall = new Font(monoFamily, 8.0f, FontStyle.Regular);
            fStatValue = new Font(uiFamily, 12.5f, FontStyle.Bold);

            appIcon = CreateGuardShieldIcon();
            if (appIcon != null)
            {
                this.Icon = appIcon;
            }
        }

        private static Icon CreateGuardShieldIcon()
        {
            Bitmap bmp = null;
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                bmp = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    AgentDrawing.DrawShieldMark(g, new Rectangle(1, 1, 30, 30), Color.FromArgb(24, 26, 31), Color.White, Color.Transparent);
                }
                hIcon = bmp.GetHicon();
                using (Icon temp = Icon.FromHandle(hIcon))
                {
                    return (Icon)temp.Clone();
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (hIcon != IntPtr.Zero) DestroyIcon(hIcon);
                if (bmp != null) bmp.Dispose();
            }
        }

        private void InitializeTrayIcon()
        {
            msgTaskbarCreated = RegisterWindowMessage("TaskbarCreated");

            trayMenu = new ContextMenuStrip();
            ToolStripMenuItem itemOpen = new ToolStripMenuItem("Buka Jendela Status GuardAgent");
            itemOpen.Font = fLabel;
            itemOpen.Click += (s, e) => ShowFromTray();

            ToolStripMenuItem itemSync = new ToolStripMenuItem("Sinkronkan dengan Server Admin Sekarang");
            itemSync.Click += async (s, e) => await ConnectAndSyncServerAsync(false);

            ToolStripMenuItem itemUpdate = new ToolStripMenuItem("Perbarui Aplikasi GuardAgent (Cek Update)...");
            itemUpdate.Click += async (s, e) => await CheckAndPerformUpdateAsync(true);

            ToolStripMenuItem itemExit = new ToolStripMenuItem("Matikan / Keluar Agen (Wajib PIN Ruangan / Pengawas)");
            itemExit.Click += async (s, e) => await PromptPinAndExitApplicationAsync();

            trayMenu.Items.Add(itemOpen);
            trayMenu.Items.Add(itemSync);
            trayMenu.Items.Add(itemUpdate);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(itemExit);

            trayIcon = new NotifyIcon
            {
                Icon = appIcon != null ? appIcon : SystemIcons.Shield,
                Text = "GuardAgent - Aktif & Dikendalikan Admin",
                ContextMenuStrip = trayMenu,
                Visible = !isPreviewMode
            };
            trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void ShowFromTray()
        {
            this.allowShowForm = true;
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void HideToTray(bool showBalloon = true)
        {
            this.allowShowForm = false;
            this.Hide();
            this.ShowInTaskbar = false;
            if (showBalloon && trayIcon != null && trayIcon.Visible)
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

            isAdminPrivilegeNow = GuardEngine.IsAdministrator();
            hasServicePrivilegeNow = !isAdminPrivilegeNow && ServicePipeClient.IsServiceAvailable();

            this.Name = "MainForm";
            this.Text = "GuardAgent v" + GuardEngine.CurrentVersion + " - Panel Pengawasan Perangkat";
            this.ClientSize = new Size(900, 660);
            this.MinimumSize = new Size(880, 650);
            this.AutoScaleMode = AutoScaleMode.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Font = new Font(uiFamily, 9.0f, FontStyle.Regular);
            this.FormClosing += MainForm_FormClosing;
            this.Resize += (s, e) => LayoutWorkspace();

            // 1. TOP STATUS COMMAND BANNER
            headerBanner = new AgentDoubleBufferedPanel
            {
                Name = "headerBanner",
                Dock = DockStyle.Top,
                Height = 86
            };
            headerBanner.Paint += HeaderBanner_Paint;

            btnThemeToggle = new AgentButton
            {
                Name = "btnThemeToggle",
                IsThemeToggle = true,
                Text = "Mode Gelap",
                Font = new Font(semiboldFamily, 8.2f, FontStyle.Bold),
                Size = new Size(112, 32),
                TabIndex = 12,
                AccessibleName = "Ganti tema terang atau gelap"
            };
            btnThemeToggle.Click += (s, e) => ApplyTheme(currentTheme.IsDark ? AgentThemePalette.Light : AgentThemePalette.Dark);
            headerBanner.Controls.Add(btnThemeToggle);
            this.Controls.Add(headerBanner);

            // 2. CONFIGURATION CARD
            configPanel = new AgentCardPanel
            {
                Name = "configPanel",
                CornerRadius = 10f,
                DividerY = 176,
                DividerPadX = 18
            };

            lblSection1 = new Label
            {
                Name = "lblSection1",
                Text = "IDENTITAS PERANGKAT & KONFIGURASI RUANGAN",
                UseMnemonic = false,
                Font = fSectionLabel,
                AutoSize = true
            };
            configPanel.Controls.Add(lblSection1);

            lblVersionTag = new Label
            {
                Name = "lblVersionTag",
                Text = "v" + GuardEngine.CurrentVersion,
                UseMnemonic = false,
                Font = fMonoSmall,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(56, 18)
            };
            configPanel.Controls.Add(lblVersionTag);

            lblNim = MakeFieldLabel("NIM / ID Perangkat");
            configPanel.Controls.Add(lblNim);
            txtNim = CreateInnerTextBox("txtNim", savedCfg.Nim, fMono, 0, "NIM atau ID Perangkat");
            pnlNim = WrapTextBoxInCard("pnlNim", txtNim, 34);
            configPanel.Controls.Add(pnlNim);

            lblName = MakeFieldLabel("Nama Peserta / Nama PC Lab");
            configPanel.Controls.Add(lblName);
            txtName = CreateInnerTextBox("txtName", savedCfg.Name, new Font(uiFamily, 9.5f, FontStyle.Regular), 1, "Nama Peserta atau Nama PC Lab");
            pnlName = WrapTextBoxInCard("pnlName", txtName, 34);
            configPanel.Controls.Add(pnlName);

            lblRoom = MakeFieldLabel("Ruangan Sesi");
            configPanel.Controls.Add(lblRoom);
            cmbRoomCode = new AgentComboBox
            {
                Name = "cmbRoomCode",
                Font = fMonoBold,
                TabIndex = 2,
                AccessibleName = "Pilih Ruangan Sesi"
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

            btnRefreshRooms = new AgentButton
            {
                Name = "btnRefreshRooms",
                IsRefreshIcon = true,
                Text = "",
                Size = new Size(34, 34),
                TabIndex = 3,
                AccessibleName = "Muat ulang daftar ruangan dari server"
            };
            btnRefreshRooms.Click += async (s, e) => await FetchAvailableRoomsAsync(true);
            configPanel.Controls.Add(btnRefreshRooms);

            lblHost = MakeFieldLabel("Hostname Windows");
            configPanel.Controls.Add(lblHost);
            txtHost = CreateInnerTextBox("txtHost", Environment.MachineName, fMono, 20, "Hostname Windows");
            txtHost.ReadOnly = true;
            txtHost.TabStop = false;
            pnlHost = WrapTextBoxInCard("pnlHost", txtHost, 34);
            configPanel.Controls.Add(pnlHost);

            rbServerMode = new AgentRadioButton
            {
                Name = "rbServerMode",
                Text = "Sinkronisasi Otomatis Admin (24/7)",
                Checked = true,
                Font = fSubtle,
                Size = new Size(275, 24),
                TabIndex = 4,
                AccessibleName = "Mode Sinkronisasi Otomatis Admin"
            };
            rbServerMode.CheckedChanged += (s, e) =>
            {
                isServerMode = rbServerMode.Checked;
                bool urlEditable = isServerMode && !isLocked;
                txtServerUrl.ReadOnly = !urlEditable;
                txtServerUrl.TabStop = urlEditable;
                if (pnlServerUrl != null) ApplyInputCardTheme(pnlServerUrl, txtServerUrl, !urlEditable);
                UpdatePrimaryButtonCopy();
            };
            configPanel.Controls.Add(rbServerMode);

            rbOfflineMode = new AgentRadioButton
            {
                Name = "rbOfflineMode",
                Text = "Mode Mandiri / Offline (PIN Lokal)",
                Font = fSubtle,
                Size = new Size(255, 24),
                TabIndex = 5,
                AccessibleName = "Mode Mandiri Offline"
            };
            configPanel.Controls.Add(rbOfflineMode);

            lblServerUrl = MakeFieldLabel("Endpoint Server");
            configPanel.Controls.Add(lblServerUrl);
            txtServerUrl = CreateInnerTextBox("txtServerUrl", savedCfg.ServerUrl, fMono, 6, "Endpoint Server Admin");
            pnlServerUrl = WrapTextBoxInCard("pnlServerUrl", txtServerUrl, 34);
            configPanel.Controls.Add(pnlServerUrl);

            chkAutoStart = new AgentCheckBox
            {
                Name = "chkAutoStart",
                Text = "Aktif terus di sistem (jalankan otomatis saat Windows menyala & berjalan di System Tray)",
                Checked = savedCfg.AutoStart,
                Font = fLabel,
                TabIndex = 7,
                AccessibleName = "Aktif terus di sistem"
            };
            chkAutoStart.CheckedChanged += (s, e) =>
            {
                if (isPreviewMode) return;
                GuardEngine.EnsureAutoStartRegistered(chkAutoStart.Checked);
                SaveCurrentAgentConfig(isRoomConfirmed);
            };
            configPanel.Controls.Add(chkAutoStart);

            this.Controls.Add(configPanel);

            // 3. UNIFIED 4-CELL TELEMETRY LEDGER
            telemetryLedger = new AgentTelemetryLedger
            {
                Name = "telemetryLedger",
                LabelFont = new Font(semiboldFamily, 7.4f, FontStyle.Bold),
                ValueFont = fStatValue
            };
            telemetryLedger.SetCell(0, GuardEngine.BlockedDomains.Count + " Domain", TelemetryTone.Neutral);
            telemetryLedger.SetCell(1, "0 Modul", TelemetryTone.Neutral);
            telemetryLedger.SetCell(2, "0 Insiden", TelemetryTone.Neutral);
            telemetryLedger.SetCell(3, isRoomConfirmed ? "Menyambung..." : "Pilih Ruangan", isRoomConfirmed ? TelemetryTone.Accent : TelemetryTone.Warning);
            this.Controls.Add(telemetryLedger);

            // 4. ACTION COMMAND BAR
            btnActivateLock = new AgentButton
            {
                Name = "btnActivateLock",
                IsPrimary = true,
                Text = "Hubungkan ke Ruangan & Sinkronkan Server",
                Font = fButton,
                TabIndex = 8,
                AccessibleName = "Hubungkan ke Ruangan dan Sinkronkan Server"
            };
            btnActivateLock.Click += BtnActivateLock_Click;
            this.Controls.Add(btnActivateLock);

            btnUnlockWithPin = new AgentButton
            {
                Name = "btnUnlockWithPin",
                IsDangerSecondary = true,
                Text = "Buka Kunci Darurat (PIN Ruangan)",
                Font = fButton,
                Enabled = false,
                TabIndex = 9,
                AccessibleName = "Buka Kunci Darurat dengan PIN Ruangan"
            };
            btnUnlockWithPin.Click += BtnUnlockWithPin_Click;
            this.Controls.Add(btnUnlockWithPin);

            btnHideToTray = new AgentButton
            {
                Name = "btnHideToTray",
                Text = "Sembunyikan ke Tray",
                Font = fSubtle,
                TabIndex = 10,
                AccessibleName = "Sembunyikan jendela ke System Tray"
            };
            btnHideToTray.Click += (s, e) => HideToTray(true);
            this.Controls.Add(btnHideToTray);

            // 5. LIVE ENDPOINT ACTIVITY JOURNAL
            lblLogHeader = new Label
            {
                Name = "lblLogHeader",
                Text = "JURNAL SINKRONISASI & PENGAWASAN ENDPOINT",
                UseMnemonic = false,
                Font = fSectionLabel,
                AutoSize = true
            };
            this.Controls.Add(lblLogHeader);

            lblLogCount = new Label
            {
                Name = "lblLogCount",
                Text = "0 entri",
                UseMnemonic = false,
                Font = fMonoSmall,
                AutoSize = true
            };
            this.Controls.Add(lblLogCount);

            btnUpdateApp = new AgentButton
            {
                Name = "btnUpdateApp",
                Text = "Cek Pembaruan",
                Font = new Font(semiboldFamily, 8.2f, FontStyle.Bold),
                Size = new Size(138, 28),
                TabIndex = 11,
                AccessibleName = "Cek pembaruan aplikasi GuardAgent"
            };
            btnUpdateApp.Click += async (s, e) => await CheckAndPerformUpdateAsync(true);
            this.Controls.Add(btnUpdateApp);

            pnlLogCard = new AgentCardPanel
            {
                Name = "pnlLogCard",
                CornerRadius = 10f
            };

            lstLogs = new ListBox
            {
                Name = "lstLogs",
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                IntegralHeight = false,
                Font = new Font(monoFamily, 8.6f, FontStyle.Regular),
                TabStop = false
            };
            lstLogs.DrawItem += LstLogs_DrawItem;
            pnlLogCard.Controls.Add(lstLogs);

            lblLogEmpty = new Label
            {
                Name = "lblLogEmpty",
                Text = "Belum ada catatan aktivitas pada sesi ini.",
                UseMnemonic = false,
                Font = fSubtle,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlLogCard.Controls.Add(lblLogEmpty);
            lblLogEmpty.BringToFront();

            this.Controls.Add(pnlLogCard);

            lblCredit = new Label
            {
                Name = "lblCredit",
                Text = "Created by Nizan",
                UseMnemonic = false,
                Font = fSubtle,
                AutoSize = true
            };
            this.Controls.Add(lblCredit);

            // Timers: heartbeatTimer berjalan TERUS-MENERUS (24/7) setelah ruangan dipilih
            guardTimer = new System.Windows.Forms.Timer { Interval = 2500 };
            guardTimer.Tick += GuardTimer_Tick;

            heartbeatTimer = new System.Windows.Forms.Timer { Interval = 3500 };
            heartbeatTimer.Tick += HeartbeatTimer_Tick;

            LayoutWorkspace();
        }

        private Label MakeFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                UseMnemonic = false,
                Font = fLabel,
                AutoSize = true
            };
        }

        private static TextBox CreateInnerTextBox(string name, string text, Font font, int tabIndex, string accName)
        {
            return new TextBox
            {
                Name = name,
                Text = text ?? string.Empty,
                BorderStyle = BorderStyle.None,
                Font = font,
                TabIndex = tabIndex,
                AccessibleName = accName
            };
        }

        private AgentCardPanel WrapTextBoxInCard(string cardName, TextBox innerTb, int cardHeight)
        {
            AgentCardPanel box = new AgentCardPanel
            {
                Name = cardName,
                CornerRadius = 7f,
                Height = cardHeight
            };
            box.Controls.Add(innerTb);
            box.Click += (s, e) => { if (innerTb.Enabled && !innerTb.ReadOnly) innerTb.Focus(); };
            innerTb.Enter += (s, e) => { if (!innerTb.ReadOnly) { box.IsFocusedBorder = true; box.Invalidate(); } };
            innerTb.Leave += (s, e) => { box.IsFocusedBorder = false; box.Invalidate(); };
            return box;
        }

        private static void CenterInnerTextBox(AgentCardPanel card, TextBox tb)
        {
            int th = Math.Max(20, tb.PreferredHeight);
            tb.Location = new Point(10, Math.Max(4, (card.Height - th) / 2));
            tb.Size = new Size(Math.Max(40, card.Width - 20), th);
        }

        private void LayoutWorkspace()
        {
            if (configPanel == null || ClientSize.Width < 400 || ClientSize.Height < 400) return;

            int padX = 24;
            int contentW = ClientSize.Width - (padX * 2);

            if (btnThemeToggle != null && headerBanner != null)
            {
                btnThemeToggle.Location = new Point(headerBanner.Width - padX - btnThemeToggle.Width, 27);
            }

            // Config Card
            configPanel.Location = new Point(padX, 102);
            configPanel.Size = new Size(contentW, 214);
            int innerW = contentW - 36;

            lblSection1.Location = new Point(18, 13);
            lblVersionTag.Location = new Point(contentW - 18 - lblVersionTag.Width, 12);

            // Row 1: 4 columns
            int gap = 12;
            int avail4 = innerW - (gap * 3);
            int wNim = Math.Max(140, (int)(avail4 * 0.21));
            int wName = Math.Max(180, (int)(avail4 * 0.28));
            int wRoom = Math.Max(175, (int)(avail4 * 0.27));
            int wHost = Math.Max(140, avail4 - wNim - wName - wRoom);

            int x1 = 18;
            int x2 = x1 + wNim + gap;
            int x3 = x2 + wName + gap;
            int x4 = x3 + wRoom + gap;

            lblNim.Location = new Point(x1, 38);
            pnlNim.Location = new Point(x1, 57);
            pnlNim.Width = wNim;
            CenterInnerTextBox(pnlNim, txtNim);

            lblName.Location = new Point(x2, 38);
            pnlName.Location = new Point(x2, 57);
            pnlName.Width = wName;
            CenterInnerTextBox(pnlName, txtName);

            lblRoom.Location = new Point(x3, 38);
            cmbRoomCode.Location = new Point(x3, 57);
            cmbRoomCode.Width = Math.Max(110, wRoom - 38);
            btnRefreshRooms.Location = new Point(x3 + wRoom - 34, 57);

            lblHost.Location = new Point(x4, 38);
            pnlHost.Location = new Point(x4, 57);
            pnlHost.Width = wHost;
            CenterInnerTextBox(pnlHost, txtHost);

            // Row 2: Mode radio toggles
            rbServerMode.Location = new Point(18, 104);
            rbOfflineMode.Location = new Point(304, 104);

            // Row 3: Endpoint Server
            lblServerUrl.Location = new Point(18, 142);
            pnlServerUrl.Location = new Point(132, 134);
            pnlServerUrl.Width = Math.Max(200, contentW - 18 - 132);
            CenterInnerTextBox(pnlServerUrl, txtServerUrl);

            // Row 4: AutoStart checkbox
            chkAutoStart.Location = new Point(18, 182);
            chkAutoStart.Width = innerW;

            // Telemetry Ledger
            telemetryLedger.Location = new Point(padX, 328);
            telemetryLedger.Size = new Size(contentW, 76);

            // Action Buttons
            int btnY = 416;
            int btnGap = 12;
            int availBtns = contentW - (btnGap * 2);
            int wPrimary = (int)(availBtns * 0.44);
            int wPin = (int)(availBtns * 0.33);
            int wTray = availBtns - wPrimary - wPin;

            btnActivateLock.Location = new Point(padX, btnY);
            btnActivateLock.Size = new Size(wPrimary, 40);

            btnUnlockWithPin.Location = new Point(padX + wPrimary + btnGap, btnY);
            btnUnlockWithPin.Size = new Size(wPin, 40);

            btnHideToTray.Location = new Point(padX + wPrimary + btnGap + wPin + btnGap, btnY);
            btnHideToTray.Size = new Size(wTray, 40);

            // Log Header + Card
            lblLogHeader.Location = new Point(padX, 478);
            lblLogCount.Location = new Point(lblLogHeader.Right + 10, 478);
            btnUpdateApp.Location = new Point(ClientSize.Width - padX - btnUpdateApp.Width, 472);

            int logTop = 506;
            int logH = Math.Max(84, ClientSize.Height - logTop - 34);
            pnlLogCard.Location = new Point(padX, logTop);
            pnlLogCard.Size = new Size(contentW, logH);

            lstLogs.Location = new Point(10, 6);
            lstLogs.Size = new Size(Math.Max(100, pnlLogCard.Width - 20), Math.Max(40, pnlLogCard.Height - 12));
            lblLogEmpty.Location = new Point(12, 8);
            lblLogEmpty.Size = new Size(Math.Max(100, pnlLogCard.Width - 24), Math.Max(30, pnlLogCard.Height - 16));

            if (lblCredit != null)
            {
                int creditW = Math.Max(lblCredit.Width, lblCredit.PreferredSize.Width);
                lblCredit.Location = new Point(ClientSize.Width - padX - creditW, pnlLogCard.Bottom + 8);
            }
        }

        private void UpdatePrimaryButtonCopy()
        {
            if (btnActivateLock == null) return;
            btnActivateLock.Text = isServerMode
                ? "Hubungkan ke Ruangan & Sinkronkan Server"
                : "Aktifkan Penguncian Mandiri (Offline)";
        }

        public void SetTheme(bool dark)
        {
            ApplyTheme(dark ? AgentThemePalette.Dark : AgentThemePalette.Light);
        }

        private void ApplyTheme(AgentThemePalette theme)
        {
            currentTheme = theme;
            this.BackColor = theme.Canvas;
            this.ForeColor = theme.InkPrimary;

            ApplyBannerTheme();

            configPanel.BackColor = theme.Surface;
            configPanel.FillColor = theme.Surface;
            configPanel.BorderColor = theme.BorderSubtle;
            configPanel.DividerColor = theme.Divider;
            configPanel.Invalidate();

            lblSection1.ForeColor = theme.InkSecondary;
            lblSection1.BackColor = theme.Surface;
            lblVersionTag.ForeColor = theme.InkMuted;
            lblVersionTag.BackColor = theme.Surface;

            lblNim.ForeColor = theme.InkSecondary;
            lblNim.BackColor = theme.Surface;
            lblName.ForeColor = theme.InkSecondary;
            lblName.BackColor = theme.Surface;
            lblRoom.ForeColor = theme.InkSecondary;
            lblRoom.BackColor = theme.Surface;
            lblHost.ForeColor = theme.InkSecondary;
            lblHost.BackColor = theme.Surface;
            lblServerUrl.ForeColor = theme.InkSecondary;
            lblServerUrl.BackColor = theme.Surface;

            ApplyInputCardTheme(pnlNim, txtNim, txtNim.ReadOnly);
            ApplyInputCardTheme(pnlName, txtName, txtName.ReadOnly);
            ApplyInputCardTheme(pnlHost, txtHost, true);
            ApplyInputCardTheme(pnlServerUrl, txtServerUrl, txtServerUrl.ReadOnly);

            cmbRoomCode.Theme = theme;
            cmbRoomCode.BackColor = theme.InputBg;
            cmbRoomCode.ForeColor = theme.InkPrimary;
            cmbRoomCode.Invalidate();

            btnRefreshRooms.Theme = theme;
            btnRefreshRooms.Invalidate();

            rbServerMode.Theme = theme;
            rbServerMode.BackColor = theme.Surface;
            rbServerMode.Invalidate();
            rbOfflineMode.Theme = theme;
            rbOfflineMode.BackColor = theme.Surface;
            rbOfflineMode.Invalidate();

            chkAutoStart.Theme = theme;
            chkAutoStart.BackColor = theme.Surface;
            chkAutoStart.Invalidate();

            telemetryLedger.Theme = theme;
            telemetryLedger.Invalidate();

            btnActivateLock.Theme = theme;
            btnActivateLock.Invalidate();
            btnUnlockWithPin.Theme = theme;
            btnUnlockWithPin.Invalidate();
            btnHideToTray.Theme = theme;
            btnHideToTray.Invalidate();

            lblLogHeader.ForeColor = theme.InkSecondary;
            lblLogHeader.BackColor = theme.Canvas;
            lblLogCount.ForeColor = theme.InkMuted;
            lblLogCount.BackColor = theme.Canvas;

            btnUpdateApp.Theme = theme;
            btnUpdateApp.Invalidate();

            pnlLogCard.BackColor = theme.Surface;
            pnlLogCard.FillColor = theme.Surface;
            pnlLogCard.BorderColor = theme.BorderSubtle;
            pnlLogCard.Invalidate();

            lstLogs.BackColor = theme.Surface;
            lstLogs.ForeColor = theme.InkPrimary;
            lstLogs.Invalidate();

            lblLogEmpty.BackColor = theme.Surface;
            lblLogEmpty.ForeColor = theme.InkMuted;

            if (lblCredit != null)
            {
                lblCredit.BackColor = theme.Canvas;
                lblCredit.ForeColor = theme.InkMuted;
            }
        }

        private void ApplyInputCardTheme(AgentCardPanel card, TextBox tb, bool isReadonlyState)
        {
            Color bg = isReadonlyState ? currentTheme.ReadonlyBg : currentTheme.InputBg;
            card.FillColor = bg;
            card.BorderColor = isReadonlyState ? currentTheme.BorderSubtle : currentTheme.BorderStrong;
            card.FocusedBorderColor = currentTheme.Accent;
            if (isReadonlyState) card.IsFocusedBorder = false;
            card.Invalidate();

            tb.BackColor = bg;
            tb.ForeColor = isReadonlyState ? currentTheme.InkSecondary : currentTheme.InkPrimary;
        }

        private void ApplyBannerTheme()
        {
            bool locked = (currentBannerState == BannerState.Locked);
            headerBanner.BackColor = locked ? currentTheme.LockedBannerBg : currentTheme.Rail;
            headerBanner.Invalidate();

            btnThemeToggle.Theme = currentTheme;
            btnThemeToggle.IsBannerOverlay = locked;
            btnThemeToggle.Text = currentTheme.IsDark ? "Mode Terang" : "Mode Gelap";
            btnThemeToggle.Invalidate();
        }

        private void HeaderBanner_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            AgentThemePalette t = currentTheme;
            bool locked = (currentBannerState == BannerState.Locked);
            int w = headerBanner.Width;
            int h = headerBanner.Height;

            if (!locked)
            {
                using (Pen borderPen = new Pen(t.BorderSubtle, 1f))
                {
                    g.DrawLine(borderPen, 0, h - 1, w, h - 1);
                }
            }

            // Privilege Badge (right of title, left of btnThemeToggle)
            string privText = isAdminPrivilegeNow ? "SYSTEM ADMIN" : (hasServicePrivilegeNow ? "SERVICE + USER" : "STANDARD USER");
            Color privBg;
            Color privBorder;
            Color privInk;
            Color privDot;

            if (locked)
            {
                privBg = Color.FromArgb(18, 78, 56);
                privBorder = Color.FromArgb(148, 224, 192);
                privInk = Color.White;
                privDot = (isAdminPrivilegeNow || hasServicePrivilegeNow) ? Color.FromArgb(115, 227, 183) : Color.FromArgb(248, 198, 112);
            }
            else
            {
                privBg = t.Surface;
                privBorder = t.BorderStrong;
                privInk = t.InkPrimary;
                privDot = (isAdminPrivilegeNow || hasServicePrivilegeNow) ? t.Accent : t.Warning;
            }

            Size privMeasure = TextRenderer.MeasureText(g, privText, fMonoSmall, new Size(200, 24), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int privW = privMeasure.Width + 28;
            int privRight = btnThemeToggle.Left - 10;
            int privX = privRight - privW;
            RectangleF privRect = new RectangleF(privX, 29f, privW, 28f);

            using (GraphicsPath privPath = AgentDrawing.CreateRoundedRectPath(privRect, 6f))
            using (SolidBrush pBg = new SolidBrush(privBg))
            using (Pen pBd = new Pen(privBorder, 1f))
            {
                g.FillPath(pBg, privPath);
                g.DrawPath(pBd, privPath);
            }
            using (SolidBrush dotB = new SolidBrush(privDot))
            {
                g.FillEllipse(dotB, privX + 10f, 39.5f, 7f, 7f);
            }
            TextRenderer.DrawText(
                g,
                privText,
                fMonoSmall,
                new Rectangle(privX + 21, 29, privW - 26, 28),
                privInk,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
            );

            // Left Content Area
            int maxLeftRight = privX - 18;
            string roomLabel = string.IsNullOrEmpty(activeRoomCode) ? "OFFLINE" : activeRoomCode;
            string nameVal = string.IsNullOrWhiteSpace(txtName.Text) ? Environment.UserName : txtName.Text.Trim();
            string nimVal = string.IsNullOrWhiteSpace(txtNim.Text) ? Environment.MachineName : txtNim.Text.Trim();

            if (locked)
            {
                AgentDrawing.DrawShieldMark(
                    g,
                    new Rectangle(24, 23, 40, 40),
                    Color.FromArgb(18, 78, 56),
                    Color.White,
                    Color.FromArgb(148, 224, 192)
                );

                string titleText = "ENDPOINT TERKUNCI: AI & ROBLOX DIBLOKIR";
                string subText = string.Format("Peserta: {0} ({1}) | Ruangan: [{2}] | Buka via PIN Ruangan atau Admin.", nameVal, nimVal, roomLabel);

                int textX = 76;
                int textW = Math.Max(200, maxLeftRight - textX);
                TextRenderer.DrawText(
                    g,
                    titleText,
                    fBannerTitle,
                    new Rectangle(textX, 18, textW, 26),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    subText,
                    fBannerSub,
                    new Rectangle(textX, 45, textW, 24),
                    t.LockedBannerSub,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
            else
            {
                string tagText = currentBannerState == BannerState.Setup ? "SIAGA - PILIH RUANGAN" : "SIAGA - TERBUKA";
                Color dotC = currentBannerState == BannerState.Setup ? t.Warning : t.Accent;
                Color tagInk = currentBannerState == BannerState.Setup ? t.WarningInk : t.AccentInk;
                string titleText = currentBannerState == BannerState.Setup
                    ? "Pilih Ruangan Sesi untuk Memulai"
                    : "Endpoint Siaga (Dikendalikan Admin)";
                string subText = !string.IsNullOrEmpty(customBannerSub)
                    ? customBannerSub
                    : (currentBannerState == BannerState.Setup
                        ? "Pilih ruangan pada daftar di bawah lalu klik 'Hubungkan ke Ruangan & Sinkronkan Server'."
                        : string.Format("Ruangan [{0}] | {1} ({2}) | Siap menerima perintah Kunci Admin.", roomLabel, nameVal, nimVal));

                int textX = 24;
                int textW = Math.Max(200, maxLeftRight - textX);

                using (SolidBrush dotB = new SolidBrush(dotC))
                {
                    g.FillEllipse(dotB, textX, 14f, 7f, 7f);
                }
                TextRenderer.DrawText(
                    g,
                    tagText,
                    fSectionLabel,
                    new Rectangle(textX + 12, 10, textW - 12, 15),
                    tagInk,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    titleText,
                    fBannerTitle,
                    new Rectangle(textX, 27, textW, 26),
                    t.InkPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    subText,
                    fBannerSub,
                    new Rectangle(textX, 54, textW, 20),
                    t.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
        }

        private void LstLogs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstLogs.Items.Count) return;
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            string line = lstLogs.Items[e.Index] != null ? lstLogs.Items[e.Index].ToString() : string.Empty;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = selected ? currentTheme.SurfaceHover : currentTheme.Surface;

            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, e.Bounds);
            }

            Color textColor = currentTheme.InkPrimary;
            if (line.IndexOf("PERINGATAN:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("INTERSEPSI:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                line.IndexOf("Gagal", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                textColor = currentTheme.DangerInk;
            }
            else if (line.IndexOf("TERHUBUNG:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     line.IndexOf("Layer ", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     line.IndexOf("PEMBARUAN:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                textColor = currentTheme.AccentInk;
            }

            int xOffset = 6;
            if (line.StartsWith("[") && line.Length > 10 && line[9] == ']')
            {
                string ts = line.Substring(0, 10);
                string body = line.Substring(10).TrimStart();
                Rectangle tsRect = new Rectangle(e.Bounds.X + xOffset, e.Bounds.Y, 76, e.Bounds.Height);
                TextRenderer.DrawText(
                    g,
                    ts,
                    lstLogs.Font,
                    tsRect,
                    currentTheme.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                );
                Rectangle bodyRect = new Rectangle(e.Bounds.X + xOffset + 78, e.Bounds.Y, Math.Max(40, e.Bounds.Width - xOffset - 84), e.Bounds.Height);
                TextRenderer.DrawText(
                    g,
                    body,
                    lstLogs.Font,
                    bodyRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
            else
            {
                Rectangle bodyRect = new Rectangle(e.Bounds.X + xOffset, e.Bounds.Y, Math.Max(40, e.Bounds.Width - xOffset - 6), e.Bounds.Height);
                TextRenderer.DrawText(
                    g,
                    line,
                    lstLogs.Font,
                    bodyRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix
                );
            }
        }

        private void UpdateLogEmptyAndCount()
        {
            if (lstLogs == null || lblLogEmpty == null || lblLogCount == null) return;
            int cnt = lstLogs.Items.Count;
            lstLogs.Visible = (cnt > 0);
            lblLogEmpty.Visible = (cnt == 0);
            lblLogCount.Text = string.Format("{0} entri", cnt);
        }

        public void ApplyPreviewState(string stateName, string roomCode, int domains, int extCount, int killedCount, string serverStatus, bool vpnDetected)
        {
            activeRoomCode = string.IsNullOrEmpty(roomCode) ? "" : roomCode.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(activeRoomCode))
            {
                if (!cmbRoomCode.Items.Contains(activeRoomCode)) cmbRoomCode.Items.Add(activeRoomCode);
                cmbRoomCode.SelectedItem = activeRoomCode;
            }

            isRoomConfirmed = !string.IsNullOrEmpty(activeRoomCode);
            killedCounter = Math.Max(0, killedCount);

            telemetryLedger.SetCell(0, domains + " Domain", TelemetryTone.Neutral);
            telemetryLedger.SetCell(1, extCount + " Modul", extCount > 0 ? TelemetryTone.Accent : TelemetryTone.Neutral);
            telemetryLedger.SetCell(2, killedCounter + " Insiden", killedCounter > 0 ? TelemetryTone.Danger : TelemetryTone.Neutral);

            TelemetryTone srvTone = vpnDetected
                ? TelemetryTone.Danger
                : (string.Equals(serverStatus, "Pilih Ruangan", StringComparison.OrdinalIgnoreCase) ? TelemetryTone.Warning : TelemetryTone.Accent);
            telemetryLedger.SetCell(3, serverStatus, srvTone);

            if (string.Equals(stateName, "LOCKED", StringComparison.OrdinalIgnoreCase))
            {
                isLocked = true;
                currentBannerState = BannerState.Locked;
                customBannerSub = null;
                SetConfigFieldsEnabled(false);
                btnActivateLock.Enabled = false;
                btnUnlockWithPin.Enabled = true;
            }
            else if (string.Equals(stateName, "STANDBY", StringComparison.OrdinalIgnoreCase))
            {
                isLocked = false;
                currentBannerState = BannerState.Standby;
                customBannerSub = null;
                SetConfigFieldsEnabled(true);
                btnActivateLock.Enabled = true;
                btnUnlockWithPin.Enabled = false;
            }
            else
            {
                isLocked = false;
                currentBannerState = BannerState.Setup;
                customBannerSub = null;
                SetConfigFieldsEnabled(true);
                btnActivateLock.Enabled = true;
                btnUnlockWithPin.Enabled = false;
            }

            ApplyBannerTheme();
            btnUnlockWithPin.Invalidate();
            btnActivateLock.Invalidate();
        }

        private void SetConfigFieldsEnabled(bool enabled)
        {
            txtNim.ReadOnly = !enabled;
            txtNim.TabStop = enabled;
            txtName.ReadOnly = !enabled;
            txtName.TabStop = enabled;
            cmbRoomCode.Enabled = enabled;
            btnRefreshRooms.Enabled = enabled;

            bool urlEditable = enabled && isServerMode;
            txtServerUrl.ReadOnly = !urlEditable;
            txtServerUrl.TabStop = urlEditable;

            rbServerMode.Enabled = enabled;
            rbOfflineMode.Enabled = enabled;
            chkAutoStart.Enabled = enabled;

            ApplyInputCardTheme(pnlNim, txtNim, txtNim.ReadOnly);
            ApplyInputCardTheme(pnlName, txtName, txtName.ReadOnly);
            ApplyInputCardTheme(pnlServerUrl, txtServerUrl, txtServerUrl.ReadOnly);
            cmbRoomCode.Invalidate();
            btnRefreshRooms.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (trayIcon != null) { trayIcon.Visible = false; trayIcon.Dispose(); trayIcon = null; }
                if (trayMenu != null) { trayMenu.Dispose(); trayMenu = null; }
                if (appIcon != null) { appIcon.Dispose(); appIcon = null; }
                if (fBannerTitle != null) fBannerTitle.Dispose();
                if (fBannerSub != null) fBannerSub.Dispose();
                if (fSectionLabel != null) fSectionLabel.Dispose();
                if (fLabel != null) fLabel.Dispose();
                if (fSubtle != null) fSubtle.Dispose();
                if (fButton != null) fButton.Dispose();
                if (fMono != null) fMono.Dispose();
                if (fMonoBold != null) fMonoBold.Dispose();
                if (fMonoSmall != null) fMonoSmall.Dispose();
                if (fStatValue != null) fStatValue.Dispose();
            }
            base.Dispose(disposing);
        }

        private string GetSelectedRoomCode()
        {
            if (cmbRoomCode == null || cmbRoomCode.SelectedItem == null) return "";
            string val = cmbRoomCode.SelectedItem.ToString().Trim();
            if (string.IsNullOrEmpty(val) || val == RoomPlaceholder) return "";
            return val.ToUpperInvariant();
        }

        private void StartAgentLifecycle()
        {
            if (lifecycleStarted) return;
            lifecycleStarted = true;

            GuardEngine.EnsureAutoStartRegistered(chkAutoStart.Checked);

            if (startMinimizedInTray)
            {
                HideToTray(false);
            }

            var unused = InitializeConnectionWithRetryAsync();
        }

        private async Task InitializeConnectionWithRetryAsync()
        {
            if (isRoomConfirmed && !string.IsNullOrEmpty(activeRoomCode))
            {
                heartbeatTimer.Start();
                AddLog("MENYAMBUNG: Menghubungkan ke server untuk ruangan [" + activeRoomCode + "]...");
            }

            int attempts = 0;
            const int maxRetries = 15;
            bool connected = false;

            while (!connected && attempts < maxRetries)
            {
                attempts++;
                try
                {
                    await FetchAvailableRoomsAsync(false);
                    string selected = GetSelectedRoomCode();
                    if (string.IsNullOrEmpty(selected) && !string.IsNullOrEmpty(activeRoomCode))
                    {
                        selected = activeRoomCode;
                    }

                    if (isRoomConfirmed && !string.IsNullOrEmpty(selected))
                    {
                        await ConnectAndSyncServerAsync(false);
                        connected = true;
                        AddLog("TERHUBUNG: Agen berhasil tersambung ke konsol ruangan [" + selected + "].");
                        break;
                    }
                }
                catch { }

                if (!connected)
                {
                    await Task.Delay(attempts <= 3 ? 3000 : 5000);
                }
            }

            if (!connected && !isRoomConfirmed)
            {
                telemetryLedger.SetCell(3, "Pilih Ruangan", TelemetryTone.Warning);
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
                            SaveCurrentAgentConfig(isRoomConfirmed);
                            AddLog("Daftar ruangan berhasil dimuat dari server (" + serverRooms.Count + " ruangan tersedia).");
                        }
                    }
                }
                else if (logSuccess)
                {
                    AddLog("Gagal memuat daftar ruangan dari " + txtServerUrl.Text.Trim() + ". Periksa Endpoint Server.");
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

            if (!string.IsNullOrEmpty(currentSel))
            {
                if (serverRooms.Contains(currentSel, StringComparer.OrdinalIgnoreCase))
                {
                    cmbRoomCode.SelectedItem = serverRooms.First(x => x.Equals(currentSel, StringComparison.OrdinalIgnoreCase));
                }
                else if (isRoomConfirmed)
                {
                    // Tetap pertahankan ruangan yang sudah dikonfirmasi agar tidak ter-reset jika koneksi server sedang refresh
                    cmbRoomCode.Items.Add(currentSel);
                    cmbRoomCode.SelectedItem = currentSel;
                }
                else
                {
                    cmbRoomCode.SelectedIndex = 0;
                }
            }
            else
            {
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
            UpdateLogEmptyAndCount();
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
                telemetryLedger.SetCell(3, "Pilih Ruangan", TelemetryTone.Warning);
                return;
            }

            activeRoomCode = selRoom;
            AddLog("Menghubungkan & menyinkronkan status dengan Server Admin (Ruangan [" + selRoom + "])...");
            string targetStatus = await JoinServerAsync(showDialogOnError);
            if (string.IsNullOrEmpty(targetStatus))
            {
                telemetryLedger.SetCell(3, "Pilih Ruangan", TelemetryTone.Warning);
                return;
            }

            isRoomConfirmed = true;
            SaveCurrentAgentConfig(true);

            telemetryLedger.SetCell(3, "Tersambung Admin", TelemetryTone.Accent);

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
            SetStandbyBannerUI(null);
        }

        private void SetStandbyBannerUI(string overrideSub)
        {
            string roomLabel = string.IsNullOrEmpty(activeRoomCode) ? "BELUM DIPILIH" : activeRoomCode;
            currentBannerState = BannerState.Standby;
            customBannerSub = overrideSub;
            ApplyBannerTheme();
            if (trayIcon != null)
            {
                trayIcon.Text = "GuardAgent: SIAGA (Terbuka) - [" + roomLabel + "]";
            }
        }

        private void ActivateProtectionState()
        {
            isLocked = true;
            string roomLabel = string.IsNullOrEmpty(activeRoomCode) ? "OFFLINE" : activeRoomCode;

            currentBannerState = BannerState.Locked;
            customBannerSub = null;
            ApplyBannerTheme();

            if (trayIcon != null)
            {
                trayIcon.Text = "GuardAgent: TERKUNCI (AI & Roblox Diblokir) - [" + roomLabel + "]";
            }

            SetConfigFieldsEnabled(false);
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
            telemetryLedger.SetCell(0, GuardEngine.BlockedDomains.Count + " Domain", TelemetryTone.Neutral);

            // Layer 2: Browser DoH & Built-in AI Policy
            GuardEngine.ApplyBrowserPolicies(false);
            AddLog("Layer 2 Aktif: Kebijakan DNS-over-HTTPS (DoH) & AI Browser dinonaktifkan.");

            // Layer 3: Karantina Ekstensi VS Code + Built-in Copilot
            quarantinedExtensions = GuardEngine.ScanAndQuarantineExtensions(false);
            telemetryLedger.SetCell(1, quarantinedExtensions.Count + " Modul", quarantinedExtensions.Count > 0 ? TelemetryTone.Accent : TelemetryTone.Neutral);
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

            SetStandbyBannerUI(reason + string.Format(" ({0} modul IDE dipulihkan | Agen tetap siaga memantau perintah Admin).", restoredCount));

            SetConfigFieldsEnabled(true);
            btnActivateLock.Enabled = true;
            btnUnlockWithPin.Enabled = false;

            quarantinedExtensions.Clear();
            telemetryLedger.SetCell(1, "0 Modul", TelemetryTone.Neutral);
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
                telemetryLedger.SetCell(2, killedCounter + " Insiden", TelemetryTone.Danger);
                string fullMsg = "Menghentikan proses terlarang (AI/Roblox): " + string.Join(", ", killed);
                AddLog("INTERSEPSI: " + fullMsg);
                if (isServerMode) { var u2 = SendViolationAsync("PROCESS_KILLED", fullMsg, "DANGER"); }
            }

            // 3. Cek ekstensi baru yang mungkin diinstal saat ujian
            List<string> newlyQuarantined = GuardEngine.ScanAndQuarantineExtensions(false);
            if (newlyQuarantined.Count != quarantinedExtensions.Count)
            {
                quarantinedExtensions = newlyQuarantined;
                telemetryLedger.SetCell(1, quarantinedExtensions.Count + " Modul", quarantinedExtensions.Count > 0 ? TelemetryTone.Accent : TelemetryTone.Neutral);
            }

            // 4. Cek VPN / Tunneling
            string vpnName;
            bool hasVpn = GuardEngine.DetectActiveVpn(out vpnName);
            if (hasVpn)
            {
                telemetryLedger.SetCell(3, "VPN Terdeteksi", TelemetryTone.Danger);
            }
            else
            {
                telemetryLedger.SetCell(3, isServerMode ? "Tersambung Admin" : "Mode Mandiri", TelemetryTone.Accent);
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
                    { "ip", GuardEngine.GetLocalIPv4Address() },
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
                if (response.ContainsKey("target_status") && !string.IsNullOrEmpty(Convert.ToString(response["target_status"])))
                {
                    targetStatus = Convert.ToString(response["target_status"]);
                }
                if (response.ContainsKey("room_pin_hash") && !string.IsNullOrEmpty(Convert.ToString(response["room_pin_hash"])))
                {
                    offlinePinHash = Convert.ToString(response["room_pin_hash"]);
                }
                if (response.ContainsKey("config"))
                {
                    var cfg = response["config"] as Dictionary<string, object>;
                    if (cfg != null)
                    {
                        if (cfg.ContainsKey("target_status") && !string.IsNullOrEmpty(Convert.ToString(cfg["target_status"])))
                        {
                            targetStatus = Convert.ToString(cfg["target_status"]);
                        }
                        if (string.IsNullOrEmpty(offlinePinHash) && cfg.ContainsKey("room_pin_hash") && !string.IsNullOrEmpty(Convert.ToString(cfg["room_pin_hash"])))
                        {
                            offlinePinHash = Convert.ToString(cfg["room_pin_hash"]);
                        }
                        else if (cfg.ContainsKey("room_pin") && !string.IsNullOrEmpty(Convert.ToString(cfg["room_pin"])))
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
                    { "ip", GuardEngine.GetLocalIPv4Address() },
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
                        telemetryLedger.SetCell(3, "Pilih Ruangan", TelemetryTone.Warning);
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

                    // Sinkronkan PIN hash khusus ruangan dari server untuk cadangan offline aman
                    string pinHash = null;
                    if (res.ContainsKey("room_pin_hash") && !string.IsNullOrEmpty(Convert.ToString(res["room_pin_hash"])))
                    {
                        pinHash = Convert.ToString(res["room_pin_hash"]);
                    }
                    else if (res.ContainsKey("config"))
                    {
                        var cfg = res["config"] as Dictionary<string, object>;
                        if (cfg != null && cfg.ContainsKey("room_pin_hash") && !string.IsNullOrEmpty(Convert.ToString(cfg["room_pin_hash"])))
                        {
                            pinHash = Convert.ToString(cfg["room_pin_hash"]);
                        }
                    }
                    if (!string.IsNullOrEmpty(pinHash))
                    {
                        offlinePinHash = pinHash;
                    }
                    else if (res.ContainsKey("room_pin") && !string.IsNullOrEmpty(Convert.ToString(res["room_pin"])))
                    {
                        offlinePin = Convert.ToString(res["room_pin"]);
                    }

                    // Jika Admin mengganti nama ruangan di server, perbarui kode ruangan lokal secara otomatis
                    string srvRoom = null;
                    if (res.ContainsKey("room_code"))
                    {
                        srvRoom = Convert.ToString(res["room_code"]).Trim().ToUpperInvariant();
                    }
                    else if (res.ContainsKey("config"))
                    {
                        var cfg = res["config"] as Dictionary<string, object>;
                        if (cfg != null && cfg.ContainsKey("room_code"))
                        {
                            srvRoom = Convert.ToString(cfg["room_code"]).Trim().ToUpperInvariant();
                        }
                    }
                    if (!string.IsNullOrEmpty(srvRoom) && !srvRoom.Equals(activeRoomCode, StringComparison.OrdinalIgnoreCase))
                    {
                        activeRoomCode = srvRoom;
                        if (!cmbRoomCode.Items.Contains(srvRoom)) cmbRoomCode.Items.Add(srvRoom);
                        cmbRoomCode.SelectedItem = srvRoom;
                        SaveCurrentAgentConfig(true);
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

                    if (res.ContainsKey("command") && string.Equals(Convert.ToString(res["command"]), "UPDATE", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLog("INSTRUKSI ADMIN DITERIMA: Memulai pembaruan otomatis (Remote Update)...");
                        var unused = CheckAndPerformUpdateAsync(false);
                    }
                }
            }
            catch { }
        }

        private async Task CheckAndPerformUpdateAsync(bool userInitiated)
        {
            try
            {
                AddLog("MEMERIKSA PEMBARUAN: Menghubungi server...");
                string endpoint = txtServerUrl.Text.Trim();
                string baseUrl = endpoint;
                int apiIdx = baseUrl.IndexOf("/api/", StringComparison.OrdinalIgnoreCase);
                if (apiIdx >= 0)
                {
                    baseUrl = baseUrl.Substring(0, apiIdx);
                }
                baseUrl = baseUrl.TrimEnd('/');

                string updateUrl = baseUrl + "/dist/AIBlockerGuard.exe";
                string fallbackUrl = "https://raw.githubusercontent.com/nizan12/addsaianextentions/main/dist/AIBlockerGuard.exe";

                string currentExePath = Application.ExecutablePath;
                string tempExePath = Path.Combine(Path.GetTempPath(), "AIBlockerGuard_update.tmp");

                AddLog("MENGUNDUH: Mengambil versi terbaru dari server...");
                byte[] exeBytes = null;

                string[] downloadUrls = new string[] { updateUrl, fallbackUrl };
                foreach (string u in downloadUrls)
                {
                    try
                    {
                        string currentTargetUrl = u;
                        exeBytes = await Task.Run(() =>
                        {
                            using (var wc = new WebClient())
                            {
                                wc.Headers.Add("User-Agent", "GuardAgent-Updater/" + GuardEngine.CurrentVersion);
                                return wc.DownloadData(currentTargetUrl);
                            }
                        });
                        if (exeBytes != null && exeBytes.Length >= 50000)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        AddLog("MENGUNDUH: Mencoba jalur cadangan repositori cloud...");
                    }
                }

                if (exeBytes == null)
                {
                    string err = "Gagal mengunduh pembaruan dari server maupun cloud.";
                    AddLog(err);
                    if (userInitiated) MessageBox.Show(err, "Gagal Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validasi file PE Windows (dimulai dengan "MZ" dan ukuran > 50KB)
                if (exeBytes == null || exeBytes.Length < 50000 || exeBytes[0] != 0x4D || exeBytes[1] != 0x5A)
                {
                    string err = "File pembaruan yang diunduh tidak valid atau server belum menyediakan file terbaru.";
                    AddLog(err);
                    if (userInitiated) MessageBox.Show(err, "Update Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cek apakah file yang diunduh sama persis dengan yang saat ini berjalan
                try
                {
                    byte[] currentBytes = File.ReadAllBytes(currentExePath);
                    if (currentBytes.Length == exeBytes.Length)
                    {
                        bool isIdentical = true;
                        for (int i = 0; i < currentBytes.Length; i++)
                        {
                            if (currentBytes[i] != exeBytes[i]) { isIdentical = false; break; }
                        }
                        if (isIdentical)
                        {
                            AddLog("PEMBARUAN: Aplikasi sudah merupakan versi terbaru (" + GuardEngine.CurrentVersion + ").");
                            if (userInitiated) MessageBox.Show("Aplikasi GuardAgent di PC ini sudah menggunakan versi terbaru (" + GuardEngine.CurrentVersion + "). Tidak perlu pembaruan.", "Sudah Versi Terbaru", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                }
                catch { }

                File.WriteAllBytes(tempExePath, exeBytes);

                if (userInitiated)
                {
                    var confirm = MessageBox.Show(
                        "Versi terbaru berhasil diunduh (" + (exeBytes.Length / 1024) + " KB)!\n\nAplikasi akan restart otomatis dalam 2 detik untuk menerapkan pembaruan.\nLanjutkan proses pembaruan?",
                        "Update Siap Dipasang",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (confirm != DialogResult.Yes)
                    {
                        try { File.Delete(tempExePath); } catch { }
                        return;
                    }
                }

                AddLog("MENERAPKAN PEMBARUAN: Memulai instalasi dan me-restart GuardAgent...");

                // Pulihkan blokir sementara agar tidak mengunci resource
                GuardEngine.EmergencyRestoreAll();

                if (!GuardEngine.IsAdministrator() && ServicePipeClient.IsServiceAvailable())
                {
                    var svcRes = ServicePipeClient.SendCommand(new Dictionary<string, object>
                    {
                        { "cmd", "REPLACE_CLIENT_EXE" },
                        { "tempPath", tempExePath },
                        { "targetPath", currentExePath }
                    });
                    if (svcRes != null && svcRes.ContainsKey("ok") && Convert.ToBoolean(svcRes["ok"]))
                    {
                        string restartBat = Path.Combine(Path.GetTempPath(), "guard_user_restart.bat");
                        string restartScript = string.Format(
                            "@echo off\r\n" +
                            "ping 127.0.0.1 -n 5 >nul\r\n" +
                            "start \"\" \"{0}\"\r\n" +
                            "del \"%~f0\" & exit\r\n",
                            currentExePath
                        );
                        File.WriteAllText(restartBat, restartScript, Encoding.ASCII);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = restartBat,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });
                        allowProcessExit = true;
                        Application.Exit();
                        Environment.Exit(0);
                        return;
                    }
                }

                // Buat script batch updater mandiri yang mengganti file dan me-restart exe
                string batPath = Path.Combine(Path.GetTempPath(), "guard_self_update.bat");
                string batScript = string.Format(
                    "@echo off\r\n" +
                    "ping 127.0.0.1 -n 2 >nul\r\n" +
                    ":retry\r\n" +
                    "del \"{0}\" >nul 2>&1\r\n" +
                    "if exist \"{0}\" (\r\n" +
                    "    ping 127.0.0.1 -n 2 >nul\r\n" +
                    "    goto retry\r\n" +
                    ")\r\n" +
                    "move /y \"{1}\" \"{0}\" >nul 2>&1\r\n" +
                    "start \"\" \"{0}\"\r\n" +
                    "del \"%~f0\" & exit\r\n",
                    currentExePath,
                    tempExePath
                );

                File.WriteAllText(batPath, batScript, Encoding.ASCII);

                var psi = new ProcessStartInfo
                {
                    FileName = batPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(psi);

                allowProcessExit = true;
                Application.Exit();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                AddLog("Error saat update: " + ex.Message);
                if (userInitiated) MessageBox.Show("Terjadi kesalahan saat memproses pembaruan:\n" + ex.Message, "Error Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                telemetryLedger.SetCell(0, GuardEngine.BlockedDomains.Count + " Domain", TelemetryTone.Neutral);
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
        private static string ComputeRoomPinHash(string room, string pin)
        {
            if (string.IsNullOrEmpty(room) || string.IsNullOrEmpty(pin)) return "";
            try
            {
                byte[] salt = Encoding.UTF8.GetBytes("guard-pin:" + room.Trim().ToUpperInvariant());
                using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(pin.Trim(), salt, 20000))
                {
                    byte[] hash = pbkdf2.GetBytes(32);
                    StringBuilder sb = new StringBuilder(hash.Length * 2);
                    foreach (byte b in hash) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
            catch { return ""; }
        }

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
                if (res != null)
                {
                    if (res.ContainsKey("unlocked") && (bool)res["unlocked"])
                    {
                        lastPinVerifyError = "";
                        return true;
                    }
                    if (res.ContainsKey("error"))
                    {
                        lastPinVerifyError = Convert.ToString(res["error"]);
                    }
                    else
                    {
                        lastPinVerifyError = "PIN Otorisasi Ruangan yang dimasukkan salah!";
                    }
                    return false;
                }
            }

            // Verifikasi offline: jika server sedang tidak dapat dihubungi (res == null),
            // lakukan verifikasi terhadap salted PBKDF2 hash cadangan lokal
            if (!string.IsNullOrEmpty(offlinePinHash))
            {
                string inputHash = ComputeRoomPinHash(activeRoomCode, inputPin);
                if (!string.IsNullOrEmpty(inputHash) && string.Equals(inputHash, offlinePinHash, StringComparison.OrdinalIgnoreCase))
                {
                    lastPinVerifyError = "";
                    return true;
                }
            }

            bool matchOffline = inputPin.Trim() == offlinePin;
            if (!matchOffline)
            {
                lastPinVerifyError = "PIN Otorisasi Ruangan yang dimasukkan salah!";
            }
            return matchOffline;
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
                string errMsg = string.IsNullOrEmpty(lastPinVerifyError)
                    ? "PIN Otorisasi Ruangan yang dimasukkan salah!"
                    : lastPinVerifyError;
                AddLog("PERINGATAN: Percobaan membuka kunci gagal (" + errMsg + ")!");
                MessageBox.Show(errMsg, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                string errMsg = string.IsNullOrEmpty(lastPinVerifyError)
                    ? "PIN Otorisasi Ruangan salah! Agen tetap aktif di sistem."
                    : lastPinVerifyError;
                MessageBox.Show(errMsg, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Form CreatePinDialogForm(string labelText)
        {
            AgentThemePalette t = currentTheme;
            Form prompt = new Form();
            prompt.Name = "PinDialogForm";
            prompt.ClientSize = new Size(420, 224);
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = "Otorisasi PIN Ruangan / Pengawas";
            prompt.StartPosition = FormStartPosition.CenterScreen;
            prompt.MaximizeBox = false;
            prompt.MinimizeBox = false;
            prompt.AutoScaleMode = AutoScaleMode.None;
            prompt.BackColor = t.Canvas;
            prompt.ForeColor = t.InkPrimary;
            prompt.Font = new Font(uiFamily, 9.0f, FontStyle.Regular);
            if (appIcon != null) prompt.Icon = appIcon;

            AgentDoubleBufferedPanel hdr = new AgentDoubleBufferedPanel();
            hdr.Location = new Point(0, 0);
            hdr.Size = new Size(420, 56);
            hdr.BackColor = t.Rail;
            hdr.Paint += (s, pe) =>
            {
                Graphics g = pe.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                AgentDrawing.DrawShieldMark(g, new Rectangle(20, 12, 32, 32), t.BadgeBg, t.BadgeStroke, Color.Transparent);
                TextRenderer.DrawText(
                    g,
                    "Otorisasi PIN Ruangan",
                    fButton,
                    new Rectangle(62, 10, 330, 20),
                    t.InkPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                );
                TextRenderer.DrawText(
                    g,
                    "Verifikasi pengawas untuk mengubah status proteksi",
                    fMonoSmall,
                    new Rectangle(62, 30, 330, 16),
                    t.InkMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix
                );
                using (Pen hp = new Pen(t.BorderSubtle, 1f))
                {
                    g.DrawLine(hp, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
                }
            };
            prompt.Controls.Add(hdr);

            Label lbl = new Label
            {
                Name = "lblPinInstruction",
                Left = 20,
                Top = 68,
                Width = 380,
                Height = 36,
                Text = labelText,
                UseMnemonic = false,
                Font = fSubtle,
                ForeColor = t.InkSecondary,
                BackColor = t.Canvas
            };
            prompt.Controls.Add(lbl);

            TextBox txt = new TextBox
            {
                Name = "txtPinInput",
                PasswordChar = '●',
                Font = new Font(monoFamily, 12f, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                BackColor = t.InputBg,
                ForeColor = t.InkPrimary,
                AccessibleName = "Masukkan PIN Otorisasi Ruangan",
                TabIndex = 0
            };

            AgentCardPanel pnlInput = new AgentCardPanel
            {
                Name = "pnlPinInput",
                Location = new Point(20, 108),
                Size = new Size(380, 38),
                CornerRadius = 7f,
                FillColor = t.InputBg,
                BorderColor = t.BorderStrong,
                FocusedBorderColor = t.Accent
            };
            CenterInnerTextBox(pnlInput, txt);
            pnlInput.Controls.Add(txt);
            pnlInput.Click += (s, e) => txt.Focus();
            txt.Enter += (s, e) => { pnlInput.IsFocusedBorder = true; pnlInput.Invalidate(); };
            txt.Leave += (s, e) => { pnlInput.IsFocusedBorder = false; pnlInput.Invalidate(); };
            prompt.Controls.Add(pnlInput);

            AgentDoubleBufferedPanel ftr = new AgentDoubleBufferedPanel();
            ftr.Location = new Point(0, 162);
            ftr.Size = new Size(420, 62);
            ftr.BackColor = t.Surface;
            ftr.Paint += (s, pe) =>
            {
                using (Pen hp = new Pen(t.BorderSubtle, 1f))
                {
                    pe.Graphics.DrawLine(hp, 0, 0, ftr.Width, 0);
                }
            };
            prompt.Controls.Add(ftr);

            AgentButton btnCancelPin = new AgentButton
            {
                Name = "btnCancelPin",
                Theme = t,
                Text = "Batal",
                Font = fSubtle,
                Location = new Point(20, 13),
                Size = new Size(96, 36),
                DialogResult = DialogResult.Cancel,
                TabIndex = 2,
                AccessibleName = "Batal verifikasi PIN"
            };
            ftr.Controls.Add(btnCancelPin);

            AgentButton btnOkPin = new AgentButton
            {
                Name = "btnOkPin",
                Theme = t,
                IsPrimary = true,
                Text = "Verifikasi PIN",
                Font = fButton,
                Location = new Point(244, 13),
                Size = new Size(156, 36),
                DialogResult = DialogResult.OK,
                TabIndex = 1,
                AccessibleName = "Verifikasi PIN Ruangan"
            };
            ftr.Controls.Add(btnOkPin);

            prompt.AcceptButton = btnOkPin;
            prompt.CancelButton = btnCancelPin;
            prompt.Shown += (s, e) => txt.Focus();
            return prompt;
        }

        private string PromptPinDialog(string labelText)
        {
            using (Form prompt = CreatePinDialogForm(labelText))
            {
                Control[] found = prompt.Controls.Find("txtPinInput", true);
                TextBox txt = found.Length > 0 ? found[0] as TextBox : null;
                return prompt.ShowDialog(this) == DialogResult.OK && txt != null ? txt.Text : "";
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Jika pengguna menekan tombol [X], JANGAN matikan aplikasi - sembunyikan ke System Tray
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
