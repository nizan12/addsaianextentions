# AI-Blocker Guard — Sistem Pemblokir AI (Web, IDE & Local LLM) untuk Ujian Mahasiswa

Sistem **AI-Blocker Guard** dirancang untuk diinstal pada PC/Laptop mahasiswa selama sesi kuis atau ujian pemrograman berlangsung. Software ini menutup akses ke seluruh jenis AI baik berbasis **Web**, **Ekstensi IDE (VS Code / JetBrains)**, **AI-Native IDE**, maupun **Local/Offline LLM** tanpa merusak konfigurasi laptop mahasiswa setelah ujian selesai.

---

## 📂 Struktur File Proyek

- [index.php](file:///c:/laragon/www/blockwebanaiinideforquiz/index.php) — **Web Dashboard Dosen / Pengawas** untuk memantau PC mahasiswa secara *real-time*, mengunci/membuka kunci seluruh kelas, dan mengelola daftar blokir AI.
- [api/index.php](file:///c:/laragon/www/blockwebanaiinideforquiz/api/index.php) — **REST API Server** untuk komunikasi antara aplikasi Desktop PC Mahasiswa dengan Dashboard Pengawas.
- [includes/Storage.php](file:///c:/laragon/www/blockwebanaiinideforquiz/includes/Storage.php) — Mesin penyimpanan data otomatis (`storage/database.json`) beserta daftar blokir AI bawaan.
- [client-src/Program.cs](file:///c:/laragon/www/blockwebanaiinideforquiz/client-src/Program.cs) — Source code C# aplikasi Desktop Client (`AIBlockerGuard.exe`).
- [client-src/Installer.cs](file:///c:/laragon/www/blockwebanaiinideforquiz/client-src/Installer.cs) — Source code C# aplikasi Setup Installer (`AIBlockerSetup.exe`).
- [build.ps1](file:///c:/laragon/www/blockwebanaiinideforquiz/build.ps1) — Skrip kompilasi otomatis menghasilkan file `.exe` siap pakai.
- **Paket Distribusi Siap Pakai (`dist/`)**:
  - [dist/AIBlockerSetup.exe](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerSetup.exe) — Installer Setup untuk PC Mahasiswa.
  - [dist/AIBlockerGuard.exe](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerGuard.exe) — Aplikasi Portable (1 file langsung jalan tanpa instalasi).
  - [dist/Emergency-Restore.bat](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/Emergency-Restore.bat) — Skrip pemulihan darurat.

---

## 🛡️ 4 Lapisan Proteksi (4-Layer Blocking)

1. **Layer 1 — Network, Hosts & DNS Guard**:
   - Memblokir 75+ domain Web AI (`chatgpt.com`, `claude.ai`, `gemini.google.com`, `deepseek.com`, `perplexity.ai`, `blackbox.ai`, `v0.dev`, `bolt.new`, dll) serta endpoint API IDE (`api.githubcopilot.com`, `api2.cursor.sh`, `codeium.com`, `tabnine.com`, `openrouter.ai`, dll).
   - Dilengkapi pengawas integritas *real-time* (jika mahasiswa mengedit file `hosts`, blokir langsung ditulis ulang otomatis dan dilaporkan ke dashboard).
2. **Layer 2 — Browser DNS-over-HTTPS (DoH) & Built-in AI Disabler**:
   - Menonaktifkan DNS-over-HTTPS (DoH) serta fitur AI bawaan browser (Copilot Sidebar di Edge, Gemini di Chrome, Leo AI di Brave) melalui kebijakan Registry Windows selama ujian.
3. **Layer 3 — IDE Extension Quarantine (VS Code & JetBrains)**:
   - Memindai folder ekstensi VS Code (`~/.vscode/extensions`) dan mengkarantina sementara ekstensi AI (`GitHub Copilot`, `Codeium`, `Cline`, `Roo Code`, `Continue`, `Tabnine`, `Blackbox`, `Supermaven`, `Amazon Q`, `Gemini Code Assist`).
   - Saat ujian selesai dan kunci dibuka, seluruh ekstensi otomatis dikembalikan ke folder semula 100%.
4. **Layer 4 — Process Killer, Local LLM Port Monitor & Anti-VPN**:
   - Menghentikan paksa aplikasi AI-Native IDE (`Cursor.exe`, `Windsurf.exe`, `Trae.exe`) dan Local/Offline LLM (`ollama.exe`, `LM Studio.exe`, `jan.exe`, `gpt4all.exe`, `llama-server.exe`) setiap 2,5 detik.
   - Memantau port lokal LLM (`11434`, `1234`, `1337`, `5001`) serta mendeteksi penggunaan VPN / Cloudflare WARP.

---

## 🚀 Cara Penggunaan

### 1. Membuka Dashboard Dosen / Pengawas
Pastikan Laragon berjalan, lalu buka di browser:
- `http://localhost/blockwebanaiinideforquiz/`
- **Kode Ruangan Default**: `KUIS-AI-01`
- **PIN Dosen / Buka Kunci Default**: `202699`

### 2. Menginstal / Menjalankan di PC Mahasiswa
1. Unduh **`AIBlockerSetup.exe`** atau **`AIBlockerGuard.exe`** dari tab **Unduh Installer** di Dashboard (atau ambil langsung dari folder `dist/`).
2. Jalankan sebagai **Administrator**.
3. Masukkan **NIM**, **Nama**, dan pilih:
   - **Mode Terhubung Server Dosen**: jika berada di jaringan LAN/WiFi kampus yang terhubung ke server Laragon dosen.
   - **Mode Mandiri / Offline**: jika tidak menggunakan server terpusat (dikunci secara lokal dan hanya dapat dibuka dengan **PIN Dosen `202699`**).
4. Klik **"🔒 Aktifkan Proteksi Ujian"**.
5. Setelah kuis selesai, Dosen dapat menekan **"Buka Kunci Semua"** dari Web Dashboard atau memberikan PIN `202699` kepada mahasiswa.

### 3. Kompilasi Ulang (Jika Mengubah Kode C#)
Jalankan perintah berikut di PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```
