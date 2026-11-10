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
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
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
                <span class="text-[10px] font-mono font-medium px-1.5 py-0.5 rounded bg-slate-100 text-slate-600 border border-slate-200">v1.2</span>
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
                        <span>Kebijakan Daftar Blokir</span>
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
                    <span>PIN Otorisasi Pengawas</span>
                    <button onclick="togglePinVisibility()" id="btnTogglePin" class="text-slate-600 hover:text-slate-900 font-medium underline decoration-slate-300">Tampilkan</button>
                </div>
                <div class="flex items-center justify-between mt-0.5">
                    <span id="sidebarUnlockPin" class="font-mono text-sm font-semibold text-slate-900 tracking-wider">••••••</span>
                    <span class="text-[10px] text-slate-400">Aman untuk Proyektor</span>
                </div>
            </div>

            <div class="pt-2 border-t border-slate-200/80 flex items-center justify-between text-[11px] text-slate-400">
                <span class="flex items-center gap-1.5">
                    <span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span> Sinkronisasi Aktif
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
                <div class="flex items-center gap-2">
                    <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400 hidden sm:inline">Ruangan:</span>
                    <select id="headerRoomSelect" onchange="onSelectRoom(this.value)" class="rounded-lg border border-slate-300 bg-slate-50 hover:bg-white px-3 py-1.5 text-xs font-mono font-semibold text-slate-900 focus:outline-none focus:border-slate-500 transition">
                        <option value="ALL">Semua Ruangan</option>
                    </select>
                    <button onclick="promptNewRoom()" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1" title="Tambah Kode Ruangan Baru">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/></svg>
                        <span class="hidden md:inline">Ruangan</span>
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
                <button onclick="simulateStudent()" class="px-3 py-1.5 rounded-lg text-xs font-medium text-slate-700 bg-white hover:bg-slate-50 border border-slate-200 transition flex items-center gap-1.5">
                    <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/></svg>
                    Simulasi Peserta
                </button>

                <div class="h-5 w-px bg-slate-200 mx-0.5"></div>

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
                                <input type="text" id="searchStudent" oninput="renderCurrentStudents()" placeholder="Cari NIM, nama, ruangan, atau hostname..." class="pl-9 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:border-slate-400 w-64">
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

                <!-- VIEW 3: KEBIJAKAN DAFTAR BLOKIR -->
                <section id="view-rules" class="hidden space-y-4">
                    <div class="card-surface rounded-xl p-6 space-y-6">
                        <div class="flex flex-wrap items-center justify-between gap-4 pb-4 border-b border-slate-200">
                            <div>
                                <h2 class="text-sm font-semibold text-slate-900">Kebijakan Pemblokiran Endpoint (4-Layer Policy)</h2>
                                <p class="text-xs text-slate-500 mt-0.5">Daftar di bawah ini didistribusikan secara otomatis ke seluruh ruangan dan perangkat mahasiswa.</p>
                            </div>
                            <button onclick="saveBlocklist()" class="px-4 py-2 rounded-lg text-xs font-medium bg-slate-900 hover:bg-slate-800 text-white transition">
                                Simpan & Terapkan Kebijakan
                            </button>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                            <div class="space-y-2">
                                <div class="flex items-center justify-between">
                                    <label class="text-xs font-semibold text-slate-800">1. Domain Web AI & Endpoint API</label>
                                    <span class="text-[11px] font-mono text-slate-400">Hosts + DNS</span>
                                </div>
                                <p class="text-xs text-slate-500">Satu domain per baris. Dialihkan ke <code>0.0.0.0</code> dengan penguncian integritas file.</p>
                                <textarea id="inputDomains" rows="16" class="w-full rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:bg-white focus:border-slate-400 focus:outline-none"></textarea>
                            </div>

                            <div class="space-y-2">
                                <div class="flex items-center justify-between">
                                    <label class="text-xs font-semibold text-slate-800">2. Proses AI-IDE & Local LLM</label>
                                    <span class="text-[11px] font-mono text-slate-400">Process Guard</span>
                                </div>
                                <p class="text-xs text-slate-500">Nama executable (tanpa <code>.exe</code>) yang dihentikan otomatis setiap siklus pemeriksaan.</p>
                                <textarea id="inputProcesses" rows="16" class="w-full rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:bg-white focus:border-slate-400 focus:outline-none"></textarea>
                            </div>

                            <div class="space-y-2">
                                <div class="flex items-center justify-between">
                                    <label class="text-xs font-semibold text-slate-800">3. ID Ekstensi IDE (VS Code)</label>
                                    <span class="text-[11px] font-mono text-slate-400">Quarantine</span>
                                </div>
                                <p class="text-xs text-slate-500">Prefix folder ekstensi pada <code>~/.vscode/extensions</code> yang dipindahkan sementara.</p>
                                <textarea id="inputExtensions" rows="16" class="w-full rounded-lg bg-slate-50 border border-slate-200 p-3 text-xs font-mono text-slate-800 focus:bg-white focus:border-slate-400 focus:outline-none"></textarea>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- VIEW 4: MANAJEMEN MULTI-RUANGAN & PIN -->
                <section id="view-settings" class="hidden">
                    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
                        <!-- Kolom Kiri: Daftar Multi-Ruangan -->
                        <div class="lg:col-span-7 card-surface rounded-xl p-6 space-y-5">
                            <div class="border-b border-slate-200 pb-4 flex items-center justify-between gap-4">
                                <div>
                                    <h2 class="text-sm font-semibold text-slate-900">Manajemen Ruangan Ujian / Laboratorium</h2>
                                    <p class="text-xs text-slate-500 mt-0.5">Kelola banyak ruangan sekaligus. Mahasiswa cukup memasukkan Kode Ruangan yang sesuai pada aplikasi desktop mereka.</p>
                                </div>
                            </div>

                            <div class="flex items-center gap-2">
                                <input type="text" id="newRoomInput" placeholder="Masukkan Kode Ruangan baru (contoh: LAB-04, RUANG-301)..." class="flex-1 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono uppercase text-slate-900 focus:border-slate-500 focus:outline-none">
                                <button onclick="addRoomFromInput()" class="px-4 py-2 rounded-lg text-xs font-medium bg-slate-900 hover:bg-slate-800 text-white transition shrink-0">
                                    + Tambah Ruangan
                                </button>
                            </div>

                            <div id="roomsListManager" class="divide-y divide-slate-200 border border-slate-200 rounded-xl overflow-hidden bg-white">
                                <!-- Diisi dinamis oleh JS -->
                            </div>
                        </div>

                        <!-- Kolom Kanan: PIN Otorisasi & Modul Proteksi -->
                        <div class="lg:col-span-5 card-surface rounded-xl p-6 space-y-6">
                            <div class="border-b border-slate-200 pb-4">
                                <h2 class="text-sm font-semibold text-slate-900">Otorisasi PIN & Modul Proteksi</h2>
                                <p class="text-xs text-slate-500 mt-0.5">PIN digunakan oleh pengawas untuk membuka kunci PC mahasiswa secara manual atau saat mode offline.</p>
                            </div>

                            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">Ruangan Utama (Default)</label>
                                    <input type="text" id="cfgRoomCode" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="KUIS-AI-01">
                                </div>
                                <div>
                                    <label class="block text-xs font-medium text-slate-700 mb-1.5">PIN Otorisasi Pengawas</label>
                                    <input type="text" id="cfgUnlockPin" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-medium text-slate-900 focus:border-slate-500 focus:outline-none" value="202699">
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
                                    <span><strong>Terminasi Proses AI-IDE & Local LLM</strong> — Menghentikan proses Cursor, Windsurf, Ollama, dan LM Studio.</span>
                                </label>
                                <label class="flex items-start gap-3 text-xs text-slate-700 cursor-pointer">
                                    <input type="checkbox" id="cfgBlockVpn" class="mt-0.5 rounded border-slate-300 text-slate-900" checked>
                                    <span><strong>Deteksi Antarmuka VPN & Tunneling</strong> — Memberikan peringatan apabila terdeteksi adapter jaringan virtual aktif.</span>
                                </label>
                            </div>

                            <div class="pt-2">
                                <button onclick="saveConfig()" class="w-full py-2 px-4 rounded-lg text-xs font-medium bg-slate-900 hover:bg-slate-800 text-white transition">
                                    Simpan Pengaturan & PIN
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
                                            <span class="text-[11px] font-mono text-slate-500">~62 KB</span>
                                        </div>
                                        <div class="text-sm font-semibold text-slate-900 mt-1">AIBlockerSetup.exe</div>
                                        <p class="text-xs text-slate-600 mt-1.5 leading-relaxed">Memasang agen ke direktori <code>Program Files\AIBlockerGuard</code> dan membuat pintasan di Desktop mahasiswa.</p>
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
                                            <span class="text-[11px] font-mono text-slate-500">~52 KB</span>
                                        </div>
                                        <div class="text-sm font-semibold text-slate-900 mt-1">AIBlockerGuard.exe</div>
                                        <p class="text-xs text-slate-600 mt-1.5 leading-relaxed">Eksekusi tunggal tanpa proses instalasi. Cocok untuk komputer laboratorium kampus.</p>
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
                                    <strong class="text-slate-900">Distribusi Agen:</strong> Bagikan file <code>AIBlockerSetup.exe</code> atau <code>AIBlockerGuard.exe</code> kepada peserta.
                                </li>
                                <li>
                                    <strong class="text-slate-900">Kode Ruangan Fleksibel:</strong> Peserta mengisi NIM, Nama, dan <strong>Kode Ruangan</strong> sesuai kelasnya (misal <code>LAB-01</code>, <code>LAB-02</code>, atau <code>KUIS-AI-01</code>).
                                </li>
                                <li>
                                    <strong class="text-slate-900">Kontrol Per-Ruangan:</strong> Pengawas dapat memfilter ruangan tertentu dari dropdown di bagian atas untuk mengunci atau membuka kunci ruangan tersebut secara terpisah.
                                </li>
                                <li>
                                    <strong class="text-slate-900">Penyelesaian Sesi:</strong> Pengawas menekan tombol <strong>Buka Kunci & Pulihkan</strong> untuk ruangan terkait (atau memberikan PIN Otorisasi).
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

    <!-- Subtle Notification Toast -->
    <div id="toast" class="fixed bottom-5 right-5 hidden z-50 px-4 py-2.5 rounded-lg bg-slate-900 text-white text-xs font-medium shadow-lg"></div>

    <script>
        let currentTab = 'monitor';
        let currentFilter = 'ALL';
        let currentViewMode = 'table';
        let selectedRoom = 'ALL';
        let cachedStudents = [];
        let cachedLogs = [];
        let cachedRooms = ['KUIS-AI-01', 'LAB-01', 'LAB-02', 'LAB-03'];
        let cachedRoomStatuses = {};
        let cachedGlobalStatus = 'LOCKED';
        let blocklistLoaded = false;
        let pinVisible = false;
        let currentUnlockPin = '202699';
        let currentRoomCode = 'KUIS-AI-01';

        const endpointEl = document.getElementById('cloudEndpointUrl');
        if (endpointEl) {
            endpointEl.textContent = window.location.origin + window.location.pathname.replace(/\/index\.(php|html)$/, '').replace(/\/$/, '') + '/api/index.php';
        }

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

        function togglePinVisibility() {
            pinVisible = !pinVisible;
            document.getElementById('sidebarUnlockPin').textContent = pinVisible ? currentUnlockPin : '••••••';
            document.getElementById('btnTogglePin').textContent = pinVisible ? 'Sembunyikan' : 'Tampilkan';
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

        function onSelectRoom(room) {
            selectedRoom = room || 'ALL';
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
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
                const r = (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase();
                counts[r] = (counts[r] || 0) + 1;
            });

            const allRooms = Array.from(new Set([...cachedRooms, ...Object.keys(counts)]));
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
                gBadge.innerHTML = `<span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span> ${selectedRoom === 'ALL' ? ' tidak terkunci' : 'Ruangan ' + escapeHtml(selectedRoom) + ' Terbuka'}`;
                btnUnlock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 bg-slate-900 text-white shadow-sm';
                btnLock.className = 'px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900';
            }
        }

        function updateScopedStats() {
            const scoped = selectedRoom === 'ALL'
                ? cachedStudents
                : cachedStudents.filter(st => (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase() === selectedRoom);

            const onlineCount = scoped.filter(st => st.is_online).length;
            const lockedCount = scoped.filter(st => st.status === 'LOCKED' || st.status === 'DRY_RUN').length;
            const violationCount = scoped.reduce((acc, st) => acc + parseInt(st.killed_processes_count || 0, 10), 0);

            document.getElementById('statOnline').textContent = onlineCount;
            document.getElementById('statTotal').textContent = scoped.length;
            document.getElementById('statLocked').textContent = lockedCount;
            document.getElementById('statViolations').textContent = violationCount;

            document.getElementById('navBadgeOnline').textContent = onlineCount;
            document.getElementById('navBadgeViolations').textContent = violationCount;
        }

        async function fetchDashboard() {
            try {
                const res = await fetch('api/index.php?action=dashboard_state');
                const data = await res.json();
                if (!data.ok) return;

                document.getElementById('statDomains').textContent = data.stats.domains_count;
                document.getElementById('statProcs').textContent = data.stats.processes_count;
                document.getElementById('statExts').textContent = data.stats.extensions_count;

                currentRoomCode = data.config.room_code || 'KUIS-AI-01';
                currentUnlockPin = data.config.unlock_pin || '202699';
                cachedGlobalStatus = data.config.global_status || 'LOCKED';
                cachedRooms = Array.isArray(data.config.rooms) && data.config.rooms.length ? data.config.rooms : [currentRoomCode, 'LAB-01', 'LAB-02', 'LAB-03'];
                cachedRoomStatuses = data.config.room_statuses || {};

                if (pinVisible) {
                    document.getElementById('sidebarUnlockPin').textContent = currentUnlockPin;
                }
                const timePart = (data.server_time || '').split(' ')[1] || '--:--:--';
                document.getElementById('lastSyncClock').textContent = timePart;

                if (!blocklistLoaded && data.blocklist) {
                    document.getElementById('inputDomains').value = (data.blocklist.domains || []).join('\n');
                    document.getElementById('inputProcesses').value = (data.blocklist.processes || []).join('\n');
                    document.getElementById('inputExtensions').value = (data.blocklist.ide_extensions || []).join('\n');
                    document.getElementById('cfgRoomCode').value = currentRoomCode;
                    document.getElementById('cfgUnlockPin').value = currentUnlockPin;
                    blocklistLoaded = true;
                }

                cachedStudents = data.students || [];
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
            const q = (document.getElementById('searchStudent')?.value || '').trim().toLowerCase();
            return cachedStudents.filter(st => {
                const stRoom = (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase();
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
                const stRoom = (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase();
                const connBadge = st.is_online
                    ? `<span class="inline-flex items-center gap-1.5 text-slate-700 font-medium"><span class="w-2 h-2 rounded-full bg-emerald-600 status-dot-live"></span>Terhubung</span>`
                    : `<span class="inline-flex items-center gap-1.5 text-slate-400"><span class="w-2 h-2 rounded-full bg-slate-300"></span>Terputus (${st.seconds_ago}d)</span>`;

                let stateBadge = '';
                if (st.status === 'LOCKED') {
                    stateBadge = `<span class="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">Terkunci</span>`;
                } else if (st.status === 'DRY_RUN') {
                    stateBadge = `<span class="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-sky-50 text-sky-800 border border-sky-200">Simulasi</span>`;
                } else {
                    stateBadge = `<span class="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200">Terbuka</span>`;
                }

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

                const isLocked = st.status === 'LOCKED' || st.status === 'DRY_RUN';
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
                const stRoom = (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase();
                const isLocked = st.status === 'LOCKED' || st.status === 'DRY_RUN';
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
            const counts = {};
            cachedStudents.forEach(st => {
                const r = (st.room_code || currentRoomCode || 'KUIS-AI-01').toUpperCase();
                counts[r] = (counts[r] || 0) + 1;
            });

            box.innerHTML = cachedRooms.map(r => {
                const count = counts[r] || 0;
                const status = (cachedRoomStatuses[r] || cachedGlobalStatus) === 'LOCKED' ? 'LOCKED' : 'UNLOCKED';
                const badge = status === 'LOCKED'
                    ? `<span class="px-2 py-0.5 rounded text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200">Terkunci</span>`
                    : `<span class="px-2 py-0.5 rounded text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200">Terbuka</span>`;

                return `
                    <div class="p-3.5 flex flex-wrap items-center justify-between gap-3 hover:bg-slate-50/80 transition text-xs">
                        <div class="flex items-center gap-3">
                            <span class="font-mono font-semibold text-slate-900 px-2.5 py-1 rounded bg-slate-100 border border-slate-200">${escapeHtml(r)}</span>
                            ${badge}
                            <span class="text-slate-500">${count} perangkat terdaftar</span>
                        </div>
                        <div class="flex items-center gap-1.5">
                            <button onclick="onSelectRoom('${escapeHtml(r)}'); switchTab('monitor');" class="px-2.5 py-1 rounded border border-slate-200 hover:bg-slate-100 text-slate-700 font-medium">Pantau</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'LOCKED')" class="px-2.5 py-1 rounded border ${status === 'LOCKED' ? 'border-slate-900 bg-slate-900 text-white' : 'border-slate-200 hover:bg-slate-100 text-slate-700'} font-medium">Kunci</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'UNLOCKED')" class="px-2.5 py-1 rounded border ${status === 'UNLOCKED' ? 'border-amber-600 bg-amber-600 text-white' : 'border-slate-200 hover:bg-slate-100 text-slate-700'} font-medium">Buka</button>
                            ${cachedRooms.length > 1 ? `<button onclick="deleteRoom('${escapeHtml(r)}')" class="px-2 py-1 rounded text-slate-400 hover:text-rose-600" title="Hapus ruangan">✕</button>` : ''}
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
                showToast(`Ruangan [${roomCode}] diubah menjadi ${status}`);
                fetchDashboard();
            }
        }

        async function promptNewRoom() {
            const code = prompt('Masukkan Kode Ruangan baru (contoh: LAB-04, RUANG-A):');
            if (!code || !code.trim()) return;
            await createRoom(code.trim());
        }

        async function addRoomFromInput() {
            const inp = document.getElementById('newRoomInput');
            if (!inp || !inp.value.trim()) return;
            await createRoom(inp.value.trim());
            inp.value = '';
        }

        async function createRoom(room_code) {
            const res = await fetch('api/index.php?action=add_room', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({room_code})
            });
            const data = await res.json();
            if (data.ok) {
                selectedRoom = data.room_code;
                showToast(`Ruangan [${data.room_code}] berhasil ditambahkan`);
                fetchDashboard();
            }
        }

        async function deleteRoom(room_code) {
            const res = await fetch('api/index.php?action=delete_room', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({room_code})
            });
            const data = await res.json();
            if (data.ok) {
                if (selectedRoom === room_code) selectedRoom = 'ALL';
                showToast(`Ruangan [${room_code}] dihapus`);
                fetchDashboard();
            }
        }

        async function sendStudentCmd(student_id, command) {
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

        async function simulateStudent() {
            const res = await fetch('api/index.php?action=simulate_student', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({room_code: selectedRoom})
            });
            const data = await res.json();
            if (data.ok) {
                showToast(`Simulasi peserta (${data.simulated.name}) ditambahkan ke [${data.room_code}]`);
                fetchDashboard();
            }
        }

        async function saveConfig() {
            const payload = {
                room_code: document.getElementById('cfgRoomCode').value,
                unlock_pin: document.getElementById('cfgUnlockPin').value,
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
                showToast('Pengaturan ruangan & PIN pengawas disimpan');
                fetchDashboard();
            }
        }

        async function saveBlocklist() {
            const domains = document.getElementById('inputDomains').value.split('\n').map(s => s.trim()).filter(Boolean);
            const processes = document.getElementById('inputProcesses').value.split('\n').map(s => s.trim()).filter(Boolean);
            const ide_extensions = document.getElementById('inputExtensions').value.split('\n').map(s => s.trim()).filter(Boolean);

            const res = await fetch('api/index.php?action=update_blocklist', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({domains, processes, ide_extensions})
            });
            const data = await res.json();
            if (data.ok) {
                showToast('Kebijakan daftar blokir berhasil diperbarui');
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

        fetchDashboard();
        setInterval(fetchDashboard, 3000);
    </script>
</body>
</html>
