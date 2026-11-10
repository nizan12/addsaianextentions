# GuardConsole & GuardAgent — Sistem Pemblokir AI (Web, IDE & Local LLM) untuk Ujian Mahasiswa

Sistem **GuardConsole & GuardAgent (AI-Blocker Guard)** dirancang untuk diinstal pada PC/Laptop mahasiswa selama sesi kuis atau ujian pemrograman berlangsung. Software ini menutup akses ke seluruh jenis AI baik berbasis **Web**, **Ekstensi IDE (VS Code / JetBrains)**, **AI-Native IDE**, maupun **Local/Offline LLM** tanpa merusak konfigurasi laptop mahasiswa setelah ujian selesai.

---

## 📥 Link Unduh (Download) Aplikasi PC Mahasiswa

Pilih paket yang sesuai untuk dibagikan dan diinstal di PC/Laptop mahasiswa:

| Paket File | Fungsi & Peruntukan | Link Unduh (Repo / Lokal) | Link Unduh via Web Server (Laragon) |
| :--- | :--- | :--- | :--- |
| **1. Installer Setup (`AIBlockerSetup.exe`)** | **(Direkomendasikan)** Memasang aplikasi ke `C:\Program Files\AIBlockerGuard` dan membuat shortcut di Desktop mahasiswa. | [Unduh AIBlockerSetup.exe](./dist/AIBlockerSetup.exe) <br>*(Lokal: [Buka File](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerSetup.exe))* | [http://localhost/blockwebanaiinideforquiz/dist/AIBlockerSetup.exe](http://localhost/blockwebanaiinideforquiz/dist/AIBlockerSetup.exe) |
| **2. Versi Portable (`AIBlockerGuard.exe`)** | **(Tanpa Instalasi)** 1 file `.exe` langsung jalan (*Run as Administrator*). Cocok untuk PC Laboratorium Kampus. | [Unduh AIBlockerGuard.exe](./dist/AIBlockerGuard.exe) <br>*(Lokal: [Buka File](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerGuard.exe))* | [http://localhost/blockwebanaiinideforquiz/dist/AIBlockerGuard.exe](http://localhost/blockwebanaiinideforquiz/dist/AIBlockerGuard.exe) |
| **3. Pemulihan Darurat (`Emergency-Restore.bat`)** | Skrip pemulihan cepat untuk mengembalikan `hosts`, kebijakan DNS browser, dan ekstensi VS Code jika PC mati mendadak saat ujian. | [Unduh Emergency-Restore.bat](./dist/Emergency-Restore.bat) <br>*(Lokal: [Buka File](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/Emergency-Restore.bat))* | [http://localhost/blockwebanaiinideforquiz/dist/Emergency-Restore.bat](http://localhost/blockwebanaiinideforquiz/dist/Emergency-Restore.bat) |

> **Akses Konsol Pengawas (Khusus Dosen):** [http://localhost/blockwebanaiinideforquiz/](http://localhost/blockwebanaiinideforquiz/)

---

## 📂 Struktur File Proyek

- [index.html](file:///c:/laragon/www/blockwebanaiinideforquiz/index.html) & [index.php](file:///c:/laragon/www/blockwebanaiinideforquiz/index.php) — **Web Dashboard Dosen / Pengawas (GuardConsole)** untuk memantau PC mahasiswa secara *real-time*, mengunci/membuka kunci seluruh kelas, dan mengelola daftar blokir AI (mendukung Vercel Cloud & Laragon Lokal).
- [api/server.js](file:///c:/laragon/www/blockwebanaiinideforquiz/api/server.js) & [api/index.php](file:///c:/laragon/www/blockwebanaiinideforquiz/api/index.php) — **REST API Server** (Serverless Node.js untuk Vercel & PHP untuk Laragon) untuk komunikasi antara aplikasi Desktop PC Mahasiswa dengan Dashboard Pengawas.
- [vercel.json](file:///c:/laragon/www/blockwebanaiinideforquiz/vercel.json) & [package.json](file:///c:/laragon/www/blockwebanaiinideforquiz/package.json) — Konfigurasi routing otomatis untuk deployment **Vercel Serverless**.
- [includes/Storage.php](file:///c:/laragon/www/blockwebanaiinideforquiz/includes/Storage.php) — Mesin penyimpanan data otomatis (`storage/database.json`) beserta daftar blokir AI bawaan.
- [client-src/Program.cs](file:///c:/laragon/www/blockwebanaiinideforquiz/client-src/Program.cs) — Source code C# aplikasi Desktop Client (`AIBlockerGuard.exe`).
- [client-src/Installer.cs](file:///c:/laragon/www/blockwebanaiinideforquiz/client-src/Installer.cs) — Source code C# aplikasi Setup Installer (`AIBlockerSetup.exe`).
- [build.ps1](file:///c:/laragon/www/blockwebanaiinideforquiz/build.ps1) — Skrip kompilasi otomatis menghasilkan file `.exe` siap pakai.
- **Paket Distribusi Siap Pakai (`dist/`)**:
  - [dist/AIBlockerSetup.exe](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerSetup.exe) — Installer Setup untuk PC Mahasiswa (`~58 KB`).
  - [dist/AIBlockerGuard.exe](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/AIBlockerGuard.exe) — Aplikasi Portable langsung jalan (`~48 KB`).
  - [dist/Emergency-Restore.bat](file:///c:/laragon/www/blockwebanaiinideforquiz/dist/Emergency-Restore.bat) — Skrip pemulihan darurat.

---

## 🛡️ 4 Lapisan Proteksi (4-Layer Blocking)

1. **Layer 1 — Network, Hosts & DNS Guard**:
   - Memblokir 100 domain Web AI (`chatgpt.com`, `claude.ai`, `gemini.google.com`, `deepseek.com`, `perplexity.ai`, `blackbox.ai`, `v0.dev`, `bolt.new`, dll) serta endpoint API IDE (`api.githubcopilot.com`, `api2.cursor.sh`, `codeium.com`, `tabnine.com`, `openrouter.ai`, dll).
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

## ☁️ Cara Deploy Online ke Vercel (Gratis)

Proyek ini sudah dilengkapi backend **Node.js Serverless** ([api/server.js](file:///c:/laragon/www/blockwebanaiinideforquiz/api/server.js)) dan konfigurasi [vercel.json](file:///c:/laragon/www/blockwebanaiinideforquiz/vercel.json) sehingga dapat langsung di-upload ke **Vercel** tanpa mengubah aplikasi `.exe` mahasiswa:

### Cara 1: Via GitHub & Dashboard Vercel
1. Push folder proyek ini ke repositori GitHub Anda.
2. Buka [vercel.com/new](https://vercel.com/new) dan **Import** repositori tersebut.
3. Klik **Deploy** (tidak perlu pengaturan *build command* tambahan).
4. Setelah online (misal `https://ujian-ai-kampus.vercel.app`), buka dashboard tersebut, masuk ke menu **Distribusi Agen (.exe)**, dan salin **URL Endpoint Server** (`https://ujian-ai-kampus.vercel.app/api/index.php`) untuk dimasukkan pada aplikasi `AIBlockerGuard.exe` di PC Mahasiswa.

### Cara 2: Langsung dari Terminal (Vercel CLI)
Jalankan perintah berikut di terminal folder proyek:
```powershell
npx vercel
```

> **Catatan Penyimpanan Vercel:** Secara *default*, [api/server.js](file:///c:/laragon/www/blockwebanaiinideforquiz/api/server.js) menggunakan penyimpanan *in-memory* + `/tmp/aiblocker_database.json` yang langsung berjalan tanpa konfigurasi apa pun selama sesi ujian aktif. Jika Anda menginginkan penyimpanan permanen lintas *cold-start*, Anda cukup menghubungkan **Upstash Redis / Vercel KV** gratis di tab **Storage** Vercel (otomatis terdeteksi lewat variabel `KV_REST_API_URL` dan `KV_REST_API_TOKEN`).

---

## 🚀 Cara Penggunaan

### 1. Membuka Dashboard Dosen / Pengawas
- **Lokal (Laragon)**: [http://localhost/blockwebanaiinideforquiz/](http://localhost/blockwebanaiinideforquiz/)
- **Online (Vercel)**: `https://nama-project-anda.vercel.app`
- **Kode Ruangan Default**: `KUIS-AI-01`
- **PIN Otorisasi Pengawas Default**: `202699`

### 2. Menginstal / Menjalankan di PC Mahasiswa
1. Unduh [AIBlockerSetup.exe](./dist/AIBlockerSetup.exe) atau [AIBlockerGuard.exe](./dist/AIBlockerGuard.exe) (bisa juga diunduh langsung dari halaman Vercel Anda).
2. Jalankan sebagai **Administrator**.
3. Masukkan **NIM**, **Nama Lengkap**, dan pilih:
   - **Terhubung Konsol Pengawas (Server)**: isi **URL Endpoint Server** dengan URL Vercel Anda (`https://nama-project-anda.vercel.app/api/index.php`) atau IP Laragon lokal.
   - **Mode Mandiri / Offline (Otorisasi PIN Lokal)**: jika tidak menggunakan server terpusat (dikunci secara lokal dan hanya dapat dibuka dengan **PIN Dosen `202699`**).
4. Klik **"Aktifkan Proteksi Ujian"**.
5. Setelah kuis selesai, Dosen dapat menekan **"Buka Kunci & Pulihkan"** dari Web Dashboard atau memberikan PIN `202699` kepada mahasiswa.

### 3. Kompilasi Ulang (Jika Mengubah Kode C#)
Jalankan perintah berikut di PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```
