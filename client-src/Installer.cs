using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
        private CheckBox chkLaunchAfter;
        private Button btnInstall;
        private Label lblStatus;

        public SetupForm()
        {
            this.Text = "GuardAgent — Setup Instalasi Perangkat Ujian";
            this.Size = new Size(520, 340);
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
                Text = "Instalasi GuardAgent v1.2",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 13),
                AutoSize = true
            };
            Label sub = new Label
            {
                Text = "Sistem Proteksi Endpoint Ujian & Integritas Akademik",
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

            chkDesktopShortcut = new CheckBox
            {
                Text = "Buat pintasan aplikasi pada layar Desktop",
                Location = new Point(22, 148),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            this.Controls.Add(chkDesktopShortcut);

            chkLaunchAfter = new CheckBox
            {
                Text = "Jalankan aplikasi setelah proses instalasi selesai",
                Location = new Point(22, 176),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            this.Controls.Add(chkLaunchAfter);

            lblStatus = new Label
            {
                Text = "Siap memasang komponen agen pada perangkat ini.",
                Location = new Point(22, 214),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 116, 139),
                Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblStatus);

            btnInstall = new Button
            {
                Text = "Pasang Agen",
                Location = new Point(322, 244),
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

                if (chkDesktopShortcut.Checked)
                {
                    CreateDesktopShortcut(targetExe);
                }

                lblStatus.Text = "Instalasi selesai pada: " + targetDir;
                MessageBox.Show("GuardAgent berhasil dipasang pada perangkat ini.", "Instalasi Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
    }
}
