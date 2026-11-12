using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AIBlockerInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }

    public class SetupForm : Form
    {
        private TextBox txtInstallDir;
        private CheckBox chkDesktopShortcut;
        private CheckBox chkAutoStart;
        private CheckBox chkLaunchAfter;
        private Button btnInstall;
        private Label lblStatus;

        public SetupForm()
        {
            this.Text = "GuardAgent — Setup Instalasi Perangkat (Aktif Terus & Dikendalikan Admin)";
            this.Size = new Size(520, 370);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.ForeColor = Color.FromArgb(15, 23, 42);
            this.Font = new Font("Segoe UI", 9.5f);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(15, 23, 42)
            };
            Label title = new Label
            {
                Text = "Instalasi GuardAgent v1.5.2 (Always-On)",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 13),
                AutoSize = true
            };
            Label sub = new Label
            {
                Text = "Agen Sistem Terpusat — Aktif & Nonaktif Dikendalikan Penuh dari Dashboard Admin",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(20, 39),
                AutoSize = true
            };
            header.Controls.Add(title);
            header.Controls.Add(sub);
            this.Controls.Add(header);

            Label lblDir = new Label
            {
                Text = "Direktori Instalasi:",
                Location = new Point(22, 85),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            this.Controls.Add(lblDir);

            txtInstallDir = new TextBox
            {
                Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AIBlockerGuard"),
                Location = new Point(22, 108),
                Width = 460,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(15, 23, 42),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9f)
            };
            this.Controls.Add(txtInstallDir);

            chkAutoStart = new CheckBox
            {
                Text = "Aktif terus di sistem (Jalankan otomatis saat Windows menyala)",
                Location = new Point(22, 144),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(chkAutoStart);

            chkDesktopShortcut = new CheckBox
            {
                Text = "Buat pintasan aplikasi pada layar Desktop",
                Location = new Point(22, 172),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            this.Controls.Add(chkDesktopShortcut);

            chkLaunchAfter = new CheckBox
            {
                Text = "Jalankan agen langsung setelah instalasi selesai",
                Location = new Point(22, 200),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            this.Controls.Add(chkLaunchAfter);

            lblStatus = new Label
            {
                Text = "Siap memasang komponen agen pada perangkat ini.",
                Location = new Point(22, 236),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblStatus);

            btnInstall = new Button
            {
                Text = "Pasang Agen",
                Location = new Point(322, 268),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;
            this.Controls.Add(btnInstall);
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                btnInstall.Enabled = false;
                string targetDir = txtInstallDir.Text.Trim();
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                string targetExe = Path.Combine(targetDir, "AIBlockerGuard.exe");

                // Hentikan proses AIBlockerGuard yang sedang berjalan agar executable dapat diperbarui tanpa konflik "Akses ditolak"
                try
                {
                    Process[] running = Process.GetProcessesByName("AIBlockerGuard");
                    foreach (Process p in running)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit(3000);
                        }
                        catch { }
                    }
                }
                catch { }

                Assembly asm = Assembly.GetExecutingAssembly();
                using (Stream resStream = asm.GetManifestResourceStream("AIBlockerGuard.exe"))
                {
                    if (resStream == null)
                    {
                        MessageBox.Show("Paket resource AIBlockerGuard.exe tidak ditemukan.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnInstall.Enabled = true;
                        return;
                    }
                    using (FileStream fs = new FileStream(targetExe, FileMode.Create, FileAccess.Write))
                    {
                        resStream.CopyTo(fs);
                    }
                }

                string restoreBat = Path.Combine(targetDir, "Restore-Normal.bat");
                File.WriteAllText(restoreBat, "@echo off\r\n\"" + targetExe + "\" --restore\r\n");

                if (chkAutoStart.Checked)
                {
                    ConfigureAutoStart(targetExe);
                }

                if (chkDesktopShortcut.Checked)
                {
                    CreateDesktopShortcut(targetExe);
                }

                lblStatus.Text = "Instalasi selesai pada: " + targetDir;
                MessageBox.Show("GuardAgent berhasil dipasang dan siap dikendalikan dari Dashboard Admin.", "Instalasi Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (chkLaunchAfter.Checked)
                {
                    Process.Start(new ProcessStartInfo(targetExe) { UseShellExecute = true });
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal melakukan instalasi (Pastikan dijalankan sebagai Administrator):\n" + ex.Message, "Instalasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnInstall.Enabled = true;
            }
        }

        private void CreateDesktopShortcut(string targetExe)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string vbsPath = Path.Combine(Path.GetTempPath(), "create_aiblocker_shortcut.vbs");
                string lnkPath = Path.Combine(desktop, "GuardAgent.lnk");
                string vbs = string.Format(
                    "Set oWS = WScript.CreateObject(\"WScript.Shell\")\r\n" +
                    "Set oLink = oWS.CreateShortcut(\"{0}\")\r\n" +
                    "oLink.TargetPath = \"{1}\"\r\n" +
                    "oLink.WorkingDirectory = \"{2}\"\r\n" +
                    "oLink.Description = \"GuardAgent — Sistem Proteksi Endpoint Ujian\"\r\n" +
                    "oLink.Save\r\n",
                    lnkPath, targetExe, Path.GetDirectoryName(targetExe)
                );
                File.WriteAllText(vbsPath, vbs);
                Process p = Process.Start(new ProcessStartInfo("wscript.exe", "\"" + vbsPath + "\"") { CreateNoWindow = true, UseShellExecute = false });
                if (p != null) p.WaitForExit(3000);
                File.Delete(vbsPath);
            }
            catch { }
        }

        private void ConfigureAutoStart(string targetExe)
        {
            try
            {
                string targetDir = Path.GetDirectoryName(targetExe);
                string runVal = "\"" + targetExe + "\" --autostart";

                // 1. Registry Run Key (HKCU & HKLM)
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                    }
                }
                catch { }

                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null) key.SetValue("AIBlockerGuard", runVal, RegistryValueKind.String);
                    }
                }
                catch { }

                // 2. Windows Task Scheduler via XML (ONLOGON untuk semua user dengan hak Administrator / HighestAvailable)
                try
                {
                    string xmlTask =
                        "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n" +
                        "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">\r\n" +
                        "  <RegistrationInfo>\r\n" +
                        "    <Description>GuardAgent - Always-On System Security Agent</Description>\r\n" +
                        "  </RegistrationInfo>\r\n" +
                        "  <Triggers>\r\n" +
                        "    <LogonTrigger>\r\n" +
                        "      <Enabled>true</Enabled>\r\n" +
                        "      <Delay>PT2S</Delay>\r\n" +
                        "    </LogonTrigger>\r\n" +
                        "  </Triggers>\r\n" +
                        "  <Principals>\r\n" +
                        "    <Principal id=\"Author\">\r\n" +
                        "      <GroupId>S-1-5-32-544</GroupId>\r\n" +
                        "      <RunLevel>HighestAvailable</RunLevel>\r\n" +
                        "    </Principal>\r\n" +
                        "  </Principals>\r\n" +
                        "  <Settings>\r\n" +
                        "    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>\r\n" +
                        "    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>\r\n" +
                        "    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>\r\n" +
                        "    <AllowHardTerminate>true</AllowHardTerminate>\r\n" +
                        "    <StartWhenAvailable>true</StartWhenAvailable>\r\n" +
                        "    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>\r\n" +
                        "    <IdleSettings>\r\n" +
                        "      <StopOnIdleEnd>false</StopOnIdleEnd>\r\n" +
                        "      <RestartOnIdle>false</RestartOnIdle>\r\n" +
                        "    </IdleSettings>\r\n" +
                        "    <AllowStartOnDemand>true</AllowStartOnDemand>\r\n" +
                        "    <Enabled>true</Enabled>\r\n" +
                        "    <Hidden>false</Hidden>\r\n" +
                        "    <RunOnlyIfIdle>false</RunOnlyIfIdle>\r\n" +
                        "    <WakeToRun>false</WakeToRun>\r\n" +
                        "    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>\r\n" +
                        "    <Priority>4</Priority>\r\n" +
                        "  </Settings>\r\n" +
                        "  <Actions Context=\"Author\">\r\n" +
                        "    <Exec>\r\n" +
                        "      <Command>" + targetExe.Replace("&", "&amp;") + "</Command>\r\n" +
                        "      <Arguments>--autostart</Arguments>\r\n" +
                        "      <WorkingDirectory>" + targetDir.Replace("&", "&amp;") + "</WorkingDirectory>\r\n" +
                        "    </Exec>\r\n" +
                        "  </Actions>\r\n" +
                        "</Task>";

                    string tempXml = Path.Combine(Path.GetTempPath(), "guardagent_setup_task.xml");
                    File.WriteAllText(tempXml, xmlTask, Encoding.Unicode);

                    ProcessStartInfo psiXml = new ProcessStartInfo("schtasks.exe", "/Create /XML \"" + tempXml + "\" /TN \"AIBlockerGuardAgent\" /F")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    using (Process p = Process.Start(psiXml))
                    {
                        if (p != null) p.WaitForExit(3000);
                    }
                    try { File.Delete(tempXml); } catch { }
                }
                catch { }

                // 3. Folder Startup Windows (CommonStartup & User Startup) via VBS background launcher
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
                        string vbsCode =
                            "On Error Resume Next\r\n" +
                            "Set WshShell = CreateObject(\"WScript.Shell\")\r\n" +
                            "ret = WshShell.Run(\"schtasks /Run /TN \"\"AIBlockerGuardAgent\"\"\", 0, True)\r\n" +
                            "If ret <> 0 Then\r\n" +
                            "    WshShell.Run \"\"\"" + targetExe.Replace("\"", "\"\"") + "\"\" --autostart\", 0, False\r\n" +
                            "End If\r\n";

                        File.WriteAllText(vbsPath, vbsCode, Encoding.ASCII);
                    }
                }
                catch { }
            }
            catch { }
        }
    }
}
