<!DOCTYPE html>
<html lang="id">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>GuardConsole — Sistem Pengawasan Ujian & Integritas Akademik</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600&display=swap" rel="stylesheet">
    <script src="https://cdn.tailwindcss.com"></script>
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    fontFamily: {
                        sans: ['Inter', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'sans-serif'],
                        mono: ['JetBrains Mono', 'Consolas', 'monospace']
                    }
                }
            }
        }
    </script>
    <style>
        body {
            font-family: 'Inter', '-apple-system', BlinkMacSystemFont, sans-serif;
            background-color: #F8FAFC;
            color: #0F172A;
            -webkit-font-smoothing: antialiased;
        }
        .tabular-nums { font-variant-numeric: tabular-nums; }
        .card-surface {
            background: #FFFFFF;
            border: 1px solid #E2E8F0;
            box-shadow: 0 1px 2px 0 rgba(15, 23, 42, 0.03);
        }
        .status-dot-live { box-shadow: 0 0 0 3px rgba(22, 163, 74, 0.15); }
        ::-webkit-scrollbar { width: 6px; height: 6px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background: #CBD5E1; border-radius: 3px; }
        ::-webkit-scrollbar-thumb:hover { background: #94A3B8; }
    </style>
</head>
<body class="h-screen overflow-hidden flex bg-[#F8FAFC] text-slate-900">

    <!-- ======================================================================= -->
    <!-- GERBANG OTENTIKASI PENGAWAS / ADMIN                                     -->
    <!-- ======================================================================= -->
    <div id="authGateOverlay" class="fixed inset-0 z-50 bg-[#0F172A] flex items-center justify-center p-4">
        <div class="w-full max-w-md bg-white rounded-2xl border border-slate-200 shadow-2xl overflow-hidden">
            <div class="p-6 bg-slate-900 text-white border-b border-slate-800 flex items-center justify-between">
                <div class="flex items-center gap-3">
                    <div class="w-9 h-9 rounded-lg bg-emerald-600 text-white flex items-center justify-center">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 3l7 4v5c0 5-3.5 8.5-7 9-3.5-.5-7-4-7-9V7l7-4z"/>
                        </svg>
                    </div>
                    <div>
                        <h1 class="text-sm font-semibold tracking-tight">GuardConsole Admin</h1>
                        <p class="text-[11px] text-slate-400">Otentikasi Konsol Pengawas Ujian</p>
                    </div>
                </div>
            </div>

            <form onsubmit="handleAdminLogin(event)" autocomplete="off" class="p-6 space-y-4">
                <div id="loginErrorBox" class="hidden p-3 rounded-lg bg-rose-50 border border-rose-200 text-xs text-rose-700 font-medium"></div>

                <div>
                    <label class="block text-xs font-semibold text-slate-700 mb-1.5">Email Pengawas / ID Admin</label>
                    <input type="text" id="loginEmail" name="guard_auth_user" required autocomplete="off" readonly onfocus="this.removeAttribute('readonly')" value="" placeholder="Masukkan email atau ID pengawas" class="w-full rounded-lg bg-slate-50 border border-slate-300 px-3.5 py-2.5 text-xs font-mono text-slate-900 focus:bg-white focus:border-slate-900 focus:outline-none">
                </div>

                <div>
                    <label class="block text-xs font-semibold text-slate-700 mb-1.5">Kata Sandi Pengawas</label>
                    <input type="password" id="loginPassword" name="guard_auth_secret" required autocomplete="new-password" readonly onfocus="this.removeAttribute('readonly')" value="" placeholder="Masukkan kata sandi pengawas" class="w-full rounded-lg bg-slate-50 border border-slate-300 px-3.5 py-2.5 text-xs font-mono text-slate-900 focus:bg-white focus:border-slate-900 focus:outline-none">
                </div>

                <div class="pt-1">
                    <button type="submit" id="btnLoginSubmit" class="w-full py-2.5 px-4 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-semibold transition flex items-center justify-center gap-2">
                        <span>Masuk ke Konsol Pengawas</span>
                    </button>
                </div>

                <div class="pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-400">
                    <span>Sistem Pengawasan Integritas Akademik</span>
                    <span>v1.5</span>
                </div>
            </form>
        </div>
    </div>

    <!-- Left Navigation Rail (Fixed Full-Height) -->
    <aside class="w-64 h-screen bg-white border-r border-slate-200 flex flex-col justify-between shrink-0 select-none overflow-hidden">
        <div class="flex flex-col flex-1 min-h-0">
            <!-- Brand Header (Exact 64px / h-16 shrink-0) -->
            <div class="h-16 shrink-0 px-5 flex items-center justify-between border-b border-slate-200">
                <div class="flex items-center gap-2.5">
                    <div class="w-8 h-8 rounded-lg bg-slate-900 text-white flex items-center justify-center">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 3l7 4v5c0 5-3.5 8.5-7 9-3.5-.5-7-4-7-9V7l7-4z"/>
                        </svg>
                    </div>
                    <div>
                        <div class="text-sm font-semibold tracking-tight text-slate-900 leading-none">GuardConsole</div>
                        <div class="text-[11px] text-slate-500 mt-1 font-medium">Exam Endpoint Control</div>
                    </div>
                </div>
                <span class="text-[10px] font-mono font-medium px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">v1.5</span>
            </div>

            <!-- Primary Navigation -->
            <div class="flex-1 overflow-y-auto p-3 space-y-1">
                <div class="px-2.5 py-1.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Pengawasan Kelas</div>
                <button onclick="switchTab('monitor')" id="nav-monitor" class="nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium bg-slate-900 text-white transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <rect x="2" y="3" width="20" height="14" rx="2" stroke-linecap="round" stroke-linejoin="round"/>
                            <path d="M8 21h8M12 17v4" stroke-linecap="round" stroke-linejoin="round"/>
                        </svg>
                        <span>Perangkat Mahasiswa</span>
                    </span>
                    <span id="navBadgeOnline" class="px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-white/20 text-white tabular-nums">0</span>
                </button>

                <button onclick="switchTab('logs')" id="nav-logs" class="nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z"/>
                        </svg>
                        <span>Log Audit & Insiden</span>
                    </span>
                    <span id="navBadgeViolations" class="px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-50 text-rose-700 border border-rose-200 tabular-nums">0</span>
                </button>

                <div class="pt-4 pb-1 px-2.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Konfigurasi & Distribusi</div>
                <button onclick="switchTab('rules')" id="nav-rules" class="nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
                        </svg>
                        <span>Kebijakan Blokir Fleksibel</span>
                    </span>
                </button>

                <button onclick="switchTab('settings')" id="nav-settings" class="nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                            <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                        </svg>
                        <span>Ruangan & PIN Pengawas</span>
                    </span>
                </button>

                <button onclick="switchTab('download')" id="nav-download" class="nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                        </svg>
                        <span>Distribusi Agen (.exe)</span>
                    </span>
                </button>
            </div>
        </div>

        <!-- Bottom Session Credentials Box -->
        <div class="shrink-0 p-4 border-t border-slate-200 bg-slate-50/70 space-y-3">
            <div>
                <div class="flex items-center justify-between text-[11px] text-slate-500">
                    <span>Ruangan Dipilih</span>
                    <button onclick="copyText(selectedRoom === 'ALL' ? currentRoomCode : selectedRoom, 'Kode ruangan disalin')" class="text-slate-400 hover:text-slate-700 font-medium">Salin</button>
                </div>
                <div id="sidebarRoomCode" class="font-mono text-sm font-semibold text-slate-900 mt-0.5">SEMUA RUANGAN</div>
            </div>

            <div class="pt-2 border-t border-slate-200/80">
                <div class="flex items-center justify-between text-[11px] text-slate-500">
                    <span id="sidebarPinLabel">PIN Master Pengawas</span>
                    <div class="flex items-center gap-2">
                        <button onclick="copyActiveRoomPin()" class="text-slate-400 hover:text-slate-700 font-medium">Salin</button>
                        <button onclick="togglePinVisibility()" id="btnTogglePin" class="text-slate-600 hover:text-slate-900 font-medium underline decoration-slate-300">Tampilkan</button>
                    </div>
                </div>
                <div class="flex items-center justify-between mt-0.5">
                    <span id="sidebarUnlockPin" class="font-mono text-sm font-semibold text-slate-900 tracking-wider">••••••</span>
                    <span class="text-[10px] text-slate-400">PIN Keluar / Buka</span>
                </div>
            </div>

            <div class="pt-2 border-t border-slate-200/80 flex items-center justify-between text-[11px] text-slate-500">
                <span id="sidebarAdminEmail" class="truncate font-mono text-[10px] text-slate-600">Pengawas Aktif</span>
                <button onclick="handleAdminLogout()" class="text-rose-600 hover:text-rose-700 font-semibold shrink-0">Keluar</button>
            </div>

            <div class="pt-1 flex items-center justify-between text-[11px] text-slate-400">
                <span class="flex items-center gap-1.5">
                    <span id="firebaseSyncDot" class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
                    <span id="firebaseSyncLabel">Sinkronisasi Aktif</span>
                </span>
                <span id="lastSyncClock" class="font-mono tabular-nums">--:--:--</span>
            </div>
        </div>
    </aside>

    <!-- Main Content Area -->
    <div class="flex-1 flex flex-col min-w-0 h-screen overflow-hidden">

        <!-- Top Institutional Action Bar (Exact 64px / h-16 shrink-0) -->
        <header class="h-16 shrink-0 bg-white border-b border-slate-200 px-6 flex items-center justify-between gap-4 z-20">
            <div class="flex items-center gap-3 min-w-0">
                <div class="flex items-center gap-1.5">
                    <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400 hidden sm:inline">Ruangan:</span>
                    <select id="headerRoomSelect" onchange="onSelectRoom(this.value)" class="rounded-lg border border-slate-300 bg-slate-50 hover:bg-white px-3 py-1.5 text-xs font-mono font-semibold text-slate-900 focus:outline-none focus:border-slate-500 transition">
                        <option value="ALL">Semua Ruangan</option>
                    </select>
                    <button onclick="promptNewRoom()" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1" title="Tambah Kode Ruangan Baru">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/></svg>
                        <span class="hidden md:inline">Tambah</span>
                    </button>
                    <button onclick="promptEditSelectedRoom()" id="btnHeaderEditRoom" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1" title="Ubah Nama / Edit Ruangan">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L6.832 19.82a4.5 4.5 0 01-1.897 1.13l-2.685.8.8-2.685a4.5 4.5 0 011.13-1.897L16.863 4.487zm0 0L19.5 7.125"/></svg>
                        <span class="hidden md:inline">Edit</span>
                    </button>
                    <button onclick="switchTab('settings')" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1" title="Buka Menu Manajemen Multi-Ruangan & PIN">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M10.5 6h9.75M10.5 6a1.5 1.5 0 11-3 0m3 0a1.5 1.5 0 10-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-9.75 0h9.75"/></svg>
                        <span class="hidden lg:inline">Kelola</span>
                    </button>
                </div>

                <div class="h-4 w-px bg-slate-200 hidden sm:block"></div>

                <span id="globalStateBadge" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">
                    <span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
                    Proteksi AI Aktif
                </span>
            </div>

            <!-- Global / Per-Room Classroom Controls -->
            <div class="flex items-center gap-2.5 shrink-0">
                <div class="inline-flex rounded-lg border border-slate-200 bg-slate-100 p-0.5">
                    <button onclick="setGlobalStatus('LOCKED')" id="btnGlobalLock" class="px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 bg-slate-900 text-white shadow-sm">
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
                        <span id="lblBtnLock">Kunci Semua Ruangan</span>
                    </button>
                    <button onclick="setGlobalStatus('UNLOCKED')" id="btnGlobalUnlock" class="px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900">
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 019.9-1"/></svg>
                        <span id="lblBtnUnlock">Buka Kunci & Pulihkan</span>
                    </button>
                </div>
            </div>
        </header>

        <!-- Scrollable Workspace Body -->
        <div class="flex-1 overflow-y-auto">
            <main class="p-6 max-w-[1400px] w-full mx-auto space-y-6">

                <!-- KPI Telemetry Strip -->
                <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
                    <div class="card-surface rounded-xl p-4">
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span>Perangkat Terhubung</span>
                            <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25m18 0A2.25 2.25 0 0018.75 3H5.25A2.25 2.25 0 003 5.25m18 0V12a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 12V5.25"/></svg>
                        </div>
                        <div class="mt-2 flex items-baseline gap-2 tabular-nums">
                            <span id="statOnline" class="text-2xl font-semibold text-slate-900 tracking-tight">0</span>
                            <span class="text-xs text-slate-500">aktif dari <span id="statTotal" class="font-medium text-slate-700">0</span> perangkat</span>
                        </div>
                    </div>

                    <div class="card-surface rounded-xl p-4">
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span>Status Terkunci (AI Blocked)</span>
                            <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z"/></svg>
                        </div>
                        <div class="mt-2 flex items-baseline gap-2 tabular-nums">
                            <span id="statLocked" class="text-2xl font-semibold text-slate-900 tracking-tight">0</span>
                            <span class="text-xs text-slate-500">perangkat terlindungi</span>
                        </div>
                    </div>

                    <div class="card-surface rounded-xl p-4">
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span>Intersepsi Proses & Port AI</span>
                            <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/></svg>
                        </div>
                        <div class="mt-2 flex items-baseline gap-2 tabular-nums">
                            <span id="statViolations" class="text-2xl font-semibold text-slate-900 tracking-tight">0</span>
                            <span class="text-xs text-slate-500">percobaan dihentikan</span>
                        </div>
                    </div>

                    <div class="card-surface rounded-xl p-4">
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span>Cakupan Kebijakan Aktif</span>
                            <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25H12"/></svg>
                        </div>
                        <div class="mt-2 flex items-baseline gap-1.5 tabular-nums">
                            <span id="statDomains" class="text-lg font-semibold text-slate-900">0</span>
                            <span class="text-xs text-slate-500">domain</span>
                            <span class="text-slate-300">·</span>
                            <span id="statProcs" class="text-lg font-semibold text-slate-900">0</span>
                            <span class="text-xs text-slate-500">proses</span>
                            <span class="text-slate-300">·</span>
                            <span id="statExts" class="text-lg font-semibold text-slate-900">0</span>
                            <span class="text-xs text-slate-500">ekstensi</span>
                        </div>
                    </div>
                </div>

                <!-- VIEW 1: PERANGKAT MAHASISWA -->
                <section id="view-monitor" class="space-y-4">
                    <div class="card-surface rounded-xl p-3 flex flex-wrap items-center justify-between gap-3">
                        <div class="flex flex-wrap items-center gap-2">
                            <div class="relative">
                                <svg class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z"/></svg>
                                <input type="search" id="searchStudent" name="guard_student_search_filter" autocomplete="off" readonly onfocus="this.removeAttribute('readonly'); if(this.value.includes('@')){this.value='';renderCurrentStudents();}" spellcheck="false" oninput="renderCurrentStudents()" placeholder="Cari NIM, nama, ruangan, atau hostname..." class="pl-9 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:border-slate-400 w-64 bg-white">
                            </div>

                            <div class="inline-flex rounded-lg border border-slate-200 p-0.5 bg-slate-50 text-xs">
                                <button onclick="setFilter('ALL')" id="filter-ALL" class="filter-pill px-2.5 py-1 rounded-md font-medium bg-white text-slate-900 shadow-sm">Semua</button>
                                <button onclick="setFilter('ONLINE')" id="filter-ONLINE" class="filter-pill px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900">Online</button>
                                <button onclick="setFilter('UNLOCKED')" id="filter-UNLOCKED" class="filter-pill px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900">Terbuka</button>
                                <button onclick="setFilter('VIOLATION')" id="filter-VIOLATION" class="filter-pill px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900">Ada Insiden</button>
                            </div>
                        </div>

                        <div class="flex items-center gap-2">
                            <span class="text-xs text-slate-500">Tampilan:</span>
                            <div class="inline-flex rounded-lg border border-slate-200 p-0.5 bg-slate-50 text-xs">
                                <button onclick="setViewMode('table')" id="viewMode-table" class="px-2.5 py-1 rounded-md font-medium bg-white text-slate-900 shadow-sm flex items-center gap-1.5">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 5.25h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5"/></svg>
                                    Tabel
                                </button>
                                <button onclick="setViewMode('grid')" id="viewMode-grid" class="px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/></svg>
                                    Denah Kelas
                                </button>
                            </div>
                        </div>
                    </div>

                    <div id="studentTableContainer" class="card-surface rounded-xl overflow-hidden">
                        <div class="overflow-x-auto">
                            <table class="w-full text-left border-collapse">
                                <thead>
                                    <tr class="border-b border-slate-200 bg-slate-50/70 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                                        <th class="py-3 px-4">Peserta</th>
                                        <th class="py-3 px-3">Ruangan</th>
                                        <th class="py-3 px-4">Perangkat & IP</th>
                                        <th class="py-3 px-3">Konektivitas</th>
                                        <th class="py-3 px-3">Status Endpoint</th>
                                        <th class="py-3 px-3">Proteksi IDE</th>
                                        <th class="py-3 px-3">Insiden</th>
                                        <th class="py-3 px-4 text-right">Tindakan</th>
                                    </tr>
                                </thead>
                                <tbody id="studentTableBody" class="divide-y divide-slate-200 text-xs">
                                    <tr>
                                        <td colspan="8" class="py-12 text-center text-slate-400">Memuat data perangkat mahasiswa...</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div id="studentGridContainer" class="hidden grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4"></div>
                </section>

                <!-- VIEW 2: LOG AUDIT & INSIDEN -->
                <section id="view-logs" class="hidden space-y-4">
                    <div class="card-surface rounded-xl overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between">
                            <div>
                                <h2 class="text-sm font-semibold text-slate-900">Jurnal Audit & Intersepsi Keamanan</h2>
                                <p class="text-xs text-slate-500 mt-0.5">Rekam jejak waktu nyata setiap perubahan status ruangan, karantina ekstensi IDE, dan penghentian aplikasi AI</p>
                            </div>
                            <button onclick="clearLogs()" class="px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-medium text-slate-600 hover:bg-slate-50 hover:text-rose-600 transition">
                                Kosongkan Jurnal
                            </button>
                        </div>
                        <div class="divide-y divide-slate-200" id="logListContainer">
                            <div class="py-12 text-center text-xs text-slate-400">Belum ada catatan insiden.</div>
                        </div>
                    </div>
                </section>

                <!-- VIEW 3: KEBIJAKAN DAFTAR BLOKIR FLEKSIBEL (GLOBAL & PER-RUANGAN) -->
                <section id="view-rules" class="hidden space-y-6">
                    <div class="card-surface rounded-xl p-6 space-y-6">
                        <div class="flex flex-wrap items-center justify-between gap-4 pb-4 border-b border-slate-200">
                            <div>
                                <h2 class="text-sm font-semibold text-slate-900">Manajemen Blokir Domain & Aplikasi Secara Fleksibel (Per-Ruangan)</h2>
                                <p class="text-xs text-slate-500 mt-0.5">Atur layanan AI, website, atau proses aplikasi mana yang diblokir secara Global maupun khusus untuk masing-masing ruangan.</p>
                            </div>
                            <div class="flex flex-wrap items-center gap-2">
                                <button onclick="toggleAllDomainGroups(true)" class="px-3 py-2 rounded-lg text-xs font-medium border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition">
                                    Pilih Semua
                                </button>
                                <button onclick="toggleAllDomainGroups(false)" class="px-3 py-2 rounded-lg text-xs font-medium border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 transition">
                                    Kosongkan Semua
                                </button>
                                <button id="btnResetRoomRules" onclick="resetRoomBlocklistToGlobal()" class="hidden px-3 py-2 rounded-lg text-xs font-medium border border-amber-300 bg-amber-50 hover:bg-amber-100 text-amber-900 transition">
                                    Reset ke Kebijakan Global
                                </button>
                                <button id="btnSaveBlocklist" onclick="saveBlocklist()" class="px-4 py-2 rounded-lg text-xs font-semibold bg-slate-900 hover:bg-slate-800 text-white transition shadow-sm">
                                    Simpan & Terapkan Kebijakan
                                </button>
                            </div>
                        </div>

                        <!-- Bar Pemilih Ruangan untuk Kebijakan Blokir -->
                        <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 flex flex-wrap items-center justify-between gap-3">
                            <div class="flex flex-wrap items-center gap-3">
                                <label for="rulesRoomSelect" class="text-xs font-semibold text-slate-800">Target Ruangan Kebijakan:</label>
                                <select id="rulesRoomSelect" onchange="onSelectRulesRoom(this.value)" class="rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-xs font-mono font-semibold text-slate-900 focus:outline-none focus:border-slate-600">
                                    <option value="ALL">Semua Ruangan (Kebijakan Global Default)</option>
                                </select>
                                <span id="rulesScopeBadge" class="inline-flex items-center px-2.5 py-1 rounded-md text-[11px] font-medium bg-slate-200/80 text-slate-700">
                                    Kebijakan Global Default
                                </span>
                            </div>
                            <div class="text-[11px] text-slate-500">
                                Ruangan yang tidak memiliki aturan khusus akan otomatis mengikuti <strong>Kebijakan Global</strong>.
                            </div>
                        </div>

                        <!-- Toggle Kategori Layanan Populer -->
                        <div>
                            <div class="flex items-center justify-between mb-3">
                                <span class="text-xs font-semibold text-slate-800">1. Pilih Cepat Berdasarkan Layanan / Kategori (Klik untuk Aktifkan / Nonaktifkan Blokir)</span>
                                <span id="activeDomainsCounterBadge" class="text-[11px] font-mono font-semibold px-2 py-0.5 rounded bg-slate-100 text-slate-800 border border-slate-200">0 domain aktif</span>
                            </div>
                            <div id="domainCategoryCards" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
                                <!-- Diisi dinamis oleh JS -->
                            </div>
                        </div>

                        <!-- Tambah Domain Fleksibel & Daftar Chip Domain -->
                        <div class="pt-4 border-t border-slate-200 space-y-3">
                            <div class="flex flex-wrap items-center justify-between gap-2">
                                <label class="text-xs font-semibold text-slate-800">2. Daftar Domain Individual & Tambah Domain Kustom</label>
                                <div class="flex items-center gap-2">
                                    <input type="text" id="customDomainInput" placeholder="Tambah domain (misal: notion.so, discord.com)..." class="w-64 rounded-lg bg-white border border-slate-300 px-3 py-1.5 text-xs font-mono text-slate-900 focus:border-slate-600 focus:outline-none">
                                    <button onclick="addCustomDomain()" class="px-3 py-1.5 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-medium transition">
                                        + Tambah Domain
                                    </button>
                                </div>
                            </div>

                            <div class="grid grid-cols-1 lg:grid-cols-12 gap-4">
                                <div class="lg:col-span-7 border border-slate-200 rounded-xl p-3.5 bg-slate-50/60 max-h-72 overflow-y-auto">
                                    <div class="text-[11px] text-slate-500 mb-2 flex items-center justify-between">
                                        <span>Centang domain yang ingin diblokir (hapus centang untuk mengizinkan akses):</span>
                                        <input type="text" id="filterDomainChipInput" oninput="renderDomainChecklist()" placeholder="Filter domain..." class="rounded border border-slate-300 bg-white px-2 py-0.5 text-[11px] font-mono text-slate-800">
                                    </div>
                                    <div id="domainChecklistContainer" class="grid grid-cols-1 sm:grid-cols-2 gap-1.5 text-xs font-mono">
                                        <!-- Diisi dinamis oleh JS -->
                                    </div>
                                </div>

                                <div class="lg:col-span-5 space-y-1.5">
                                    <div class="flex items-center justify-between">
                                        <span class="text-[11px] font-semibold text-slate-600">Mode Teks Mentah (Sinkron Otomatis)</span>
                                        <span class="text-[10px] font-mono text-slate-400">1 baris = 1 domain</span>
                                    </div>
                                    <textarea id="inputDomains" oninput="syncChecklistFromTextarea()" rows="11" class="w-full rounded-xl bg-white border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:border-slate-500 focus:outline-none"></textarea>
                                </div>
                            </div>
                        </div>

                        <!-- Proses & Ekstensi IDE -->
                        <div class="pt-4 border-t border-slate-200 grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="space-y-2">
                                <div class="flex items-center justify-between">
                                    <label class="text-xs font-semibold text-slate-800">3. Proses Aplikasi Desktop yang Diblokir (AI-IDE, LLM, Roblox)</label>
                                    <span class="text-[11px] font-mono text-slate-400">Process Guard</span>
                                </div>
                                <p class="text-xs text-slate-500">Nama executable (tanpa <code>.exe</code>) yang dihentikan otomatis saat ruangan dikunci.</p>
                                <textarea id="inputProcesses" rows="10" class="w-full rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:bg-white focus:border-slate-400 focus:outline-none"></textarea>
                            </div>

                            <div class="space-y-2">
                                <div class="flex items-center justify-between">
                                    <label class="text-xs font-semibold text-slate-800">4. ID Ekstensi IDE yang Dikarantina (VS Code)</label>
                                    <span class="text-[11px] font-mono text-slate-400">Extension Guard</span>
                                </div>
                                <p class="text-xs text-slate-500">Prefix folder ekstensi pada <code>~/.vscode/extensions</code> yang dikunci sementara.</p>
                                <textarea id="inputExtensions" rows="10" class="w-full rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:bg-white focus:border-slate-400 focus:outline-none"></textarea>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- VIEW 4: MANAJEMEN MULTI-RUANGAN & PIN -->
                <section id="view-settings" class="hidden">
                    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
                        <!-- Kolom Kiri: Daftar Multi-Ruangan & PIN Masing-Masing Ruangan -->
                        <div class="lg:col-span-7 card-surface rounded-xl p-6 space-y-5">
                            <div class="border-b border-slate-200 pb-4 flex items-center justify-between gap-4">
                                <div>
                                    <h2 class="text-sm font-semibold text-slate-900">Manajemen Ruangan & PIN Otorisasi Per-Ruangan</h2>
                                    <p class="text-xs text-slate-500 mt-0.5">Setiap ruangan memiliki Kode Ruangan dan PIN Otorisasi Keluar tersendiri. Ketika aplikasi dibuka di PC, peserta memilih salah satu ruangan di bawah ini.</p>
                                </div>
                            </div>

                            <div class="grid grid-cols-1 sm:grid-cols-12 gap-2">
                                <input type="text" id="newRoomInput" placeholder="Kode Ruangan baru (misal: LAB-04)..." class="sm:col-span-5 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono uppercase text-slate-900 focus:border-slate-500 focus:outline-none">
                                <input type="text" id="newRoomPinInput" placeholder="PIN Keluar Ruangan (opsional, 6 digit)" class="sm:col-span-4 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono text-slate-900 focus:border-slate-500 focus:outline-none">
                                <button onclick="addRoomFromInput()" class="sm:col-span-3 px-3 py-2 rounded-lg text-xs font-medium bg-slate-900 hover:bg-slate-800 text-white transition shrink-0">
                                    + Tambah Ruangan
                                </button>
                            </div>

                            <div id="roomsListManager" class="divide-y divide-slate-200 border border-slate-200 rounded-xl overflow-hidden bg-white">
                                <!-- Diisi dinamis oleh JS -->
                            </div>
                        </div>

                        <!-- Kolom Kanan: PIN Master & Modul Proteksi -->
                        <div class="lg:col-span-5 card-surface rounded-xl p-6 space-y-6">
                            <div class="border-b border-slate-200 pb-4">
                                <h2 class="text-sm font-semibold text-slate-900">Kredensial Login Admin & Modul Proteksi</h2>
                                <p class="text-xs text-slate-500 mt-0.5">Atur PIN Master Pengawas serta kata sandi untuk login ke dashboard web ini.</p>
                            </div>

                            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">Ruangan Utama (Opsional)</label>
                                    <input type="text" id="cfgRoomCode" autocomplete="off" placeholder="Kosongkan jika belum ada ruangan" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="">
                                </div>
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">PIN Master Pengawas (Semua Ruangan)</label>
                                    <input type="text" id="cfgUnlockPin" autocomplete="off" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="202699">
                                </div>
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">Email Login Pengawas</label>
                                    <input type="text" id="cfgAdminEmail" autocomplete="off" readonly onfocus="this.removeAttribute('readonly')" placeholder="Kosongkan jika tidak diubah" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="">
                                </div>
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">Kata Sandi Login Dashboard</label>
                                    <input type="text" id="cfgAdminPassword" autocomplete="off" readonly onfocus="this.removeAttribute('readonly')" style="-webkit-text-security: disc;" placeholder="Kosongkan jika tidak diubah" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="">
                                </div>
                            </div>

                            <div class="space-y-3 pt-4 border-t border-slate-200">
                                <div class="text-xs font-semibold text-slate-800">Modul Penegakan Keamanan Endpoint</div>
                                <label class="flex items-start gap-3 text-xs text-slate-700 cursor-pointer">
                                    <input type="checkbox" id="cfgBlockDoh" class="mt-0.5 rounded border-slate-300 text-slate-900" checked>
                                    <span><strong>Nonaktifkan DNS-over-HTTPS (DoH) & AI Browser</strong> — Menerapkan kebijakan Registry sementara pada Chrome, Edge, dan Brave.</span>
                                </label>
                                <label class="flex items-start gap-3 text-xs text-slate-700 cursor-pointer">
                                    <input type="checkbox" id="cfgQuarantineExt" class="mt-0.5 rounded border-slate-300 text-slate-900" checked>
                                    <span><strong>Matikan Copilot Built-in & Karantina Ekstensi AI IDE</strong> — Mengunci fitur Chat/Copilot VS Code (<code>chat.disableAIFeatures</code>) dan mengkarantina ekstensi AI.</span>
                                </label>
                                <label class="flex items-start gap-3 text-xs text-slate-700 cursor-pointer">
                                    <input type="checkbox" id="cfgKillProcs" class="mt-0.5 rounded border-slate-300 text-slate-900" checked>
                                    <span><strong>Terminasi Proses AI-IDE, Local LLM & Roblox</strong> — Menghentikan proses Cursor, Windsurf, Ollama, LM Studio, dan Roblox Player/Studio.</span>
                                </label>
                                <label class="flex items-start gap-3 text-xs text-slate-700 cursor-pointer">
                                    <input type="checkbox" id="cfgBlockVpn" class="mt-0.5 rounded border-slate-300 text-slate-900" checked>
                                    <span><strong>Deteksi Antarmuka VPN & Tunneling</strong> — Memberikan peringatan apabila terdeteksi adapter jaringan virtual aktif.</span>
                                </label>
                            </div>

                            <div class="pt-2">
                                <button onclick="saveConfig()" class="w-full py-2 px-4 rounded-lg text-xs font-medium bg-slate-900 hover:bg-slate-800 text-white transition">
                                    Simpan Pengaturan & PIN Master
                                </button>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- VIEW 5: DISTRIBUSI AGEN (.EXE) -->
                <section id="view-download" class="hidden space-y-6">
                    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
                        <div class="lg:col-span-7 card-surface rounded-xl p-6 space-y-6">
                            <div class="border-b border-slate-200 pb-4">
                                <h2 class="text-sm font-semibold text-slate-900">Paket Distribusi Agen Perangkat Mahasiswa (Windows x64)</h2>
                                <p class="text-xs text-slate-500 mt-0.5">Kompilasi native menggunakan .NET Framework bawaan Windows 10/11. Tidak memerlukan instalasi runtime tambahan.</p>
                            </div>

                            <div class="p-3.5 rounded-lg bg-slate-50 border border-slate-200 text-xs flex items-center justify-between gap-3">
                                <div>
                                    <span class="text-slate-500">URL Endpoint Server untuk Diisi di Aplikasi Mahasiswa:</span>
                                    <div id="cloudEndpointUrl" class="font-mono font-semibold text-slate-900 mt-0.5"></div>
                                </div>
                                <button onclick="copyText(document.getElementById('cloudEndpointUrl').textContent, 'URL Endpoint Server disalin')" class="px-3 py-1.5 rounded bg-white border border-slate-300 hover:bg-slate-100 text-slate-700 font-medium shrink-0">
                                    Salin URL
                                </button>
                            </div>

                            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <div class="p-4 rounded-xl border border-slate-200 bg-slate-50/50 flex flex-col justify-between">
                                    <div>
                                        <div class="flex items-center justify-between">
                                            <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-500">Paket Standar</span>
                                            <span class="text-[11px] font-mono text-slate-500">~82 KB</span>
                                        </div>
                                        <div class="text-sm font-semibold text-slate-900 mt-1">AIBlockerSetup.exe</div>
                                        <p class="text-xs text-slate-600 mt-1.5 leading-relaxed">Memasang agen ke direktori <code>Program Files\AIBlockerGuard</code> dan mendaftarkan agen berjalan otomatis di sistem.</p>
                                    </div>
                                    <div class="mt-5">
                                        <a href="dist/AIBlockerSetup.exe" download class="w-full py-2 px-3 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-medium flex items-center justify-center gap-2 transition">
                                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
                                            Unduh Installer (.exe)
                                        </a>
                                    </div>
                                </div>

                                <div class="p-4 rounded-xl border border-slate-200 bg-white flex flex-col justify-between">
                                    <div>
                                        <div class="flex items-center justify-between">
                                            <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-500">Paket Lab / Portable</span>
                                            <span class="text-[11px] font-mono text-slate-500">~72 KB</span>
                                        </div>
                                        <div class="text-sm font-semibold text-slate-900 mt-1">AIBlockerGuard.exe</div>
                                        <p class="text-xs text-slate-600 mt-1.5 leading-relaxed">Eksekusi tunggal tanpa proses instalasi. Saat dibuka, peserta memilih ruangan yang tersedia dari daftar.</p>
                                    </div>
                                    <div class="mt-5">
                                        <a href="dist/AIBlockerGuard.exe" download class="w-full py-2 px-3 rounded-lg bg-white hover:bg-slate-50 text-slate-900 border border-slate-300 text-xs font-medium flex items-center justify-center gap-2 transition">
                                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
                                            Unduh Portable (.exe)
                                        </a>
                                    </div>
                                </div>
                            </div>

                            <div class="p-4 rounded-xl border border-slate-200 bg-slate-50 flex items-center justify-between gap-4">
                                <div>
                                    <div class="text-xs font-semibold text-slate-900">Utilitas Pemulihan Darurat (Emergency-Restore.bat)</div>
                                    <p class="text-xs text-slate-500 mt-0.5">Digunakan apabila perangkat mahasiswa kehabisan daya saat sesi terkunci untuk mengembalikan file hosts dan ekstensi IDE secara instan.</p>
                                </div>
                                <a href="dist/Emergency-Restore.bat" download class="shrink-0 px-3 py-1.5 rounded-lg bg-white hover:bg-slate-100 text-slate-700 border border-slate-300 text-xs font-medium transition">
                                    Unduh Skrip
                                </a>
                            </div>
                        </div>

                        <div class="lg:col-span-5 card-surface rounded-xl p-6 space-y-4">
                            <h3 class="text-sm font-semibold text-slate-900 border-b border-slate-200 pb-3">Standar Operasional Multi-Ruangan</h3>
                            <ol class="space-y-3.5 text-xs text-slate-600 list-decimal list-inside leading-relaxed">
                                <li>
                                    <strong class="text-slate-900">Siapkan Ruangan & PIN:</strong> Buat ruangan di menu <strong>Ruangan & PIN Pengawas</strong>. Setiap ruangan memiliki PIN Otorisasi Keluar tersendiri.
                                </li>
                                <li>
                                    <strong class="text-slate-900">Pilih Kebijakan Domain Fleksibel:</strong> Di menu <strong>Kebijakan Blokir Fleksibel</strong>, centang kategori atau domain mana saja yang ingin diblokir atau diizinkan.
                                </li>
                                <li>
                                    <strong class="text-slate-900">Pilih Ruangan di Aplikasi:</strong> Ketika aplikasi dibuka di PC mahasiswa, peserta memilih ruangan dari dropdown yang tersedia.
                                </li>
                                <li>
                                    <strong class="text-slate-900">PIN Otorisasi Keluar Per-Ruangan:</strong> Untuk menutup agen dari System Tray atau membuka kunci lokal, gunakan PIN sesuai ruangan tersebut atau PIN Master Pengawas.
                                </li>
                            </ol>
                        </div>
                    </div>
                </section>

            </main>
        </div>
    </div>

    <!-- Modal Detail Perangkat Mahasiswa -->
    <div id="studentModal" class="fixed inset-0 bg-slate-900/40 backdrop-blur-[1px] hidden z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-xl border border-slate-200 shadow-xl max-w-lg w-full overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between">
                <div>
                    <h3 id="modalStudentName" class="text-sm font-semibold text-slate-900">Detail Perangkat Peserta</h3>
                    <p id="modalStudentSub" class="text-xs text-slate-500 font-mono mt-0.5"></p>
                </div>
                <button onclick="closeStudentModal()" class="text-slate-400 hover:text-slate-700 text-sm font-medium px-2 py-1">✕</button>
            </div>
            <div class="p-5 space-y-4 text-xs">
                <div class="grid grid-cols-2 gap-3 bg-slate-50 p-3 rounded-lg border border-slate-200/80">
                    <div>
                        <span class="text-slate-500">Sistem Operasi:</span>
                        <div id="modalOs" class="font-medium text-slate-900 mt-0.5">-</div>
                    </div>
                    <div>
                        <span class="text-slate-500">Waktu Terhubung:</span>
                        <div id="modalJoined" class="font-mono font-medium text-slate-900 mt-0.5">-</div>
                    </div>
                </div>
                <div>
                    <div class="font-semibold text-slate-800 mb-1.5">Daftar Proteksi & Ekstensi IDE yang Dikarantina:</div>
                    <div id="modalExtList" class="p-3 rounded-lg border border-slate-200 bg-slate-50 font-mono text-[11px] text-slate-700 max-h-40 overflow-y-auto space-y-1"></div>
                </div>
            </div>
            <div class="px-5 py-3 bg-slate-50 border-t border-slate-200 flex justify-end">
                <button onclick="closeStudentModal()" class="px-3.5 py-1.5 rounded-lg bg-slate-900 text-white text-xs font-medium">Tutup</button>
            </div>
        </div>
    </div>

    <!-- Modal Tambah / Edit Ruangan & PIN Otorisasi -->
    <div id="roomModal" class="fixed inset-0 bg-slate-900/40 backdrop-blur-[1px] hidden z-50 flex items-center justify-center p-4">
        <div class="bg-white rounded-xl border border-slate-200 shadow-xl max-w-md w-full overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between">
                <div>
                    <h3 id="roomModalTitle" class="text-sm font-semibold text-slate-900">Edit Ruangan & PIN Otorisasi</h3>
                    <p id="roomModalSub" class="text-xs text-slate-500 mt-0.5">Atur Kode Ruangan dan PIN Otorisasi untuk keluar/membuka kunci ruangan ini.</p>
                </div>
                <button onclick="closeRoomModal()" class="text-slate-400 hover:text-slate-700 text-sm font-medium px-2 py-1">✕</button>
            </div>
            <div class="p-5 space-y-4 text-xs">
                <input type="hidden" id="modalOldRoomCode" value="">
                <div>
                    <label class="block text-xs font-medium text-slate-700 mb-1.5">Kode Ruangan</label>
                    <input type="text" id="modalRoomCodeInput" placeholder="Contoh: LAB-01, RUANG-UJIAN-A" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono uppercase font-semibold text-slate-900 focus:border-slate-500 focus:outline-none">
                </div>
                <div>
                    <label class="block text-xs font-medium text-slate-700 mb-1.5">PIN Otorisasi Keluar / Buka Kunci Ruangan</label>
                    <input type="text" id="modalRoomPinInput" placeholder="Contoh: 202601 (4-12 karakter)" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-semibold text-slate-900 focus:border-slate-500 focus:outline-none">
                    <p class="text-[11px] text-slate-400 mt-1">Digunakan pada PC di ruangan ini ketika pengawas ingin membuka kunci manual atau menghentikan agen.</p>
                </div>
            </div>
            <div class="px-5 py-3 bg-slate-50 border-t border-slate-200 flex justify-end gap-2">
                <button onclick="closeRoomModal()" class="px-3.5 py-1.5 rounded-lg border border-slate-300 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium">Batal</button>
                <button onclick="submitRoomModal()" class="px-4 py-1.5 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-medium">Simpan Ruangan</button>
            </div>
        </div>
    </div>

    <!-- Subtle Notification Toast -->
    <div id="toast" class="fixed bottom-5 right-5 hidden z-50 px-4 py-2.5 rounded-lg bg-slate-900 text-white text-xs font-medium shadow-lg"></div>

    <!-- ======================================================================= -->
    <!-- FIREBASE SDK INITIALIZATION (blockai-ba569)                             -->
    <!-- ======================================================================= -->
    <script type="module">
        import { initializeApp } from "https://www.gstatic.com/firebasejs/11.0.1/firebase-app.js";
        import { getAnalytics } from "https://www.gstatic.com/firebasejs/11.0.1/firebase-analytics.js";
        import { getAuth, signInWithEmailAndPassword, onAuthStateChanged, signOut } from "https://www.gstatic.com/firebasejs/11.0.1/firebase-auth.js";
        import { getFirestore, doc, setDoc, getDoc } from "https://www.gstatic.com/firebasejs/11.0.1/firebase-firestore.js";

        const firebaseConfig = {
            apiKey: "AIzaSyBMMOxxu21GYPboTL6RiX3jDAFfgMb0gNY",
            authDomain: "blockai-ba569.firebaseapp.com",
            projectId: "blockai-ba569",
            storageBucket: "blockai-ba569.firebasestorage.app",
            messagingSenderId: "852320379154",
            appId: "1:852320379154:web:7bbfa2069647161fc5d3cb",
            measurementId: "G-186W7K0HK0"
        };

        try {
            const app = initializeApp(firebaseConfig);
            let analytics = null;
            try { analytics = getAnalytics(app); } catch (e) {}
            const auth = getAuth(app);
            const db = getFirestore(app);

            window.__firebaseApp = {
                app,
                auth,
                db,
                signInWithEmailAndPassword,
                signOut,
                saveStateToFirestore: async (payloadObj) => {
                    try {
                        await setDoc(doc(db, "guard_system", "main_state"), {
                            payload: JSON.stringify(payloadObj),
                            updatedAt: new Date().toISOString()
                        }, { merge: true });
                    } catch (e) {}
                },
                loadStateFromFirestore: async () => {
                    try {
                        const snap = await getDoc(doc(db, "guard_system", "main_state"));
                        if (snap.exists() && snap.data()?.payload) {
                            return JSON.parse(snap.data().payload);
                        }
                    } catch (e) {}
                    return null;
                }
            };

            onAuthStateChanged(auth, (user) => {
                if (user && user.email) {
                    window.completeAdminAuth(user.email);
                }
            });
        } catch (err) {
            console.warn("Firebase init notice:", err);
        }
    </script>

    <script>
        // Kategori Domain Fleksibel
        const DOMAIN_CATALOG_GROUPS = [
            {
                id: 'chatgpt',
                title: 'ChatGPT & OpenAI',
                desc: 'Web ChatGPT & API OpenAI',
                domains: ['chatgpt.com', 'www.chatgpt.com', 'openai.com', 'www.openai.com', 'api.openai.com', 'auth.openai.com', 'chat.openai.com']
            },
            {
                id: 'claude',
                title: 'Claude & Anthropic',
                desc: 'Web Claude AI & API Anthropic',
                domains: ['claude.ai', 'www.claude.ai', 'anthropic.com', 'www.anthropic.com', 'api.anthropic.com']
            },
            {
                id: 'gemini',
                title: 'Google Gemini & AI Studio',
                desc: 'Gemini, Bard, MakerSuite & AI Studio',
                domains: ['gemini.google.com', 'bard.google.com', 'aistudio.google.com', 'generativelanguage.googleapis.com', 'makersuite.google.com', 'idx.google.com']
            },
            {
                id: 'copilot_ide',
                title: 'GitHub Copilot & MS Copilot',
                desc: 'Copilot VS Code, Proxy & Bing Copilot',
                domains: [
                    'copilot.microsoft.com', 'www.copilot.microsoft.com', 'sydney.bing.com', 'edgeservices.bing.com',
                    'githubcopilot.com', 'api.githubcopilot.com',
                    'api.individual.githubcopilot.com', 'api.business.githubcopilot.com', 'api.enterprise.githubcopilot.com',
                    'proxy.githubcopilot.com', 'proxy.individual.githubcopilot.com', 'proxy.business.githubcopilot.com', 'proxy.enterprise.githubcopilot.com',
                    'api-model-lab.githubcopilot.com',
                    'copilot-proxy.githubusercontent.com', 'copilot-telemetry.githubusercontent.com', 'origin-tracker.githubusercontent.com', 'default.exp-tas.com'
                ]
            },
            {
                id: 'deepseek_perplexity',
                title: 'DeepSeek, Perplexity, Grok & Poe',
                desc: 'Mesin pencari AI & chatbot alternatif',
                domains: [
                    'deepseek.com', 'www.deepseek.com', 'chat.deepseek.com', 'api.deepseek.com',
                    'perplexity.ai', 'www.perplexity.ai', 'api.perplexity.ai',
                    'poe.com', 'www.poe.com', 'grok.com', 'www.grok.com', 'x.ai', 'api.x.ai',
                    'mistral.ai', 'chat.mistral.ai', 'api.mistral.ai',
                    'meta.ai', 'www.meta.ai', 'pi.ai', 'inflection.ai', 'you.com', 'www.you.com'
                ]
            },
            {
                id: 'coding_ai',
                title: 'Cursor, Windsurf, Blackbox & v0',
                desc: 'AI Code Generator & App Builder',
                domains: [
                    'blackbox.ai', 'www.blackbox.ai', 'api.blackbox.ai', 'phind.com', 'www.phind.com',
                    'v0.dev', 'www.v0.dev', 'v0.app', 'bolt.new', 'www.bolt.new', 'lovable.dev', 'www.lovable.dev',
                    'replit.com', 'ai.replit.com',
                    'cursor.sh', 'www.cursor.sh', 'cursor.com', 'www.cursor.com', 'api2.cursor.sh', 'api3.cursor.sh', 'repo42.cursor.sh',
                    'codeium.com', 'www.codeium.com', 'server.codeium.com', 'windsurf.com', 'codeiumdata.com',
                    'tabnine.com', 'www.tabnine.com', 'api.tabnine.com', 'supermaven.com', 'www.supermaven.com', 'api.supermaven.com',
                    'continue.dev', 'www.continue.dev', 'sourcegraph.com', 'cody.dev'
                ]
            },
            {
                id: 'llm_api',
                title: 'HuggingFace, OpenRouter & Groq',
                desc: 'API Inference & Model Hosting',
                domains: [
                    'huggingface.co', 'www.huggingface.co', 'api-inference.huggingface.co',
                    'openrouter.ai', 'www.openrouter.ai', 'api.openrouter.ai',
                    'api.groq.com', 'console.groq.com', 'groq.com',
                    'together.ai', 'api.together.xyz', 'fireworks.ai', 'api.fireworks.ai', 'cohere.com', 'api.cohere.ai'
                ]
            },
            {
                id: 'roblox',
                title: 'Roblox Web, GameJoin & CDN',
                desc: 'Seluruh akses website & server game Roblox',
                domains: [
                    'roblox.com', 'www.roblox.com', 'web.roblox.com', 'ro.blox.com',
                    'api.roblox.com', 'apis.roblox.com', 'auth.roblox.com', 'accountsettings.roblox.com', 'accountinformation.roblox.com',
                    'assetdelivery.roblox.com', 'avatar.roblox.com', 'badges.roblox.com', 'billing.roblox.com', 'catalog.roblox.com', 'chat.roblox.com',
                    'clientsettings.roblox.com', 'clientsettingscdn.roblox.com', 'client-telemetry.roblox.com',
                    'contentstore.roblox.com', 'create.roblox.com', 'develop.roblox.com', 'devforum.roblox.com', 'economy.roblox.com',
                    'ecsv2.roblox.com', 'ephemeralcounters.api.roblox.com', 'engagementpayouts.roblox.com', 'followings.roblox.com',
                    'friends.roblox.com', 'gameinternationalization.roblox.com', 'gamejoin.roblox.com', 'gamepersistence.roblox.com',
                    'games.roblox.com', 'groups.roblox.com', 'inventory.roblox.com', 'itemconfiguration.roblox.com',
                    'lms.roblox.com', 'locale.roblox.com', 'localizationtables.roblox.com', 'metrics.roblox.com',
                    'notifications.roblox.com', 'points.roblox.com', 'premiumfeatures.roblox.com', 'presence.roblox.com', 'privatemessages.roblox.com',
                    'publish.roblox.com', 'realtime.roblox.com', 'search.roblox.com', 'thumbnails.roblox.com', 'trades.roblox.com',
                    'translationroles.roblox.com', 'translations.roblox.com', 'users.roblox.com', 'voice.roblox.com',
                    'rbxcdn.com', 'www.rbxcdn.com', 'setup.rbxcdn.com', 'tr.rbxcdn.com', 'js.rbxcdn.com', 'css.rbxcdn.com', 'images.rbxcdn.com',
                    't0.rbxcdn.com', 't1.rbxcdn.com', 't2.rbxcdn.com', 't3.rbxcdn.com', 't4.rbxcdn.com', 't5.rbxcdn.com', 't6.rbxcdn.com', 't7.rbxcdn.com',
                    'c0.rbxcdn.com', 'c1.rbxcdn.com', 'c2.rbxcdn.com', 'c3.rbxcdn.com', 'c4.rbxcdn.com', 'c5.rbxcdn.com', 'c6.rbxcdn.com', 'c7.rbxcdn.com',
                    'robloxlabs.com', 'corp.roblox.com', 'en.help.roblox.com'
                ]
            }
        ];

        let currentTab = 'monitor';
        let currentFilter = 'ALL';
        let currentViewMode = 'table';
        let selectedRoom = 'ALL';
        let selectedRulesRoom = 'ALL';
        let cachedStudents = [];
        let cachedLogs = [];
        let cachedRooms = [];
        let cachedRoomStatuses = {};
        let cachedRoomPins = {};
        let cachedRoomBlocklists = {};
        let cachedGlobalBlocklist = { domains: [], processes: [], ide_extensions: [] };
        let cachedGlobalStatus = 'LOCKED';
        let blocklistLoaded = false;
        let pinVisible = false;
        let currentUnlockPin = '202699';
        let currentRoomCode = '';
        let allKnownDomains = [];
        let activeBlockedDomainsSet = new Set();

        // Inisialisasi daftar seluruh domain katalog
        DOMAIN_CATALOG_GROUPS.forEach(g => {
            g.domains.forEach(d => {
                const clean = d.trim().toLowerCase();
                if (!allKnownDomains.includes(clean)) allKnownDomains.push(clean);
            });
        });

        const endpointEl = document.getElementById('cloudEndpointUrl');
        if (endpointEl) {
            endpointEl.textContent = window.location.origin + window.location.pathname.replace(/\/index\.(php|html)$/, '').replace(/\/$/, '') + '/api/index.php';
        }

        // ====================================================================
        // OTENTIKASI & KEAMANAN DASHBOARD PENGAWAS
        // ====================================================================
        // Transparent fetch interceptor: otomatis melampirkan header Authorization: Bearer <token>
        // ke semua request API admin, dan menangani redirect jika sesi berakhir (401).
        const _nativeFetch = window.fetch;
        window.fetch = function(url, options = {}) {
            const token = sessionStorage.getItem('guard_admin_token');
            if (token && typeof url === 'string' && url.includes('api/')) {
                options = options || {};
                options.headers = options.headers || {};
                if (options.headers instanceof Headers) {
                    if (!options.headers.has('Authorization')) {
                        options.headers.set('Authorization', 'Bearer ' + token);
                    }
                } else {
                    if (!options.headers['Authorization'] && !options.headers['authorization']) {
                        options.headers['Authorization'] = 'Bearer ' + token;
                    }
                }
            }
            return _nativeFetch.call(this, url, options).then(res => {
                if (res.status === 401 && typeof url === 'string' && url.includes('api/') && !url.includes('admin_login')) {
                    res.clone().json().then(j => {
                        if (j && j.auth_required) {
                            handleAdminLogout('Sesi Anda telah berakhir. Silakan login kembali.');
                        }
                    }).catch(() => {});
                }
                return res;
            });
        };

        function checkExistingAuthSession() {
            const savedEmail = sessionStorage.getItem('guard_admin_auth');
            const savedToken = sessionStorage.getItem('guard_admin_token');
            if (savedEmail && savedToken) {
                completeAdminAuth(savedEmail, savedToken);
            }
        }

        const knownStudentsCache = new Map(); // id -> { student, seenAtMs }
        const explicitlyRemovedStudentIds = new Set();
        const explicitlyDeletedRooms = new Set();

        window.completeAdminAuth = function(email, token) {
            sessionStorage.setItem('guard_admin_auth', email);
            if (token) {
                sessionStorage.setItem('guard_admin_token', token);
            }
            const gate = document.getElementById('authGateOverlay');
            if (gate) gate.classList.add('hidden');
            const emailEl = document.getElementById('sidebarAdminEmail');
            if (emailEl) emailEl.textContent = email;
            const searchInp = document.getElementById('searchStudent');
            if (searchInp && (searchInp.value.includes('@') || searchInp.value.toLowerCase() === String(email || '').toLowerCase())) {
                searchInp.value = '';
                renderCurrentStudents();
            }
            fetchDashboard();
        };

        async function handleAdminLogin(e) {
            e.preventDefault();
            const email = document.getElementById('loginEmail').value.trim();
            const password = document.getElementById('loginPassword').value.trim();
            const errBox = document.getElementById('loginErrorBox');
            const btn = document.getElementById('btnLoginSubmit');
            errBox.classList.add('hidden');
            btn.disabled = true;
            btn.textContent = 'Memverifikasi Kredensial...';

            try {
                // 1. Verifikasi kredensial ke Server API (Vercel Node.js / PHP) dan dapatkan token HMAC
                const res = await fetch('api/index.php?action=admin_login', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({ email, password })
                });
                const data = await res.json();
                if (data && data.ok) {
                    completeAdminAuth(data.user?.email || email, data.token);

                    // 2. Sinkronkan juga sesi Firebase Auth jika tersedia
                    if (window.__firebaseApp && window.__firebaseApp.auth && email.includes('@')) {
                        try {
                            await window.__firebaseApp.signInWithEmailAndPassword(window.__firebaseApp.auth, email, password);
                        } catch (fbErr) {}
                    }

                    showToast('Selamat datang di Konsol Pengawas GuardConsole');
                } else {
                    errBox.textContent = data.error || 'Email atau Kata Sandi tidak valid.';
                    errBox.classList.remove('hidden');
                }
            } catch (err) {
                errBox.textContent = 'Gagal menghubungi server otentikasi.';
                errBox.classList.remove('hidden');
            } finally {
                btn.disabled = false;
                btn.textContent = 'Masuk ke Konsol Pengawas';
            }
        }

        async function handleAdminLogout(noticeMessage = '') {
            sessionStorage.removeItem('guard_admin_auth');
            sessionStorage.removeItem('guard_admin_token');
            if (window.__firebaseApp && window.__firebaseApp.auth) {
                try { await window.__firebaseApp.signOut(window.__firebaseApp.auth); } catch (e) {}
            }
            const emailInp = document.getElementById('loginEmail');
            const passInp = document.getElementById('loginPassword');
            if (emailInp) { emailInp.value = ''; emailInp.setAttribute('readonly', 'readonly'); }
            if (passInp) { passInp.value = ''; passInp.setAttribute('readonly', 'readonly'); }
            const errBox = document.getElementById('loginErrorBox');
            if (errBox && noticeMessage) {
                errBox.textContent = noticeMessage;
                errBox.classList.remove('hidden');
            }
            document.getElementById('authGateOverlay').classList.remove('hidden');
        }

        // ====================================================================
        // MANAJEMEN DOMAIN FLEKSIBEL (KATEGORI + CHECKLIST + CUSTOM DOMAIN)
        // ====================================================================
        function initFlexibleDomainManager(domainsArray) {
            activeBlockedDomainsSet = new Set((domainsArray || []).map(d => String(d).trim().toLowerCase()).filter(Boolean));
            activeBlockedDomainsSet.forEach(d => {
                if (!allKnownDomains.includes(d)) {
                    allKnownDomains.unshift(d);
                }
            });
            syncTextareaFromSet();
            renderDomainCategoryCards();
            renderDomainChecklist();
        }

        function syncTextareaFromSet() {
            const ta = document.getElementById('inputDomains');
            if (ta) {
                ta.value = Array.from(activeBlockedDomainsSet).join('\n');
            }
            const badge = document.getElementById('activeDomainsCounterBadge');
            if (badge) {
                badge.textContent = `${activeBlockedDomainsSet.size} domain diblokir`;
            }
        }

        function syncChecklistFromTextarea() {
            const ta = document.getElementById('inputDomains');
            if (!ta) return;
            const list = ta.value.split('\n').map(s => s.trim().toLowerCase()).filter(Boolean);
            activeBlockedDomainsSet = new Set(list);
            list.forEach(d => {
                if (!allKnownDomains.includes(d)) allKnownDomains.unshift(d);
            });
            const badge = document.getElementById('activeDomainsCounterBadge');
            if (badge) badge.textContent = `${activeBlockedDomainsSet.size} domain diblokir`;
            renderDomainCategoryCards();
            renderDomainChecklist();
        }

        function renderDomainCategoryCards() {
            const box = document.getElementById('domainCategoryCards');
            if (!box) return;
            box.innerHTML = DOMAIN_CATALOG_GROUPS.map(g => {
                const total = g.domains.length;
                const activeCount = g.domains.filter(d => activeBlockedDomainsSet.has(d.toLowerCase())).length;
                const isAllActive = activeCount === total;
                const isPartial = activeCount > 0 && activeCount < total;

                const borderClass = isAllActive
                    ? 'border-slate-900 bg-slate-900 text-white'
                    : (isPartial ? 'border-amber-500 bg-amber-50/60 text-slate-900' : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50');
                const badgeClass = isAllActive
                    ? 'bg-emerald-500/20 text-emerald-200 border-emerald-400/30'
                    : (isPartial ? 'bg-amber-100 text-amber-800 border-amber-300' : 'bg-slate-100 text-slate-500 border-slate-200');
                const statusText = isAllActive ? 'Diblokir' : (isPartial ? `${activeCount}/${total}` : 'Diizinkan');

                return `
                    <div onclick="toggleDomainGroup('${g.id}')" class="cursor-pointer rounded-xl border p-3.5 transition flex flex-col justify-between ${borderClass}">
                        <div class="flex items-start justify-between gap-2">
                            <div class="font-semibold text-xs">${escapeHtml(g.title)}</div>
                            <span class="px-1.5 py-0.5 rounded text-[10px] font-mono border shrink-0 ${badgeClass}">${statusText}</span>
                        </div>
                        <div class="mt-2 flex items-center justify-between text-[11px] ${isAllActive ? 'text-slate-300' : 'text-slate-500'}">
                            <span>${escapeHtml(g.desc)}</span>
                            <span class="font-mono">${total} host</span>
                        </div>
                    </div>
                `;
            }).join('');
        }

        function toggleDomainGroup(groupId) {
            const group = DOMAIN_CATALOG_GROUPS.find(g => g.id === groupId);
            if (!group) return;
            const allActive = group.domains.every(d => activeBlockedDomainsSet.has(d.toLowerCase()));
            group.domains.forEach(d => {
                const clean = d.toLowerCase();
                if (allActive) {
                    activeBlockedDomainsSet.delete(clean);
                } else {
                    activeBlockedDomainsSet.add(clean);
                }
            });
            syncTextareaFromSet();
            renderDomainCategoryCards();
            renderDomainChecklist();
        }

        function toggleAllDomainGroups(enable) {
            if (enable) {
                allKnownDomains.forEach(d => activeBlockedDomainsSet.add(d.toLowerCase()));
            } else {
                activeBlockedDomainsSet.clear();
            }
            syncTextareaFromSet();
            renderDomainCategoryCards();
            renderDomainChecklist();
        }

        function toggleSingleDomain(domain, checked) {
            const clean = String(domain).trim().toLowerCase();
            if (checked) {
                activeBlockedDomainsSet.add(clean);
            } else {
                activeBlockedDomainsSet.delete(clean);
            }
            syncTextareaFromSet();
            renderDomainCategoryCards();
        }

        function addCustomDomain() {
            const inp = document.getElementById('customDomainInput');
            if (!inp || !inp.value.trim()) return;
            const raw = inp.value.trim().toLowerCase().replace(/^https?:\/\//, '').replace(/\/.*$/, '');
            if (!raw) return;
            if (!allKnownDomains.includes(raw)) {
                allKnownDomains.unshift(raw);
            }
            activeBlockedDomainsSet.add(raw);
            inp.value = '';
            syncTextareaFromSet();
            renderDomainCategoryCards();
            renderDomainChecklist();
            showToast(`Domain [${raw}] ditambahkan ke daftar blokir`);
        }

        function renderDomainChecklist() {
            const box = document.getElementById('domainChecklistContainer');
            if (!box) return;
            const q = (document.getElementById('filterDomainChipInput')?.value || '').trim().toLowerCase();
            const filtered = allKnownDomains.filter(d => !q || d.includes(q));

            box.innerHTML = filtered.map(d => {
                const isChecked = activeBlockedDomainsSet.has(d);
                return `
                    <label class="flex items-center justify-between gap-2 px-2.5 py-1.5 rounded-lg border ${isChecked ? 'bg-white border-slate-300 text-slate-900 font-medium' : 'bg-slate-100/60 border-transparent text-slate-400'} cursor-pointer hover:border-slate-400 transition">
                        <span class="flex items-center gap-2 truncate">
                            <input type="checkbox" ${isChecked ? 'checked' : ''} onchange="toggleSingleDomain('${escapeHtml(d)}', this.checked)" class="rounded border-slate-300 text-slate-900">
                            <span class="truncate">${escapeHtml(d)}</span>
                        </span>
                    </label>
                `;
            }).join('');
        }

        // ====================================================================
        // UTILITAS UI & NAVIGASI
        // ====================================================================
        function showToast(msg, isError = false) {
            const t = document.getElementById('toast');
            t.textContent = msg;
            t.className = `fixed bottom-5 right-5 z-50 px-4 py-2.5 rounded-lg text-xs font-medium shadow-lg transition ${
                isError ? 'bg-rose-700 text-white' : 'bg-slate-900 text-white'
            }`;
            t.classList.remove('hidden');
            setTimeout(() => t.classList.add('hidden'), 3000);
        }

        function copyText(text, msg) {
            navigator.clipboard.writeText(text);
            showToast(msg || 'Teks disalin');
        }

        function getActiveDisplayPin() {
            if (selectedRoom !== 'ALL' && cachedRoomPins[selectedRoom]) {
                return cachedRoomPins[selectedRoom];
            }
            return currentUnlockPin;
        }

        function copyActiveRoomPin() {
            const pin = getActiveDisplayPin();
            const label = selectedRoom === 'ALL' ? 'PIN Master Pengawas' : `PIN Ruangan [${selectedRoom}]`;
            copyText(pin, `${label} (${pin}) disalin`);
        }

        function updateSidebarPinUI() {
            const lbl = document.getElementById('sidebarPinLabel');
            const val = document.getElementById('sidebarUnlockPin');
            const btn = document.getElementById('btnTogglePin');
            if (lbl) {
                lbl.textContent = selectedRoom === 'ALL' ? 'PIN Master Pengawas' : `PIN Ruangan [${selectedRoom}]`;
            }
            if (val) {
                val.textContent = pinVisible ? getActiveDisplayPin() : '••••••';
            }
            if (btn) {
                btn.textContent = pinVisible ? 'Sembunyikan' : 'Tampilkan';
            }
        }

        function togglePinVisibility() {
            pinVisible = !pinVisible;
            updateSidebarPinUI();
        }

        function switchTab(tab) {
            currentTab = tab;
            ['monitor', 'logs', 'rules', 'settings', 'download'].forEach(id => {
                document.getElementById('view-' + id).classList.toggle('hidden', id !== tab);
                const btn = document.getElementById('nav-' + id);
                if (id === tab) {
                    btn.className = 'nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium bg-slate-900 text-white transition';
                } else {
                    btn.className = 'nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition';
                }
            });

            const badgeOnline = document.getElementById('navBadgeOnline');
            if (badgeOnline) {
                badgeOnline.className = tab === 'monitor'
                    ? 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-white/20 text-white tabular-nums'
                    : 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-slate-200/80 text-slate-700 tabular-nums';
            }

            const badgeViolations = document.getElementById('navBadgeViolations');
            if (badgeViolations) {
                badgeViolations.className = tab === 'logs'
                    ? 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-500 text-white tabular-nums'
                    : 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-50 text-rose-700 border border-rose-200 tabular-nums';
            }
        }

        function getEffectiveBlocklistForRoom(roomCode) {
            const norm = String(roomCode || 'ALL').trim().toUpperCase();
            if (norm !== 'ALL' && cachedRoomBlocklists && cachedRoomBlocklists[norm]) {
                const rb = cachedRoomBlocklists[norm];
                return {
                    domains: Array.isArray(rb.domains) ? rb.domains : (cachedGlobalBlocklist.domains || []),
                    processes: Array.isArray(rb.processes) ? rb.processes : (cachedGlobalBlocklist.processes || []),
                    ide_extensions: Array.isArray(rb.ide_extensions) ? rb.ide_extensions : (cachedGlobalBlocklist.ide_extensions || []),
                    isCustom: true
                };
            }
            return {
                domains: cachedGlobalBlocklist.domains || [],
                processes: cachedGlobalBlocklist.processes || [],
                ide_extensions: cachedGlobalBlocklist.ide_extensions || [],
                isCustom: false
            };
        }

        function loadBlocklistIntoEditorForRoom(roomCode) {
            selectedRulesRoom = roomCode || 'ALL';
            if (selectedRulesRoom !== 'ALL' && !cachedRooms.includes(selectedRulesRoom)) {
                selectedRulesRoom = 'ALL';
            }
            const eff = getEffectiveBlocklistForRoom(selectedRulesRoom);
            initFlexibleDomainManager(eff.domains || []);
            const procEl = document.getElementById('inputProcesses');
            const extEl = document.getElementById('inputExtensions');
            if (procEl) procEl.value = (eff.processes || []).join('\n');
            if (extEl) extEl.value = (eff.ide_extensions || []).join('\n');
            updateRulesRoomSelectorUI();
        }

        function updateRulesRoomSelectorUI() {
            const sel = document.getElementById('rulesRoomSelect');
            if (sel) {
                const globalCount = (cachedGlobalBlocklist.domains || []).length;
                let html = `<option value="ALL">Semua Ruangan — Kebijakan Global (${globalCount} domain)</option>`;
                cachedRooms.forEach(r => {
                    const hasCustom = Boolean(cachedRoomBlocklists && cachedRoomBlocklists[r]);
                    const dCount = hasCustom ? (cachedRoomBlocklists[r].domains || []).length : globalCount;
                    const modeText = hasCustom ? `Kebijakan Khusus · ${dCount} domain` : `Mengikuti Global · ${dCount} domain`;
                    html += `<option value="${escapeHtml(r)}">Ruangan ${escapeHtml(r)} (${modeText})</option>`;
                });
                sel.innerHTML = html;
                if (selectedRulesRoom !== 'ALL' && !cachedRooms.includes(selectedRulesRoom)) {
                    selectedRulesRoom = 'ALL';
                }
                sel.value = selectedRulesRoom;
            }

            const badge = document.getElementById('rulesScopeBadge');
            const btnReset = document.getElementById('btnResetRoomRules');
            const btnSave = document.getElementById('btnSaveBlocklist');

            if (selectedRulesRoom === 'ALL') {
                if (badge) {
                    badge.className = 'inline-flex items-center px-2.5 py-1 rounded-md text-[11px] font-medium bg-slate-200/80 text-slate-800';
                    badge.textContent = 'Mode Edit: Kebijakan Global (Semua Ruangan)';
                }
                if (btnReset) btnReset.classList.add('hidden');
                if (btnSave) btnSave.textContent = 'Simpan Kebijakan Global';
            } else {
                const hasCustom = Boolean(cachedRoomBlocklists && cachedRoomBlocklists[selectedRulesRoom]);
                if (badge) {
                    if (hasCustom) {
                        badge.className = 'inline-flex items-center px-2.5 py-1 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-800 border border-emerald-200';
                        badge.textContent = `Ruangan [${selectedRulesRoom}] Menggunakan Kebijakan Khusus`;
                    } else {
                        badge.className = 'inline-flex items-center px-2.5 py-1 rounded-md text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200';
                        badge.textContent = `Ruangan [${selectedRulesRoom}] Mengikuti Global (Simpan untuk buat khusus)`;
                    }
                }
                if (btnReset) btnReset.classList.toggle('hidden', !hasCustom);
                if (btnSave) btnSave.textContent = `Simpan Khusus Ruangan [${selectedRulesRoom}]`;
            }
        }

        function onSelectRulesRoom(roomCode) {
            loadBlocklistIntoEditorForRoom(roomCode || 'ALL');
        }

        function openRoomRulesEditor(roomCode) {
            selectedRoom = roomCode || 'ALL';
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
            const hdrSel = document.getElementById('headerRoomSelect');
            if (hdrSel) hdrSel.value = selectedRoom;
            loadBlocklistIntoEditorForRoom(selectedRoom);
            updateSidebarPinUI();
            updateHeaderStatusUI();
            updateScopedStats();
            switchTab('rules');
        }

        function onSelectRoom(room) {
            selectedRoom = room || 'ALL';
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
            loadBlocklistIntoEditorForRoom(selectedRoom);
            updateSidebarPinUI();
            updateHeaderStatusUI();
            updateScopedStats();
            renderCurrentStudents();
            renderLogs(cachedLogs);
            renderRoomsManager();
        }

        function setFilter(filter) {
            currentFilter = filter;
            ['ALL', 'ONLINE', 'UNLOCKED', 'VIOLATION'].forEach(f => {
                const el = document.getElementById('filter-' + f);
                if (f === filter) {
                    el.className = 'filter-pill px-2.5 py-1 rounded-md font-medium bg-white text-slate-900 shadow-sm';
                } else {
                    el.className = 'filter-pill px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900';
                }
            });
            renderCurrentStudents();
        }

        function setViewMode(mode) {
            currentViewMode = mode;
            document.getElementById('studentTableContainer').classList.toggle('hidden', mode !== 'table');
            document.getElementById('studentGridContainer').classList.toggle('hidden', mode !== 'grid');
            document.getElementById('viewMode-table').className = mode === 'table'
                ? 'px-2.5 py-1 rounded-md font-medium bg-white text-slate-900 shadow-sm flex items-center gap-1.5'
                : 'px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5';
            document.getElementById('viewMode-grid').className = mode === 'grid'
                ? 'px-2.5 py-1 rounded-md font-medium bg-white text-slate-900 shadow-sm flex items-center gap-1.5'
                : 'px-2.5 py-1 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5';
            renderCurrentStudents();
        }

        function updateRoomSelectorOptions() {
            const sel = document.getElementById('headerRoomSelect');
            if (!sel) return;
            const counts = {};
            cachedStudents.forEach(st => {
                const r = (st.room_code || currentRoomCode || '-').toUpperCase();
                counts[r] = (counts[r] || 0) + 1;
            });

            const allRooms = Array.from(new Set(cachedRooms));
            cachedRooms = allRooms;

            let html = `<option value="ALL">Semua Ruangan (${cachedStudents.length} PC)</option>`;
            allRooms.forEach(r => {
                const c = counts[r] || 0;
                const st = (cachedRoomStatuses[r] || cachedGlobalStatus) === 'LOCKED' ? 'Terkunci' : 'Terbuka';
                html += `<option value="${escapeHtml(r)}">${escapeHtml(r)} (${c} PC · ${st})</option>`;
            });
            sel.innerHTML = html;
            if (selectedRoom !== 'ALL' && !allRooms.includes(selectedRoom)) {
                selectedRoom = 'ALL';
            }
            sel.value = selectedRoom;
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
            updateRulesRoomSelectorUI();
        }

        function updateHeaderStatusUI() {
            const gBadge = document.getElementById('globalStateBadge');
            const btnLock = document.getElementById('btnGlobalLock');
            const btnUnlock = document.getElementById('btnGlobalUnlock');
            const lblLock = document.getElementById('lblBtnLock');
            const lblUnlock = document.getElementById('lblBtnUnlock');

            const activeStatus = selectedRoom === 'ALL'
                ? cachedGlobalStatus
                : (cachedRoomStatuses[selectedRoom] || cachedGlobalStatus);

            lblLock.textContent = selectedRoom === 'ALL' ? 'Kunci Semua Ruangan' : `Kunci ${selectedRoom}`;
            lblUnlock.textContent = selectedRoom === 'ALL' ? 'Buka Semua & Pulihkan' : `Buka ${selectedRoom}`;

            if (activeStatus === 'LOCKED') {
                gBadge.className = 'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200';
                gBadge.innerHTML = `<span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span> ${selectedRoom === 'ALL' ? 'Semua Ruangan Terkunci' : 'Ruangan ' + escapeHtml(selectedRoom) + ' Terkunci'}`;
                btnLock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 bg-slate-900 text-white shadow-sm';
                btnUnlock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900';
            } else {
                gBadge.className = 'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200';
                gBadge.innerHTML = `<span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span> ${selectedRoom === 'ALL' ? 'Semua Ruangan Terbuka' : 'Ruangan ' + escapeHtml(selectedRoom) + ' Terbuka'}`;
                btnUnlock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 bg-slate-900 text-white shadow-sm';
                btnLock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900';
            }
        }

        function updateScopedStats() {
            const scoped = selectedRoom === 'ALL'
                ? cachedStudents
                : cachedStudents.filter(st => (st.room_code || currentRoomCode || '-').toUpperCase() === selectedRoom);

            const onlineCount = scoped.filter(st => st.is_online).length;
            const lockedCount = scoped.filter(st => st.status === 'LOCKED').length;
            const violationCount = scoped.reduce((acc, st) => acc + parseInt(st.killed_processes_count || 0, 10), 0);

            document.getElementById('statOnline').textContent = onlineCount;
            document.getElementById('statTotal').textContent = scoped.length;
            document.getElementById('statLocked').textContent = lockedCount;
            document.getElementById('statViolations').textContent = violationCount;

            const effBl = getEffectiveBlocklistForRoom(selectedRoom);
            document.getElementById('statDomains').textContent = (effBl.domains || []).length;
            document.getElementById('statProcs').textContent = (effBl.processes || []).length;
            document.getElementById('statExts').textContent = (effBl.ide_extensions || []).length;

            document.getElementById('navBadgeOnline').textContent = onlineCount;
            document.getElementById('navBadgeViolations').textContent = violationCount;
        }

        async function syncStateToFirebaseIfAvailable(configObj, blocklistObj) {
            if (window.__firebaseApp && window.__firebaseApp.saveStateToFirestore) {
                await window.__firebaseApp.saveStateToFirestore({
                    config: configObj || {
                        room_code: currentRoomCode,
                        rooms: cachedRooms,
                        room_statuses: cachedRoomStatuses,
                        room_pins: cachedRoomPins,
                        room_blocklists: cachedRoomBlocklists,
                        unlock_pin: currentUnlockPin,
                        global_status: cachedGlobalStatus,
                        blocklist_customized: true
                    },
                    blocklist: blocklistObj
                });
            }
        }

        async function fetchDashboard() {
            if (!sessionStorage.getItem('guard_admin_token')) return;
            try {
                const res = await fetch('api/index.php?action=dashboard_state');
                const data = await res.json();
                if (!data.ok) return;

                currentRoomCode = data.config.room_code || '';
                currentUnlockPin = data.config.unlock_pin || '202699';
                cachedGlobalStatus = data.config.global_status || 'LOCKED';
                const serverRooms = Array.isArray(data.config.rooms) ? data.config.rooms : [];
                cachedRooms = serverRooms.filter(r => !explicitlyDeletedRooms.has(r));
                cachedRoomStatuses = data.config.room_statuses || {};
                cachedRoomPins = data.config.room_pins || {};
                cachedRoomBlocklists = data.config.room_blocklists || {};
                if (data.blocklist) {
                    cachedGlobalBlocklist = data.blocklist;
                }

                updateSidebarPinUI();
                const timePart = (data.server_time || '').split(' ')[1] || '--:--:--';
                document.getElementById('lastSyncClock').textContent = timePart;

                if (!blocklistLoaded && data.blocklist) {
                    loadBlocklistIntoEditorForRoom(selectedRulesRoom);
                    document.getElementById('cfgRoomCode').value = currentRoomCode;
                    document.getElementById('cfgUnlockPin').value = currentUnlockPin;
                    blocklistLoaded = true;
                } else {
                    updateRulesRoomSelectorUI();
                }

                // Anti-Flicker & Multi-Container Sync Engine:
                // Simpan setiap peserta yang aktif ke knownStudentsCache agar jika Vercel membagi request ke container baru (cold start),
                // baris peserta tidak pernah berkedip hilang dan container baru otomatis di-rehydrate.
                const nowMs = Date.now();
                const incomingStudents = Array.isArray(data.students) ? data.students : [];
                const incomingIds = new Set();

                incomingStudents.forEach(st => {
                    if (!st || !st.id) return;
                    const stRoom = String(st.room_code || '').toUpperCase();
                    if (explicitlyRemovedStudentIds.has(st.id) || explicitlyDeletedRooms.has(stRoom)) return;
                    incomingIds.add(st.id);
                    knownStudentsCache.set(st.id, {
                        student: { ...st },
                        seenAtMs: nowMs - ((st.seconds_ago || 0) * 1000)
                    });
                });

                const mergedStudents = [...incomingStudents.filter(st => st && !explicitlyRemovedStudentIds.has(st.id) && !explicitlyDeletedRooms.has(String(st.room_code || '').toUpperCase()))];
                const missingActiveForRehydrate = [];

                for (const [id, entry] of knownStudentsCache.entries()) {
                    const ageSec = Math.floor((nowMs - entry.seenAtMs) / 1000);
                    const stRoom = String(entry.student.room_code || '').toUpperCase();
                    if (ageSec > 90 || explicitlyRemovedStudentIds.has(id) || explicitlyDeletedRooms.has(stRoom)) {
                        knownStudentsCache.delete(id);
                        continue;
                    }
                    if (!incomingIds.has(id)) {
                        const preserved = {
                            ...entry.student,
                            seconds_ago: ageSec,
                            is_online: ageSec <= 40
                        };
                        mergedStudents.push(preserved);
                        if (ageSec <= 45) {
                            missingActiveForRehydrate.push(preserved);
                        }
                    }
                }

                // Jika container Vercel yang merespons belum memiliki data peserta aktif ini, sinkronkan secara instan di background
                if (missingActiveForRehydrate.length > 0) {
                    fetch('api/index.php?action=rehydrate_state', {
                        method: 'POST',
                        headers: {'Content-Type': 'application/json'},
                        body: JSON.stringify({
                            config: {
                                room_code: currentRoomCode,
                                rooms: cachedRooms,
                                deleted_rooms: Array.from(explicitlyDeletedRooms),
                                room_statuses: cachedRoomStatuses,
                                room_pins: cachedRoomPins,
                                unlock_pin: currentUnlockPin,
                                global_status: cachedGlobalStatus
                            },
                            students: missingActiveForRehydrate
                        })
                    }).catch(() => {});
                }

                mergedStudents.sort((a, b) => {
                    if (a.is_online !== b.is_online) return a.is_online ? -1 : 1;
                    return String(a.nim).localeCompare(String(b.nim));
                });

                cachedStudents = mergedStudents;
                cachedLogs = data.logs || [];

                updateRoomSelectorOptions();
                updateHeaderStatusUI();
                updateScopedStats();
                renderCurrentStudents();
                renderLogs(cachedLogs);
                renderRoomsManager();
            } catch (e) {
                console.error(e);
            }
        }

        function getFilteredStudents() {
            const searchEl = document.getElementById('searchStudent');
            let q = (searchEl?.value || '').trim().toLowerCase();
            // Abaikan dan bersihkan otomatis jika browser password manager mengisi email login (mis. admin@guard.edu) ke kolom pencarian
            if (q && (q.includes('@') || q === 'admin')) {
                searchEl.value = '';
                q = '';
            }
            return cachedStudents.filter(st => {
                const stRoom = (st.room_code || currentRoomCode || '-').toUpperCase();
                if (selectedRoom !== 'ALL' && stRoom !== selectedRoom) return false;
                if (currentFilter === 'ONLINE' && !st.is_online) return false;
                if (currentFilter === 'UNLOCKED' && st.status === 'LOCKED') return false;
                if (currentFilter === 'VIOLATION' && !(st.killed_processes_count > 0 || st.vpn_detected)) return false;
                if (q) {
                    const hay = `${st.nim} ${st.name} ${stRoom} ${st.hostname} ${st.ip}`.toLowerCase();
                    if (!hay.includes(q)) return false;
                }
                return true;
            });
        }

        function renderCurrentStudents() {
            const list = getFilteredStudents();
            const tbody = document.getElementById('studentTableBody');
            const grid = document.getElementById('studentGridContainer');

            if (!list.length) {
                const emptyMsg = selectedRoom === 'ALL'
                    ? `Belum ada perangkat mahasiswa yang terhubung.`
                    : `Belum ada perangkat mahasiswa yang terhubung di ruangan [${escapeHtml(selectedRoom)}].`;
                tbody.innerHTML = `<tr><td colspan="8" class="py-12 text-center text-slate-400 text-xs">${emptyMsg}</td></tr>`;
                grid.innerHTML = `<div class="col-span-full card-surface rounded-xl py-12 text-center text-slate-400 text-xs">${emptyMsg}</div>`;
                return;
            }

            tbody.innerHTML = list.map(st => {
                const stRoom = (st.room_code || currentRoomCode || '-').toUpperCase();
                const connBadge = st.is_online
                    ? `<span class="inline-flex items-center gap-1.5 text-slate-700 font-medium"><span class="w-2 h-2 rounded-full bg-emerald-600 status-dot-live"></span>Terhubung</span>`
                    : `<span class="inline-flex items-center gap-1.5 text-slate-400"><span class="w-2 h-2 rounded-full bg-slate-300"></span>Terputus (${st.seconds_ago}d)</span>`;

                const isLocked = st.status === 'LOCKED';
                let stateBadge = isLocked
                    ? `<span class="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">Terkunci</span>`
                    : `<span class="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200">Terbuka</span>`;

                if (st.vpn_detected) {
                    stateBadge += ` <span class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold bg-rose-50 text-rose-700 border border-rose-200 ml-1">VPN</span>`;
                }

                const extCount = st.quarantined_count || 0;
                const extSummary = extCount > 0
                    ? `<button onclick="openStudentModal('${escapeHtml(st.id)}')" class="text-slate-700 hover:text-slate-900 font-medium underline decoration-slate-300">${extCount} modul dikunci</button>`
                    : `<span class="text-slate-400">Bersih</span>`;

                const incidents = st.killed_processes_count || 0;
                const incidentCell = incidents > 0
                    ? `<span class="inline-flex items-center px-2 py-0.5 rounded font-mono text-[11px] font-semibold bg-rose-50 text-rose-700 border border-rose-200 tabular-nums">${incidents}x</span>`
                    : `<span class="text-slate-400 font-mono">0</span>`;

                const lockBtnClass = isLocked
                    ? 'px-2.5 py-1 rounded border border-slate-900 bg-slate-900 text-white font-medium transition'
                    : 'px-2.5 py-1 rounded border border-slate-200 bg-white hover:bg-slate-100 text-slate-700 font-medium transition';
                const unlockBtnClass = !isLocked
                    ? 'px-2.5 py-1 rounded border border-amber-600 bg-amber-600 text-white font-medium transition'
                    : 'px-2.5 py-1 rounded border border-slate-200 bg-white hover:bg-slate-100 text-slate-700 font-medium transition';

                return `
                    <tr class="hover:bg-slate-50/80 transition">
                        <td class="py-3 px-4">
                            <div class="font-semibold text-slate-900">${escapeHtml(st.name)}</div>
                            <div class="text-[11px] text-slate-500 font-mono mt-0.5">${escapeHtml(st.nim)}</div>
                        </td>
                        <td class="py-3 px-3">
                            <button onclick="onSelectRoom('${escapeHtml(stRoom)}')" class="inline-flex items-center px-2 py-0.5 rounded font-mono text-[11px] font-semibold bg-slate-100 hover:bg-slate-200 text-slate-800 border border-slate-200 transition">${escapeHtml(stRoom)}</button>
                        </td>
                        <td class="py-3 px-4 font-mono text-[11px] text-slate-600">
                            <div>${escapeHtml(st.hostname)}</div>
                            <div class="text-slate-400">${escapeHtml(st.ip || '127.0.0.1')}</div>
                        </td>
                        <td class="py-3 px-3">${connBadge}</td>
                        <td class="py-3 px-3">${stateBadge}</td>
                        <td class="py-3 px-3">${extSummary}</td>
                        <td class="py-3 px-3">${incidentCell}</td>
                        <td class="py-3 px-4 text-right whitespace-nowrap space-x-1">
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'LOCKED')" class="${lockBtnClass}">Kunci</button>
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'UNLOCKED')" class="${unlockBtnClass}">Buka</button>
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'REMOVE')" class="px-2 py-1 rounded text-slate-400 hover:text-rose-600 transition" title="Hapus sesi">✕</button>
                        </td>
                    </tr>
                `;
            }).join('');

            grid.innerHTML = list.map(st => {
                const stRoom = (st.room_code || currentRoomCode || '-').toUpperCase();
                const isLocked = st.status === 'LOCKED';
                const topBorder = !st.is_online
                    ? 'border-t-4 border-t-slate-300'
                    : (isLocked ? 'border-t-4 border-t-emerald-600' : 'border-t-4 border-t-amber-500');
                const statusLabel = isLocked ? 'Terkunci' : 'Terbuka';

                return `
                    <div class="card-surface rounded-xl p-4 flex flex-col justify-between ${topBorder}">
                        <div>
                            <div class="flex items-start justify-between gap-2">
                                <div>
                                    <div class="font-semibold text-xs text-slate-900">${escapeHtml(st.name)}</div>
                                    <div class="text-[11px] font-mono text-slate-500">${escapeHtml(st.nim)}</div>
                                </div>
                                <span class="px-1.5 py-0.5 rounded font-mono text-[10px] font-semibold bg-slate-100 text-slate-700 border border-slate-200">${escapeHtml(stRoom)}</span>
                            </div>

                            <div class="mt-3 pt-3 border-t border-slate-100 space-y-1 text-[11px] text-slate-600">
                                <div class="flex justify-between">
                                    <span class="text-slate-400">Host:</span>
                                    <span class="font-mono">${escapeHtml(st.hostname)}</span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-slate-400">Status Endpoint:</span>
                                    <span class="font-semibold ${isLocked ? 'text-emerald-700' : 'text-amber-700'}">${statusLabel}</span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-slate-400">Proteksi IDE:</span>
                                    <span class="font-mono">${st.quarantined_count || 0} modul</span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-slate-400">Insiden AI:</span>
                                    <span class="font-mono ${(st.killed_processes_count || 0) > 0 ? 'text-rose-600 font-semibold' : ''}">${st.killed_processes_count || 0}x</span>
                                </div>
                            </div>
                        </div>

                        <div class="mt-4 pt-2.5 border-t border-slate-100 flex items-center justify-between gap-1.5 text-[11px]">
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'LOCKED')" class="flex-1 py-1 rounded border ${isLocked ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 hover:bg-slate-50 text-slate-700'} font-medium">Kunci</button>
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'UNLOCKED')" class="flex-1 py-1 rounded border ${!isLocked ? 'border-amber-600 bg-amber-600 text-white' : 'border-slate-200 hover:bg-slate-50 text-slate-700'} font-medium">Buka</button>
                            <button onclick="openStudentModal('${escapeHtml(st.id)}')" class="px-2 py-1 rounded border border-slate-200 hover:bg-slate-50 text-slate-500">Detail</button>
                        </div>
                    </div>
                `;
            }).join('');
        }

        function renderRoomsManager() {
            const box = document.getElementById('roomsListManager');
            if (!box) return;
            if (!cachedRooms.length) {
                box.innerHTML = `<div class="p-6 text-center text-xs text-slate-400">Belum ada ruangan yang dibuat. Silakan tambah ruangan secara mandiri menggunakan form di atas atau tombol <strong>+ Tambah</strong> di header.</div>`;
                return;
            }
            const counts = {};
            cachedStudents.forEach(st => {
                const r = (st.room_code || currentRoomCode || '-').toUpperCase();
                counts[r] = (counts[r] || 0) + 1;
            });

            box.innerHTML = cachedRooms.map(r => {
                const count = counts[r] || 0;
                const status = (cachedRoomStatuses[r] || cachedGlobalStatus) === 'LOCKED' ? 'LOCKED' : 'UNLOCKED';
                const roomPin = cachedRoomPins[r] || currentUnlockPin || '202699';
                const hasCustomBl = Boolean(cachedRoomBlocklists && cachedRoomBlocklists[r]);
                const effDomainsCount = hasCustomBl
                    ? (cachedRoomBlocklists[r].domains || []).length
                    : (cachedGlobalBlocklist.domains || []).length;
                const blBadge = hasCustomBl
                    ? `<span class="px-2 py-0.5 rounded text-[10px] font-medium bg-indigo-50 text-indigo-800 border border-indigo-200">Blokir Khusus (${effDomainsCount} domain)</span>`
                    : `<span class="px-2 py-0.5 rounded text-[10px] font-medium bg-slate-100 text-slate-600 border border-slate-200">Blokir Global (${effDomainsCount} domain)</span>`;
                const badge = status === 'LOCKED'
                    ? `<span class="px-2 py-0.5 rounded text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">Terkunci</span>`
                    : `<span class="px-2 py-0.5 rounded text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200">Terbuka</span>`;

                return `
                    <div class="p-3.5 flex flex-wrap items-center justify-between gap-3 hover:bg-slate-50/80 transition text-xs">
                        <div class="flex flex-wrap items-center gap-2">
                            <span class="font-mono font-semibold text-slate-900 px-2.5 py-1 rounded bg-slate-100 border border-slate-200">${escapeHtml(r)}</span>
                            ${badge}
                            ${blBadge}
                            <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded bg-slate-50 border border-slate-200 text-slate-700">
                                <span class="text-[11px] text-slate-500">PIN:</span>
                                <strong class="font-mono text-slate-900">${escapeHtml(roomPin)}</strong>
                                <button onclick="copyText('${escapeHtml(roomPin)}', 'PIN Ruangan [${escapeHtml(r)}] disalin')" class="text-[10px] text-slate-400 hover:text-slate-800 underline ml-0.5">Salin</button>
                            </span>
                            <span class="text-slate-500">${count} PC</span>
                        </div>
                        <div class="flex flex-wrap items-center gap-1.5">
                            <button onclick="onSelectRoom('${escapeHtml(r)}'); switchTab('monitor');" class="px-2.5 py-1 rounded border border-slate-200 hover:bg-slate-100 text-slate-700 font-medium">Pantau</button>
                            <button onclick="openRoomRulesEditor('${escapeHtml(r)}')" class="px-2.5 py-1 rounded border border-slate-200 hover:bg-slate-100 text-slate-700 font-medium">Atur Blokir</button>
                            <button onclick="openEditRoomModal('${escapeHtml(r)}')" class="px-2.5 py-1 rounded border border-slate-200 hover:bg-slate-100 text-slate-700 font-medium">Edit & PIN</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'LOCKED')" class="px-2.5 py-1 rounded border ${status === 'LOCKED' ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 hover:bg-slate-100 text-slate-700'} font-medium">Kunci</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'UNLOCKED')" class="px-2.5 py-1 rounded border ${status === 'UNLOCKED' ? 'border-amber-600 bg-amber-600 text-white' : 'border-slate-200 hover:bg-slate-100 text-slate-700'} font-medium">Buka</button>
                            <button onclick="deleteRoom('${escapeHtml(r)}')" class="px-2 py-1 rounded text-slate-400 hover:text-rose-600" title="Hapus ruangan">✕</button>
                        </div>
                    </div>
                `;
            }).join('');
        }

        function openStudentModal(studentId) {
            const st = cachedStudents.find(x => x.id === studentId);
            if (!st) return;
            document.getElementById('modalStudentName').textContent = `${st.name} (${st.nim}) — [${st.room_code || currentRoomCode}]`;
            document.getElementById('modalStudentSub').textContent = `${st.hostname} · ${st.ip || '127.0.0.1'}`;
            document.getElementById('modalOs').textContent = st.os_info || 'Windows 10/11';
            document.getElementById('modalJoined').textContent = st.joined_at || '-';

            const box = document.getElementById('modalExtList');
            if (st.quarantined_list && st.quarantined_list.length) {
                box.innerHTML = st.quarantined_list.map(e => `<div class="py-1 border-b border-slate-200/60 last:border-0">${escapeHtml(e)}</div>`).join('');
            } else {
                box.innerHTML = `<div class="text-slate-400 font-sans">Tidak ada ekstensi AI yang terdeteksi pada perangkat ini.</div>`;
            }
            document.getElementById('studentModal').classList.remove('hidden');
        }

        function closeStudentModal() {
            document.getElementById('studentModal').classList.add('hidden');
        }

        function renderLogs(logs) {
            const box = document.getElementById('logListContainer');
            const filtered = selectedRoom === 'ALL'
                ? logs
                : logs.filter(l => !l.room_code || l.room_code === 'ALL' || l.room_code.toUpperCase() === selectedRoom);

            if (!filtered.length) {
                box.innerHTML = `<div class="py-12 text-center text-xs text-slate-400">Belum ada catatan insiden.</div>`;
                return;
            }
            box.innerHTML = filtered.map(l => {
                let badge = `<span class="px-2 py-0.5 rounded text-[10px] font-mono font-medium bg-slate-100 text-slate-700">${escapeHtml(l.type)}</span>`;
                if (l.severity === 'DANGER') {
                    badge = `<span class="px-2 py-0.5 rounded text-[10px] font-mono font-semibold bg-rose-50 text-rose-700 border border-rose-200">${escapeHtml(l.type)}</span>`;
                } else if (l.severity === 'WARNING') {
                    badge = `<span class="px-2 py-0.5 rounded text-[10px] font-mono font-semibold bg-amber-50 text-amber-800 border border-amber-200">${escapeHtml(l.type)}</span>`;
                }
                const roomTag = l.room_code && l.room_code !== 'ALL'
                    ? `<span class="px-1.5 py-0.5 rounded font-mono text-[10px] bg-slate-100 text-slate-600 border border-slate-200">${escapeHtml(l.room_code)}</span>`
                    : '';

                return `
                    <div class="px-5 py-3 flex flex-wrap items-center justify-between gap-4 hover:bg-slate-50/70 text-xs">
                        <div class="flex items-center gap-2.5 min-w-0">
                            ${badge}
                            ${roomTag}
                            <span class="font-medium text-slate-900">${escapeHtml(l.name)}</span>
                            <span class="font-mono text-[11px] text-slate-400">(${escapeHtml(l.nim)} · ${escapeHtml(l.hostname)})</span>
                            <span class="text-slate-600 truncate">${escapeHtml(l.message)}</span>
                        </div>
                        <span class="font-mono text-[11px] text-slate-400 tabular-nums shrink-0">${escapeHtml(l.timestamp)}</span>
                    </div>
                `;
            }).join('');
        }

        async function setGlobalStatus(status) {
            const res = await fetch('api/index.php?action=set_global_status', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({status, room_code: selectedRoom})
            });
            const data = await res.json();
            if (data.ok) {
                Object.keys(knownStudentsCache).forEach(id => {
                    const entry = knownStudentsCache[id];
                    if (entry && entry.student) {
                        const rCode = (entry.student.room_code || '').toUpperCase();
                        if (!selectedRoom || selectedRoom === 'ALL' || rCode === selectedRoom.toUpperCase()) {
                            entry.student.status = status;
                            entry.student.target_status = status;
                        }
                    }
                });
                const scopeText = selectedRoom === 'ALL' ? 'Seluruh ruangan' : `Ruangan [${selectedRoom}]`;
                showToast(status === 'LOCKED' ? `${scopeText} dikunci (Proteksi AI Aktif)` : `${scopeText} dibuka dan dipulihkan`);
                fetchDashboard();
            }
        }

        async function setRoomStatusDirect(roomCode, status) {
            const res = await fetch('api/index.php?action=set_global_status', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({status, room_code: roomCode})
            });
            const data = await res.json();
            if (data.ok) {
                Object.keys(knownStudentsCache).forEach(id => {
                    const entry = knownStudentsCache[id];
                    if (entry && entry.student) {
                        const rCode = (entry.student.room_code || '').toUpperCase();
                        if (rCode === (roomCode || '').toUpperCase()) {
                            entry.student.status = status;
                            entry.student.target_status = status;
                        }
                    }
                });
                showToast(`Ruangan [${roomCode}] diubah menjadi ${status}`);
                fetchDashboard();
            }
        }

        function promptNewRoom() {
            document.getElementById('roomModalTitle').textContent = 'Tambah Ruangan Baru';
            document.getElementById('roomModalSub').textContent = 'Buat Kode Ruangan baru beserta PIN Otorisasi khusus untuk ruangan ini.';
            document.getElementById('modalOldRoomCode').value = '';
            document.getElementById('modalRoomCodeInput').value = '';
            const nextPin = '2026' + String(cachedRooms.length + 1).padStart(2, '0');
            document.getElementById('modalRoomPinInput').value = nextPin;
            document.getElementById('roomModal').classList.remove('hidden');
            setTimeout(() => document.getElementById('modalRoomCodeInput').focus(), 50);
        }

        function promptEditSelectedRoom() {
            if (selectedRoom === 'ALL') {
                switchTab('settings');
                showToast('Pilih ruangan yang ingin diedit pada daftar di bawah ini, atau pilih dari dropdown atas.');
                return;
            }
            openEditRoomModal(selectedRoom);
        }

        function openEditRoomModal(roomCode) {
            const pin = cachedRoomPins[roomCode] || currentUnlockPin || '202699';
            document.getElementById('roomModalTitle').textContent = `Edit Ruangan [${roomCode}] & PIN`;
            document.getElementById('roomModalSub').textContent = 'Ubah Kode Ruangan atau ganti PIN Otorisasi Keluar untuk ruangan ini.';
            document.getElementById('modalOldRoomCode').value = roomCode;
            document.getElementById('modalRoomCodeInput').value = roomCode;
            document.getElementById('modalRoomPinInput').value = pin;
            document.getElementById('roomModal').classList.remove('hidden');
            setTimeout(() => document.getElementById('modalRoomCodeInput').focus(), 50);
        }

        function closeRoomModal() {
            document.getElementById('roomModal').classList.add('hidden');
        }

        async function submitRoomModal() {
            const oldRoom = document.getElementById('modalOldRoomCode').value.trim();
            const newRoom = document.getElementById('modalRoomCodeInput').value.trim();
            const roomPin = document.getElementById('modalRoomPinInput').value.trim();

            if (!newRoom) {
                showToast('Kode Ruangan tidak boleh kosong', true);
                return;
            }

            if (!oldRoom) {
                await createRoom(newRoom, roomPin);
                closeRoomModal();
                return;
            }

            const res = await fetch('api/index.php?action=rename_room', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({old_room_code: oldRoom, new_room_code: newRoom, room_pin: roomPin})
            });
            const data = await res.json();
            if (data.ok) {
                if (Array.isArray(data.rooms)) cachedRooms = data.rooms;
                if (data.room_statuses) cachedRoomStatuses = data.room_statuses;
                if (data.room_pins) cachedRoomPins = data.room_pins;
                if (selectedRoom === oldRoom) selectedRoom = data.room_code;
                closeRoomModal();
                showToast(`Ruangan [${data.room_code}] & PIN (${data.room_pin || roomPin}) berhasil disimpan`);
                fetchDashboard();
            } else {
                showToast(data.error || 'Gagal menyimpan perubahan ruangan', true);
            }
        }

        async function addRoomFromInput() {
            const inp = document.getElementById('newRoomInput');
            const pinInp = document.getElementById('newRoomPinInput');
            if (!inp || !inp.value.trim()) {
                showToast('Masukkan Kode Ruangan baru terlebih dahulu', true);
                return;
            }
            const pinVal = pinInp ? pinInp.value.trim() : '';
            await createRoom(inp.value.trim(), pinVal);
            inp.value = '';
            if (pinInp) pinInp.value = '';
        }

        async function createRoom(room_code, room_pin = '') {
            const norm = String(room_code || '').trim().toUpperCase().replace(/[^A-Z0-9_-]/g, '-');
            explicitlyDeletedRooms.delete(norm);
            const res = await fetch('api/index.php?action=add_room', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({room_code, room_pin})
            });
            const data = await res.json();
            if (data.ok) {
                explicitlyDeletedRooms.delete(data.room_code);
                if (Array.isArray(data.rooms)) cachedRooms = data.rooms;
                if (data.room_statuses) cachedRoomStatuses = data.room_statuses;
                if (data.room_pins) cachedRoomPins = data.room_pins;
                selectedRoom = data.room_code;
                showToast(`Ruangan [${data.room_code}] ditambahkan (PIN: ${data.room_pin})`);
                fetchDashboard();
            } else {
                showToast(data.error || 'Gagal menambahkan ruangan', true);
            }
        }

        async function deleteRoom(room_code) {
            const norm = String(room_code || '').trim().toUpperCase();
            const res = await fetch('api/index.php?action=delete_room', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({room_code})
            });
            const data = await res.json();
            if (data.ok) {
                explicitlyDeletedRooms.add(norm);
                for (const [id, entry] of knownStudentsCache.entries()) {
                    if (String(entry.student.room_code || '').toUpperCase() === norm) {
                        knownStudentsCache.delete(id);
                    }
                }
                if (Array.isArray(data.rooms)) cachedRooms = data.rooms;
                if (data.room_statuses) cachedRoomStatuses = data.room_statuses;
                if (data.room_pins) cachedRoomPins = data.room_pins;
                cachedStudents = cachedStudents.filter(st => (st.room_code || '').toUpperCase() !== norm);
                if (selectedRoom === room_code) selectedRoom = 'ALL';
                showToast(`Ruangan [${room_code}] berhasil dihapus`);
                fetchDashboard();
            } else {
                showToast(data.error || 'Gagal menghapus ruangan', true);
            }
        }

        async function sendStudentCmd(student_id, command) {
            if (command === 'REMOVE') {
                explicitlyRemovedStudentIds.add(student_id);
                knownStudentsCache.delete(student_id);
                setTimeout(() => explicitlyRemovedStudentIds.delete(student_id), 25000);
            } else if (knownStudentsCache.has(student_id)) {
                const entry = knownStudentsCache.get(student_id);
                entry.student.status = command;
                entry.student.target_status = command;
            }
            const res = await fetch('api/index.php?action=set_student_command', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({student_id, command})
            });
            const data = await res.json();
            if (data.ok) {
                showToast('Perintah berhasil dikirim ke perangkat peserta');
                fetchDashboard();
            }
        }

        async function saveConfig() {
            const payload = {
                room_code: document.getElementById('cfgRoomCode').value,
                unlock_pin: document.getElementById('cfgUnlockPin').value,
                admin_email: document.getElementById('cfgAdminEmail').value,
                admin_password: document.getElementById('cfgAdminPassword').value,
                block_doh: document.getElementById('cfgBlockDoh').checked,
                quarantine_extensions: document.getElementById('cfgQuarantineExt').checked,
                kill_processes: document.getElementById('cfgKillProcs').checked,
                block_vpn: document.getElementById('cfgBlockVpn').checked
            };
            const res = await fetch('api/index.php?action=update_config', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(payload)
            });
            const data = await res.json();
            if (data.ok) {
                document.getElementById('cfgAdminPassword').value = '';
                await syncStateToFirebaseIfAvailable(data.config, null);
                showToast('Pengaturan ruangan, kredensial & PIN disimpan');
                fetchDashboard();
            }
        }

        async function saveBlocklist() {
            const targetRoom = selectedRulesRoom || 'ALL';
            const domains = document.getElementById('inputDomains').value.split('\n').map(s => s.trim().toLowerCase()).filter(Boolean);
            const processes = document.getElementById('inputProcesses').value.split('\n').map(s => s.trim()).filter(Boolean);
            const ide_extensions = document.getElementById('inputExtensions').value.split('\n').map(s => s.trim()).filter(Boolean);

            const res = await fetch('api/index.php?action=update_blocklist', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({ room_code: targetRoom, domains, processes, ide_extensions })
            });
            const data = await res.json();
            if (data.ok) {
                if (data.blocklist) cachedGlobalBlocklist = data.blocklist;
                if (data.room_blocklists) cachedRoomBlocklists = data.room_blocklists;
                loadBlocklistIntoEditorForRoom(targetRoom);
                updateScopedStats();
                renderRoomsManager();
                await syncStateToFirebaseIfAvailable(null, data.blocklist);
                const label = targetRoom === 'ALL' ? 'Kebijakan Global (Semua Ruangan)' : `Kebijakan khusus Ruangan [${targetRoom}]`;
                showToast(`${label} berhasil disimpan (${domains.length} domain aktif)`);
                fetchDashboard();
            }
        }

        async function resetRoomBlocklistToGlobal() {
            const targetRoom = selectedRulesRoom || 'ALL';
            if (targetRoom === 'ALL') return;
            const res = await fetch('api/index.php?action=update_blocklist', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({ room_code: targetRoom, reset_room: true })
            });
            const data = await res.json();
            if (data.ok) {
                if (data.blocklist) cachedGlobalBlocklist = data.blocklist;
                if (data.room_blocklists) cachedRoomBlocklists = data.room_blocklists;
                loadBlocklistIntoEditorForRoom(targetRoom);
                updateScopedStats();
                renderRoomsManager();
                await syncStateToFirebaseIfAvailable(null, data.blocklist);
                showToast(`Ruangan [${targetRoom}] dikembalikan untuk mengikuti Kebijakan Global`);
                fetchDashboard();
            }
        }

        async function clearLogs() {
            await fetch('api/index.php?action=clear_logs', {method: 'POST'});
            showToast('Jurnal audit dikosongkan');
            fetchDashboard();
        }

        function escapeHtml(str) {
            return String(str || '').replace(/[&<>"']/g, s => ({
                '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
            }[s]));
        }

        checkExistingAuthSession();
        fetchDashboard();
        setInterval(fetchDashboard, 3000);
    </script>
</body>
</html>
