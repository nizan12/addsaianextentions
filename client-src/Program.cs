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
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AIBlockerGuard
{
    static class Program
    {
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
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
            // IDE AI APIs & Proxies
            "api.githubcopilot.com", "copilot-proxy.githubusercontent.com", "copilot-telemetry.githubusercontent.com", "default.exp-tas.com",
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
            "cohere.com", "api.cohere.ai"
        };

        public static List<string> BlockedProcesses = new List<string>
        {
            "Cursor", "Windsurf", "Trae", "PearAI", "Void",
            "ChatGPT", "Claude", "Copilot", "Perplexity",
            "ollama", "ollama_llama_server", "ollama app",
            "LM Studio", "lm-studio", "jan", "gpt4all",
            "llama-server", "llama-cli", "koboldcpp", "AnythingLLM", "Msty"
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
        // LAYER 2: BROWSER DNS-OVER-HTTPS (DoH) & BUILT-IN AI POLICY
        // --------------------------------------------------------------------
        public static void ApplyBrowserPolicies(bool dryRun)
        {
            if (dryRun) return;
            try
            {
                // Chrome: Matikan DoH & Built-in Gemini AI
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Google\Chrome"))
                {
                    if (key != null)
                    {
                        key.SetValue("DnsOverHttpsMode", "off", RegistryValueKind.String);
                        key.SetValue("GenAiDefaultSettings", 2, RegistryValueKind.DWord);
                    }
                }
                // Microsoft Edge: Matikan DoH & Copilot Sidebar
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge"))
                {
                    if (key != null)
                    {
                        key.SetValue("DnsOverHttpsMode", "off", RegistryValueKind.String);
                        key.SetValue("HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                        key.SetValue("CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                    }
                }
                // Brave Browser: Matikan DoH & Leo AI
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\BraveSoftware\Brave"))
                {
                    if (key != null)
                    {
                        key.SetValue("DnsOverHttpsMode", "off", RegistryValueKind.String);
                        key.SetValue("BraveAIChatEnabled", 0, RegistryValueKind.DWord);
                    }
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
                    @"SOFTWARE\Policies\BraveSoftware\Brave"
                };
                foreach (string sk in subKeys)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(sk, true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue("DnsOverHttpsMode", false);
                            key.DeleteValue("GenAiDefaultSettings", false);
                            key.DeleteValue("HubsSidebarEnabled", false);
                            key.DeleteValue("CopilotCDPPageContext", false);
                            key.DeleteValue("BraveAIChatEnabled", false);
                        }
                    }
                }
            }
            catch { }
        }

        // --------------------------------------------------------------------
        // LAYER 3: IDE EXTENSION QUARANTINE (VS Code & JetBrains)
        // --------------------------------------------------------------------
        public static List<string> ScanAndQuarantineExtensions(bool dryRun)
        {
            EnsureDirectories();
            List<string> detectedNames = new List<string>();
            List<QuarantineEntry> manifest = LoadManifest();

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
    }

    // ============================================================================
    // MAIN DESKTOP FORM UI (PC MAHASISWA)
    // ============================================================================
    public class MainForm : Form
    {
        // State
        private bool isLocked = false;
        private bool isDryRun = false;
        private bool isServerMode = true;
        private string offlinePin = "202699";
        private int killedCounter = 0;
        private List<string> quarantinedExtensions = new List<string>();
        private Timer guardTimer;
        private Timer heartbeatTimer;
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

        // UI Controls
        private Panel headerBanner;
        private Label lblBannerTitle;
        private Label lblBannerSub;
        private Label lblAdminBadge;

        private TextBox txtNim;
        private TextBox txtName;
        private TextBox txtServerUrl;
        private TextBox txtRoomCode;
        private RadioButton rbServerMode;
        private RadioButton rbOfflineMode;
        private CheckBox chkDryRun;

        private Button btnActivateLock;
        private Button btnUnlockWithPin;
        private Button btnEmergencyRestore;

        private Label lblStatDomains;
        private Label lblStatExtensions;
        private Label lblStatKilled;
        private Label lblStatVpn;
        private ListBox lstLogs;

        public MainForm()
        {
            InitializeUi();
            CheckInitialEnvironment();
        }

        private void InitializeUi()
        {
            this.Text = "GuardAgent — Sistem Integritas & Proteksi Endpoint Ujian";
            this.Size = new Size(860, 650);
            this.MinimumSize = new Size(820, 620);
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
                BackColor = Color.FromArgb(15, 23, 42), // Deep Slate saat persiapan
                Padding = new Padding(22, 14, 22, 14)
            };

            lblBannerTitle = new Label
            {
                Text = "STATUS ENDPOINT: PERSIAPAN SESI (BELUM TERKUNCI)",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 16)
            };

            lblBannerSub = new Label
            {
                Text = "Lengkapi identitas peserta di bawah ini lalu aktifkan proteksi sebelum ujian dimulai.",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                AutoSize = true,
                Location = new Point(22, 44)
            };

            lblAdminBadge = new Label
            {
                Text = GuardEngine.IsAdministrator() ? "ADMINISTRATOR" : "STANDARD USER",
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
                Size = new Size(800, 182),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            configPanel.Paint += DrawCardBorder;

            Label lblSection1 = new Label
            {
                Text = "PARAMETER PESERTA & KONEKSI SESI UJIAN",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(18, 14),
                AutoSize = true
            };
            configPanel.Controls.Add(lblSection1);

            // Baris Input 1: NIM & Nama
            configPanel.Controls.Add(MakeLabel("Nomor Induk Mahasiswa (NIM)", 18, 42));
            txtNim = MakeTextBox("240101001", 18, 62, 170);
            configPanel.Controls.Add(txtNim);

            configPanel.Controls.Add(MakeLabel("Nama Lengkap Peserta", 204, 42));
            txtName = MakeTextBox(Environment.UserName, 204, 62, 230);
            configPanel.Controls.Add(txtName);

            configPanel.Controls.Add(MakeLabel("Kode Ruangan Sesi", 450, 42));
            txtRoomCode = MakeTextBox("KUIS-AI-01", 450, 62, 130);
            configPanel.Controls.Add(txtRoomCode);

            configPanel.Controls.Add(MakeLabel("Identitas Perangkat", 596, 42));
            TextBox txtHost = MakeTextBox(Environment.MachineName, 596, 62, 184);
            txtHost.ReadOnly = true;
            txtHost.BackColor = Color.FromArgb(241, 245, 249);
            txtHost.ForeColor = Color.FromArgb(100, 116, 139);
            configPanel.Controls.Add(txtHost);

            // Baris Input 2: Mode Server vs Offline & URL Server
            rbServerMode = new RadioButton
            {
                Text = "Terhubung Konsol Pengawas (Server)",
                Location = new Point(18, 102),
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
                Text = "Mode Mandiri / Offline (Otorisasi PIN Lokal)",
                Location = new Point(265, 102),
                AutoSize = true,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Regular)
            };
            configPanel.Controls.Add(rbOfflineMode);

            configPanel.Controls.Add(MakeLabel("Endpoint Server:", 18, 136));
            txtServerUrl = MakeTextBox("http://localhost/blockwebanaiinideforquiz/api/index.php", 125, 133, 395);
            configPanel.Controls.Add(txtServerUrl);

            chkDryRun = new CheckBox
            {
                Text = "Mode Uji Coba (Tanpa Blokir)",
                Location = new Point(540, 135),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular)
            };
            configPanel.Controls.Add(chkDryRun);

            this.Controls.Add(configPanel);

            // 3. KARTU TELEMETRI 4-LAYER
            int cardY = 294;
            int cardW = 191;
            lblStatDomains = CreateStatCard("CAKUPAN DOMAIN & API", GuardEngine.BlockedDomains.Count + " Domain", Color.FromArgb(15, 23, 42), 22, cardY, cardW);
            lblStatExtensions = CreateStatCard("KARANTINA EKSTENSI IDE", "0 Ekstensi", Color.FromArgb(15, 23, 42), 225, cardY, cardW);
            lblStatKilled = CreateStatCard("INTERSEPSI PROSES AI", "0 Insiden", Color.FromArgb(15, 23, 42), 428, cardY, cardW);
            lblStatVpn = CreateStatCard("INTEGRITAS JARINGAN", "Terverifikasi", Color.FromArgb(4, 120, 87), 631, cardY, cardW);

            // 4. TOMBOL KONTROL UTAMA
            btnActivateLock = new Button
            {
                Text = "Aktifkan Proteksi Ujian",
                Location = new Point(22, 384),
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
                Text = "Buka Kunci (PIN Pengawas)",
                Location = new Point(410, 384),
                Size = new Size(245, 42),
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

            btnEmergencyRestore = new Button
            {
                Text = "Pulihkan Standar",
                Location = new Point(667, 384),
                Size = new Size(155, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnEmergencyRestore.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEmergencyRestore.Click += BtnEmergencyRestore_Click;
            this.Controls.Add(btnEmergencyRestore);

            // 5. LOG AKTIVITAS REAL-TIME
            Label lblLogHeader = new Label
            {
                Text = "JURNAL AUDIT ENDPOINT LOKAL",
                UseMnemonic = false,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(22, 442),
                AutoSize = true
            };
            this.Controls.Add(lblLogHeader);

            lstLogs = new ListBox
            {
                Location = new Point(22, 464),
                Size = new Size(800, 132),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.8f),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lstLogs);

            // Timers
            guardTimer = new Timer { Interval = 2500 };
            guardTimer.Tick += GuardTimer_Tick;

            heartbeatTimer = new Timer { Interval = 4000 };
            heartbeatTimer.Tick += HeartbeatTimer_Tick;
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
            AddLog("Agen Guard siap. Hak akses Administrator: " + (GuardEngine.IsAdministrator() ? "AKTIF" : "TIDAK AKTIF"));
            if (!GuardEngine.IsAdministrator())
            {
                AddLog("PERINGATAN: Jalankan aplikasi sebagai Administrator agar penguncian Hosts & Registry DoH berlaku penuh.");
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
        // AKTIVASI & PENONAKTIFAN KUNCI UJIAN
        // ====================================================================
        private async void BtnActivateLock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNim.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("NIM dan Nama Lengkap wajib diisi sebelum mengaktifkan proteksi ujian.", "Validasi Identitas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isDryRun = chkDryRun.Checked;
            btnActivateLock.Enabled = false;

            if (isServerMode)
            {
                AddLog("Menghubungi Konsol Pengawas (" + txtServerUrl.Text.Trim() + ")...");
                bool joined = await JoinServerAsync();
                if (!joined)
                {
                    AddLog("Koneksi ke server gagal. Anda dapat memilih 'Mode Mandiri / Offline' apabila ujian dilaksanakan tanpa server.");
                    btnActivateLock.Enabled = true;
                    return;
                }
            }

            ActivateProtectionState();
        }

        private void ActivateProtectionState()
        {
            isLocked = true;

            // Update UI Banner ke Emerald (Terkunci & Terverifikasi)
            headerBanner.BackColor = isDryRun ? Color.FromArgb(30, 58, 138) : Color.FromArgb(6, 95, 70);
            lblBannerTitle.Text = isDryRun
                ? "STATUS ENDPOINT: MODE SIMULASI AKTIF (PEMANTAUAN TANPA BLOKIR)"
                : "STATUS ENDPOINT: PROTEKSI UJIAN AKTIF — AKSES AI DIBLOKIR";
            lblBannerSub.Text = string.Format("Peserta: {0} ({1}) · Sesi: {2} · Penutupan aplikasi dinonaktifkan selama ujian.",
                txtName.Text.Trim(), txtNim.Text.Trim(), txtRoomCode.Text.Trim());

            txtNim.Enabled = false;
            txtName.Enabled = false;
            txtRoomCode.Enabled = false;
            txtServerUrl.Enabled = false;
            rbServerMode.Enabled = false;
            rbOfflineMode.Enabled = false;
            chkDryRun.Enabled = false;
            btnActivateLock.Enabled = false;
            btnEmergencyRestore.Enabled = false;
            btnUnlockWithPin.Enabled = true;

            // Layer 1: Hosts & DNS
            string hostsMsg;
            GuardEngine.ApplyHostsBlock(isDryRun, out hostsMsg);
            AddLog(hostsMsg);
            lblStatDomains.Text = GuardEngine.BlockedDomains.Count + " Domain";

            // Layer 2: Browser DoH & Built-in AI Policy
            GuardEngine.ApplyBrowserPolicies(isDryRun);
            AddLog(isDryRun
                ? "[SIMULASI] Kebijakan penonaktifan DNS-over-HTTPS (DoH) Chrome/Edge/Brave disiapkan."
                : "Layer 2 Aktif: Kebijakan DNS-over-HTTPS (DoH) & AI Browser dinonaktifkan.");

            // Layer 3: Karantina Ekstensi VS Code
            quarantinedExtensions = GuardEngine.ScanAndQuarantineExtensions(isDryRun);
            lblStatExtensions.Text = quarantinedExtensions.Count + " Ekstensi";
            if (quarantinedExtensions.Count > 0)
            {
                string extMsg = (isDryRun ? "[SIMULASI] Terdeteksi " : "Layer 3 Aktif: Mengkarantina ") +
                    quarantinedExtensions.Count + " ekstensi AI IDE: " + string.Join(", ", quarantinedExtensions);
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

            // Layer 4: Mulai Timer Watcher Proses & Heartbeat
            guardTimer.Start();
            if (isServerMode) heartbeatTimer.Start();
            RunGuardCycle();
        }

        private void DeactivateProtectionState(string reason)
        {
            isLocked = false;
            guardTimer.Stop();
            heartbeatTimer.Stop();

            GuardEngine.RemoveHostsBlock();
            GuardEngine.RemoveBrowserPolicies();
            int restoredCount = GuardEngine.RestoreQuarantinedExtensions();

            headerBanner.BackColor = Color.FromArgb(15, 23, 42);
            lblBannerTitle.Text = "STATUS ENDPOINT: SESI SELESAI — KONFIGURASI PC DIPULIHKAN";
            lblBannerSub.Text = reason + string.Format(" ({0} ekstensi IDE telah dikembalikan).", restoredCount);

            txtNim.Enabled = true;
            txtName.Enabled = true;
            txtRoomCode.Enabled = true;
            txtServerUrl.Enabled = isServerMode;
            rbServerMode.Enabled = true;
            rbOfflineMode.Enabled = true;
            chkDryRun.Enabled = true;
            btnActivateLock.Enabled = true;
            btnEmergencyRestore.Enabled = true;
            btnUnlockWithPin.Enabled = false;

            lblStatExtensions.Text = "0 Ekstensi";
            AddLog("SESI BERAKHIR: " + reason + ". File hosts, kebijakan DNS, dan " + restoredCount + " ekstensi IDE telah dipulihkan.");
        }

        // ====================================================================
        // TIMER SIKLUS PENGAWASAN LOKAL (SETIAP 2.5 DETIK)
        // ====================================================================
        private void GuardTimer_Tick(object sender, EventArgs e)
        {
            if (!isLocked) return;
            RunGuardCycle();
        }

        private void RunGuardCycle()
        {
            // 1. Cek integritas file hosts
            if (!isDryRun && !GuardEngine.IsHostsBlockIntact())
            {
                string msg;
                GuardEngine.ApplyHostsBlock(false, out msg);
                string alert = "Terdeteksi perubahan pada file hosts. Aturan pemblokiran diterapkan kembali.";
                AddLog("PERINGATAN: " + alert);
                if (isServerMode) { var u1 = SendViolationAsync("HOSTS_TAMPER", alert, "DANGER"); }
            }

            // 2. Cek & matikan proses AI IDE / Local LLM
            List<string> killed = GuardEngine.ScanAndKillAiProcesses(isDryRun);
            if (killed.Count > 0)
            {
                killedCounter += killed.Count;
                lblStatKilled.Text = killedCounter + " Insiden";
                lblStatKilled.ForeColor = Color.FromArgb(185, 28, 28);
                string actionWord = isDryRun ? "[SIMULASI] Terdeteksi proses AI aktif: " : "Menghentikan proses/port AI terlarang: ";
                string fullMsg = actionWord + string.Join(", ", killed);
                AddLog("INTERSEPSI: " + fullMsg);
                if (isServerMode) { var u2 = SendViolationAsync("PROCESS_KILLED", fullMsg, "DANGER"); }
            }

            // 3. Cek ekstensi baru yang mungkin diinstal saat ujian
            List<string> newlyQuarantined = GuardEngine.ScanAndQuarantineExtensions(isDryRun);
            if (newlyQuarantined.Count != quarantinedExtensions.Count)
            {
                quarantinedExtensions = newlyQuarantined;
                lblStatExtensions.Text = quarantinedExtensions.Count + " Ekstensi";
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
                lblStatVpn.Text = "Terverifikasi";
                lblStatVpn.ForeColor = Color.FromArgb(4, 120, 87);
            }
        }

        // ====================================================================
        // KOMUNIKASI DENGAN SERVER DASHBOARD DOSEN
        // ====================================================================
        private async Task<bool> JoinServerAsync()
        {
            try
            {
                var payload = new Dictionary<string, object>
                {
                    { "action", "join" },
                    { "nim", txtNim.Text.Trim() },
                    { "name", txtName.Text.Trim() },
                    { "hostname", Environment.MachineName },
                    { "room_code", txtRoomCode.Text.Trim() },
                    { "os_info", Environment.OSVersion.VersionString },
                    { "version", "1.0.0" }
                };

                var response = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (response == null || !response.ContainsKey("ok") || !(bool)response["ok"])
                {
                    string err = response != null && response.ContainsKey("error") ? Convert.ToString(response["error"]) : "Koneksi ke server gagal.";
                    MessageBox.Show(err, "Gagal Terhubung ke Ruangan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Sinkronkan daftar blokir dari Server jika ada
                if (response.ContainsKey("blocklist"))
                {
                    var bl = response["blocklist"] as Dictionary<string, object>;
                    if (bl != null)
                    {
                        UpdateLocalBlocklistFromServer(bl);
                    }
                }

                AddLog("Berhasil terhubung ke Server Ruangan: " + txtRoomCode.Text.Trim());
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak dapat menghubungi Server Dashboard:\n" + ex.Message, "Koneksi Server", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private async void HeartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (!isServerMode) return;
            try
            {
                string vpnName;
                bool vpnActive = GuardEngine.DetectActiveVpn(out vpnName);

                var payload = new Dictionary<string, object>
                {
                    { "action", "heartbeat" },
                    { "nim", txtNim.Text.Trim() },
                    { "name", txtName.Text.Trim() },
                    { "hostname", Environment.MachineName },
                    { "status", isDryRun ? "DRY_RUN" : (isLocked ? "LOCKED" : "UNLOCKED") },
                    { "quarantined_count", quarantinedExtensions.Count },
                    { "quarantined_list", quarantinedExtensions },
                    { "killed_processes_count", killedCounter },
                    { "vpn_detected", vpnActive }
                };

                var res = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (res != null && res.ContainsKey("ok") && (bool)res["ok"])
                {
                    string target = res.ContainsKey("target_status") ? Convert.ToString(res["target_status"]) : "LOCKED";
                    if (target.Equals("UNLOCKED", StringComparison.OrdinalIgnoreCase) && isLocked)
                    {
                        DeactivateProtectionState("Dibuka dari jarak jauh oleh Dosen melalui Dashboard Pengawas");
                    }
                    else if (target.Equals("LOCKED", StringComparison.OrdinalIgnoreCase) && !isLocked)
                    {
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
                    if (doms != null && doms.Count > 0)
                    {
                        GuardEngine.BlockedDomains = doms.Cast<object>().Select(x => Convert.ToString(x)).ToList();
                    }
                }
                if (bl.ContainsKey("processes"))
                {
                    var procs = bl["processes"] as System.Collections.ArrayList;
                    if (procs != null && procs.Count > 0)
                    {
                        GuardEngine.BlockedProcesses = procs.Cast<object>().Select(x => Convert.ToString(x)).ToList();
                    }
                }
                if (bl.ContainsKey("ide_extensions"))
                {
                    var exts = bl["ide_extensions"] as System.Collections.ArrayList;
                    if (exts != null && exts.Count > 0)
                    {
                        GuardEngine.BlockedExtensionPatterns = exts.Cast<object>().Select(x => Convert.ToString(x)).ToList();
                    }
                }
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
        // BUKA KUNCI DENGAN PIN DOSEN & PEMULIHAN
        // ====================================================================
        private async void BtnUnlockWithPin_Click(object sender, EventArgs e)
        {
            string inputPin = PromptPinDialog();
            if (string.IsNullOrEmpty(inputPin)) return;

            if (isServerMode)
            {
                var payload = new Dictionary<string, object>
                {
                    { "action", "verify_pin" },
                    { "pin", inputPin },
                    { "nim", txtNim.Text.Trim() },
                    { "name", txtName.Text.Trim() },
                    { "hostname", Environment.MachineName }
                };
                var res = await PostJsonAsync(txtServerUrl.Text.Trim(), payload);
                if (res != null && res.ContainsKey("unlocked") && (bool)res["unlocked"])
                {
                    DeactivateProtectionState("Kunci dibuka menggunakan PIN Dosen yang terverifikasi Server");
                    return;
                }
            }

            // Fallback ke PIN Offline default jika server tidak terjangkau atau Mode Offline
            if (inputPin.Trim() == offlinePin)
            {
                DeactivateProtectionState("Kunci dibuka menggunakan PIN Pengawas Lokal");
            }
            else
            {
                AddLog("PERINGATAN: Percobaan membuka kunci gagal (PIN Pengawas salah)!");
                MessageBox.Show("PIN Dosen/Pengawas yang dimasukkan salah!", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEmergencyRestore_Click(object sender, EventArgs e)
        {
            if (isLocked)
            {
                MessageBox.Show("Tidak dapat memulihkan tanpa PIN saat Mode Ujian sedang Terkunci!", "Terkunci", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            GuardEngine.EmergencyRestoreAll();
            AddLog("Pemulihan bersih selesai: File hosts, Browser DoH, dan ekstensi IDE telah dipastikan normal.");
            MessageBox.Show("Sistem PC Anda telah dipastikan bersih dan kembali ke pengaturan normal.", "Pemulihan Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string PromptPinDialog()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 380;
                prompt.Height = 190;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Masukkan PIN Dosen / Pengawas";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;
                prompt.BackColor = Color.FromArgb(15, 23, 42);
                prompt.ForeColor = Color.White;

                Label lbl = new Label { Left = 20, Top = 20, Width = 320, Text = "Masukkan PIN Pengawas untuk membuka blokir AI:" };
                TextBox txt = new TextBox { Left = 20, Top = 50, Width = 320, PasswordChar = '●', Font = new Font("Consolas", 12f) };
                Button btnOk = new Button
                {
                    Text = "Verifikasi & Buka Kunci",
                    Left = 170,
                    Width = 170,
                    Top = 95,
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
            if (isLocked)
            {
                e.Cancel = true;
                MessageBox.Show(
                    "Aplikasi AI-Blocker Guard tidak dapat ditutup selama sesi kuis/ujian masih TERKUNCI!\n\nMinta Dosen/Pengawas untuk membuka kunci dari Dashboard atau masukkan PIN Pengawas.",
                    "Proteksi Ujian Aktif",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop
                );
            }
            else
            {
                // Pastikan semua aturan dibersihkan saat keluar dalam keadaan tidak terkunci
                GuardEngine.EmergencyRestoreAll();
            }
        }
    }
}
