using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace AIBlockerService
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args != null && args.Any(a => a.Equals("--console", StringComparison.OrdinalIgnoreCase)))
            {
                GuardSystemService svc = new GuardSystemService();
                svc.StartDebug();
                Console.WriteLine("AIBlockerGuardService berjalan di mode console. Tekan ENTER untuk berhenti...");
                Console.ReadLine();
                svc.StopDebug();
                return;
            }
            ServiceBase.Run(new GuardSystemService());
        }
    }

    public class GuardSystemService : ServiceBase
    {
        public const string PipeName = "AIBlockerGuardPipe";
        public const string ServiceVersion = "1.6.0";

        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "AIBlockerGuard"
        );
        private static readonly string HostsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"drivers\etc\hosts"
        );
        private const string HostsStartMarker = "# >>> AI-BLOCKER-GUARD START >>>";
        private const string HostsEndMarker = "# <<< AI-BLOCKER-GUARD END <<<";

        private volatile bool isRunning = false;
        private Thread pipeThread;

        public GuardSystemService()
        {
            this.ServiceName = "AIBlockerGuardService";
            this.CanStop = true;
            this.CanShutdown = true;
        }

        public void StartDebug() { OnStart(new string[0]); }
        public void StopDebug() { OnStop(); }

        protected override void OnStart(string[] args)
        {
            try { if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir); } catch { }
            isRunning = true;
            pipeThread = new Thread(PipeServerLoop) { IsBackground = true };
            pipeThread.Start();
        }

        protected override void OnStop()
        {
            isRunning = false;
            try
            {
                // Buka koneksi dummy agar WaitForConnection segera keluar
                using (var dummy = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                {
                    dummy.Connect(300);
                }
            }
            catch { }
        }

        protected override void OnShutdown()
        {
            OnStop();
            base.OnShutdown();
        }

        private void PipeServerLoop()
        {
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));

            JavaScriptSerializer ser = new JavaScriptSerializer { MaxJsonLength = 8 * 1024 * 1024 };

            while (isRunning)
            {
                NamedPipeServerStream server = null;
                try
                {
                    server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.InOut,
                        4,
                        PipeTransmissionMode.Byte,
                        PipeOptions.None,
                        65536,
                        65536,
                        ps
                    );

                    server.WaitForConnection();
                    if (!isRunning) break;

                    string reqJson = null;
                    using (StreamReader reader = new StreamReader(server, Encoding.UTF8, false, 4096, true))
                    {
                        reqJson = reader.ReadLine();
                    }

                    Dictionary<string, object> resp = ProcessRequest(reqJson, ser);
                    string respJson = ser.Serialize(resp);

                    using (StreamWriter writer = new StreamWriter(server, Encoding.UTF8, 4096, true))
                    {
                        writer.WriteLine(respJson);
                        writer.Flush();
                    }
                    server.WaitForPipeDrain();
                }
                catch
                {
                    Thread.Sleep(200);
                }
                finally
                {
                    if (server != null)
                    {
                        try { server.Dispose(); } catch { }
                    }
                }
            }
        }

        private Dictionary<string, object> ProcessRequest(string reqJson, JavaScriptSerializer ser)
        {
            if (string.IsNullOrWhiteSpace(reqJson))
            {
                return new Dictionary<string, object> { { "ok", false }, { "error", "Empty request" } };
            }

            try
            {
                Dictionary<string, object> req = ser.Deserialize<Dictionary<string, object>>(reqJson);
                if (req == null || !req.ContainsKey("cmd"))
                {
                    return new Dictionary<string, object> { { "ok", false }, { "error", "Missing cmd" } };
                }

                string cmd = Convert.ToString(req["cmd"]).Trim().ToUpperInvariant();

                if (cmd == "PING")
                {
                    return new Dictionary<string, object>
                    {
                        { "ok", true },
                        { "service", "AIBlockerGuardService" },
                        { "version", ServiceVersion }
                    };
                }

                if (cmd == "APPLY_SYSTEM_LOCK")
                {
                    List<string> domains = ExtractStringList(req, "domains");
                    bool blockDoh = req.ContainsKey("block_doh") && Convert.ToBoolean(req["block_doh"]);
                    string msg;
                    bool hostsOk = ApplyHostsBlock(domains, out msg);
                    if (blockDoh)
                    {
                        ApplyBrowserPolicies(domains);
                    }
                    else
                    {
                        RemoveBrowserPolicies();
                    }
                    return new Dictionary<string, object>
                    {
                        { "ok", hostsOk },
                        { "message", msg }
                    };
                }

                if (cmd == "RESTORE_SYSTEM")
                {
                    RemoveHostsBlock();
                    RemoveBrowserPolicies();
                    return new Dictionary<string, object> { { "ok", true } };
                }

                if (cmd == "CHECK_HOSTS")
                {
                    List<string> domains = ExtractStringList(req, "domains");
                    bool intact = IsHostsBlockIntact(domains);
                    return new Dictionary<string, object> { { "ok", true }, { "intact", intact } };
                }

                if (cmd == "REPLACE_CLIENT_EXE")
                {
                    string tempPath = req.ContainsKey("tempPath") ? Convert.ToString(req["tempPath"]) : "";
                    string targetPath = req.ContainsKey("targetPath") ? Convert.ToString(req["targetPath"]) : "";
                    string serviceDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    // Hanya izinkan mengganti AIBlockerGuard.exe di direktori instalasi yang sama
                    if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(targetPath) ||
                        !File.Exists(tempPath) ||
                        !string.Equals(Path.GetDirectoryName(targetPath), serviceDir, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(Path.GetFileName(targetPath), "AIBlockerGuard.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Dictionary<string, object> { { "ok", false }, { "error", "Invalid update path" } };
                    }

                    byte[] bytes = File.ReadAllBytes(tempPath);
                    if (bytes.Length < 50000 || bytes[0] != 0x4D || bytes[1] != 0x5A)
                    {
                        return new Dictionary<string, object> { { "ok", false }, { "error", "Invalid PE binary" } };
                    }

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            Thread.Sleep(1800);
                            for (int i = 0; i < 10; i++)
                            {
                                try
                                {
                                    File.Copy(tempPath, targetPath, true);
                                    try { File.Delete(tempPath); } catch { }
                                    break;
                                }
                                catch
                                {
                                    Thread.Sleep(700);
                                }
                            }
                        }
                        catch { }
                    });

                    return new Dictionary<string, object> { { "ok", true } };
                }

                return new Dictionary<string, object> { { "ok", false }, { "error", "Unknown command: " + cmd } };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { { "ok", false }, { "error", ex.Message } };
            }
        }

        private static List<string> ExtractStringList(Dictionary<string, object> req, string key)
        {
            List<string> list = new List<string>();
            if (!req.ContainsKey(key) || req[key] == null) return list;
            var arr = req[key] as System.Collections.ArrayList;
            if (arr != null)
            {
                foreach (object item in arr)
                {
                    string s = Convert.ToString(item ?? "").Trim().ToLowerInvariant();
                    if (!string.IsNullOrEmpty(s)) list.Add(s);
                }
            }
            return list;
        }

        private static bool ApplyHostsBlock(List<string> domains, out string statusMessage)
        {
            try
            {
                if (!Directory.Exists(AppDataDir)) Directory.CreateDirectory(AppDataDir);
                string backupPath = Path.Combine(AppDataDir, "hosts.original.bak");
                if (File.Exists(HostsPath) && !File.Exists(backupPath))
                {
                    File.Copy(HostsPath, backupPath, false);
                }

                FileInfo fi = new FileInfo(HostsPath);
                if (fi.Exists && fi.IsReadOnly) fi.IsReadOnly = false;

                string content = File.Exists(HostsPath) ? File.ReadAllText(HostsPath, Encoding.UTF8) : "";
                content = StripHostsBlockContent(content);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(content.TrimEnd());
                sb.AppendLine();
                sb.AppendLine(HostsStartMarker);
                sb.AppendLine("# Dibuat otomatis oleh AI-Blocker Guard Service selama sesi ujian/kuis berlangsung.");
                foreach (string rawDomain in domains.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    string d = (rawDomain ?? "").Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(d) || d.Contains(" ")) continue;
                    sb.AppendLine("0.0.0.0 " + d);
                    sb.AppendLine("::0 " + d);
                }
                sb.AppendLine(HostsEndMarker);

                File.WriteAllText(HostsPath, sb.ToString(), Encoding.UTF8);
                FlushDns();

                statusMessage = string.Format("Layer 1 Aktif (Service): {0} domain diblokir di sistem DNS.", domains.Distinct(StringComparer.OrdinalIgnoreCase).Count());
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = "Service gagal memodifikasi hosts: " + ex.Message;
                return false;
            }
        }

        private static bool IsHostsBlockIntact(List<string> domains)
        {
            try
            {
                if (domains == null || domains.Count == 0) return true;
                if (!File.Exists(HostsPath)) return false;
                string content = File.ReadAllText(HostsPath, Encoding.UTF8);
                int startIdx = content.IndexOf(HostsStartMarker, StringComparison.Ordinal);
                int endIdx = content.IndexOf(HostsEndMarker, StringComparison.Ordinal);
                if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return false;

                string blockSection = content.Substring(startIdx, endIdx - startIdx);
                int expectedCount = domains
                    .Select(d => (d ?? "").Trim().ToLowerInvariant())
                    .Where(d => !string.IsNullOrEmpty(d) && !d.Contains(" "))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                int actualLines = blockSection.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Count(l => l.StartsWith("0.0.0.0 ", StringComparison.Ordinal));
                if (actualLines != expectedCount) return false;

                foreach (string rawDomain in domains.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    string d = (rawDomain ?? "").Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(d) || d.Contains(" ")) continue;
                    if (!blockSection.Contains("0.0.0.0 " + d)) return false;
                }
                return true;
            }
            catch { return false; }
        }

        private static void RemoveHostsBlock()
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
            int endIdx = content.IndexOf(HostsEndMarker, StringComparison.Ordinal);
            if (startIdx >= 0 && endIdx > startIdx)
            {
                string before = content.Substring(0, startIdx);
                string after = content.Substring(endIdx + HostsEndMarker.Length);
                return (before.TrimEnd() + Environment.NewLine + after.TrimStart()).Trim();
            }
            return content;
        }

        private static void FlushDns()
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
                    if (p != null) p.WaitForExit(2000);
                }
            }
            catch { }
        }

        private static void ApplyBrowserPolicies(List<string> domains)
        {
            string[] paths = new string[]
            {
                @"SOFTWARE\Policies\Google\Chrome",
                @"SOFTWARE\Policies\Microsoft\Edge",
                @"SOFTWARE\Policies\BraveSoftware\Brave"
            };
            foreach (string regPath in paths)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(regPath))
                    {
                        if (key != null)
                        {
                            key.SetValue("DnsOverHttpsMode", "off", RegistryValueKind.String);
                            key.SetValue("BuiltInDnsClientEnabled", 0, RegistryValueKind.DWord);
                            key.SetValue("QuicAllowed", 0, RegistryValueKind.DWord);

                            if (regPath.EndsWith("Chrome"))
                            {
                                key.SetValue("GenAiDefaultSettings", 2, RegistryValueKind.DWord);
                            }
                            else if (regPath.EndsWith("Edge"))
                            {
                                key.SetValue("HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                                key.SetValue("CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                            }
                            else if (regPath.EndsWith("Brave"))
                            {
                                key.SetValue("BraveAIChatEnabled", 0, RegistryValueKind.DWord);
                            }

                            key.DeleteSubKeyTree("URLBlocklist", false);
                            if (domains != null && domains.Count > 0)
                            {
                                using (RegistryKey ubKey = key.CreateSubKey("URLBlocklist"))
                                {
                                    if (ubKey != null)
                                    {
                                        int idx = 1;
                                        foreach (string rawDomain in domains)
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
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\VSCode"))
                {
                    if (key != null) key.SetValue("ChatEnabled", 0, RegistryValueKind.DWord);
                }
            }
            catch { }
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Mozilla\Firefox\DNSOverHTTPS"))
                {
                    if (key != null)
                    {
                        key.SetValue("Enabled", 0, RegistryValueKind.DWord);
                        key.SetValue("Locked", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private static void RemoveBrowserPolicies()
        {
            string[] paths = new string[]
            {
                @"SOFTWARE\Policies\Google\Chrome",
                @"SOFTWARE\Policies\Microsoft\Edge",
                @"SOFTWARE\Policies\BraveSoftware\Brave",
                @"SOFTWARE\Policies\Microsoft\VSCode"
            };
            foreach (string regPath in paths)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, true))
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
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Policies\Mozilla\Firefox\DNSOverHTTPS", false);
            }
            catch { }
        }
    }
}
