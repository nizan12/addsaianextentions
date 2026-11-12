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
                Text = "Instalasi GuardAgent v1.6.0 (Service + Always-On)",
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
                string serviceExe = Path.Combine(targetDir, "AIBlockerService.exe");

                // Hentikan service & proses yang sedang berjalan agar executable dapat diperbarui tanpa konflik "Akses ditolak"
                RunHiddenProcess("sc.exe", "stop AIBlockerGuardService", 4000);
                foreach (string procName in new string[] { "AIBlockerGuard", "AIBlockerService" })
                {
                    try
                    {
                        foreach (Process p in Process.GetProcessesByName(procName))
                        {
                            try { p.Kill(); p.WaitForExit(3000); } catch { }
                        }
                    }
                    catch { }
                }

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

                using (Stream svcStream = asm.GetManifestResourceStream("AIBlockerService.exe"))
                {
                    if (svcStream != null)
                    {
                        using (FileStream fs = new FileStream(serviceExe, FileMode.Create, FileAccess.Write))
                        {
                            svcStream.CopyTo(fs);
                        }
                    }
                }

                // Pastikan folder C:\ProgramData\AIBlockerGuard dapat ditulis oleh user standar (mhs)
                try
                {
                    string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AIBlockerGuard");
                    if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                    RunHiddenProcess("icacls.exe", "\"" + appDataDir + "\" /grant *S-1-5-32-545:(OI)(CI)M /T /Q", 3000);
                }
                catch { }

                // Daftarkan & jalankan Windows Service (berjalan sebagai LocalSystem tanpa pop-up UAC di akun mahasiswa)
                if (File.Exists(serviceExe))
                {
                    RunHiddenProcess("sc.exe", "delete AIBlockerGuardService", 3000);
                    RunHiddenProcess("sc.exe", "create AIBlockerGuardService binPath= \"\\\"" + serviceExe + "\\\"\" start= auto DisplayName= \"AI-Blocker Guard System Service\"", 4000);
                    RunHiddenProcess("sc.exe", "description AIBlockerGuardService \"Layanan sistem untuk proteksi Hosts, DNS DoH, dan keamanan ujian AI-Blocker Guard.\"", 2500);
                    RunHiddenProcess("sc.exe", "failure AIBlockerGuardService reset= 86400 actions= restart/5000/restart/5000/restart/5000", 2500);
                    RunHiddenProcess("sc.exe", "start AIBlockerGuardService", 4000);
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

        private static void RunHiddenProcess(string fileName, string args, int timeoutMs)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p != null) p.WaitForExit(timeoutMs);
                }
            }
            catch { }
        }

        private void ConfigureAutoStart(string targetExe)
        {
            try
            {
                string runVal = "\"" + targetExe + "\" --autostart";

                // 1. Registry Run Key (HKCU & HKLM) — berjalan untuk semua user (termasuk mhs) tanpa pop-up UAC karena asInvoker
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

                // 2. Hapus Task Scheduler lama (jika ada) agar tidak terjadi elevasi ganda
                RunHiddenProcess("schtasks.exe", "/Delete /TN \"AIBlockerGuardAgent\" /F", 2500);

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
                            "WshShell.Run \"\"\"" + targetExe.Replace("\"", "\"\"") + "\"\" --autostart\", 0, False\r\n";

                        File.WriteAllText(vbsPath, vbsCode, Encoding.ASCII);
                    }
                }
                catch { }
            }
            catch { }
        }
    }
}
