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
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: #F8FAFC;
            background-image:
                radial-gradient(at 100% 0%, rgba(224, 231, 255, 0.4) 0px, transparent 50%),
                radial-gradient(at 0% 0%, rgba(209, 250, 229, 0.3) 0px, transparent 50%),
                radial-gradient(at 50% 100%, rgba(241, 245, 249, 0.7) 0px, transparent 50%);
            background-attachment: fixed;
            color: #0F172A;
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
            text-rendering: optimizeLegibility;
        }
        .tabular-nums { font-variant-numeric: tabular-nums; }
        .card-surface {
            background: rgba(255, 255, 255, 0.94);
            backdrop-filter: blur(14px);
            -webkit-backdrop-filter: blur(14px);
            border: 1px solid rgba(226, 232, 240, 0.85);
            box-shadow: 0 1px 2px 0 rgba(15, 23, 42, 0.04), 0 0 0 1px rgba(255, 255, 255, 0.8) inset;
            transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
        }
        .card-surface:hover {
            border-color: rgba(203, 213, 225, 0.95);
            box-shadow: 0 4px 14px -2px rgba(15, 23, 42, 0.05), 0 2px 4px -1px rgba(15, 23, 42, 0.02);
        }
        .radar-beacon {
            position: relative;
            display: inline-flex;
        }
        .radar-beacon::before {
            content: '';
            position: absolute;
            inset: -3px;
            border-radius: 9999px;
            background: rgba(16, 185, 129, 0.35);
            animation: radar-ping 2s cubic-bezier(0, 0, 0.2, 1) infinite;
        }
        @keyframes radar-ping {
            0% { transform: scale(0.9); opacity: 1; }
            75%, 100% { transform: scale(2.2); opacity: 0; }
        }
        .btn-tactile {
            transition: transform 0.1s ease, box-shadow 0.15s ease, background 0.15s ease;
        }
        .btn-tactile:active {
            transform: scale(0.97);
        }
        .status-dot-live { box-shadow: 0 0 0 3px rgba(22, 163, 74, 0.25); }
        .safe-area-pb { padding-bottom: max(0.6rem, env(safe-area-inset-bottom)); }
        .no-scrollbar::-webkit-scrollbar { display: none; }
        .no-scrollbar { -ms-overflow-style: none; scrollbar-width: none; }
        .shadow-2xs { box-shadow: 0 1px 2px 0 rgba(15, 23, 42, 0.05); }
        ::-webkit-scrollbar { width: 6px; height: 6px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background: #CBD5E1; border-radius: 4px; }
        ::-webkit-scrollbar-thumb:hover { background: #94A3B8; }
        .radar-danger {
            position: relative;
            display: inline-flex;
        }
        .radar-danger::before {
            content: '';
            position: absolute;
            inset: -3px;
            border-radius: 9999px;
            background: rgba(225, 29, 72, 0.4);
            animation: radar-ping 1.5s cubic-bezier(0, 0, 0.2, 1) infinite;
        }
        @keyframes radar-ripple {
            0% { transform: scale(0.6); opacity: 0.85; }
            50% { opacity: 0.35; }
            100% { transform: scale(2.2); opacity: 0; }
        }
        .radar-wave-1 {
            animation: radar-ripple 2.6s cubic-bezier(0.16, 1, 0.3, 1) infinite;
        }
        .radar-wave-2 {
            animation: radar-ripple 2.6s cubic-bezier(0.16, 1, 0.3, 1) infinite;
            animation-delay: 0.8s;
            animation-fill-mode: backwards;
        }
        .radar-wave-3 {
            animation: radar-ripple 2.6s cubic-bezier(0.16, 1, 0.3, 1) infinite;
            animation-delay: 1.6s;
            animation-fill-mode: backwards;
        }
        .toggle-switch-track {
            transition: background-color 0.2s cubic-bezier(0.16, 1, 0.3, 1), border-color 0.2s ease;
        }
        .toggle-switch-thumb {
            transition: transform 0.2s cubic-bezier(0.16, 1, 0.3, 1);
        }
        @media (max-width: 640px) {
            input:not([type="checkbox"]):not([type="radio"]), select, textarea {
                font-size: 16px !important;
            }
        }
    </style>
</head>
<body class="min-h-screen text-slate-900 flex flex-col lg:flex-row antialiased selection:bg-slate-900 selection:text-white">

    <!-- ======================================================================= -->
    <!-- GERBANG OTENTIKASI PENGAWAS / ADMIN                                     -->
    <!-- ======================================================================= -->
    <div id="authGateOverlay" class="fixed inset-0 z-50 bg-[#0B0F19] flex items-center justify-center p-4" style="background-image: radial-gradient(rgba(255, 255, 255, 0.07) 1px, transparent 1px); background-size: 28px 28px;">
        <div class="w-full max-w-md bg-white rounded-3xl border border-slate-200/90 shadow-[0_25px_60px_-15px_rgba(0,0,0,0.5)] overflow-hidden">
            <div class="p-6 bg-gradient-to-b from-slate-900 to-slate-950 text-white border-b border-slate-800/80 flex items-center justify-between">
                <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-500 text-white flex items-center justify-center shadow-lg shadow-emerald-950/40 border border-emerald-400/30">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2.2" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 3l7 4v5c0 5-3.5 8.5-7 9-3.5-.5-7-4-7-9V7l7-4z"/>
                        </svg>
                    </div>
                    <div>
                        <div class="flex items-center gap-2">
                            <h1 class="text-sm font-bold tracking-tight text-white">GuardConsole</h1>
                            <span class="px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold bg-emerald-500/20 text-emerald-300 border border-emerald-400/30">v1.5</span>
                        </div>
                        <p class="text-[11px] text-slate-400 mt-0.5 font-medium flex items-center gap-1.5">
                            <span class="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
                            Konsol Pengawas Ujian Terenkripsi
                        </p>
                    </div>
                </div>
            </div>

            <form onsubmit="handleAdminLogin(event)" autocomplete="off" class="p-6 space-y-4">
                <div id="loginErrorBox" class="hidden p-3 rounded-xl bg-rose-50 border border-rose-200 text-xs text-rose-700 font-semibold flex items-center gap-2"></div>

                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5 uppercase tracking-wider text-[10px]">Email Pengawas / ID Admin</label>
                    <div class="relative">
                        <input type="text" id="loginEmail" name="guard_auth_user" required autocomplete="off" readonly onfocus="this.removeAttribute('readonly')" value="" placeholder="Masukkan email atau ID pengawas" class="w-full rounded-xl bg-slate-50 border border-slate-200 px-3.5 py-2.5 text-xs font-mono text-slate-900 focus:bg-white focus:border-slate-900 focus:ring-2 focus:ring-slate-900/10 focus:outline-none transition shadow-2xs">
                    </div>
                </div>

                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5 uppercase tracking-wider text-[10px]">Kata Sandi Pengawas</label>
                    <div class="relative">
                        <input type="password" id="loginPassword" name="guard_auth_secret" required autocomplete="new-password" readonly onfocus="this.removeAttribute('readonly')" value="" placeholder="Masukkan kata sandi pengawas" class="w-full rounded-xl bg-slate-50 border border-slate-200 px-3.5 py-2.5 text-xs font-mono text-slate-900 focus:bg-white focus:border-slate-900 focus:ring-2 focus:ring-slate-900/10 focus:outline-none transition shadow-2xs">
                    </div>
                </div>

                <div class="pt-1">
                    <button type="submit" id="btnLoginSubmit" class="btn-tactile w-full py-2.5 px-4 rounded-xl bg-gradient-to-b from-slate-900 to-slate-950 hover:from-slate-800 hover:to-slate-900 text-white text-xs font-bold transition shadow-md flex items-center justify-center gap-2">
                        <svg class="w-4 h-4 text-emerald-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9"/></svg>
                        <span>Buka Konsol Pengawas</span>
                    </button>
                </div>

                <div class="pt-3 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-400 font-medium">
                    <span class="flex items-center gap-1.5">
                        <span class="w-1.5 h-1.5 rounded-full bg-slate-300"></span>
                        Academic Integrity Engine
                    </span>
                    <span class="font-mono text-[10px]">TLS 1.3 Active</span>
                </div>
            </form>
        </div>
    </div>

    <!-- Mobile Top Header Bar (Visible on < lg screens) -->
    <header id="mobileTopBar" class="lg:hidden sticky top-0 z-40 bg-white/95 backdrop-blur-md border-b border-slate-200 px-3.5 py-2.5 flex items-center justify-between shadow-2xs">
        <div class="flex items-center gap-2.5">
            <button type="button" onclick="toggleMobileSidebar()" class="p-1.5 -ml-1 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 active:bg-slate-200 transition focus:outline-none" aria-label="Buka Menu Drawer">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/>
                </svg>
            </button>
            <div class="flex items-center gap-2">
                <div class="w-7 h-7 rounded-lg bg-slate-900 text-white flex items-center justify-center shadow-xs">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 3l7 4v5c0 5-3.5 8.5-7 9-3.5-.5-7-4-7-9V7l7-4z"/>
                    </svg>
                </div>
                <div>
                    <span class="font-bold text-sm tracking-tight text-slate-900 leading-tight">GuardConsole</span>
                    <span class="text-[9px] font-mono px-1 py-0.2 rounded bg-slate-100 text-slate-600 border border-slate-200 ml-1">v1.5</span>
                </div>
            </div>
        </div>
        <div class="flex items-center gap-2">
            <select id="mobileHeaderRoomSelect" onchange="onSelectRoom(this.value)" class="rounded-lg border border-slate-300 bg-slate-50 px-2.5 py-1 text-xs font-mono font-semibold text-slate-900 focus:outline-none max-w-[140px] truncate">
                <option value="ALL">Semua Ruangan</option>
            </select>
            <div id="mobileGlobalStateDot" class="w-2.5 h-2.5 rounded-full bg-emerald-600 shrink-0" title="Status Proteksi Ruangan"></div>
        </div>
    </header>

    <!-- Mobile Drawer Backdrop -->
    <div id="mobileNavBackdrop" onclick="toggleMobileSidebar(false)" class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs transition-opacity duration-300 opacity-0 pointer-events-none lg:hidden hidden"></div>

    <!-- Mobile Off-Canvas Navigation Drawer -->
    <aside id="mobileNavDrawer" class="fixed inset-y-0 left-0 z-50 w-[290px] max-w-[85vw] bg-white shadow-2xl flex flex-col justify-between transform -translate-x-full transition-transform duration-300 ease-in-out lg:hidden select-none">
        <div class="flex flex-col flex-1 min-h-0">
            <!-- Brand Header inside Drawer -->
            <div class="h-16 shrink-0 px-5 flex items-center justify-between border-b border-slate-200 bg-slate-50/50">
                <div class="flex items-center gap-2.5">
                    <div class="w-8 h-8 rounded-lg bg-slate-900 text-white flex items-center justify-center shadow-xs">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 3l7 4v5c0 5-3.5 8.5-7 9-3.5-.5-7-4-7-9V7l7-4z"/>
                        </svg>
                    </div>
                    <div>
                        <div class="text-sm font-bold tracking-tight text-slate-900 leading-none">GuardConsole</div>
                        <div class="text-[11px] text-slate-500 mt-1 font-medium">Exam Endpoint Control</div>
                    </div>
                </div>
                <button onclick="toggleMobileSidebar(false)" class="p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition" aria-label="Tutup Menu">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
            </div>

            <!-- Primary Navigation inside Drawer -->
            <div class="flex-1 overflow-y-auto p-3.5 space-y-1.5">
                <div class="px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Pengawasan Kelas</div>
                <button onclick="switchTab('monitor')" id="mob-drawer-nav-monitor" class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-semibold bg-slate-900 text-white transition shadow-2xs">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <rect x="2" y="3" width="20" height="14" rx="2" stroke-linecap="round" stroke-linejoin="round"/>
                            <path d="M8 21h8M12 17v4" stroke-linecap="round" stroke-linejoin="round"/>
                        </svg>
                        <span>Perangkat Mahasiswa</span>
                    </span>
                    <span id="mobDrawerBadgeOnline" class="px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-white/20 text-white tabular-nums">0</span>
                </button>

                <button onclick="switchTab('logs')" id="mob-drawer-nav-logs" class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z"/>
                        </svg>
                        <span>Log Audit & Insiden</span>
                    </span>
                    <span id="mobDrawerBadgeViolations" class="px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-50 text-rose-700 border border-rose-200 tabular-nums">0</span>
                </button>

                <div class="pt-4 pb-1 px-2.5 text-[11px] font-semibold uppercase tracking-wider text-slate-400">Konfigurasi & Distribusi</div>
                <button onclick="switchTab('rules')" id="mob-drawer-nav-rules" class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
                        </svg>
                        <span>Kebijakan Blokir Fleksibel</span>
                    </span>
                </button>

                <button onclick="switchTab('settings')" id="mob-drawer-nav-settings" class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                            <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                        </svg>
                        <span>Ruangan & PIN Pengawas</span>
                    </span>
                </button>

                <button onclick="switchTab('download')" id="mob-drawer-nav-download" class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition">
                    <span class="flex items-center gap-2.5">
                        <svg class="w-4 h-4 shrink-0 opacity-80" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
                        </svg>
                        <span>Distribusi Agen (.exe)</span>
                    </span>
                </button>
            </div>
        </div>

        <!-- Drawer Footer -->
        <div class="shrink-0 p-4 border-t border-slate-200 bg-slate-50/80 space-y-3">
            <div class="flex items-center justify-between">
                <span class="text-[11px] text-slate-500">PIN Master / Ruangan</span>
                <button onclick="copyActiveRoomPin()" class="text-xs text-slate-800 hover:text-slate-900 font-semibold underline decoration-slate-300">Salin PIN</button>
            </div>
            <div class="flex items-center justify-between text-xs pt-1 border-t border-slate-200/80">
                <span id="mobDrawerAdminEmail" class="truncate font-mono text-[11px] text-slate-600">Pengawas</span>
                <button onclick="handleAdminLogout()" class="text-rose-600 hover:text-rose-700 font-semibold px-2 py-1 rounded hover:bg-rose-50 transition">Keluar</button>
            </div>
        </div>
    </aside>

    <!-- Left Navigation Rail (Desktop: Visible only on lg screens) -->
    <aside class="hidden lg:flex lg:w-64 lg:h-screen lg:sticky lg:top-0 bg-white border-r border-slate-200 flex-col justify-between shrink-0 select-none overflow-hidden">
        <div class="flex flex-col flex-1 min-h-0">
            <!-- Brand Header (Exact 64px / h-16 shrink-0) -->
            <div class="h-16 shrink-0 px-5 flex items-center justify-between border-b border-slate-200">
                <div class="flex items-center gap-2.5">
                    <div class="w-8 h-8 rounded-lg bg-slate-900 text-white flex items-center justify-center shadow-xs">
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
    <div class="flex-1 flex flex-col min-w-0 lg:h-screen lg:overflow-hidden">

        <!-- Top Institutional Action Bar (Clean, Responsive, Command-Center Polish) -->
        <header class="min-h-16 py-2.5 sm:py-0 shrink-0 bg-white border-b border-slate-200 px-3.5 sm:px-6 flex flex-wrap lg:flex-nowrap items-center justify-between gap-3 z-20">
            <div class="flex flex-wrap items-center gap-2 sm:gap-3 min-w-0 flex-1">
                <div class="flex items-center gap-1.5 flex-wrap sm:flex-nowrap">
                    <span class="text-[11px] font-semibold uppercase tracking-wider text-slate-400 hidden sm:inline">Ruangan:</span>
                    <select id="headerRoomSelect" onchange="onSelectRoom(this.value)" class="rounded-lg border border-slate-300 bg-slate-50 hover:bg-white px-2.5 sm:px-3 py-1.5 text-xs font-mono font-semibold text-slate-900 focus:outline-none focus:border-slate-500 transition shadow-2xs">
                        <option value="ALL">Semua Ruangan</option>
                    </select>
                    <button onclick="promptNewRoom()" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1 shadow-2xs" title="Tambah Kode Ruangan Baru">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/></svg>
                        <span class="hidden md:inline">Tambah</span>
                    </button>
                    <button onclick="promptEditSelectedRoom()" id="btnHeaderEditRoom" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1 shadow-2xs" title="Ubah Nama / Edit Ruangan">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L6.832 19.82a4.5 4.5 0 01-1.897 1.13l-2.685.8.8-2.685a4.5 4.5 0 011.13-1.897L16.863 4.487zm0 0L19.5 7.125"/></svg>
                        <span class="hidden md:inline">Edit</span>
                    </button>
                    <button onclick="switchTab('settings')" class="px-2.5 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs font-medium text-slate-700 transition flex items-center gap-1 shadow-2xs" title="Buka Menu Manajemen Multi-Ruangan & PIN">
                        <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M10.5 6h9.75M10.5 6a1.5 1.5 0 11-3 0m3 0a1.5 1.5 0 10-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-9.75 0h9.75"/></svg>
                        <span class="hidden lg:inline">Kelola</span>
                    </button>
                </div>

                <div class="h-4 w-px bg-slate-200 hidden sm:block"></div>

                <span id="globalStateBadge" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200 whitespace-nowrap shadow-2xs">
                    <span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
                    Proteksi AI Aktif
                </span>
            </div>

            <!-- Global / Per-Room Classroom Controls -->
            <div class="flex items-center gap-2 shrink-0">
                <div class="inline-flex rounded-lg border border-slate-200 bg-slate-100 p-0.5 shadow-2xs">
                    <button onclick="setGlobalStatus('LOCKED')" id="btnGlobalLock" class="px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-semibold transition flex items-center gap-1.5 bg-slate-900 text-white shadow-xs">
                        <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
                        <span id="lblBtnLock">Kunci Semua Ruangan</span>
                    </button>
                    <button onclick="setGlobalStatus('UNLOCKED')" id="btnGlobalUnlock" class="px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900">
                        <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 019.9-1"/></svg>
                        <span id="lblBtnUnlock">Buka Kunci & Pulihkan</span>
                    </button>
                </div>
            </div>
        </header>

        <!-- Scrollable Workspace Body -->
        <div class="flex-1 overflow-y-auto">
            <main class="p-3.5 sm:p-5 lg:p-6 max-w-[1400px] w-full mx-auto space-y-4 sm:space-y-6 pb-24 lg:pb-8">

                <!-- KPI Telemetry Strip (Artisan Command Caliber) -->
                <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
                    <!-- 1. Perangkat Terhubung -->
                    <div class="card-surface rounded-2xl p-4 sm:p-5 relative overflow-hidden group">
                        <div class="absolute -right-6 -bottom-6 w-24 h-24 rounded-full bg-slate-900/[0.03] group-hover:bg-slate-900/[0.06] transition-colors pointer-events-none"></div>
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span class="truncate font-semibold tracking-tight text-slate-600">Perangkat Terhubung</span>
                            <span class="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[10px] font-mono font-bold bg-slate-100 text-slate-700 border border-slate-200/80">
                                <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                                LIVE
                            </span>
                        </div>
                        <div class="mt-3 flex items-baseline gap-2 tabular-nums">
                            <span id="statOnline" class="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight leading-none">0</span>
                            <span class="text-xs text-slate-400 font-medium">/ <span id="statTotal" class="font-bold text-slate-700">0</span> PC terdaftar</span>
                        </div>
                        <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500 font-medium">
                            <span class="flex items-center gap-1.5">
                                <svg class="w-3.5 h-3.5 text-slate-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25m18 0A2.25 2.25 0 0018.75 3H5.25A2.25 2.25 0 003 5.25m18 0V12a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 12V5.25"/></svg>
                                <span>Konektivitas Ujian</span>
                            </span>
                            <span class="text-[10px] font-mono text-slate-400">Heartbeat 3s</span>
                        </div>
                    </div>

                    <!-- 2. Status Terkunci -->
                    <div class="card-surface rounded-2xl p-4 sm:p-5 relative overflow-hidden group">
                        <div class="absolute -right-6 -bottom-6 w-24 h-24 rounded-full bg-emerald-500/[0.04] group-hover:bg-emerald-500/[0.08] transition-colors pointer-events-none"></div>
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span class="truncate font-semibold tracking-tight text-slate-600">Status Terkunci</span>
                            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-mono font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                                <svg class="w-3 h-3 text-emerald-600" fill="none" stroke="currentColor" stroke-width="2.5" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5"/></svg>
                                ENFORCED
                            </span>
                        </div>
                        <div class="mt-3 flex items-baseline gap-2 tabular-nums">
                            <span id="statLocked" class="text-2xl sm:text-3xl font-extrabold text-emerald-600 tracking-tight leading-none">0</span>
                            <span class="text-xs text-slate-400 font-medium">endpoint aman</span>
                        </div>
                        <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500 font-medium">
                            <span class="flex items-center gap-1.5">
                                <svg class="w-3.5 h-3.5 text-emerald-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z"/></svg>
                                <span>Proteksi AI Aktif</span>
                            </span>
                            <span class="text-[10px] font-mono text-emerald-600 font-bold">100% Target</span>
                        </div>
                    </div>

                    <!-- 3. Intersepsi AI -->
                    <div class="card-surface rounded-2xl p-4 sm:p-5 relative overflow-hidden group">
                        <div class="absolute -right-6 -bottom-6 w-24 h-24 rounded-full bg-rose-500/[0.04] group-hover:bg-rose-500/[0.08] transition-colors pointer-events-none"></div>
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span class="truncate font-semibold tracking-tight text-slate-600">Intersepsi AI</span>
                            <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-mono font-bold bg-rose-50 text-rose-700 border border-rose-200">
                                <span class="w-1.5 h-1.5 rounded-full bg-rose-500 animate-pulse"></span>
                                SIEM LOG
                            </span>
                        </div>
                        <div class="mt-3 flex items-baseline gap-2 tabular-nums">
                            <span id="statViolations" class="text-2xl sm:text-3xl font-extrabold text-rose-600 tracking-tight leading-none">0</span>
                            <span class="text-xs text-slate-400 font-medium">insiden dicegat</span>
                        </div>
                        <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500 font-medium">
                            <span class="flex items-center gap-1.5">
                                <svg class="w-3.5 h-3.5 text-rose-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"/></svg>
                                <span>Aplikasi / Domain Diblokir</span>
                            </span>
                            <span class="text-[10px] font-mono text-slate-400">Zero Trust</span>
                        </div>
                    </div>

                    <!-- 4. Cakupan Kebijakan -->
                    <div class="card-surface rounded-2xl p-4 sm:p-5 relative overflow-hidden group">
                        <div class="absolute -right-6 -bottom-6 w-24 h-24 rounded-full bg-indigo-500/[0.04] group-hover:bg-indigo-500/[0.08] transition-colors pointer-events-none"></div>
                        <div class="flex items-center justify-between text-xs text-slate-500 font-medium">
                            <span class="truncate font-semibold tracking-tight text-slate-600">Cakupan Kebijakan</span>
                            <span class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-mono font-bold bg-indigo-50 text-indigo-700 border border-indigo-200">
                                ACTIVE
                            </span>
                        </div>
                        <div class="mt-3 flex items-baseline gap-1.5 tabular-nums">
                            <span id="statDomains" class="text-2xl sm:text-3xl font-extrabold text-indigo-600 tracking-tight leading-none">0</span>
                            <span class="text-xs text-slate-400 font-medium">domain AI</span>
                        </div>
                        <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500 font-medium">
                            <div class="flex items-center gap-1 font-mono text-[10px]">
                                <span class="px-1.5 py-0.2 rounded bg-slate-100 text-slate-700"><span id="statProcs">0</span> proc</span>
                                <span class="px-1.5 py-0.2 rounded bg-slate-100 text-slate-700"><span id="statExts">0</span> ext</span>
                            </div>
                            <span class="text-[10px] text-indigo-600 font-bold hover:underline cursor-pointer" onclick="switchTab('rules')">Atur &rarr;</span>
                        </div>
                    </div>
                </div>

                <!-- VIEW 1: PERANGKAT MAHASISWA -->
                <section id="view-monitor" class="space-y-4">
                    <div class="card-surface rounded-xl p-3 sm:p-3.5 flex flex-col md:flex-row md:items-center justify-between gap-3">
                        <div class="flex flex-col sm:flex-row sm:items-center gap-2.5 flex-1 min-w-0">
                            <div class="relative w-full sm:w-72 lg:w-80 shrink-0">
                                <svg class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z"/></svg>
                                <input type="search" id="searchStudent" name="guard_student_search_filter" autocomplete="off" readonly onfocus="this.removeAttribute('readonly'); if(this.value.includes('@')){this.value='';renderCurrentStudents();}" spellcheck="false" oninput="renderCurrentStudents()" placeholder="Cari NIM, nama, ruangan, host..." class="w-full pl-9 pr-3 py-2 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder-slate-400 focus:outline-none focus:border-slate-500 bg-white shadow-2xs">
                            </div>

                            <div class="overflow-x-auto no-scrollbar py-0.5 flex items-center">
                                <div class="inline-flex rounded-lg border border-slate-200 p-0.5 bg-slate-50 text-xs shrink-0">
                                    <button onclick="setFilter('ALL')" id="filter-ALL" class="filter-pill px-3 py-1.5 rounded-md font-semibold bg-white text-slate-900 shadow-2xs whitespace-nowrap">Semua</button>
                                    <button onclick="setFilter('ONLINE')" id="filter-ONLINE" class="filter-pill px-3 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 whitespace-nowrap">Online</button>
                                    <button onclick="setFilter('UNLOCKED')" id="filter-UNLOCKED" class="filter-pill px-3 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 whitespace-nowrap">Terbuka</button>
                                    <button onclick="setFilter('VIOLATION')" id="filter-VIOLATION" class="filter-pill px-3 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 whitespace-nowrap">Ada Insiden</button>
                                </div>
                            </div>
                        </div>

                        <div class="flex items-center justify-between sm:justify-end gap-2 border-t md:border-t-0 pt-2 md:pt-0 border-slate-100">
                            <span class="text-xs text-slate-500 font-medium">Tampilan:</span>
                            <div class="inline-flex rounded-lg border border-slate-200 p-0.5 bg-slate-50 text-xs shrink-0">
                                <button onclick="setViewMode('table')" id="viewMode-table" class="px-2.5 py-1.5 rounded-md font-medium bg-white text-slate-900 shadow-2xs flex items-center gap-1.5">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M3.75 5.25h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5m-16.5 4.5h16.5"/></svg>
                                    <span>Tabel</span>
                                </button>
                                <button onclick="setViewMode('grid')" id="viewMode-grid" class="px-2.5 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/></svg>
                                    <span>Denah Kelas</span>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div id="studentTableContainer" class="card-surface rounded-xl overflow-hidden shadow-xs">
                        <div class="overflow-x-auto">
                            <table class="w-full text-left border-collapse">
                                <thead>
                                    <tr class="border-b border-slate-200 bg-slate-50/70 text-[11px] font-semibold uppercase tracking-wider text-slate-500 whitespace-nowrap">
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

                    <div id="studentGridContainer" class="hidden grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3.5 sm:gap-4"></div>
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
                            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2.5">
                                <label class="text-xs font-semibold text-slate-800">2. Daftar Domain Individual & Tambah Domain Kustom</label>
                                <div class="flex items-center gap-2 w-full sm:w-auto">
                                    <input type="text" id="customDomainInput" placeholder="Tambah domain (misal: notion.so, discord.com)..." class="flex-1 sm:w-64 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono text-slate-900 focus:border-slate-600 focus:outline-none shadow-2xs">
                                    <button onclick="addCustomDomain()" class="px-3.5 py-2 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-semibold transition shrink-0 shadow-xs">
                                        + Tambah
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

                            <div class="grid grid-cols-1 sm:grid-cols-12 gap-2.5">
                                <input type="text" id="newRoomInput" placeholder="Kode Ruangan baru (misal: LAB-04)..." class="sm:col-span-5 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono uppercase font-semibold text-slate-900 focus:border-slate-500 focus:outline-none shadow-2xs">
                                <input type="text" id="newRoomPinInput" placeholder="PIN Keluar Ruangan (opsional, 6 digit)" class="sm:col-span-4 rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono text-slate-900 focus:border-slate-500 focus:outline-none shadow-2xs">
                                <button onclick="addRoomFromInput()" class="sm:col-span-3 px-3 py-2 rounded-lg text-xs font-semibold bg-slate-900 hover:bg-slate-800 text-white transition shrink-0 shadow-xs">
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

    <!-- Mobile Bottom Navigation Bar (Thumb-Zone Navigation on < lg screens) -->
    <nav id="mobileBottomNav" class="lg:hidden fixed bottom-0 inset-x-0 z-40 bg-white/95 backdrop-blur-md border-t border-slate-200 safe-area-pb shadow-lg flex items-center justify-around px-1 py-1 select-none">
        <button onclick="switchTab('monitor')" id="mob-nav-monitor" class="flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-900 transition relative">
            <div class="relative">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                    <rect x="2" y="3" width="20" height="14" rx="2" stroke-linecap="round" stroke-linejoin="round"/>
                    <path d="M8 21h8M12 17v4" stroke-linecap="round" stroke-linejoin="round"/>
                </svg>
                <span id="mobBadgeOnline" class="absolute -top-1.5 -right-2.5 px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold bg-slate-900 text-white tabular-nums">0</span>
            </div>
            <span class="text-[10px] font-bold tracking-tight leading-none">Pantau</span>
        </button>

        <button onclick="switchTab('logs')" id="mob-nav-logs" class="flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-400 hover:text-slate-700 transition relative">
            <div class="relative">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z"/>
                </svg>
                <span id="mobBadgeViolations" class="absolute -top-1.5 -right-2.5 px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold bg-rose-600 text-white tabular-nums">0</span>
            </div>
            <span class="text-[10px] font-medium tracking-tight leading-none">Log</span>
        </button>

        <button onclick="switchTab('rules')" id="mob-nav-rules" class="flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-400 hover:text-slate-700 transition">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
            </svg>
            <span class="text-[10px] font-medium tracking-tight leading-none">Kebijakan</span>
        </button>

        <button onclick="switchTab('settings')" id="mob-nav-settings" class="flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-400 hover:text-slate-700 transition">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
            <span class="text-[10px] font-medium tracking-tight leading-none">Ruangan</span>
        </button>

        <button onclick="switchTab('download')" id="mob-nav-download" class="flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-400 hover:text-slate-700 transition">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            <span class="text-[10px] font-medium tracking-tight leading-none">Unduh</span>
        </button>
    </nav>

    <!-- Modal Detail Perangkat Mahasiswa -->
    <div id="studentModal" class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs hidden z-50 flex items-center justify-center p-3 sm:p-4">
        <div class="bg-white rounded-2xl border border-slate-200 shadow-2xl max-w-lg w-full max-h-[90vh] flex flex-col overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between shrink-0 bg-white">
                <div>
                    <h3 id="modalStudentName" class="text-sm font-semibold text-slate-900">Detail Perangkat Peserta</h3>
                    <p id="modalStudentSub" class="text-xs text-slate-500 font-mono mt-0.5"></p>
                </div>
                <button onclick="closeStudentModal()" class="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition" aria-label="Tutup Dialog">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
            </div>
            <div class="p-4 sm:p-5 space-y-4 text-xs overflow-y-auto flex-1">
                <div class="grid grid-cols-2 gap-3 bg-slate-50 p-3 rounded-xl border border-slate-200/80">
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
                    <div id="modalExtList" class="p-3 rounded-xl border border-slate-200 bg-slate-50 font-mono text-[11px] text-slate-700 max-h-36 overflow-y-auto space-y-1"></div>
                </div>
                <div>
                    <div class="flex items-center justify-between mb-1.5">
                        <span class="font-semibold text-slate-800">Riwayat Insiden & Aktivitas Terlarang:</span>
                        <span id="modalIncidentCountBadge" class="px-2 py-0.5 rounded text-[10px] font-mono font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">0 Insiden</span>
                    </div>
                    <div id="modalIncidentList" class="p-3 rounded-xl border border-slate-200 bg-slate-50 text-[11px] text-slate-700 max-h-44 overflow-y-auto space-y-2"></div>
                </div>
            </div>
            <div class="px-5 py-3.5 bg-slate-50 border-t border-slate-200 flex items-center justify-between shrink-0">
                <button id="modalBtnUpdate" class="btn-tactile px-3 py-1.5 rounded-lg border border-indigo-200 bg-indigo-50 hover:bg-indigo-100 text-indigo-700 text-xs font-semibold shadow-2xs flex items-center gap-1.5 transition" title="Kirim perintah self-update ke PC ini">
                    <svg class="w-3.5 h-3.5 text-indigo-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0l3.181 3.183a8.25 8.25 0 0013.803-3.7M4.031 9.865a8.25 8.25 0 0113.803-3.7l3.181 3.182m0-4.991v4.99"/></svg>
                    <span>Perbarui GuardAgent (Update)</span>
                </button>
                <button onclick="closeStudentModal()" class="px-4 py-2 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-semibold transition shadow-xs">Tutup</button>
            </div>
        </div>
    </div>

    <!-- Modal Tambah / Edit Ruangan & PIN Otorisasi -->
    <div id="roomModal" class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs hidden z-50 flex items-center justify-center p-3 sm:p-4">
        <div class="bg-white rounded-2xl border border-slate-200 shadow-2xl max-w-md w-full max-h-[90vh] flex flex-col overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between shrink-0 bg-white">
                <div>
                    <h3 id="roomModalTitle" class="text-sm font-semibold text-slate-900">Edit Ruangan & PIN Otorisasi</h3>
                    <p id="roomModalSub" class="text-xs text-slate-500 mt-0.5">Atur Kode Ruangan dan PIN Otorisasi untuk keluar/membuka kunci ruangan ini.</p>
                </div>
                <button onclick="closeRoomModal()" class="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 transition" aria-label="Tutup Dialog">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
                </button>
            </div>
            <div class="p-4 sm:p-5 space-y-4 text-xs overflow-y-auto flex-1">
                <input type="hidden" id="modalOldRoomCode" value="">
                <div>
                    <label class="block text-xs font-semibold text-slate-700 mb-1.5">Kode Ruangan</label>
                    <input type="text" id="modalRoomCodeInput" placeholder="Contoh: LAB-01, RUANG-UJIAN-A" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono uppercase font-semibold text-slate-900 focus:border-slate-500 focus:outline-none shadow-2xs">
                </div>
                <div>
                    <label class="block text-xs font-semibold text-slate-700 mb-1.5">PIN Otorisasi Keluar / Buka Kunci Ruangan</label>
                    <input type="text" id="modalRoomPinInput" placeholder="Contoh: 202601 (4-12 karakter)" class="w-full rounded-lg bg-white border border-slate-300 px-3 py-2 text-xs font-mono font-semibold text-slate-900 focus:border-slate-500 focus:outline-none shadow-2xs">
                    <p class="text-[11px] text-slate-400 mt-1">Digunakan pada PC di ruangan ini ketika pengawas ingin membuka kunci manual atau menghentikan agen.</p>
                </div>
            </div>
            <div class="px-5 py-3.5 bg-slate-50 border-t border-slate-200 flex justify-end gap-2.5 shrink-0">
                <button onclick="closeRoomModal()" class="px-3.5 py-2 rounded-lg border border-slate-300 bg-white hover:bg-slate-50 text-slate-700 text-xs font-medium transition">Batal</button>
                <button onclick="submitRoomModal()" class="px-4 py-2 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs font-semibold transition shadow-xs">Simpan Ruangan</button>
            </div>
        </div>
    </div>

    <!-- Subtle Notification Toast -->
    <div id="toast" class="fixed bottom-20 lg:bottom-5 right-4 sm:right-5 hidden z-50 px-4 py-2.5 rounded-xl bg-slate-900 text-white text-xs font-semibold shadow-xl border border-slate-800 transition"></div>

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
                const savedToken = sessionStorage.getItem('guard_admin_token');
                if (user && user.email && savedToken) {
                    window.completeAdminAuth(user.email, savedToken);
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
        const recentLocalStudentCommands = new Map(); // id -> { status, ts }
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
            const mobEmailEl = document.getElementById('mobDrawerAdminEmail');
            if (mobEmailEl) mobEmailEl.textContent = email;
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

        const GROUP_BRAND_ICONS = {
            chatgpt: `<svg class="w-5 h-5 text-[#10A37F]" viewBox="0 0 24 24" fill="currentColor"><path d="M22.28 9.82a5.98 5.98 0 0 0-.51-4.91 6.05 6.05 0 0 0-6.51-2.9A6.06 6.06 0 0 0 4.98 4.18a5.98 5.98 0 0 0-4 2.9 6.05 6.05 0 0 0 .75 7.1 5.98 5.98 0 0 0 .51 4.91 6.05 6.05 0 0 0 6.51 2.9A5.98 5.98 0 0 0 13.26 24a6.05 6.05 0 0 0 5.77-4.2 5.99 5.99 0 0 0 4-2.9 6.05 6.05 0 0 0-.75-7.08zm-9.02 12.61a4.48 4.48 0 0 1-2.88-1.04l.14-.08 4.78-2.76a.8.8 0 0 0 .4-.68v-6.74l2.02 1.17a.07.07 0 0 1 .04.05v5.58a4.5 4.5 0 0 1-4.5 4.5zm-9.66-4.52a4.47 4.47 0 0 1-.53-3.01l.14.08 4.78 2.76a.77.77 0 0 0 .78 0l5.85-3.37v2.33a.08.08 0 0 1-.04.06L9.74 19.95a4.5 4.5 0 0 1-6.14-2.04zM2.34 8.78a4.48 4.48 0 0 1 2.34-1.96v5.6a.79.79 0 0 0 .39.68l5.85 3.37-2.02 1.16a.08.08 0 0 1-.07 0l-4.83-2.79A4.5 4.5 0 0 1 2.34 8.78zm16.6 3.86L13.09 9.27l2.02-1.16a.08.08 0 0 1 .07 0l4.83 2.79a4.5 4.5 0 0 1-.68 8.1v-5.67a.79.79 0 0 0-.4-.69zm2.01-3.03l-.14-.08-4.78-2.78a.78.78 0 0 0-.78 0L9.41 10.13V7.8a.08.08 0 0 1 .03-.06l4.94-2.86a4.5 4.5 0 0 1 6.58 4.71zm-9.69-3.22v6.74l-2.02-1.17a.07.07 0 0 1-.04-.05V3.35a4.5 4.5 0 0 1 7.37-3.45l-.14.08-4.78 2.76a.8.8 0 0 0-.39.68zM10.53 9.47l2.92-1.69 2.93 1.69v3.37l-2.93 1.69-2.92-1.69z"/></svg>`,
            claude: `<svg class="w-5 h-5 text-[#CC785C]" viewBox="0 0 24 24" fill="currentColor"><path d="M4.5 10.5C3.67 10.5 3 11.17 3 12s.67 1.5 1.5 1.5h2.5c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5H4.5zm12.5 0c-.83 0-1.5.67-1.5 1.5s.67 1.5 1.5 1.5h2.5c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5H17zM10.5 4.5c0-.83.67-1.5 1.5-1.5s1.5.67 1.5 1.5v2.5c0 .83-.67 1.5-1.5 1.5s-1.5-.67-1.5-1.5V4.5zm0 12.5c0-.83.67-1.5 1.5-1.5s1.5.67 1.5 1.5v2.5c0 .83-.67 1.5-1.5 1.5s-1.5-.67-1.5-1.5V17zM6.62 6.62c-.59-.59-1.54-.59-2.12 0s-.59 1.54 0 2.12l1.77 1.77c.59.59 1.54.59 2.12 0s.59-1.54 0-2.12L6.62 6.62zm10.76 10.76c-.59-.59-1.54-.59-2.12 0s-.59 1.54 0 2.12l1.77 1.77c.59.59 1.54.59 2.12 0s.59-1.54 0-2.12l-1.77-1.77zM6.62 17.38l-1.77-1.77c-.59-.59-.59-1.54 0-2.12s1.54-.59 2.12 0l1.77 1.77c.59.59.59 1.54 0 2.12-.59.58-1.54.58-2.12 0zm10.76-10.76l1.77-1.77c.59-.59 1.54-.59 2.12 0s.59 1.54 0 2.12l-1.77 1.77c-.59.59-1.54.59-2.12 0s-.59-1.54 0-2.12z"/></svg>`,
            gemini: `<svg class="w-5 h-5" viewBox="0 0 24 24" fill="none"><path d="M12 24C12 17.37 6.63 12 0 12C6.63 12 12 6.63 12 0C12 6.63 17.37 12 24 12C17.37 12 12 17.37 12 24Z" fill="url(#gemini-grad)"/><defs><linearGradient id="gemini-grad" x1="0" y1="0" x2="24" y2="24" gradientUnits="userSpaceOnUse"><stop stop-color="#4E82EE"/><stop offset="0.5" stop-color="#9C59DE"/><stop offset="1" stop-color="#DE5E78"/></linearGradient></defs></svg>`,
            copilot_ide: `<svg class="w-5 h-5 text-slate-900" viewBox="0 0 24 24" fill="currentColor"><path fill-rule="evenodd" clip-rule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.53 1.032 1.53 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z"/></svg>`,
            copilot: `<svg class="w-5 h-5 text-slate-900" viewBox="0 0 24 24" fill="currentColor"><path fill-rule="evenodd" clip-rule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.53 1.032 1.53 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z"/></svg>`,
            deepseek_perplexity: `<svg class="w-5 h-5 text-[#0066FF]" viewBox="0 0 24 24" fill="currentColor"><path d="M22 13.5C22 8.8 17.5 5 12 5C6.5 5 2 8.8 2 13.5C2 16.5 4 19.1 7 20.5L6 22H9L11 20C11.3 20 11.7 20 12 20C17.5 20 22 17.1 22 13.5ZM7.5 13C6.7 13 6 12.3 6 11.5C6 10.7 6.7 10 7.5 10C8.3 10 9 10.7 9 11.5C9 12.3 8.3 13 7.5 13ZM18.5 15C16.8 16.2 14.5 17 12 17C10.5 17 9.1 16.7 7.9 16.1C7.6 16 7.4 15.6 7.6 15.3C7.8 15 8.2 14.8 8.5 15C9.5 15.5 10.7 15.8 12 15.8C14.1 15.8 16.1 15.1 17.5 14.1C17.8 13.9 18.2 14 18.4 14.3C18.6 14.6 18.5 15 18.5 15Z"/></svg>`,
            deepseek: `<svg class="w-5 h-5 text-[#0066FF]" viewBox="0 0 24 24" fill="currentColor"><path d="M22 13.5C22 8.8 17.5 5 12 5C6.5 5 2 8.8 2 13.5C2 16.5 4 19.1 7 20.5L6 22H9L11 20C11.3 20 11.7 20 12 20C17.5 20 22 17.1 22 13.5ZM7.5 13C6.7 13 6 12.3 6 11.5C6 10.7 6.7 10 7.5 10C8.3 10 9 10.7 9 11.5C9 12.3 8.3 13 7.5 13ZM18.5 15C16.8 16.2 14.5 17 12 17C10.5 17 9.1 16.7 7.9 16.1C7.6 16 7.4 15.6 7.6 15.3C7.8 15 8.2 14.8 8.5 15C9.5 15.5 10.7 15.8 12 15.8C14.1 15.8 16.1 15.1 17.5 14.1C17.8 13.9 18.2 14 18.4 14.3C18.6 14.6 18.5 15 18.5 15Z"/></svg>`,
            coding_ai: `<svg class="w-5 h-5 text-slate-900" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L2 7.5V16.5L12 22L22 16.5V7.5L12 2ZM12 4.3L19.3 8.3L12 12.3L4.7 8.3L12 4.3ZM4 9.9L11 13.7V20.1L4 16.2V9.9ZM13 20.1V13.7L20 9.9V16.2L13 20.1Z"/></svg>`,
            ide_ai: `<svg class="w-5 h-5 text-slate-900" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L2 7.5V16.5L12 22L22 16.5V7.5L12 2ZM12 4.3L19.3 8.3L12 12.3L4.7 8.3L12 4.3ZM4 9.9L11 13.7V20.1L4 16.2V9.9ZM13 20.1V13.7L20 9.9V16.2L13 20.1Z"/></svg>`,
            llm_api: `<svg class="w-5 h-5" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" fill="#FFD21E"/><circle cx="8" cy="10" r="1.5" fill="#1F2937"/><circle cx="16" cy="10" r="1.5" fill="#1F2937"/><path d="M8 14.5C8.8 16.5 10.3 17.5 12 17.5C13.7 17.5 15.2 16.5 16 14.5" stroke="#1F2937" stroke-width="1.8" stroke-linecap="round" fill="none"/><path d="M4 14C4.5 15.5 6 16.5 7.5 16" stroke="#E65100" stroke-width="1.5" stroke-linecap="round" fill="none"/><path d="M20 14C19.5 15.5 18 16.5 16.5 16" stroke="#E65100" stroke-width="1.5" stroke-linecap="round" fill="none"/></svg>`,
            api_llm: `<svg class="w-5 h-5" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" fill="#FFD21E"/><circle cx="8" cy="10" r="1.5" fill="#1F2937"/><circle cx="16" cy="10" r="1.5" fill="#1F2937"/><path d="M8 14.5C8.8 16.5 10.3 17.5 12 17.5C13.7 17.5 15.2 16.5 16 14.5" stroke="#1F2937" stroke-width="1.8" stroke-linecap="round" fill="none"/><path d="M4 14C4.5 15.5 6 16.5 7.5 16" stroke="#E65100" stroke-width="1.5" stroke-linecap="round" fill="none"/><path d="M20 14C19.5 15.5 18 16.5 16.5 16" stroke="#E65100" stroke-width="1.5" stroke-linecap="round" fill="none"/></svg>`,
            roblox: `<svg class="w-5 h-5 text-[#E11D48]" viewBox="0 0 24 24" fill="currentColor"><path d="M5.16 2.5L21.5 6.84L18.84 23.16L2.5 18.82L5.16 2.5ZM10.59 10.05L9.61 16.03L15.6 17.01L16.58 11.03L10.59 10.05Z"/></svg>`
        };

        const BRAND_SVG_URLS = {
            chatgpt: "https://cdn.worldvectorlogo.com/logos/chatgpt-6.svg",
            gemini: "https://cdn.worldvectorlogo.com/logos/gemini-icon-logo.svg",
            claude: "https://cdn.worldvectorlogo.com/logos/claude-logo.svg",
            copilot_ide: "https://cdn.worldvectorlogo.com/logos/github-icon-1.svg",
            copilot: "https://cdn.worldvectorlogo.com/logos/github-icon-1.svg",
            deepseek_perplexity: "https://cdn.worldvectorlogo.com/logos/deepseek-2.svg",
            deepseek: "https://cdn.worldvectorlogo.com/logos/deepseek-2.svg",
            coding_ai: "https://cdn.worldvectorlogo.com/logos/28106466-5830-463a-8f91-0bb7a23ab920/cursor.svg",
            ide_ai: "https://cdn.worldvectorlogo.com/logos/28106466-5830-463a-8f91-0bb7a23ab920/cursor.svg",
            llm_api: "https://cdn.worldvectorlogo.com/logos/huggingface-2.svg",
            api_llm: "https://cdn.worldvectorlogo.com/logos/huggingface-2.svg",
            roblox: "https://cdn.worldvectorlogo.com/logos/roblox-studio-1.svg"
        };

        function renderDomainCategoryCards() {
            const box = document.getElementById('domainCategoryCards');
            if (!box) return;
            box.innerHTML = DOMAIN_CATALOG_GROUPS.map(g => {
                const total = g.domains.length;
                const activeCount = g.domains.filter(d => activeBlockedDomainsSet.has(d.toLowerCase())).length;
                const isAllActive = activeCount === total;
                const isPartial = activeCount > 0 && activeCount < total;

                const brandUrl = BRAND_SVG_URLS[g.id];
                const fallbackSvg = GROUP_BRAND_ICONS[g.id] || `<svg class="w-5 h-5 text-slate-700" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/></svg>`;
                const icon = brandUrl
                    ? `<img src="${brandUrl}" class="w-5 h-5 object-contain select-none" alt="${escapeHtml(g.title)}" loading="lazy" onerror="this.style.display='none'; if(this.nextElementSibling) this.nextElementSibling.classList.remove('hidden');"><span class="hidden flex items-center justify-center">${fallbackSvg}</span>`
                    : fallbackSvg;

                const thumbTranslate = isAllActive ? 'translate-x-4' : (isPartial ? 'translate-x-2' : 'translate-x-0');
                const trackBg = isAllActive
                    ? 'bg-slate-900 border-slate-800'
                    : (isPartial ? 'bg-amber-500 border-amber-600' : 'bg-slate-200 border-slate-300');

                const statusBadge = isAllActive
                    ? `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-bold bg-emerald-50 text-emerald-800 border border-emerald-200">
                        <span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
                        Semua Diblokir
                       </span>`
                    : (isPartial
                        ? `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-bold bg-amber-50 text-amber-800 border border-amber-200">
                            <span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span>
                            ${activeCount}/${total} Diblokir
                           </span>`
                        : `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-medium bg-slate-100 text-slate-500 border border-slate-200">
                            Diizinkan
                           </span>`);

                const cardBorder = isAllActive
                    ? 'border-slate-900/40 bg-gradient-to-b from-white to-slate-50/70 shadow-sm'
                    : (isPartial ? 'border-amber-400/50 bg-gradient-to-b from-white to-amber-50/30' : 'border-slate-200 bg-white hover:border-slate-300');

                const BRAND_CONTAINER_COLORS = {
                    chatgpt: 'bg-emerald-50/90 border-emerald-200/80',
                    claude: 'bg-orange-50/90 border-orange-200/80',
                    gemini: 'bg-indigo-50/90 border-indigo-200/80',
                    copilot_ide: 'bg-slate-100 border-slate-200/90',
                    deepseek_perplexity: 'bg-blue-50/90 border-blue-200/80',
                    coding_ai: 'bg-slate-100 border-slate-200/90',
                    llm_api: 'bg-amber-50/90 border-amber-200/80',
                    roblox: 'bg-rose-50/90 border-rose-200/80'
                };
                const iconBg = BRAND_CONTAINER_COLORS[g.id] || 'bg-slate-100 border-slate-200/80';

                return `
                    <div onclick="toggleDomainGroup('${g.id}')" class="btn-tactile cursor-pointer rounded-2xl border p-4 transition flex flex-col justify-between ${cardBorder} select-none">
                        <div>
                            <div class="flex items-start justify-between gap-2.5">
                                <div class="flex items-center gap-2.5 min-w-0">
                                    <div class="w-9 h-9 rounded-xl ${iconBg} border flex items-center justify-center shrink-0 shadow-2xs">
                                        ${icon}
                                    </div>
                                    <div class="min-w-0">
                                        <div class="font-bold text-xs text-slate-900 truncate">${escapeHtml(g.title)}</div>
                                        <div class="text-[11px] text-slate-500 truncate mt-0.5">${escapeHtml(g.desc)}</div>
                                    </div>
                                </div>

                                <!-- iOS-Style Toggle Switch -->
                                <div class="relative inline-flex items-center shrink-0">
                                    <div class="toggle-switch-track w-9 h-5 rounded-full border ${trackBg} p-0.5">
                                        <div class="toggle-switch-thumb w-3.5 h-3.5 rounded-full bg-white shadow-xs transform ${thumbTranslate}"></div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between">
                            ${statusBadge}
                            <span class="text-[11px] font-mono text-slate-400 font-medium">${total} host</span>
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
            if (!t) return;
            t.textContent = msg;
            t.className = `fixed bottom-20 lg:bottom-5 right-4 sm:right-5 z-50 px-4 py-2.5 rounded-xl text-xs font-semibold shadow-xl border transition ${
                isError ? 'bg-rose-700 text-white border-rose-800' : 'bg-slate-900 text-white border-slate-800'
            }`;
            t.classList.remove('hidden');
            setTimeout(() => t.classList.add('hidden'), 3000);
        }

        function toggleMobileSidebar(forceState) {
            const drawer = document.getElementById('mobileNavDrawer');
            const backdrop = document.getElementById('mobileNavBackdrop');
            if (!drawer || !backdrop) return;
            const isCurrentlyOpen = drawer.classList.contains('translate-x-0');
            const targetState = typeof forceState === 'boolean' ? forceState : !isCurrentlyOpen;

            if (targetState) {
                backdrop.classList.remove('hidden');
                requestAnimationFrame(() => {
                    backdrop.classList.remove('opacity-0', 'pointer-events-none');
                    backdrop.classList.add('opacity-100');
                    drawer.classList.remove('-translate-x-full');
                    drawer.classList.add('translate-x-0');
                });
            } else {
                backdrop.classList.remove('opacity-100');
                backdrop.classList.add('opacity-0', 'pointer-events-none');
                drawer.classList.remove('translate-x-0');
                drawer.classList.add('-translate-x-full');
                setTimeout(() => {
                    backdrop.classList.add('hidden');
                }, 300);
            }
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
                const viewEl = document.getElementById('view-' + id);
                if (viewEl) viewEl.classList.toggle('hidden', id !== tab);

                // Desktop sidebar button
                const btn = document.getElementById('nav-' + id);
                if (btn) {
                    btn.className = id === tab
                        ? 'nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-semibold bg-slate-900 text-white transition shadow-2xs'
                        : 'nav-item w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition';
                }

                // Mobile drawer button
                const mobDrawerBtn = document.getElementById('mob-drawer-nav-' + id);
                if (mobDrawerBtn) {
                    mobDrawerBtn.className = id === tab
                        ? 'w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-semibold bg-slate-900 text-white transition shadow-2xs'
                        : 'w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-medium text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition';
                }

                // Mobile bottom nav button
                const mobBottomBtn = document.getElementById('mob-nav-' + id);
                if (mobBottomBtn) {
                    mobBottomBtn.className = id === tab
                        ? 'flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-900 transition relative font-bold'
                        : 'flex-1 py-1.5 px-1 flex flex-col items-center justify-center gap-1 text-slate-400 hover:text-slate-700 transition relative font-medium';
                }
            });

            // Badge online indicator styling
            const badgeOnline = document.getElementById('navBadgeOnline');
            const drawerBadgeOnline = document.getElementById('mobDrawerBadgeOnline');
            const bottomBadgeOnline = document.getElementById('mobBadgeOnline');
            if (badgeOnline) {
                badgeOnline.className = tab === 'monitor'
                    ? 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-white/20 text-white tabular-nums'
                    : 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-slate-200/80 text-slate-700 tabular-nums';
            }
            if (drawerBadgeOnline) {
                drawerBadgeOnline.className = tab === 'monitor'
                    ? 'px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-white/20 text-white tabular-nums'
                    : 'px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-slate-200/80 text-slate-700 tabular-nums';
            }
            if (bottomBadgeOnline) {
                bottomBadgeOnline.className = tab === 'monitor'
                    ? 'absolute -top-1.5 -right-2.5 px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold bg-slate-900 text-white tabular-nums'
                    : 'absolute -top-1.5 -right-2.5 px-1.5 py-0.2 rounded-full text-[9px] font-mono font-bold bg-slate-400 text-white tabular-nums';
            }

            // Badge violations indicator styling
            const badgeViolations = document.getElementById('navBadgeViolations');
            const drawerBadgeViolations = document.getElementById('mobDrawerBadgeViolations');
            const bottomBadgeViolations = document.getElementById('mobBadgeViolations');
            if (badgeViolations) {
                badgeViolations.className = tab === 'logs'
                    ? 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-500 text-white tabular-nums'
                    : 'px-1.5 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-50 text-rose-700 border border-rose-200 tabular-nums';
            }
            if (drawerBadgeViolations) {
                drawerBadgeViolations.className = tab === 'logs'
                    ? 'px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-500 text-white tabular-nums'
                    : 'px-2 py-0.5 text-[10px] font-mono font-semibold rounded bg-rose-50 text-rose-700 border border-rose-200 tabular-nums';
            }

            // Auto-close mobile drawer on tab tap
            toggleMobileSidebar(false);

            // Scroll workspace smoothly to top on tab switch
            const scrollArea = document.querySelector('.overflow-y-auto');
            if (scrollArea) scrollArea.scrollTop = 0;
            window.scrollTo({top: 0, behavior: 'smooth'});
        }

        function getEffectiveBlocklistForRoom(roomCode) {
            const norm = String(roomCode || 'ALL').trim().toUpperCase();
            if (norm !== 'ALL' && cachedRoomBlocklists && cachedRoomBlocklists[norm]) {
                const rb = cachedRoomBlocklists[norm];
                const rDomains = Array.isArray(rb) ? rb : (Array.isArray(rb.domains) ? rb.domains : (cachedGlobalBlocklist.domains || []));
                const rProcs = (rb && Array.isArray(rb.processes)) ? rb.processes : (cachedGlobalBlocklist.processes || []);
                const rExts = (rb && Array.isArray(rb.ide_extensions)) ? rb.ide_extensions : (cachedGlobalBlocklist.ide_extensions || []);
                return {
                    domains: rDomains,
                    processes: rProcs,
                    ide_extensions: rExts,
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
                    const dCount = hasCustom 
                        ? (Array.isArray(cachedRoomBlocklists[r]) ? cachedRoomBlocklists[r].length : (cachedRoomBlocklists[r].domains || []).length)
                        : globalCount;
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
            const mobSel = document.getElementById('mobileHeaderRoomSelect');
            if (mobSel) mobSel.value = selectedRoom;
            loadBlocklistIntoEditorForRoom(selectedRoom);
            updateSidebarPinUI();
            updateHeaderStatusUI();
            updateScopedStats();
            switchTab('rules');
        }

        function onSelectRoom(room) {
            selectedRoom = room || 'ALL';
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
            const hdrSel = document.getElementById('headerRoomSelect');
            if (hdrSel) hdrSel.value = selectedRoom;
            const mobSel = document.getElementById('mobileHeaderRoomSelect');
            if (mobSel) mobSel.value = selectedRoom;
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
                if (el) {
                    el.className = f === filter
                        ? 'filter-pill px-3 py-1.5 rounded-md font-semibold bg-white text-slate-900 shadow-2xs whitespace-nowrap'
                        : 'filter-pill px-3 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 whitespace-nowrap';
                }
            });
            renderCurrentStudents();
        }

        function setViewMode(mode) {
            currentViewMode = mode;
            document.getElementById('studentTableContainer').classList.toggle('hidden', mode !== 'table');
            document.getElementById('studentGridContainer').classList.toggle('hidden', mode !== 'grid');
            document.getElementById('viewMode-table').className = mode === 'table'
                ? 'px-2.5 py-1.5 rounded-md font-semibold bg-white text-slate-900 shadow-2xs flex items-center gap-1.5'
                : 'px-2.5 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5';
            document.getElementById('viewMode-grid').className = mode === 'grid'
                ? 'px-2.5 py-1.5 rounded-md font-semibold bg-white text-slate-900 shadow-2xs flex items-center gap-1.5'
                : 'px-2.5 py-1.5 rounded-md font-medium text-slate-600 hover:text-slate-900 flex items-center gap-1.5';
            renderCurrentStudents();
        }

        function updateRoomSelectorOptions() {
            const sel = document.getElementById('headerRoomSelect');
            const mobSel = document.getElementById('mobileHeaderRoomSelect');
            if (!sel && !mobSel) return;
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

            if (sel) {
                sel.innerHTML = html;
                if (selectedRoom !== 'ALL' && !allRooms.includes(selectedRoom)) {
                    selectedRoom = 'ALL';
                }
                sel.value = selectedRoom;
            }
            if (mobSel) {
                mobSel.innerHTML = html;
                mobSel.value = selectedRoom;
            }
            document.getElementById('sidebarRoomCode').textContent = selectedRoom === 'ALL' ? 'SEMUA RUANGAN' : selectedRoom;
            updateRulesRoomSelectorUI();
        }

        function updateHeaderStatusUI() {
            const gBadge = document.getElementById('globalStateBadge');
            const btnLock = document.getElementById('btnGlobalLock');
            const btnUnlock = document.getElementById('btnGlobalUnlock');
            const lblLock = document.getElementById('lblBtnLock');
            const lblUnlock = document.getElementById('lblBtnUnlock');
            const mobDot = document.getElementById('mobileGlobalStateDot');

            const activeStatus = selectedRoom === 'ALL'
                ? cachedGlobalStatus
                : (cachedRoomStatuses[selectedRoom] || cachedGlobalStatus);

            if (lblLock) lblLock.textContent = selectedRoom === 'ALL' ? 'Kunci Semua Ruangan' : `Kunci ${selectedRoom}`;
            if (lblUnlock) lblUnlock.textContent = selectedRoom === 'ALL' ? 'Buka Semua & Pulihkan' : `Buka ${selectedRoom}`;

            if (activeStatus === 'LOCKED') {
                if (gBadge) {
                    gBadge.className = 'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-emerald-50 text-emerald-800 border border-emerald-200 whitespace-nowrap shadow-2xs';
                    gBadge.innerHTML = `<span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span> ${selectedRoom === 'ALL' ? 'Semua Ruangan Terkunci' : 'Ruangan ' + escapeHtml(selectedRoom) + ' Terkunci'}`;
                }
                if (btnLock) btnLock.className = 'px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-semibold transition flex items-center gap-1.5 bg-slate-900 text-white shadow-xs';
                if (btnUnlock) btnUnlock.className = 'px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900';
                if (mobDot) {
                    mobDot.className = 'w-2.5 h-2.5 rounded-full bg-emerald-600 shrink-0';
                    mobDot.title = 'Proteksi AI Aktif';
                }
            } else {
                if (gBadge) {
                    gBadge.className = 'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-medium bg-amber-50 text-amber-800 border border-amber-200 whitespace-nowrap shadow-2xs';
                    gBadge.innerHTML = `<span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span> ${selectedRoom === 'ALL' ? 'Semua Ruangan Terbuka' : 'Ruangan ' + escapeHtml(selectedRoom) + ' Terbuka'}`;
                }
                if (btnUnlock) btnUnlock.className = 'px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-semibold transition flex items-center gap-1.5 bg-slate-900 text-white shadow-xs';
                if (btnLock) btnLock.className = 'px-2.5 sm:px-3 py-1.5 rounded-md text-xs font-medium transition flex items-center gap-1.5 text-slate-600 hover:text-slate-900';
                if (mobDot) {
                    mobDot.className = 'w-2.5 h-2.5 rounded-full bg-amber-500 shrink-0';
                    mobDot.title = 'Ruangan Terbuka';
                }
            }
        }

        function updateScopedStats() {
            const scoped = selectedRoom === 'ALL'
                ? cachedStudents
                : cachedStudents.filter(st => (st.room_code || currentRoomCode || '-').toUpperCase() === selectedRoom);

            const onlineCount = scoped.filter(st => st.is_online).length;
            const lockedCount = scoped.filter(st => st.status === 'LOCKED').length;
            const violationCount = scoped.reduce((acc, st) => acc + parseInt(st.killed_processes_count || 0, 10), 0);

            const statOnlineEl = document.getElementById('statOnline');
            if (statOnlineEl) statOnlineEl.textContent = onlineCount;
            const statTotalEl = document.getElementById('statTotal');
            if (statTotalEl) statTotalEl.textContent = scoped.length;
            const statLockedEl = document.getElementById('statLocked');
            if (statLockedEl) statLockedEl.textContent = lockedCount;
            const statViolationsEl = document.getElementById('statViolations');
            if (statViolationsEl) statViolationsEl.textContent = violationCount;

            const effBl = getEffectiveBlocklistForRoom(selectedRoom);
            const statDomainsEl = document.getElementById('statDomains');
            if (statDomainsEl) statDomainsEl.textContent = (effBl.domains || []).length;
            const statProcsEl = document.getElementById('statProcs');
            if (statProcsEl) statProcsEl.textContent = (effBl.processes || []).length;
            const statExtsEl = document.getElementById('statExts');
            if (statExtsEl) statExtsEl.textContent = (effBl.ide_extensions || []).length;

            // Desktop badge
            const badgeOnline = document.getElementById('navBadgeOnline');
            if (badgeOnline) badgeOnline.textContent = onlineCount;
            const badgeViolations = document.getElementById('navBadgeViolations');
            if (badgeViolations) badgeViolations.textContent = violationCount;

            // Mobile badges
            const mobBadgeOnline = document.getElementById('mobBadgeOnline');
            if (mobBadgeOnline) mobBadgeOnline.textContent = onlineCount;
            const mobDrawerBadgeOnline = document.getElementById('mobDrawerBadgeOnline');
            if (mobDrawerBadgeOnline) mobDrawerBadgeOnline.textContent = onlineCount;

            const mobBadgeViolations = document.getElementById('mobBadgeViolations');
            if (mobBadgeViolations) mobBadgeViolations.textContent = violationCount;
            const mobDrawerBadgeViolations = document.getElementById('mobDrawerBadgeViolations');
            if (mobDrawerBadgeViolations) mobDrawerBadgeViolations.textContent = violationCount;
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
                    const recentCmd = recentLocalStudentCommands.get(st.id);
                    if (recentCmd && (nowMs - recentCmd.ts) < 12000) {
                        st.status = recentCmd.status;
                        st.target_status = recentCmd.status;
                    }
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
                        const recentCmd = recentLocalStudentCommands.get(id);
                        if (recentCmd && (nowMs - recentCmd.ts) < 12000) {
                            entry.student.status = recentCmd.status;
                            entry.student.target_status = recentCmd.status;
                        }
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

                for (const [id, cmd] of recentLocalStudentCommands.entries()) {
                    if (nowMs - cmd.ts > 15000) {
                        recentLocalStudentCommands.delete(id);
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

        function getStudentInitials(name) {
            if (!name) return 'ST';
            const parts = String(name).trim().split(/\s+/).filter(Boolean);
            if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
            return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        }

        const AVATAR_PALETTES = [
            { bg: 'from-blue-600 to-indigo-700', text: 'text-white' },
            { bg: 'from-emerald-600 to-teal-700', text: 'text-white' },
            { bg: 'from-violet-600 to-purple-700', text: 'text-white' },
            { bg: 'from-amber-500 to-orange-600', text: 'text-white' },
            { bg: 'from-rose-500 to-pink-600', text: 'text-white' },
            { bg: 'from-cyan-600 to-blue-700', text: 'text-white' },
            { bg: 'from-slate-700 to-slate-900', text: 'text-white' }
        ];

        function getAvatarPalette(str) {
            let hash = 0;
            const s = String(str || 'default');
            for (let i = 0; i < s.length; i++) {
                hash = s.charCodeAt(i) + ((hash << 5) - hash);
            }
            const idx = Math.abs(hash) % AVATAR_PALETTES.length;
            return AVATAR_PALETTES[idx];
        }

        function renderCurrentStudents() {
            const list = getFilteredStudents();
            const tbody = document.getElementById('studentTableBody');
            const grid = document.getElementById('studentGridContainer');

            if (!list.length) {
                const emptyMsg = selectedRoom === 'ALL'
                    ? 'Belum ada perangkat mahasiswa yang terhubung di seluruh ruangan.'
                    : `Belum ada perangkat mahasiswa yang terhubung di ruangan [${escapeHtml(selectedRoom)}].`;

                const endpointUrl = window.location.origin + window.location.pathname.replace(/\/index\.(php|html)$/, '').replace(/\/$/, '') + '/api/index.php';

                const emptyStateHtml = `
                    <div class="py-14 px-4 text-center max-w-md mx-auto flex flex-col items-center">
                        <div class="relative w-24 h-24 flex items-center justify-center mb-3">
                            <div class="radar-wave-1 absolute inset-0 rounded-full border-2 border-emerald-500/50 bg-emerald-500/10 pointer-events-none"></div>
                            <div class="radar-wave-2 absolute inset-0 rounded-full border-2 border-emerald-500/35 bg-emerald-500/5 pointer-events-none"></div>
                            <div class="radar-wave-3 absolute inset-0 rounded-full border border-emerald-500/20 pointer-events-none"></div>
                            <div class="relative z-10 w-12 h-12 rounded-full bg-slate-900 text-white flex items-center justify-center shadow-xl border border-slate-700/80">
                                <svg class="w-6 h-6 text-emerald-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z"/>
                                </svg>
                            </div>
                        </div>
                        <h3 class="text-sm font-bold text-slate-900">Menunggu Sambungan Agen Peserta</h3>
                        <p class="text-xs text-slate-500 mt-1.5 leading-relaxed">${emptyMsg} Jalankan aplikasi <code class="font-mono bg-slate-100 px-1 py-0.5 rounded text-slate-700">GuardAgent.exe</code> di workstation ujian untuk mulai memantau.</p>
                        <div class="mt-4 flex flex-wrap items-center justify-center gap-2">
                            <button onclick="copyText('${endpointUrl}', 'URL Endpoint Server disalin')" class="btn-tactile px-3.5 py-2 rounded-xl bg-slate-900 hover:bg-slate-800 text-white text-xs font-semibold shadow-xs flex items-center gap-1.5">
                                <svg class="w-3.5 h-3.5 text-emerald-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M8.25 7.5V6.108c0-1.135.845-2.098 1.976-2.192.373-.03.748-.057 1.124-.08M15.75 18H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08M15.75 18.75v-1.875a3.375 3.375 0 00-3.375-3.375h-1.5a1.125 1.125 0 01-1.125-1.125v-1.5A3.375 3.375 0 006.375 7.5H5.25m11.25 11.25H9.75M5.25 7.5v9a2.25 2.25 0 002.25 2.25h2.25"/></svg>
                                <span>Salin URL Server</span>
                            </button>
                            <button onclick="switchTab('download')" class="btn-tactile px-3.5 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 text-xs font-semibold shadow-2xs flex items-center gap-1.5">
                                <svg class="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>
                                <span>Distribusi Agen (.exe)</span>
                            </button>
                        </div>
                    </div>
                `;

                tbody.innerHTML = `<tr><td colspan="8" class="p-0">${emptyStateHtml}</td></tr>`;
                grid.innerHTML = `<div class="col-span-full card-surface rounded-2xl">${emptyStateHtml}</div>`;
                return;
            }

            tbody.innerHTML = list.map(st => {
                const stRoom = (st.room_code || currentRoomCode || '-').toUpperCase();
                const initials = getStudentInitials(st.name);
                const palette = getAvatarPalette(st.name + st.nim);
                const isLocked = st.status === 'LOCKED';

                const connBadge = st.is_online
                    ? `<div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-emerald-50 text-emerald-800 border border-emerald-200/80 font-medium text-[11px] shadow-2xs">
                        <span class="radar-beacon"><span class="w-2 h-2 rounded-full bg-emerald-600"></span></span>
                        <span>Aktif</span>
                      </div>`
                    : `<div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-slate-100 text-slate-500 border border-slate-200 font-medium text-[11px]">
                        <span class="w-2 h-2 rounded-full bg-slate-400"></span>
                        <span>Terputus</span>
                      </div>`;

                let stateBadge = isLocked
                    ? `<span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-bold bg-emerald-50 text-emerald-800 border border-emerald-200/90 shadow-2xs">
                        <svg class="w-3.5 h-3.5 text-emerald-600 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z"/></svg>
                        <span>Terkunci</span>
                      </span>`
                    : `<span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-bold bg-amber-50 text-amber-800 border border-amber-200/90 shadow-2xs">
                        <svg class="w-3.5 h-3.5 text-amber-600 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M13.5 10.5V6.75a4.5 4.5 0 119 0v3.75M3.75 21.75h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H3.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z"/></svg>
                        <span>Bypass</span>
                      </span>`;

                if (st.vpn_detected) {
                    stateBadge += ` <span class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[10px] font-bold bg-rose-50 text-rose-700 border border-rose-300 ml-1 shadow-2xs animate-pulse" title="Peringatan: Perangkat menggunakan koneksi proxy atau VPN!">
                        <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z"/></svg>
                        <span>VPN</span>
                    </span>`;
                }

                const extCount = st.quarantined_count || 0;
                const extSummary = extCount > 0
                    ? `<button onclick="openStudentModal('${escapeHtml(st.id)}')" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-indigo-50/90 hover:bg-indigo-100 text-indigo-700 border border-indigo-200/80 font-mono text-[11px] font-semibold transition shadow-2xs" title="Proteksi Aktif: ${extCount} modul/fitur AI diamankan & dikarantina. Klik untuk melihat detail.">
                        <svg class="w-3.5 h-3.5 text-indigo-500" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M14.25 9.75L16.5 12l-2.25 2.25m-4.5 0L7.5 12l2.25-2.25M6 20.25h12A2.25 2.25 0 0020.25 18V6A2.25 2.25 0 0018 3.75H6A2.25 2.25 0 003.75 6v12A2.25 2.25 0 006 20.25z"/></svg>
                        <span>${extCount} modul dikunci</span>
                      </button>`
                    : `<span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-emerald-50 text-emerald-800 border border-emerald-200/80 font-medium text-[11px]" title="Proteksi IDE Aktif: Tidak ditemukan ekstensi/modul AI terlarang pada perangkat ini">
                        <svg class="w-3.5 h-3.5 text-emerald-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5"/></svg>
                        <span>Bersih (0 AI)</span>
                      </span>`;

                const incidents = st.killed_processes_count || 0;
                const incidentCell = incidents > 0
                    ? `<button onclick="openStudentModal('${escapeHtml(st.id)}')" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg font-mono text-[11px] font-bold bg-rose-50 text-rose-700 border border-rose-200/80 hover:bg-rose-100 transition shadow-2xs" title="Lihat riwayat proses yang dicegat">
                        <span class="radar-danger"><span class="w-1.5 h-1.5 rounded-full bg-rose-600"></span></span>
                        <span>${incidents}x dicegat</span>
                      </button>`
                    : `<span class="text-slate-400 font-mono text-[11px]">0</span>`;

                return `
                    <tr class="hover:bg-slate-50/90 transition group">
                        <td class="py-3 px-4">
                            <div class="flex items-center gap-3">
                                <div class="relative shrink-0">
                                    <div class="w-9 h-9 rounded-xl bg-gradient-to-tr ${palette.bg} ${palette.text} flex items-center justify-center font-bold text-xs shadow-xs border border-white/60">
                                        ${initials}
                                    </div>
                                    <span class="absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-white ${st.is_online ? 'bg-emerald-500' : 'bg-slate-300'}" title="${st.is_online ? 'Aktif Terhubung' : 'Terputus'}"></span>
                                </div>
                                <div class="min-w-0">
                                    <div class="flex items-center gap-1.5 font-bold text-slate-900 truncate">
                                        <span>${escapeHtml(st.name)}</span>
                                        ${isLocked ? '<svg class="w-3.5 h-3.5 text-emerald-600 shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" title="Terkunci Terlindungi"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z"/></svg>' : ''}
                                    </div>
                                    <div class="text-[11px] font-mono text-slate-500 mt-0.5">
                                        ${escapeHtml(st.nim)}
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="py-3 px-3">
                            <button onclick="onSelectRoom('${escapeHtml(stRoom)}')" class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg font-mono text-xs font-bold bg-slate-100 hover:bg-slate-200 text-slate-800 border border-slate-200/80 transition" title="Filter ruangan ${escapeHtml(stRoom)}">
                                <span>${escapeHtml(stRoom)}</span>
                            </button>
                        </td>
                        <td class="py-3 px-4">
                            <div class="flex items-center gap-2">
                                <div class="w-7 h-7 rounded-lg bg-slate-100 border border-slate-200/70 text-slate-500 flex items-center justify-center shrink-0">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="1.75" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25m18 0A2.25 2.25 0 0018.75 3H5.25A2.25 2.25 0 003 5.25m18 0V12a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 12V5.25"/></svg>
                                </div>
                                <div class="min-w-0 font-mono text-xs">
                                    <div class="font-semibold text-slate-800 truncate">${escapeHtml(st.hostname)}</div>
                                    <button onclick="copyText('${escapeHtml(st.ip || '127.0.0.1')}', 'IP [${escapeHtml(st.ip || '127.0.0.1')}] disalin')" class="text-[11px] text-slate-400 hover:text-slate-700 flex items-center gap-1 group/ip transition" title="Klik untuk salin IP">
                                        <span>${escapeHtml(st.ip || '127.0.0.1')}</span>
                                        <svg class="w-3 h-3 opacity-0 group-hover/ip:opacity-100 transition-opacity" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"/></svg>
                                    </button>
                                </div>
                            </div>
                        </td>
                        <td class="py-3 px-3">${connBadge}</td>
                        <td class="py-3 px-3">${stateBadge}</td>
                        <td class="py-3 px-3">${extSummary}</td>
                        <td class="py-3 px-3">${incidentCell}</td>
                        <td class="py-3 px-4 text-right whitespace-nowrap">
                            <div class="inline-flex items-center gap-1 rounded-xl bg-slate-100 p-0.5 border border-slate-200/70 shadow-2xs">
                                <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'LOCKED')" class="btn-tactile px-2.5 py-1 rounded-lg text-xs font-bold transition flex items-center gap-1 ${isLocked ? 'bg-slate-900 text-white shadow-xs' : 'text-slate-600 hover:text-slate-900'}" title="Kunci perangkat ini">
                                    <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
                                    <span>Kunci</span>
                                </button>
                                <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'UNLOCKED')" class="btn-tactile px-2.5 py-1 rounded-lg text-xs font-bold transition flex items-center gap-1 ${!isLocked ? 'bg-amber-600 text-white shadow-xs' : 'text-slate-600 hover:text-slate-900'}" title="Buka kunci perangkat ini">
                                    <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 019.9-1"/></svg>
                                    <span>Buka</span>
                                </button>
                                <button onclick="openStudentModal('${escapeHtml(st.id)}')" class="btn-tactile p-1.5 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-white transition" title="Detail perangkat">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"/></svg>
                                </button>
                                <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'REMOVE')" class="btn-tactile p-1.5 rounded-lg text-slate-400 hover:text-rose-600 hover:bg-white transition" title="Hapus sesi">
                                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;
            }).join('');

            grid.innerHTML = list.map(st => {
                const stRoom = (st.room_code || currentRoomCode || '-').toUpperCase();
                const initials = getStudentInitials(st.name);
                const palette = getAvatarPalette(st.name + st.nim);
                const isLocked = st.status === 'LOCKED';
                const incidents = st.killed_processes_count || 0;
                const extCount = st.quarantined_count || 0;

                return `
                    <div class="card-surface rounded-2xl p-4 flex flex-col justify-between border-t-4 ${isLocked ? 'border-t-emerald-600' : 'border-t-amber-500'} hover:shadow-lg transition">
                        <div>
                            <!-- Node Top Header -->
                            <div class="flex items-center justify-between pb-3 border-b border-slate-100">
                                <div class="flex items-center gap-2">
                                    <div class="w-7 h-7 rounded-lg bg-slate-900 text-white flex items-center justify-center font-mono text-[10px] font-bold shadow-xs">
                                        WS
                                    </div>
                                    <div class="font-mono text-xs font-bold text-slate-900 truncate max-w-[120px]">${escapeHtml(st.hostname)}</div>
                                </div>
                                <div class="flex items-center gap-1.5">
                                    <span class="px-2 py-0.5 rounded-md font-mono text-[10px] font-bold bg-slate-100 text-slate-700 border border-slate-200">${escapeHtml(stRoom)}</span>
                                    <span class="w-2.5 h-2.5 rounded-full ${st.is_online ? 'bg-emerald-500 status-dot-live' : 'bg-slate-300'}" title="${st.is_online ? 'Online' : 'Offline'}"></span>
                                </div>
                            </div>

                            <!-- Student Profile -->
                            <div class="flex items-center gap-3 pt-3">
                                <div class="w-10 h-10 rounded-xl bg-gradient-to-tr ${palette.bg} ${palette.text} flex items-center justify-center font-bold text-sm shadow-xs border border-white/60 shrink-0">
                                    ${initials}
                                </div>
                                <div class="min-w-0 flex-1">
                                    <div class="font-bold text-sm text-slate-900 truncate">${escapeHtml(st.name)}</div>
                                    <div class="text-[11px] font-mono text-slate-500">${escapeHtml(st.nim)}</div>
                                </div>
                            </div>

                            <!-- Spec Telemetry -->
                            <div class="mt-3.5 p-2.5 rounded-xl bg-slate-50/80 border border-slate-200/60 space-y-2 text-xs">
                                <div class="flex items-center justify-between">
                                    <span class="text-slate-400 text-[11px]">Endpoint IP</span>
                                    <span class="font-mono font-medium text-slate-700 text-[11px]">${escapeHtml(st.ip || '127.0.0.1')}</span>
                                </div>
                                <div class="flex items-center justify-between">
                                    <span class="text-slate-400 text-[11px]">Proteksi AI</span>
                                    <span class="font-bold text-[11px] ${isLocked ? 'text-emerald-700' : 'text-amber-700'}">${isLocked ? 'Terkunci Aktif' : 'Bypass / Terbuka'}</span>
                                </div>
                                <div class="flex items-center justify-between">
                                    <span class="text-slate-400 text-[11px]">Modul Ekstensi</span>
                                    <span class="font-mono text-[11px] ${extCount > 0 ? 'text-indigo-700 font-semibold' : 'text-emerald-700 font-medium'}">${extCount > 0 ? extCount + ' dikunci' : 'Bersih (0 AI)'}</span>
                                </div>
                                <div class="flex items-center justify-between">
                                    <span class="text-slate-400 text-[11px]">Intersepsi AI</span>
                                    <span class="font-mono text-[11px] ${incidents > 0 ? 'text-rose-600 font-bold' : 'text-slate-500'}">${incidents > 0 ? incidents + 'x dicegat' : '0'}</span>
                                </div>
                            </div>
                        </div>

                        <!-- Node Actions -->
                        <div class="mt-4 pt-3 border-t border-slate-100 flex items-center gap-1.5">
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'LOCKED')" class="btn-tactile flex-1 py-1.5 rounded-xl font-bold text-xs transition ${isLocked ? 'bg-slate-900 text-white shadow-xs' : 'bg-slate-100 hover:bg-slate-200 text-slate-700'}">Kunci</button>
                            <button onclick="sendStudentCmd('${escapeHtml(st.id)}', 'UNLOCKED')" class="btn-tactile flex-1 py-1.5 rounded-xl font-bold text-xs transition ${!isLocked ? 'bg-amber-600 text-white shadow-xs' : 'bg-slate-100 hover:bg-slate-200 text-slate-700'}">Buka</button>
                            <button onclick="openStudentModal('${escapeHtml(st.id)}')" class="btn-tactile p-1.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-600" title="Detail Perangkat">
                                <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"/></svg>
                            </button>
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
                    ? (Array.isArray(cachedRoomBlocklists[r]) ? cachedRoomBlocklists[r].length : (cachedRoomBlocklists[r].domains || []).length)
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
                            <button onclick="onSelectRoom('${escapeHtml(r)}'); switchTab('monitor');" class="btn-tactile px-2.5 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 font-semibold shadow-2xs">Pantau</button>
                            <button onclick="openRoomRulesEditor('${escapeHtml(r)}')" class="btn-tactile px-2.5 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 font-semibold shadow-2xs">Atur Blokir</button>
                            <button onclick="openEditRoomModal('${escapeHtml(r)}')" class="btn-tactile px-2.5 py-1 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-slate-700 font-semibold shadow-2xs">Edit & PIN</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'LOCKED')" class="btn-tactile px-2.5 py-1 rounded-lg border ${status === 'LOCKED' ? 'border-slate-900 bg-slate-900 text-white shadow-xs' : 'border-slate-200 bg-white hover:bg-slate-50 text-slate-700'} font-semibold">Kunci</button>
                            <button onclick="setRoomStatusDirect('${escapeHtml(r)}', 'UNLOCKED')" class="btn-tactile px-2.5 py-1 rounded-lg border ${status === 'UNLOCKED' ? 'border-amber-600 bg-amber-600 text-white shadow-xs' : 'border-slate-200 bg-white hover:bg-slate-50 text-slate-700'} font-semibold">Buka</button>
                            <button onclick="deleteRoom('${escapeHtml(r)}')" class="btn-tactile p-1.5 rounded-lg text-slate-400 hover:text-rose-600 hover:bg-rose-50 transition" title="Hapus ruangan">✕</button>
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

            const boxIncidents = document.getElementById('modalIncidentList');
            const badgeIncidents = document.getElementById('modalIncidentCountBadge');
            const studentLogs = cachedLogs.filter(l =>
                (l.nim && String(l.nim).toLowerCase() === String(st.nim).toLowerCase()) ||
                (l.hostname && String(l.hostname).toLowerCase() === String(st.hostname).toLowerCase())
            );

            const count = parseInt(st.killed_processes_count || 0, 10);
            if (badgeIncidents) {
                badgeIncidents.textContent = `${count} Insiden Terdeteksi`;
                badgeIncidents.className = count > 0
                    ? 'px-2 py-0.5 rounded text-[10px] font-mono font-semibold bg-rose-50 text-rose-700 border border-rose-200'
                    : 'px-2 py-0.5 rounded text-[10px] font-mono font-medium bg-emerald-50 text-emerald-700 border border-emerald-200';
            }

            if (boxIncidents) {
                if (studentLogs.length) {
                    boxIncidents.innerHTML = studentLogs.map(l => {
                        let color = l.severity === 'DANGER' ? 'text-rose-700 font-semibold' : (l.severity === 'WARNING' ? 'text-amber-700' : 'text-slate-600');
                        return `
                            <div class="p-2.5 rounded-lg bg-white border border-slate-200/80 shadow-xs">
                                <div class="flex items-center justify-between gap-2">
                                    <span class="font-mono text-[10px] font-semibold px-1.5 py-0.5 rounded bg-slate-100 text-slate-800 border border-slate-200">${escapeHtml(l.type)}</span>
                                    <span class="font-mono text-[10px] text-slate-400">${escapeHtml(l.timestamp)}</span>
                                </div>
                                <div class="mt-1 text-[11px] ${color} leading-relaxed">${escapeHtml(l.message)}</div>
                            </div>
                        `;
                    }).join('');
                } else {
                    boxIncidents.innerHTML = `<div class="text-slate-400 font-sans p-1">Bersih — Tidak ada catatan pelanggaran terdeteksi pada perangkat ini.</div>`;
                }
            }

            const btnUpdate = document.getElementById('modalBtnUpdate');
            if (btnUpdate) {
                btnUpdate.onclick = () => {
                    if (confirm(`Kirim instruksi pembaruan aplikasi ke PC [${st.hostname}] (${st.nim})?\n\nAplikasi GuardAgent pada PC ini akan otomatis mengunduh versi terbaru dan me-restart dirinya sendiri tanpa perlu download manual dari browser.`)) {
                        sendStudentCmd(st.id, 'UPDATE');
                    }
                };
            }

            document.getElementById('studentModal').classList.remove('hidden');
        }

        function closeStudentModal() {
            document.getElementById('studentModal').classList.add('hidden');
        }

        function renderLogs(logs) {
            const box = document.getElementById('logListContainer');
            if (!box) return;
            const filtered = selectedRoom === 'ALL'
                ? logs
                : logs.filter(l => !l.room_code || l.room_code === 'ALL' || l.room_code.toUpperCase() === selectedRoom);

            if (!filtered.length) {
                box.innerHTML = `
                    <div class="py-14 text-center max-w-sm mx-auto flex flex-col items-center">
                        <div class="w-12 h-12 rounded-2xl bg-emerald-50 border border-emerald-200/80 flex items-center justify-center text-emerald-600 shadow-sm mb-3">
                            <svg class="w-6 h-6" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z"/></svg>
                        </div>
                        <h4 class="text-xs font-bold text-slate-900">Jurnal Audit Bersih & Terlindungi</h4>
                        <p class="text-[11px] text-slate-500 mt-1">Belum ada intersepsi proses atau insiden pelanggaran yang tercatat pada sesi ini.</p>
                    </div>
                `;
                return;
            }

            box.innerHTML = filtered.map(l => {
                let badge = `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-mono font-bold bg-slate-100 text-slate-700 border border-slate-200">${escapeHtml(l.type)}</span>`;
                let iconBg = 'bg-slate-100 text-slate-600';

                if (l.type === 'KILL_PROCESS' || l.severity === 'DANGER') {
                    badge = `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-mono font-bold bg-rose-50 text-rose-700 border border-rose-200"><span class="w-1.5 h-1.5 rounded-full bg-rose-600"></span>${escapeHtml(l.type)}</span>`;
                    iconBg = 'bg-rose-50 text-rose-600 border border-rose-200/80';
                } else if (l.type === 'IDE_QUARANTINE') {
                    badge = `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-mono font-bold bg-purple-50 text-purple-700 border border-purple-200"><span class="w-1.5 h-1.5 rounded-full bg-purple-600"></span>${escapeHtml(l.type)}</span>`;
                    iconBg = 'bg-purple-50 text-purple-600 border border-purple-200/80';
                } else if (l.type === 'ROOM_LOCK' || l.type === 'GLOBAL_LOCK') {
                    badge = `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-mono font-bold bg-emerald-50 text-emerald-800 border border-emerald-200"><span class="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>${escapeHtml(l.type)}</span>`;
                    iconBg = 'bg-emerald-50 text-emerald-600 border border-emerald-200/80';
                } else if (l.type === 'ROOM_UNLOCK' || l.type === 'GLOBAL_UNLOCK' || l.severity === 'WARNING') {
                    badge = `<span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[10px] font-mono font-bold bg-amber-50 text-amber-800 border border-amber-200"><span class="w-1.5 h-1.5 rounded-full bg-amber-500"></span>${escapeHtml(l.type)}</span>`;
                    iconBg = 'bg-amber-50 text-amber-600 border border-amber-200/80';
                }

                const roomTag = l.room_code && l.room_code !== 'ALL'
                    ? `<span class="px-1.5 py-0.5 rounded-md font-mono text-[10px] font-bold bg-slate-100 text-slate-700 border border-slate-200">${escapeHtml(l.room_code)}</span>`
                    : '';

                // Format process or domain in message as code badge
                let formattedMsg = escapeHtml(l.message);
                formattedMsg = formattedMsg.replace(/\[([^\]]+)\]/g, '<code class="px-1.5 py-0.5 rounded bg-slate-100 font-mono text-[11px] font-semibold text-slate-800 border border-slate-200">$1</code>');

                return `
                    <div class="p-3.5 sm:px-5 sm:py-3.5 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 hover:bg-slate-50/90 transition text-xs border-b border-slate-100 last:border-0 group">
                        <div class="flex flex-col sm:flex-row sm:items-center gap-2.5 min-w-0 flex-1">
                            <div class="flex items-center gap-2 shrink-0 flex-wrap">
                                ${badge}
                                ${roomTag}
                                <span class="font-bold text-slate-900">${escapeHtml(l.name)}</span>
                                <span class="font-mono text-[11px] text-slate-400">(${escapeHtml(l.nim || '')}${l.hostname ? ' · ' + escapeHtml(l.hostname) : ''})</span>
                            </div>
                            <div class="text-slate-600 sm:truncate font-mono sm:font-sans text-[11px] sm:text-xs bg-slate-50 sm:bg-transparent px-3 py-2 sm:p-0 rounded-xl sm:rounded-none border sm:border-0 border-slate-200/60 sm:border-transparent leading-relaxed">
                                ${formattedMsg}
                            </div>
                        </div>
                        <div class="flex items-center gap-2 shrink-0 self-end sm:self-auto">
                            <span class="font-mono text-[11px] text-slate-400 tabular-nums">${escapeHtml(l.timestamp)}</span>
                        </div>
                    </div>
                `;
            }).join('');
        }

        async function setGlobalStatus(status) {
            const nowMs = Date.now();
            for (const [id, entry] of knownStudentsCache.entries()) {
                if (entry && entry.student) {
                    const rCode = (entry.student.room_code || '').toUpperCase();
                    if (!selectedRoom || selectedRoom === 'ALL' || rCode === selectedRoom.toUpperCase()) {
                        entry.student.status = status;
                        entry.student.target_status = status;
                        recentLocalStudentCommands.set(id, { status, ts: nowMs });
                    }
                }
            }
            renderCurrentStudents();

            const res = await fetch('api/index.php?action=set_global_status', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({status, room_code: selectedRoom})
            });
            const data = await res.json();
            if (data.ok) {
                const scopeText = selectedRoom === 'ALL' ? 'Seluruh ruangan' : `Ruangan [${selectedRoom}]`;
                showToast(status === 'LOCKED'
                    ? `${scopeText} dikunci (Proteksi AI Aktif)`
                    : `${scopeText} dibuka & dipulihkan! Minta peserta tutup & buka kembali browser (Chrome/Edge).`);
                fetchDashboard();
            }
        }

        async function setRoomStatusDirect(roomCode, status) {
            const nowMs = Date.now();
            for (const [id, entry] of knownStudentsCache.entries()) {
                if (entry && entry.student) {
                    const rCode = (entry.student.room_code || '').toUpperCase();
                    if (rCode === (roomCode || '').toUpperCase()) {
                        entry.student.status = status;
                        entry.student.target_status = status;
                        recentLocalStudentCommands.set(id, { status, ts: nowMs });
                    }
                }
            }
            renderCurrentStudents();

            const res = await fetch('api/index.php?action=set_global_status', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({status, room_code: roomCode})
            });
            const data = await res.json();
            if (data.ok) {
                showToast(status === 'LOCKED'
                    ? `Ruangan [${roomCode}] dikunci (Proteksi AI Aktif)`
                    : `Ruangan [${roomCode}] dibuka! Minta peserta tutup & buka kembali browser (Chrome/Edge).`);
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
                recentLocalStudentCommands.delete(student_id);
                setTimeout(() => explicitlyRemovedStudentIds.delete(student_id), 25000);
            } else if (['LOCKED', 'UNLOCKED'].includes(command)) {
                recentLocalStudentCommands.set(student_id, { status: command, ts: Date.now() });
                if (knownStudentsCache.has(student_id)) {
                    const entry = knownStudentsCache.get(student_id);
                    entry.student.status = command;
                    entry.student.target_status = command;
                }
                const localSt = cachedStudents.find(s => s.id === student_id);
                if (localSt) {
                    localSt.status = command;
                    localSt.target_status = command;
                }
                renderCurrentStudents();
            }
            const res = await fetch('api/index.php?action=set_student_command', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({student_id, command})
            });
            const data = await res.json();
            if (data.ok) {
                if (command === 'UPDATE') {
                    showToast('Instruksi pembaruan (self-update) berhasil dikirim ke PC peserta');
                } else {
                    showToast('Perintah berhasil dikirim ke perangkat peserta');
                }
                fetchDashboard();
            }
        }

        async function saveConfig() {
            const payload = {
                room_code: document.getElementById('cfgRoomCode').value,
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
                const passEl = document.getElementById('cfgAdminPassword');
                if (passEl) passEl.value = '';
                await syncStateToFirebaseIfAvailable(data.config, null);
                showToast('Pengaturan ruangan & modul proteksi disimpan');
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
                await syncStateToFirebaseIfAvailable({
                    room_code: currentRoomCode,
                    rooms: cachedRooms,
                    room_statuses: cachedRoomStatuses,
                    room_pins: cachedRoomPins,
                    room_blocklists: cachedRoomBlocklists,
                    global_status: cachedGlobalStatus,
                    blocklist_customized: true
                }, data.blocklist || cachedGlobalBlocklist);
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
                await syncStateToFirebaseIfAvailable({
                    room_code: currentRoomCode,
                    rooms: cachedRooms,
                    room_statuses: cachedRoomStatuses,
                    room_pins: cachedRoomPins,
                    room_blocklists: cachedRoomBlocklists,
                    global_status: cachedGlobalStatus,
                    blocklist_customized: true
                }, data.blocklist || cachedGlobalBlocklist);
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
