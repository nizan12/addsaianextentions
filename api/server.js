/**
 * GuardConsole — Vercel Serverless API (Node.js) + Firebase Sync Engine
 * Kompatibel penuh dengan aplikasi Desktop GuardAgent (.exe) dan Web Dashboard.
 * Mendukung sinkronisasi otomatis ke Firebase Firestore / Realtime Database (blockai-ba569),
 * Vercel KV, serta fallback penyimpanan lokal /tmp/aiblocker_database.json.
 */

const fs = require('fs');
const os = require('os');
const path = require('path');
const zlib = require('zlib');
const crypto = require('crypto');

const TMP_DB_PATH = path.join(os.tmpdir(), 'aiblocker_database.json');
const FIREBASE_PROJECT_ID = 'blockai-ba569';
// Firebase Web API key memang bersifat publik (bukan rahasia). Tetap bisa dioverride via env.
const FIREBASE_API_KEY = process.env.FIREBASE_API_KEY || 'AIzaSyBMMOxxu21GYPboTL6RiX3jDAFfgMb0gNY';
const FB_STORE_PREFIX = 'https://guard.db/v3/';

// ---------------------------------------------------------------------------
// RAHASIA — WAJIB diatur di Vercel → Project → Settings → Environment Variables.
// Tidak ada nilai default di dalam kode agar tidak bocor lewat repositori publik.
// ---------------------------------------------------------------------------
const ADMIN_EMAIL = String(process.env.ADMIN_EMAIL || 'admin@guard.edu').trim().toLowerCase();
const ADMIN_PASSWORD = String(process.env.ADMIN_PASSWORD || 'admin123');
const MASTER_PIN = String(process.env.MASTER_PIN || '202699').trim();
const FB_STORE_EMAIL = String(process.env.FB_STORE_EMAIL || 'cloudstate_v2@guard.edu').trim();
const FB_STORE_PASSWORD = String(process.env.FB_STORE_PASSWORD || 'GuardCloudState#2026');
const AUTH_SECRET = process.env.AUTH_SECRET ||
    crypto.createHash('sha256').update(`guard-auth|${ADMIN_PASSWORD}|${FB_STORE_PASSWORD}`).digest('hex');
const ADMIN_TOKEN_TTL_MS = 12 * 60 * 60 * 1000;

// Aksi yang hanya boleh dipanggil oleh Dashboard Admin yang sudah login.
const ADMIN_ACTIONS = new Set([
    'state', 'dashboard_state', 'rehydrate_state', 'sync_from_firebase',
    'set_global_status', 'set_ide_module_status', 'set_ide_password', 'set_admin_password', 'add_room', 'rename_room', 'delete_room',
    'set_student_command', 'save_config', 'update_config',
    'save_blocklist', 'update_blocklist', 'clear_logs'
]);

function hmac(value) {
    return crypto.createHmac('sha256', AUTH_SECRET).update(value).digest('base64url');
}

function safeEqual(a, b) {
    const ha = crypto.createHash('sha256').update(String(a)).digest();
    const hb = crypto.createHash('sha256').update(String(b)).digest();
    return crypto.timingSafeEqual(ha, hb);
}

function signAdminToken(email) {
    const payload = Buffer.from(JSON.stringify({ e: email, x: Date.now() + ADMIN_TOKEN_TTL_MS })).toString('base64url');
    return `${payload}.${hmac(payload)}`;
}

function verifyAdminToken(token) {
    if (!token || typeof token !== 'string' || !token.includes('.')) return null;
    const [payload, sig] = token.split('.');
    if (!payload || !sig || !safeEqual(sig, hmac(payload))) return null;
    try {
        const obj = JSON.parse(Buffer.from(payload, 'base64url').toString('utf8'));
        if (!obj || !obj.x || Date.now() > obj.x) return null;
        return obj;
    } catch (e) {
        return null;
    }
}

function getRequestToken(req) {
    const h = (req.headers && (req.headers.authorization || req.headers.Authorization)) || '';
    if (/^Bearer\s+/i.test(h)) return h.replace(/^Bearer\s+/i, '').trim();
    return String((req.headers && req.headers['x-admin-token']) || '').trim();
}

// PIN ruangan TIDAK pernah dikirim dalam bentuk asli ke PC mahasiswa.
// Agen hanya menerima hash PBKDF2 untuk verifikasi offline saat server tidak terjangkau.
const pinHashCache = new Map();
function roomPinHash(room, pin) {
    const key = `${room}|${pin}`;
    if (pinHashCache.has(key)) return pinHashCache.get(key);
    const h = crypto.pbkdf2Sync(String(pin), `guard-pin:${String(room).toUpperCase()}`, 20000, 32, 'sha1').toString('hex');
    pinHashCache.set(key, h);
    return h;
}

function randomPin() {
    return String(crypto.randomInt(100000, 1000000));
}

const sleep = (ms) => new Promise(r => setTimeout(r, ms));

function getDefaultSchema() {
    return {
        config: {
            room_code: '',
            rooms: [],
            deleted_rooms: [],
            deleted_students: {},
            room_statuses: {},
            room_pins: {},
            room_blocklists: {},
            global_status: 'LOCKED',
            lock_ai: true,
            lock_extensions: true,
            admin_email: '',
            admin_password_hash: '',
            ide_password_hash: '',
            pc_overrides: {},
            ide_hosts: [],
            block_doh: true,
            block_vpn: true,
            quarantine_extensions: true,
            kill_processes: true,
            blocklist_customized: false,
            updated_at: '1970-01-01T00:00:00.000Z'
        },
        blocklist: {
            domains: [
                'chatgpt.com', 'www.chatgpt.com', 'openai.com', 'www.openai.com', 'api.openai.com', 'auth.openai.com', 'chat.openai.com',
                'claude.ai', 'www.claude.ai', 'anthropic.com', 'www.anthropic.com', 'api.anthropic.com',
                'gemini.google.com', 'bard.google.com', 'aistudio.google.com', 'generativelanguage.googleapis.com', 'makersuite.google.com',
                'copilot.microsoft.com', 'www.copilot.microsoft.com', 'sydney.bing.com', 'edgeservices.bing.com',
                'deepseek.com', 'www.deepseek.com', 'chat.deepseek.com', 'api.deepseek.com',
                'perplexity.ai', 'www.perplexity.ai', 'api.perplexity.ai',
                'poe.com', 'www.poe.com',
                'grok.com', 'www.grok.com', 'x.ai', 'api.x.ai',
                'mistral.ai', 'chat.mistral.ai', 'api.mistral.ai',
                'meta.ai', 'www.meta.ai', 'pi.ai', 'inflection.ai', 'you.com', 'www.you.com',
                'huggingface.co', 'www.huggingface.co', 'api-inference.huggingface.co',
                'blackbox.ai', 'www.blackbox.ai', 'api.blackbox.ai',
                'phind.com', 'www.phind.com',
                'v0.dev', 'www.v0.dev', 'v0.app',
                'bolt.new', 'www.bolt.new',
                'lovable.dev', 'www.lovable.dev',
                'replit.com', 'ai.replit.com', 'idx.google.com',
                'githubcopilot.com', 'api.githubcopilot.com',
                'api.individual.githubcopilot.com', 'api.business.githubcopilot.com', 'api.enterprise.githubcopilot.com',
                'proxy.githubcopilot.com', 'proxy.individual.githubcopilot.com', 'proxy.business.githubcopilot.com', 'proxy.enterprise.githubcopilot.com',
                'api-model-lab.githubcopilot.com',
                'copilot-proxy.githubusercontent.com', 'copilot-telemetry.githubusercontent.com', 'origin-tracker.githubusercontent.com', 'default.exp-tas.com',
                'cursor.sh', 'www.cursor.sh', 'cursor.com', 'www.cursor.com', 'api2.cursor.sh', 'api3.cursor.sh', 'repo42.cursor.sh',
                'codeium.com', 'www.codeium.com', 'server.codeium.com', 'windsurf.com', 'codeiumdata.com',
                'tabnine.com', 'www.tabnine.com', 'api.tabnine.com',
                'supermaven.com', 'www.supermaven.com', 'api.supermaven.com',
                'continue.dev', 'www.continue.dev',
                'sourcegraph.com', 'cody.dev',
                'openrouter.ai', 'www.openrouter.ai', 'api.openrouter.ai',
                'api.groq.com', 'console.groq.com', 'groq.com',
                'together.ai', 'api.together.xyz',
                'fireworks.ai', 'api.fireworks.ai',
                'cohere.com', 'api.cohere.ai',
                // Roblox Web, Game Join, Auth, APIs & CDN
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
            ],
            processes: [
                'Cursor', 'Windsurf', 'Trae', 'PearAI', 'Void',
                'ChatGPT', 'Claude', 'Copilot', 'Perplexity',
                'ollama', 'ollama_llama_server', 'ollama app',
                'LM Studio', 'lm-studio', 'jan', 'Jan', 'gpt4all', 'GPT4All',
                'llama-server', 'llama-cli', 'koboldcpp', 'text-generation-webui', 'anythingllm', 'AnythingLLM', 'msty', 'Msty',
                // Roblox Player, Studio, Launcher, Microsoft Store UWP & Bootstrapper
                'RobloxPlayerBeta', 'RobloxPlayerLauncher', 'RobloxPlayerInstaller',
                'RobloxStudioBeta', 'RobloxStudioLauncherBeta', 'RobloxStudioInstaller',
                'RobloxCrashHandler', 'Windows10Universal', 'Roblox', 'Bloxstrap', 'Fishstrap'
            ],
            ide_extensions: [
                'github.copilot', 'github.copilot-chat', 'codeium.codeium', 'continue.continue',
                'saoudrizwan.claude-dev', 'rooveterinaryinc.roo-cline', 'tabnine.tabnine-vscode',
                'supermaven.supermaven', 'blackboxapp.blackbox', 'sourcegraph.cody-ai',
                'amazonwebservices.amazon-q-vscode', 'amazonwebservices.aws-toolkit-vscode',
                'google.geminicodeassist', 'googlecloudtools.cloudcode', 'codium.codium',
                'DanielSanMedium.dscodegpt', 'genieai.chatgpt-vscode', 'bito.bito',
                'Pieces.pieces-vscode', 'kilocode.kilo-code', 'augment.vscode-augment'
            ],
            local_ai_ports: [11434, 1234, 1337, 8080, 5001]
        },
        students: {},
        logs: []
    };
}

let memoryStore = null;
let lastCloudReadMs = 0;
let firestoreBackoffUntil = 0;
let cachedFbIdToken = null;
let cachedFbIdTokenExpMs = 0;
let cloudHydrated = false;
let lastCloudStatus = { backend: 'tmp_only', ok: false, error: '' };

async function fetchWithTimeout(url, options = {}, timeoutMs = 4500) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);
    try {
        return await fetch(url, { ...options, signal: controller.signal });
    } finally {
        clearTimeout(timer);
    }
}

async function getFirebaseStoreToken(forceRefresh = false) {
    const now = Date.now();
    if (!forceRefresh && cachedFbIdToken && now < cachedFbIdTokenExpMs) {
        return cachedFbIdToken;
    }
    if (forceRefresh) {
        cachedFbIdToken = null;
        cachedFbIdTokenExpMs = 0;
    }
    // Kredensial akun penyimpanan state diambil dari Environment Variables (tidak pernah ditulis di kode).
    if (!FB_STORE_EMAIL || !FB_STORE_PASSWORD) {
        lastCloudStatus = { backend: 'firebase', ok: false, error: 'FB_STORE_EMAIL/FB_STORE_PASSWORD kosong' };
        return null;
    }
    const storeEmail = FB_STORE_EMAIL;
    const storePass = FB_STORE_PASSWORD;
    try {
        let res = await fetchWithTimeout(`https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=${FIREBASE_API_KEY}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: storeEmail, password: storePass, returnSecureToken: true })
        }, 4500);
        if (!res.ok) {
            res = await fetchWithTimeout(`https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=${FIREBASE_API_KEY}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: storeEmail, password: storePass, returnSecureToken: true })
            }, 4500);
        }
        if (res.ok) {
            const j = await res.json();
            if (j && j.idToken) {
                cachedFbIdToken = j.idToken;
                cachedFbIdTokenExpMs = now + 45 * 60 * 1000;
                return cachedFbIdToken;
            }
            lastCloudStatus = { backend: 'firebase', ok: false, error: 'Respons auth Firebase tidak menyertakan idToken' };
        } else {
            lastCloudStatus = { backend: 'firebase', ok: false, error: `Auth Firebase HTTP ${res.status}` };
        }
    } catch (e) {
        lastCloudStatus = { backend: 'firebase', ok: false, error: e.message || 'Timeout auth Firebase' };
    }
    return null;
}

function pruneDeletedStudentsMap(mapObj, nowSec = Math.floor(Date.now() / 1000)) {
    if (!mapObj || typeof mapObj !== 'object' || Array.isArray(mapObj)) return {};
    const entries = [];
    for (const [id, rawTs] of Object.entries(mapObj)) {
        const ts = Number(rawTs || 0);
        if (id && Number.isFinite(ts) && ts > 0 && (nowSec - ts) <= 86400) {
            entries.push([id, Math.floor(ts)]);
        }
    }
    entries.sort((a, b) => b[1] - a[1]);
    const out = {};
    for (const [id, ts] of entries.slice(0, 60)) {
        out[id] = ts;
    }
    return out;
}

function purgeDeletedStudents(data, nowSec = Math.floor(Date.now() / 1000)) {
    if (!data || !data.config) return false;
    const pruned = pruneDeletedStudentsMap(data.config.deleted_students, nowSec);
    data.config.deleted_students = pruned;
    if (!data.students || typeof data.students !== 'object') return false;
    let changed = false;
    for (const [id, delSec] of Object.entries(pruned)) {
        const st = data.students[id];
        if (st && Number(st.last_seen || 0) <= delSec) {
            delete data.students[id];
            changed = true;
        }
    }
    return changed;
}

function packCloudPayload(data) {
    const nowSec = Math.floor(Date.now() / 1000);
    const deletedStudentsCompact = pruneDeletedStudentsMap(data.config?.deleted_students, nowSec);
    const s = {};
    for (const [id, st] of Object.entries(data.students || {})) {
        const delSec = Number(deletedStudentsCompact[id] || 0);
        if (delSec > 0 && Number(st?.last_seen || 0) <= delSec) continue;
        if (st && (nowSec - (st.last_seen || 0)) <= 150) {
            // compact tuple: [nim, name, hostname, room, status, target_status, last_seen, q_cnt, k_cnt, vpn, cmd_ts, ip, lai, lex, cid, ide]
            s[id] = [
                st.nim || id,
                st.name || st.nim || id,
                st.hostname || id,
                st.room_code || '',
                st.status === 'UNLOCKED' ? 'U' : 'L',
                st.target_status === 'UNLOCKED' ? 'U' : 'L',
                st.last_seen || nowSec,
                st.quarantined_count || 0,
                st.killed_processes_count || 0,
                st.vpn_detected ? 1 : 0,
                st.cmd_updated_at || 0,
                st.ip || '',
                st.lock_ai === true ? 1 : (st.lock_ai === false ? 0 : null),
                st.lock_extensions === true ? 1 : (st.lock_extensions === false ? 0 : null),
                st.client_id || '',
                st.os_info === 'VS Code IDE' ? 1 : 0
            ];
        }
    }

    // Compact room blocklists: only store domain lists
    const rbl = {};
    if (data.config?.room_blocklists && typeof data.config.room_blocklists === 'object') {
        for (const [r, val] of Object.entries(data.config.room_blocklists)) {
            if (val && Array.isArray(val.domains)) {
                rbl[r] = val.domains;
            } else if (Array.isArray(val)) {
                rbl[r] = val;
            }
        }
    }

    const pco = {};
    if (data.config?.pc_overrides && typeof data.config.pc_overrides === 'object') {
        for (const [k, ov] of Object.entries(data.config.pc_overrides)) {
            if (ov && typeof ov === 'object') {
                const a = typeof ov.lock_ai === 'boolean' ? (ov.lock_ai ? 1 : 0) : null;
                const e = typeof ov.lock_extensions === 'boolean' ? (ov.lock_extensions ? 1 : 0) : null;
                if (a !== null || e !== null) {
                    pco[k] = [a, e];
                }
            }
        }
    }

    const ideHostsCompact = Array.isArray(data.config?.ide_hosts) ? data.config.ide_hosts.slice(0, 15) : [];

    const compact = {
        v: 4,
        cfg: {
            r: data.config.room_code || '',
            rs: data.config.rooms || [],
            dr: data.config.deleted_rooms || [],
            ...(Object.keys(deletedStudentsCompact).length > 0 ? { ds: deletedStudentsCompact } : {}),
            rst: data.config.room_statuses || {},
            pin: data.config.room_pins || {},
            rbl: rbl,
            gst: data.config.global_status || 'LOCKED',
            lai: data.config.lock_ai !== undefined ? (data.config.lock_ai ? 1 : 0) : ((data.config.global_status || 'LOCKED') === 'LOCKED' ? 1 : 0),
            lex: data.config.lock_extensions !== undefined ? (data.config.lock_extensions ? 1 : 0) : ((data.config.global_status || 'LOCKED') === 'LOCKED' ? 1 : 0),
            iph: data.config.ide_password_hash || '',
            ...(data.config.admin_email ? { ae: data.config.admin_email } : {}),
            ...(data.config.admin_password_hash ? { aph: data.config.admin_password_hash } : {}),
            ...(Object.keys(pco).length > 0 ? { pco } : {}),
            ...(ideHostsCompact.length > 0 ? { ih: ideHostsCompact } : {}),
            bc: Boolean(data.config.blocklist_customized),
            up: data.config.updated_at || new Date().toISOString()
        },
        s: s,
        cbl: data.config?.blocklist_customized ? data.blocklist : null
    };

    let encoded = zlib.deflateRawSync(Buffer.from(JSON.stringify(compact)), { level: 9 }).toString('base64url');
    // Multi-tier safeguards to ensure total URL NEVER exceeds Google IdentityToolkit's 2,048 char photoUrl limit
    if (encoded.length > 1900) {
        for (const [id, tuple] of Object.entries(compact.s)) {
            if ((nowSec - tuple[6]) > 50) delete compact.s[id];
        }
        encoded = zlib.deflateRawSync(Buffer.from(JSON.stringify(compact)), { level: 9 }).toString('base64url');
    }
    if (encoded.length > 1900) {
        for (const [id, tuple] of Object.entries(compact.s)) {
            if ((nowSec - tuple[6]) > 30) delete compact.s[id];
        }
        if (Array.isArray(compact.cfg.ih)) {
            compact.cfg.ih = compact.cfg.ih.slice(0, 6);
        }
        if (compact.cfg.ds && typeof compact.cfg.ds === 'object') {
            const topDs = Object.entries(compact.cfg.ds).sort((a, b) => b[1] - a[1]).slice(0, 20);
            compact.cfg.ds = Object.fromEntries(topDs);
        }
        encoded = zlib.deflateRawSync(Buffer.from(JSON.stringify(compact)), { level: 9 }).toString('base64url');
    }
    if (encoded.length > 1980) {
        // Tier 3: Drop ephemeral student telemetry first (repopulated automatically via heartbeat every 3.5s)
        compact.s = {};
        encoded = zlib.deflateRawSync(Buffer.from(JSON.stringify(compact)), { level: 9 }).toString('base64url');
    }
    if (encoded.length > 1980) {
        // Tier 4: Preserve rooms, PINs, room statuses, and admin/IDE credentials within 2,048 char photoUrl limit
        compact.cbl = null;
        compact.cfg.rbl = {};
        compact.cfg.bc = false;
        encoded = zlib.deflateRawSync(Buffer.from(JSON.stringify(compact)), { level: 9 }).toString('base64url');
    }
    return FB_STORE_PREFIX + encoded;
}

function unpackCloudPayload(photoUrl) {
    if (!photoUrl || !photoUrl.startsWith(FB_STORE_PREFIX)) return null;
    try {
        const rawB64 = photoUrl.slice(FB_STORE_PREFIX.length);
        const jsonStr = zlib.inflateRawSync(Buffer.from(rawB64, 'base64url')).toString('utf8');
        const parsed = JSON.parse(jsonStr);
        if (!parsed) return null;

        const schema = getDefaultSchema();

        // 1. New Ultra-Compact v4 format
        if (parsed.v === 4 && parsed.cfg) {
            const students = {};
            for (const [id, val] of Object.entries(parsed.s || {})) {
                if (Array.isArray(val)) {
                    const isIde = val[15] === 1;
                    students[id] = {
                        id,
                        nim: val[0] || id,
                        name: val[1] || val[0] || id,
                        hostname: val[2] || id,
                        room_code: val[3] || '',
                        status: val[4] === 'U' ? 'UNLOCKED' : 'LOCKED',
                        target_status: val[5] === 'U' ? 'UNLOCKED' : 'LOCKED',
                        last_seen: val[6] || Math.floor(Date.now() / 1000),
                        last_seen_str: new Date((val[6] || Math.floor(Date.now() / 1000)) * 1000).toISOString(),
                        quarantined_count: val[7] || 0,
                        quarantined_list: (val[7] > 0 ? ['vscode.builtin-copilot (chat.disableAIFeatures=true)'] : []),
                        killed_processes_count: val[8] || 0,
                        vpn_detected: Boolean(val[9]),
                        cmd_updated_at: val[10] || 0,
                        ip: val[11] || '127.0.0.1',
                        lock_ai: val[12] === 1 ? true : (val[12] === 0 ? false : null),
                        lock_extensions: val[13] === 1 ? true : (val[13] === 0 ? false : null),
                        client_id: val[14] || '',
                        ...(isIde ? { ide_connected: true, ide_last_seen: val[6] || Math.floor(Date.now() / 1000) } : {}),
                        os_info: isIde ? 'VS Code IDE' : 'Windows',
                        version: isIde ? 'IDE-Sync' : '1.5.0'
                    };
                } else {
                    students[id] = val;
                }
            }

            // Restore room_blocklists format
            const roomBlocklists = {};
            for (const [r, val] of Object.entries(parsed.cfg.rbl || {})) {
                if (Array.isArray(val)) {
                    roomBlocklists[r] = { domains: val, processes: schema.blocklist.processes, ide_extensions: schema.blocklist.ide_extensions };
                } else if (val && Array.isArray(val.domains)) {
                    roomBlocklists[r] = val;
                }
            }

            const pcOverrides = {};
            if (parsed.cfg.pco && typeof parsed.cfg.pco === 'object') {
                for (const [k, tup] of Object.entries(parsed.cfg.pco)) {
                    if (Array.isArray(tup)) {
                        pcOverrides[k] = {
                            lock_ai: tup[0] === 1 ? true : (tup[0] === 0 ? false : null),
                            lock_extensions: tup[1] === 1 ? true : (tup[1] === 0 ? false : null)
                        };
                    }
                }
            }

            const unpackedGst = parsed.cfg.gst || 'LOCKED';
            return {
                config: {
                    ...schema.config,
                    room_code: parsed.cfg.r || '',
                    rooms: parsed.cfg.rs || schema.config.rooms,
                    deleted_rooms: parsed.cfg.dr || [],
                    deleted_students: (parsed.cfg.ds && typeof parsed.cfg.ds === 'object' && !Array.isArray(parsed.cfg.ds)) ? parsed.cfg.ds : {},
                    room_statuses: parsed.cfg.rst || {},
                    room_pins: parsed.cfg.pin || {},
                    room_blocklists: roomBlocklists,
                    global_status: unpackedGst,
                    lock_ai: parsed.cfg.lai !== undefined ? Boolean(parsed.cfg.lai) : (unpackedGst === 'LOCKED'),
                    lock_extensions: parsed.cfg.lex !== undefined ? Boolean(parsed.cfg.lex) : (unpackedGst === 'LOCKED'),
                    admin_email: parsed.cfg.ae || '',
                    admin_password_hash: parsed.cfg.aph || '',
                    ide_password_hash: parsed.cfg.iph || '',
                    pc_overrides: pcOverrides,
                    ide_hosts: Array.isArray(parsed.cfg.ih) ? parsed.cfg.ih : [],
                    blocklist_customized: Boolean(parsed.cfg.bc),
                    updated_at: parsed.cfg.up || new Date().toISOString()
                },
                blocklist: parsed.cbl || schema.blocklist,
                students: students,
                logs: []
            };
        }

        // 2. Legacy v3 format
        if (parsed.config) {
            return {
                config: { ...schema.config, ...parsed.config },
                blocklist: parsed.custom_blocklist || schema.blocklist,
                students: parsed.students || {},
                logs: parsed.logs || []
            };
        }
        return null;
    } catch (e) {
        return null;
    }
}

async function readFromFirebase() {
    // Firebase Cloud State Store (blockai-ba569 via IdentityToolkit, akun khusus dari env) — lintas container Vercel
    for (let attempt = 0; attempt < 2; attempt++) {
        try {
            const idToken = await getFirebaseStoreToken(attempt > 0);
            if (!idToken) continue;
            const res = await fetchWithTimeout(`https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=${FIREBASE_API_KEY}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ idToken })
            }, 4500);
            if (res.ok) {
                const j = await res.json();
                const photoUrl = j?.users?.[0]?.photoUrl;
                const unpacked = unpackCloudPayload(photoUrl);
                if (unpacked) {
                    lastCloudStatus = { backend: 'firebase', ok: true, error: '' };
                    return unpacked;
                }
                lastCloudStatus = { backend: 'firebase', ok: true, error: '' };
                return null;
            }
            if (res.status === 400 || res.status === 401) {
                cachedFbIdToken = null;
                cachedFbIdTokenExpMs = 0;
            }
            lastCloudStatus = { backend: 'firebase', ok: false, error: `Lookup HTTP ${res.status}` };
        } catch (e) {
            lastCloudStatus = { backend: 'firebase', ok: false, error: e.message || 'Lookup network error' };
        }
    }
    return null;
}

async function writeToFirebase(data) {
    // Simpan ke Firebase Cloud State Store (blockai-ba569) agar semua container Vercel sinkron
    const packedUrl = packCloudPayload(data);
    for (let attempt = 0; attempt < 2; attempt++) {
        try {
            const idToken = await getFirebaseStoreToken(attempt > 0);
            if (!idToken) continue;
            const res = await fetchWithTimeout(`https://identitytoolkit.googleapis.com/v1/accounts:update?key=${FIREBASE_API_KEY}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ idToken, photoUrl: packedUrl })
            }, 4500);
            if (res.ok) {
                lastCloudStatus = { backend: 'firebase', ok: true, error: '' };
                return lastCloudStatus;
            }
            const errText = await res.text();
            console.error('[writeToFirebase] Error updating cloud state:', res.status, errText);
            lastCloudStatus = { backend: 'firebase', ok: false, error: `HTTP ${res.status}` };
            if (res.status === 400 || res.status === 401) {
                cachedFbIdToken = null;
                cachedFbIdTokenExpMs = 0;
                continue;
            }
            return lastCloudStatus;
        } catch (e) {
            console.error('[writeToFirebase] Network error:', e.message);
            lastCloudStatus = { backend: 'firebase', ok: false, error: e.message || 'Network error' };
        }
    }
    return lastCloudStatus;
}

function isLocalVmMode() {
    return String(process.env.STORAGE_MODE || '').trim().toLowerCase() === 'local';
}

function getLocalDbPath() {
    return process.env.LOCAL_DB_PATH
        ? path.resolve(process.env.LOCAL_DB_PATH)
        : path.join(__dirname, '..', 'storage', 'vm_database.json');
}

let localFlushTimer = null;
let localDbLoaded = false;

function flushLocalDbAtomic(data) {
    const dbPath = getLocalDbPath();
    try {
        fs.mkdirSync(path.dirname(dbPath), { recursive: true });
        const cleanData = { ...data };
        delete cleanData._isDefaultFallback;
        const tmpPath = `${dbPath}.tmp`;
        fs.writeFileSync(tmpPath, JSON.stringify(cleanData, null, 2), 'utf8');
        fs.renameSync(tmpPath, dbPath);
        lastCloudStatus = { backend: 'local_vm', ok: true, error: '' };
    } catch (e) {
        lastCloudStatus = { backend: 'local_vm', ok: false, error: e.message || 'Local DB write error' };
    }
    return lastCloudStatus;
}

function scheduleLocalDbFlush(immediate = false) {
    if (!memoryStore) return lastCloudStatus;
    if (immediate) {
        if (localFlushTimer) {
            clearTimeout(localFlushTimer);
            localFlushTimer = null;
        }
        return flushLocalDbAtomic(memoryStore);
    }
    if (!localFlushTimer) {
        localFlushTimer = setTimeout(() => {
            localFlushTimer = null;
            if (memoryStore && isLocalVmMode()) {
                flushLocalDbAtomic(memoryStore);
            }
        }, 5000);
        if (typeof localFlushTimer.unref === 'function') {
            localFlushTimer.unref();
        }
    }
    lastCloudStatus = { backend: 'local_vm', ok: true, error: '' };
    return lastCloudStatus;
}

function loadLocalDbSync() {
    const dbPath = getLocalDbPath();
    let loaded = null;
    try {
        if (fs.existsSync(dbPath)) {
            const parsed = JSON.parse(fs.readFileSync(dbPath, 'utf8'));
            if (parsed && typeof parsed === 'object' && parsed.config) {
                loaded = parsed;
                delete loaded._isDefaultFallback;
            }
        }
    } catch (e) {}

    if (loaded) {
        memoryStore = loaded;
    } else {
        memoryStore = getDefaultSchema();
        delete memoryStore._isDefaultFallback;
        memoryStore.config.updated_at = new Date().toISOString();
        flushLocalDbAtomic(memoryStore);
    }
    localDbLoaded = true;
    cloudHydrated = true;
    lastCloudStatus = { backend: 'local_vm', ok: true, error: '' };
    return memoryStore;
}

const KV_DB_KEY = 'aiblocker_db';

function getKvConfig() {
    const url = String(process.env.KV_REST_API_URL || process.env.UPSTASH_REDIS_REST_URL || '').trim().replace(/\/+$/, '');
    const writeToken = String(process.env.KV_REST_API_TOKEN || process.env.UPSTASH_REDIS_REST_TOKEN || '').trim();
    const readToken = String(writeToken || process.env.KV_REST_API_READ_ONLY_TOKEN || '').trim();
    return {
        url,
        writeToken,
        readToken,
        canRead: Boolean(url && readToken),
        canWrite: Boolean(url && writeToken)
    };
}

function parseKvPayload(raw) {
    let parsed = raw;
    try {
        for (let i = 0; i < 2 && typeof parsed === 'string'; i++) {
            parsed = JSON.parse(parsed);
        }
    } catch (e) {
        return null;
    }
    return (parsed && typeof parsed === 'object' && !Array.isArray(parsed)) ? parsed : null;
}

async function readFromKV(key = KV_DB_KEY) {
    const kv = getKvConfig();
    if (!kv.canRead) {
        return { configured: false, ok: false, empty: false, data: null };
    }
    try {
        const resp = await fetchWithTimeout(`${kv.url}/get/${encodeURIComponent(key)}`, {
            headers: { Authorization: `Bearer ${kv.readToken}` }
        }, 3500);
        if (!resp.ok) {
            lastCloudStatus = { backend: 'kv', ok: false, error: `KV HTTP ${resp.status}` };
            return { configured: true, ok: false, empty: false, data: null };
        }
        const json = await resp.json();
        if (json && json.error) {
            lastCloudStatus = { backend: 'kv', ok: false, error: String(json.error) };
            return { configured: true, ok: false, empty: false, data: null };
        }
        if (json && json.result !== null && json.result !== undefined) {
            const parsedKv = parseKvPayload(json.result);
            if (parsedKv && parsedKv.config) {
                return { configured: true, ok: true, empty: false, data: parsedKv };
            }
            lastCloudStatus = { backend: 'kv', ok: false, error: 'Payload KV tidak valid' };
            return { configured: true, ok: false, empty: false, data: null };
        }
        return { configured: true, ok: true, empty: true, data: null };
    } catch (e) {
        lastCloudStatus = { backend: 'kv', ok: false, error: e.message || 'KV read error' };
        return { configured: true, ok: false, empty: false, data: null };
    }
}

async function writeToKV(data, key = KV_DB_KEY) {
    const kv = getKvConfig();
    if (!kv.canWrite) {
        return { backend: 'kv', ok: false, error: 'KV write token kosong' };
    }
    try {
        const cleanData = { ...data };
        delete cleanData._isDefaultFallback;
        const kvResp = await fetchWithTimeout(`${kv.url}/set/${encodeURIComponent(key)}`, {
            method: 'POST',
            headers: {
                Authorization: `Bearer ${kv.writeToken}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(cleanData)
        }, 3500);
        let kvJson = null;
        try {
            kvJson = await kvResp.json();
        } catch (e) {}
        if (!kvResp.ok || (kvJson && kvJson.error)) {
            const errMsg = (kvJson && kvJson.error) ? String(kvJson.error) : `KV HTTP ${kvResp.status}`;
            return { backend: 'kv', ok: false, error: errMsg };
        }
        return { backend: 'kv', ok: true, error: '' };
    } catch (e) {
        return { backend: 'kv', ok: false, error: e.message || 'KV write error' };
    }
}

async function readData(forceCloud = false) {
    const now = Date.now();

    if (isLocalVmMode()) {
        if (!memoryStore || !localDbLoaded) {
            loadLocalDbSync();
        }
        purgeDeletedStudents(memoryStore, Math.floor(now / 1000));
        lastCloudStatus = { backend: 'local_vm', ok: true, error: '' };
        return memoryStore;
    }

    // 1. Jika Upstash Redis / Vercel KV dikonfigurasi
    const kvRes = await readFromKV();
    if (kvRes.ok && kvRes.data) {
        cloudHydrated = true;
        lastCloudStatus = { backend: 'kv', ok: true, error: '' };
        memoryStore = kvRes.data;
        delete memoryStore._isDefaultFallback;
        purgeDeletedStudents(memoryStore, Math.floor(now / 1000));
        try {
            fs.writeFileSync(TMP_DB_PATH, JSON.stringify(memoryStore, null, 2), 'utf8');
        } catch (e) {}
        return memoryStore;
    }

    // 2. Baca memory / /tmp terlebih dahulu
    if (!memoryStore) {
        try {
            if (fs.existsSync(TMP_DB_PATH)) {
                memoryStore = JSON.parse(fs.readFileSync(TMP_DB_PATH, 'utf8'));
            }
        } catch (e) {}
    }

    // 3. Sinkronkan dari Firebase Cloud Store agar semua container Vercel memiliki state yang sama!
    if (forceCloud || !memoryStore || memoryStore._isDefaultFallback || (kvRes.configured && kvRes.empty) || (now - lastCloudReadMs) >= 1000) {
        lastCloudReadMs = now;
        const fbData = await readFromFirebase();
        if (fbData && fbData.config) {
            cloudHydrated = true;
            lastCloudStatus = { backend: 'firebase', ok: true, error: '' };
            const nowSec = Math.floor(now / 1000);
            if (!memoryStore || memoryStore._isDefaultFallback) {
                memoryStore = fbData;
                delete memoryStore._isDefaultFallback;
            } else {
                const localDeleted = memoryStore.config?.deleted_students || {};
                const fbDeleted = fbData.config.deleted_students || {};
                const mergedDeleted = { ...localDeleted };
                for (const [id, ts] of Object.entries(fbDeleted)) {
                    const numTs = Number(ts || 0);
                    if (numTs > Number(mergedDeleted[id] || 0)) {
                        mergedDeleted[id] = numTs;
                    }
                }

                // Gabungkan konfigurasi terbaru & daftar mahasiswa aktif dari seluruh container
                const fbTime = Date.parse(fbData.config.updated_at || 0) || 0;
                const localTime = Date.parse(memoryStore.config?.updated_at || 0) || 0;
                if (fbTime >= localTime) {
                    memoryStore.config = fbData.config;
                    if (fbData.blocklist) {
                        memoryStore.blocklist = fbData.blocklist;
                    }
                } else if (Array.isArray(fbData.config.rooms) && fbData.config.rooms.length > 0 && (!Array.isArray(memoryStore.config?.rooms) || memoryStore.config.rooms.length === 0)) {
                    const delRooms = new Set(Array.isArray(memoryStore.config?.deleted_rooms) ? memoryStore.config.deleted_rooms : []);
                    const recoveredRooms = fbData.config.rooms.filter(r => !delRooms.has(r));
                    if (recoveredRooms.length > 0) {
                        memoryStore.config.rooms = recoveredRooms;
                        memoryStore.config.room_statuses = { ...(fbData.config.room_statuses || {}), ...(memoryStore.config.room_statuses || {}) };
                        memoryStore.config.room_pins = { ...(fbData.config.room_pins || {}), ...(memoryStore.config.room_pins || {}) };
                    }
                }

                const mergedStudents = { ...(fbData.students || {}) };
                for (const [id, localSt] of Object.entries(memoryStore.students || {})) {
                    const cloudSt = mergedStudents[id];
                    if (!cloudSt) {
                        mergedStudents[id] = localSt;
                    } else {
                        const newerTelemetry = (localSt.last_seen || 0) >= (cloudSt.last_seen || 0) ? localSt : cloudSt;
                        const localCmdTs = localSt.cmd_updated_at || 0;
                        const cloudCmdTs = cloudSt.cmd_updated_at || 0;
                        const newerCmd = cloudCmdTs >= localCmdTs ? cloudSt : localSt;
                        const resolvedTarget = newerCmd.target_status || newerTelemetry.target_status || 'LOCKED';
                        const isRecentCmd = (now - Math.max(localCmdTs, cloudCmdTs)) < 9000;
                        mergedStudents[id] = {
                            ...newerTelemetry,
                            target_status: resolvedTarget,
                            status: isRecentCmd ? resolvedTarget : newerTelemetry.status,
                            cmd_updated_at: Math.max(localCmdTs, cloudCmdTs)
                        };
                    }
                }
                for (const [id, st] of Object.entries(mergedStudents)) {
                    const delSec = Number(mergedDeleted[id] || 0);
                    if (delSec > 0 && Number(st?.last_seen || 0) > delSec) {
                        delete mergedDeleted[id];
                    }
                }
                memoryStore.config.deleted_students = pruneDeletedStudentsMap(mergedDeleted, nowSec);
                memoryStore.students = mergedStudents;
                if ((!memoryStore.logs || !memoryStore.logs.length) && Array.isArray(fbData.logs)) {
                    memoryStore.logs = fbData.logs;
                }
            }
            purgeDeletedStudents(memoryStore, nowSec);
            try {
                fs.writeFileSync(TMP_DB_PATH, JSON.stringify(memoryStore, null, 2), 'utf8');
            } catch (e) {}
            if (kvRes.configured && kvRes.empty && getKvConfig().canWrite) {
                const seedRes = await writeToKV(memoryStore);
                if (seedRes.ok) {
                    lastCloudStatus = seedRes;
                }
            }
        }
    }

    if (!memoryStore) {
        memoryStore = getDefaultSchema();
        memoryStore._isDefaultFallback = true;
    }
    return memoryStore;
}

async function writeData(data, options = {}) {
    const allowFallbackWrite = Boolean(options && options.allowFallbackWrite);
    if (allowFallbackWrite) {
        delete data._isDefaultFallback;
        cloudHydrated = true;
    }
    memoryStore = data;
    lastCloudReadMs = Date.now();

    if (isLocalVmMode()) {
        delete data._isDefaultFallback;
        cloudHydrated = true;
        const immediate = Boolean(options && (options.allowFallbackWrite || options.immediate));
        return scheduleLocalDbFlush(immediate);
    }

    try {
        fs.writeFileSync(TMP_DB_PATH, JSON.stringify(data, null, 2), 'utf8');
    } catch (e) {}

    // Lindungi state cloud agar fallback kosong akibat cold-start timeout tidak pernah menimpa Firebase
    if (data._isDefaultFallback && !allowFallbackWrite) {
        return lastCloudStatus;
    }

    const kv = getKvConfig();
    if (kv.canWrite) {
        const kvWriteRes = await writeToKV(data);
        if (kvWriteRes.ok) {
            lastCloudStatus = kvWriteRes;
            return lastCloudStatus;
        }
        const fbFallbackRes = await writeToFirebase(data);
        if (fbFallbackRes.ok) {
            return fbFallbackRes;
        }
        lastCloudStatus = kvWriteRes;
        return lastCloudStatus;
    }

    return await writeToFirebase(data);
}

function nowFormatted() {
    const d = new Date();
    return d.toISOString().replace('T', ' ').substring(0, 19);
}

function ensureRoomsConfig(data) {
    // PENTING: TIDAK ADA ruangan default bawaan! Ruangan ditambah mandiri oleh Admin.
    if (!Array.isArray(data.config.rooms)) {
        data.config.rooms = [];
    }
    if (!Array.isArray(data.config.deleted_rooms)) {
        data.config.deleted_rooms = [];
    }
    if (!data.config.deleted_students || typeof data.config.deleted_students !== 'object' || Array.isArray(data.config.deleted_students)) {
        data.config.deleted_students = {};
    }
    if (!data.config.room_statuses || typeof data.config.room_statuses !== 'object') {
        data.config.room_statuses = {};
    }
    if (!data.config.room_pins || typeof data.config.room_pins !== 'object') {
        data.config.room_pins = {};
    }
    if (!data.config.room_blocklists || typeof data.config.room_blocklists !== 'object') {
        data.config.room_blocklists = {};
    }
    const globLocked = (data.config.global_status || 'LOCKED') === 'LOCKED';
    if (typeof data.config.lock_ai !== 'boolean') {
        data.config.lock_ai = globLocked;
    }
    if (typeof data.config.lock_extensions !== 'boolean') {
        data.config.lock_extensions = globLocked;
    }
    if (typeof data.config.admin_email !== 'string') {
        data.config.admin_email = '';
    }
    if (typeof data.config.admin_password_hash !== 'string') {
        data.config.admin_password_hash = '';
    }
    if (typeof data.config.ide_password_hash !== 'string') {
        data.config.ide_password_hash = '';
    }
    if (!data.config.pc_overrides || typeof data.config.pc_overrides !== 'object' || Array.isArray(data.config.pc_overrides)) {
        data.config.pc_overrides = {};
    }
    if (!Array.isArray(data.config.ide_hosts)) {
        data.config.ide_hosts = [];
    }
    // Pastikan kredensial plaintext tidak pernah tersimpan di state JSON
    delete data.config.admin_password;
    delete data.config.unlock_pin;
    data.config.rooms.forEach((r) => {
        if (!data.config.room_statuses[r]) {
            data.config.room_statuses[r] = data.config.global_status || 'LOCKED';
        }
        if (!data.config.room_pins[r]) {
            data.config.room_pins[r] = randomPin();
        }
    });
    if (data.config.rooms.length > 0 && (!data.config.room_code || !data.config.rooms.includes(data.config.room_code))) {
        data.config.room_code = data.config.rooms[0];
    } else if (data.config.rooms.length === 0) {
        data.config.room_code = '';
    }

    // PENTING: Jangan paksa merge daftar domain default apabila Admin sudah mengatur/menonaktifkan domain secara fleksibel!
    if (!data.blocklist || typeof data.blocklist !== 'object') {
        data.blocklist = getDefaultSchema().blocklist;
    } else if (!data.config.blocklist_customized) {
        const defaults = getDefaultSchema().blocklist;
        data.blocklist.domains = Array.from(new Set([...(data.blocklist.domains || []), ...defaults.domains]));
        data.blocklist.processes = Array.from(new Set([...(data.blocklist.processes || []), ...defaults.processes]));
        data.blocklist.ide_extensions = Array.from(new Set([...(data.blocklist.ide_extensions || []), ...defaults.ide_extensions]));
    }
}

function getEffectiveBlocklist(data, roomCode) {
    const norm = String(roomCode || '').trim().toUpperCase();
    const globalBl = data.blocklist || getDefaultSchema().blocklist;
    if (norm && norm !== 'ALL' && data.config?.room_blocklists && data.config.room_blocklists[norm]) {
        const rb = data.config.room_blocklists[norm];
        return {
            domains: Array.isArray(rb.domains) ? rb.domains : (globalBl.domains || []),
            processes: Array.isArray(rb.processes) ? rb.processes : (globalBl.processes || []),
            ide_extensions: Array.isArray(rb.ide_extensions) ? rb.ide_extensions : (globalBl.ide_extensions || []),
            local_ai_ports: globalBl.local_ai_ports || [11434, 1234, 1337, 8080, 5001]
        };
    }
    return globalBl;
}

function registerRoomByAdmin(data, roomCode, customPin = '') {
    ensureRoomsConfig(data);
    const norm = String(roomCode || '').trim().toUpperCase().replace(/[^A-Z0-9_-]/g, '-');
    if (!norm) return '';
    data.config.deleted_rooms = (data.config.deleted_rooms || []).filter(r => r !== norm);
    if (!data.config.rooms.includes(norm)) {
        data.config.rooms.push(norm);
    }
    if (!data.config.room_statuses[norm]) {
        data.config.room_statuses[norm] = data.config.global_status || 'LOCKED';
    }
    if (customPin && String(customPin).trim()) {
        data.config.room_pins[norm] = String(customPin).trim();
    } else if (!data.config.room_pins[norm]) {
        data.config.room_pins[norm] = randomPin();
    }
    return norm;
}

function addLog(data, nim, name, hostname, type, message, severity = 'INFO', roomCode = '') {
    if (!Array.isArray(data.logs)) data.logs = [];
    data.logs.unshift({
        id: 'log_' + crypto.randomBytes(4).toString('hex'),
        timestamp: nowFormatted(),
        nim,
        name,
        hostname,
        room_code: roomCode || '',
        type,
        message,
        severity
    });
    if (data.logs.length > 300) {
        data.logs = data.logs.slice(0, 300);
    }
}

function findMatchingStudentKey(data, studentId, upperHost, clientId, clientIp) {
    const students = data.students || {};
    if (studentId && students[studentId]) {
        return studentId;
    }
    if (upperHost) {
        for (const [id, st] of Object.entries(students)) {
            if (st && String(st.hostname || '').trim().toUpperCase() === upperHost) {
                return id;
            }
        }
    }
    if (clientId) {
        for (const [id, st] of Object.entries(students)) {
            if (st && String(st.client_id || '').trim() === clientId) {
                return id;
            }
        }
    }
    if (clientIp && clientIp !== '127.0.0.1' && clientIp !== '::1') {
        const ipMatches = [];
        for (const [id, st] of Object.entries(students)) {
            if (st && String(st.ip || '').trim() === clientIp) {
                ipMatches.push(id);
            }
        }
        if (ipMatches.length === 1) {
            return ipMatches[0];
        }
    }
    return null;
}

function resolveDeviceIdeLocks(data, params, clientIp) {
    const globLocked = (data.config.global_status || 'LOCKED') === 'LOCKED';
    const globalAi = typeof data.config.lock_ai === 'boolean' ? data.config.lock_ai : globLocked;
    const globalExt = typeof data.config.lock_extensions === 'boolean' ? data.config.lock_extensions : globLocked;

    const studentId = String(params.student_id || '').trim();
    const upperHost = String(params.hostname || '').trim().toUpperCase();
    const clientId = String(params.client_id || '').trim();

    const matchedKey = findMatchingStudentKey(data, studentId, upperHost, clientId, clientIp);
    const matchedStudent = matchedKey ? data.students[matchedKey] : null;
    const resolvedHost = upperHost || String(matchedStudent?.hostname || '').trim().toUpperCase();

    const pco = data.config.pc_overrides || {};
    let override = null;
    for (const cand of [matchedKey, studentId, resolvedHost, clientId]) {
        if (cand && pco[cand] && typeof pco[cand] === 'object') {
            override = pco[cand];
            break;
        }
    }

    let lockAi = globalAi;
    if (matchedStudent && typeof matchedStudent.lock_ai === 'boolean') {
        lockAi = matchedStudent.lock_ai;
    } else if (override && typeof override.lock_ai === 'boolean') {
        lockAi = override.lock_ai;
    }

    let lockExt = globalExt;
    if (matchedStudent && typeof matchedStudent.lock_extensions === 'boolean') {
        lockExt = matchedStudent.lock_extensions;
    } else if (override && typeof override.lock_extensions === 'boolean') {
        lockExt = override.lock_extensions;
    }

    const hostKnown = Boolean(upperHost && (data.config.ide_hosts || []).some((ih) => ih && String(ih.h || '').trim().toUpperCase() === upperHost));

    return {
        lock_ai: lockAi,
        lock_extensions: lockExt,
        global_lock_ai: globalAi,
        global_lock_extensions: globalExt,
        device_known: Boolean(matchedStudent || hostKnown),
        matched_student_id: matchedKey || null,
        matched_hostname: resolvedHost
    };
}

function sanitizeConfigForDashboard(cfg) {
    const out = { ...(cfg || {}) };
    out.has_custom_admin_password = Boolean(out.admin_password_hash && String(out.admin_password_hash).trim());
    out.admin_email_effective = (out.admin_email && String(out.admin_email).trim()) ? String(out.admin_email).trim() : ADMIN_EMAIL;
    delete out.admin_password_hash;
    delete out.admin_password;
    return out;
}

module.exports = async (req, res) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Admin-Token, X-Client-Token');
    res.setHeader('Cache-Control', 'no-store');

    if (req.method === 'OPTIONS') {
        return res.status(200).end();
    }

    const body = typeof req.body === 'string' ? (() => { try { return JSON.parse(req.body); } catch { return {}; } })() : (req.body || {});
    const queryParams = { ...(req.query || {}) };
    if (req.url && req.url.includes('?')) {
        try {
            const u = new URL(req.url, 'http://localhost');
            for (const [k, v] of u.searchParams.entries()) {
                if (queryParams[k] === undefined) {
                    queryParams[k] = v;
                }
            }
        } catch (e) {}
    }
    const action = queryParams.action || body.action || '';
    const clientIp = ((req.headers && req.headers['x-forwarded-for']) || req.socket?.remoteAddress || '127.0.0.1').split(',')[0].trim();

    // ---------------------------------------------------------------
    // LOGIN ADMIN — mendukung kustomisasi panel admin + fallback ENV
    // ---------------------------------------------------------------
    if (action === 'admin_login') {
        if (!ADMIN_EMAIL || !ADMIN_PASSWORD) {
            return res.status(503).json({
                ok: false,
                error: 'Login Admin belum dikonfigurasi. Atur ADMIN_EMAIL dan ADMIN_PASSWORD di Environment Variables server.'
            });
        }
        const email = String(body.email || '').trim().toLowerCase();
        const password = String(body.password || '');
        const authData = await readData(true);
        ensureRoomsConfig(authData);

        const effectiveEmail = (authData.config.admin_email && String(authData.config.admin_email).trim())
            ? String(authData.config.admin_email).trim().toLowerCase()
            : ADMIN_EMAIL;
        const customPassHash = String(authData.config.admin_password_hash || '').trim();
        const effectivePassHash = customPassHash || crypto.createHash('sha256').update(ADMIN_PASSWORD).digest('hex');
        const inputPassHash = crypto.createHash('sha256').update(password).digest('hex');

        if (email && password && safeEqual(email, effectiveEmail) && safeEqual(inputPassHash, effectivePassHash)) {
            return res.status(200).json({
                ok: true,
                token: signAdminToken(effectiveEmail),
                expires_in: Math.floor(ADMIN_TOKEN_TTL_MS / 1000),
                user: { email: effectiveEmail, name: 'Dosen / Pengawas Ujian', role: 'ADMIN' }
            });
        }
        await sleep(600); // perlambat percobaan tebak password
        return res.status(401).json({ ok: false, error: 'Email atau Kata Sandi tidak sesuai.' });
    }

    // Semua aksi Dashboard Admin wajib membawa token login yang valid
    if (ADMIN_ACTIONS.has(action) && !verifyAdminToken(getRequestToken(req))) {
        return res.status(401).json({ ok: false, auth_required: true, error: 'Sesi Admin tidak valid atau telah berakhir. Silakan login kembali.' });
    }

    try {
        const isMutating = action !== 'dashboard_state' && action !== 'state' && action !== 'get_rooms' && action !== 'blocklist';
        const forceCloudParam = queryParams.force_cloud === '1' || queryParams.force_cloud === 'true';
        const data = await readData(isMutating || forceCloudParam);
        if (!data.students || Array.isArray(data.students)) data.students = {};
        ensureRoomsConfig(data);
        purgeDeletedStudents(data);

        switch (action) {

            case 'rehydrate_state':
            case 'sync_from_firebase': {
                // Hanya rehydrate peserta aktif yang berada di ruangan resmi yang sudah dibuat Admin
                if (Array.isArray(body.students)) {
                    const delMap = data.config.deleted_students || {};
                    for (const st of body.students) {
                        const stRoom = String(st?.room_code || '').trim().toUpperCase();
                        if (st && st.id && stRoom && data.config.rooms.includes(stRoom) && !data.students[st.id]) {
                            const delSec = Number(delMap[st.id] || 0);
                            if (!delSec || Number(st.last_seen || 0) > delSec) {
                                data.students[st.id] = st;
                            }
                        }
                    }
                }
                ensureRoomsConfig(data);
                purgeDeletedStudents(data);
                await writeData(data);
                return res.status(200).json({ ok: true });
            }

            case 'get_rooms': {
                return res.status(200).json({
                    ok: true,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    default_room: data.config.room_code || data.config.rooms[0] || ''
                });
            }

            case 'ide_register': {
                const rawHost = String(body.hostname ?? queryParams.hostname ?? '').trim();
                const upperHost = rawHost.toUpperCase();
                const clientId = String(body.client_id ?? queryParams.client_id ?? '').trim();
                const username = String(body.username ?? queryParams.username ?? 'VS Code User').trim();
                if (!upperHost) {
                    return res.status(400).json({ ok: false, error: 'Hostname diperlukan.' });
                }
                const nowSec = Math.floor(Date.now() / 1000);
                const matchedKey = findMatchingStudentKey(data, '', upperHost, clientId, '');
                if (matchedKey) {
                    let effectiveNow = nowSec;
                    if (data.config.deleted_students && data.config.deleted_students[matchedKey]) {
                        effectiveNow = Math.max(nowSec, Number(data.config.deleted_students[matchedKey] || 0) + 1);
                        delete data.config.deleted_students[matchedKey];
                        data.config.updated_at = new Date().toISOString();
                    }
                    data.students[matchedKey].ide_connected = true;
                    data.students[matchedKey].ide_last_seen = effectiveNow;
                    data.students[matchedKey].last_seen = effectiveNow;
                    data.students[matchedKey].last_seen_str = nowFormatted();
                    if (clientId) {
                        data.students[matchedKey].client_id = clientId;
                    }
                    await writeData(data);
                    return res.status(200).json({ ok: true, student_id: matchedKey, mode: 'existing_student' });
                }

                if (data.config.rooms.length > 0) {
                    const targetRoom = data.config.room_code || data.config.rooms[0];
                    const safeId = 'IDE_' + upperHost.replace(/[^A-Z0-9_-]/g, '_');
                    let effectiveNow = nowSec;
                    if (data.config.deleted_students && data.config.deleted_students[safeId]) {
                        effectiveNow = Math.max(nowSec, Number(data.config.deleted_students[safeId] || 0) + 1);
                        delete data.config.deleted_students[safeId];
                        data.config.updated_at = new Date().toISOString();
                    }
                    const roomStatus = data.config.room_statuses[targetRoom] || data.config.global_status || 'LOCKED';
                    const hostOverride = (data.config.pc_overrides && typeof data.config.pc_overrides[upperHost] === 'object')
                        ? data.config.pc_overrides[upperHost]
                        : null;
                    const existing = data.students[safeId] || {};
                    data.students[safeId] = {
                        id: safeId,
                        nim: 'VS CODE',
                        name: username || upperHost,
                        hostname: upperHost,
                        client_id: clientId,
                        room_code: targetRoom,
                        ip: clientIp,
                        os_info: 'VS Code IDE',
                        version: 'IDE-Sync',
                        status: existing.status || roomStatus,
                        target_status: existing.target_status || roomStatus,
                        lock_ai: existing.lock_ai !== undefined ? existing.lock_ai : (hostOverride?.lock_ai ?? null),
                        lock_extensions: existing.lock_extensions !== undefined ? existing.lock_extensions : (hostOverride?.lock_extensions ?? null),
                        quarantined_count: existing.quarantined_count || 0,
                        quarantined_list: existing.quarantined_list || [],
                        killed_processes_count: existing.killed_processes_count || 0,
                        vpn_detected: false,
                        joined_at: existing.joined_at || nowFormatted(),
                        ide_connected: true,
                        ide_last_seen: effectiveNow,
                        last_seen: effectiveNow,
                        last_seen_str: nowFormatted()
                    };
                    await writeData(data);
                    return res.status(200).json({ ok: true, student_id: safeId, mode: 'ide_student' });
                }

                const hosts = [];
                let found = false;
                for (const ih of (data.config.ide_hosts || [])) {
                    if (ih && ih.h) {
                        if (String(ih.h).toUpperCase() === upperHost) {
                            hosts.push({ h: upperHost, id: clientId, ip: clientIp, ls: nowSec });
                            found = true;
                        } else {
                            hosts.push(ih);
                        }
                    }
                }
                if (!found) {
                    hosts.unshift({ h: upperHost, id: clientId, ip: clientIp, ls: nowSec });
                }
                data.config.ide_hosts = hosts.slice(0, 20);
                await writeData(data);
                return res.status(200).json({ ok: true, hostname: upperHost, mode: 'standalone_host' });
            }

            case 'blocklist': {
                const resolved = resolveDeviceIdeLocks(data, {
                    hostname: queryParams.hostname ?? body.hostname ?? '',
                    student_id: queryParams.student_id ?? body.student_id ?? '',
                    client_id: queryParams.client_id ?? body.client_id ?? ''
                }, clientIp);
                return res.status(200).json({
                    ok: true,
                    device_known: resolved.device_known,
                    matched_student_id: resolved.matched_student_id,
                    matched_hostname: resolved.matched_hostname,
                    config: {
                        room_code: data.config.room_code || '',
                        rooms: data.config.rooms || [],
                        global_status: data.config.global_status || 'LOCKED',
                        lock_ai: resolved.lock_ai,
                        lock_extensions: resolved.lock_extensions,
                        global_lock_ai: resolved.global_lock_ai,
                        global_lock_extensions: resolved.global_lock_extensions,
                        ide_password_hash: data.config.ide_password_hash || '',
                        quarantine_extensions: data.config.quarantine_extensions !== undefined ? Boolean(data.config.quarantine_extensions) : true,
                        kill_processes: data.config.kill_processes !== undefined ? Boolean(data.config.kill_processes) : true
                    },
                    blocklist: data.blocklist
                });
            }

            case 'join':
            case 'handshake': {
                const nim = String(body.nim || '').trim();
                const name = String(body.name || '').trim();
                const hostname = String(body.hostname || 'PC-CLIENT').trim();
                const rawRoom = String(body.room_code || '').trim().toUpperCase();
                const osInfo = String(body.os_info || 'Windows').trim();
                const version = String(body.version || '1.5.0').trim();

                if (!nim || !name) {
                    return res.status(400).json({ ok: false, error: 'NIM dan Nama Mahasiswa wajib diisi.', rooms: data.config.rooms });
                }

                if (!rawRoom || !data.config.rooms.includes(rawRoom)) {
                    return res.status(400).json({
                        ok: false,
                        error: data.config.rooms.length === 0
                            ? 'Belum ada Ruangan yang dibuat oleh Admin. Silakan tambahkan Ruangan terlebih dahulu di Dashboard Admin.'
                            : `Kode Ruangan [${rawRoom || '-'}] tidak ditemukan. Silakan pilih Ruangan yang tersedia terlebih dahulu.`,
                        rooms: data.config.rooms
                    });
                }

                const normalizedRoom = rawRoom;
                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const existing = data.students[studentId] || null;
                const roomDefaultState = data.config.room_statuses[normalizedRoom] || data.config.global_status || 'LOCKED';
                const desiredState = (existing && existing.room_code === normalizedRoom && existing.target_status) ? existing.target_status : roomDefaultState;
                const roomPin = data.config.room_pins[normalizedRoom] || '';
                const effectiveBl = getEffectiveBlocklist(data, normalizedRoom);
                const nowSec = Math.floor(Date.now() / 1000);
                let effectiveNow = nowSec;
                if (data.config.deleted_students && data.config.deleted_students[studentId]) {
                    effectiveNow = Math.max(nowSec, Number(data.config.deleted_students[studentId] || 0) + 1);
                    delete data.config.deleted_students[studentId];
                    data.config.updated_at = new Date().toISOString();
                }
                const upperHost = hostname.toUpperCase();
                const hostOverride = (upperHost && data.config.pc_overrides && typeof data.config.pc_overrides[upperHost] === 'object')
                    ? data.config.pc_overrides[upperHost]
                    : null;

                data.students[studentId] = {
                    id: studentId,
                    nim,
                    name,
                    hostname,
                    room_code: normalizedRoom,
                    ip: (body.ip && String(body.ip).trim()) || clientIp,
                    os_info: osInfo,
                    version,
                    status: desiredState,
                    target_status: desiredState,
                    lock_ai: existing?.lock_ai !== undefined ? existing.lock_ai : (hostOverride?.lock_ai ?? null),
                    lock_extensions: existing?.lock_extensions !== undefined ? existing.lock_extensions : (hostOverride?.lock_extensions ?? null),
                    cmd_updated_at: existing?.cmd_updated_at || Date.now(),
                    quarantined_count: existing?.quarantined_count || 0,
                    quarantined_list: existing?.quarantined_list || [],
                    blocked_domains_count: (effectiveBl.domains || []).length,
                    killed_processes_count: existing?.killed_processes_count || 0,
                    vpn_detected: false,
                    pin_fail: existing?.pin_fail || 0,
                    pin_lock_until: existing?.pin_lock_until || 0,
                    joined_at: existing?.joined_at || nowFormatted(),
                    last_seen: effectiveNow,
                    last_seen_str: nowFormatted()
                };

                addLog(data, nim, name, hostname, 'JOIN', `PC Mahasiswa terhubung ke ruangan [${normalizedRoom}] (${osInfo}). Status: ${desiredState}`, 'INFO', normalizedRoom);
                await writeData(data);

                return res.status(200).json({
                    ok: true,
                    student_id: studentId,
                    room_code: normalizedRoom,
                    room_pin_hash: roomPin ? roomPinHash(normalizedRoom, roomPin) : '',
                    target_status: desiredState,
                    rooms: data.config.rooms,
                    config: {
                        room_code: normalizedRoom,
                        room_pin_hash: roomPin ? roomPinHash(normalizedRoom, roomPin) : '',
                        target_status: desiredState,
                        block_doh: data.config.block_doh,
                        block_vpn: data.config.block_vpn,
                        quarantine_extensions: data.config.quarantine_extensions,
                        kill_processes: data.config.kill_processes
                    },
                    blocklist: effectiveBl
                });
            }

            case 'heartbeat': {
                const nim = String(body.nim || '').trim();
                const name = String(body.name || 'Mahasiswa').trim();
                const hostname = String(body.hostname || 'PC').trim();
                const currentStatus = String(body.status || 'LOCKED').trim() === 'UNLOCKED' ? 'UNLOCKED' : 'LOCKED';
                const quarantinedCount = parseInt(body.quarantined_count || 0, 10);
                const quarantinedList = Array.isArray(body.quarantined_list) ? body.quarantined_list : [];
                const killedCount = parseInt(body.killed_processes_count || 0, 10);
                const vpnDetected = Boolean(body.vpn_detected);

                if (!nim) {
                    return res.status(400).json({ ok: false, error: 'NIM diperlukan untuk heartbeat.' });
                }

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const globalTarget = data.config.global_status || 'LOCKED';
                const nowSec = Math.floor(Date.now() / 1000);
                let effectiveNow = nowSec;
                if (data.config.deleted_students && data.config.deleted_students[studentId]) {
                    effectiveNow = Math.max(nowSec, Number(data.config.deleted_students[studentId] || 0) + 1);
                    delete data.config.deleted_students[studentId];
                    data.config.updated_at = new Date().toISOString();
                }
                const reqRoom = String(body.room_code || '').trim().toUpperCase();

                // Ruangan harus terdaftar secara resmi di data.config.rooms yang dibuat mandiri oleh Admin
                if (!reqRoom || !data.config.rooms.includes(reqRoom)) {
                    if (data.students[studentId]) {
                        delete data.students[studentId];
                        await writeData(data);
                    }
                    return res.status(200).json({
                        ok: false,
                        require_room_selection: true,
                        rooms: data.config.rooms,
                        target_status: 'UNLOCKED',
                        error: 'Ruangan belum terdaftar atau telah dihapus oleh Admin. Silakan pilih ruangan aktif.'
                    });
                }

                const upperHost = hostname.toUpperCase();
                const hostOverride = (upperHost && data.config.pc_overrides && typeof data.config.pc_overrides[upperHost] === 'object')
                    ? data.config.pc_overrides[upperHost]
                    : null;

                if (!data.students[studentId]) {
                    data.students[studentId] = {
                        id: studentId,
                        nim,
                        name,
                        hostname,
                        room_code: reqRoom,
                        ip: (body.ip && String(body.ip).trim()) || clientIp,
                        os_info: 'Windows',
                        version: '1.5.0',
                        target_status: data.config.room_statuses[reqRoom] || globalTarget,
                        lock_ai: hostOverride?.lock_ai ?? null,
                        lock_extensions: hostOverride?.lock_extensions ?? null,
                        cmd_updated_at: Date.now(),
                        joined_at: nowFormatted()
                    };
                }

                const st = data.students[studentId];
                if (name) st.name = name;
                st.room_code = reqRoom;
                const effectiveBl = getEffectiveBlocklist(data, st.room_code);
                const resolvedTarget = st.target_status || data.config.room_statuses[st.room_code] || globalTarget;
                const isRecentCmd = (Date.now() - (st.cmd_updated_at || 0)) < 9000;

                const prevVpn = Boolean(st.vpn_detected);
                st.target_status = resolvedTarget;
                st.status = isRecentCmd ? resolvedTarget : currentStatus;
                st.hostname = hostname;
                st.ip = (body.ip && String(body.ip).trim()) || st.ip || clientIp;
                st.quarantined_count = quarantinedCount;
                st.quarantined_list = quarantinedList;
                st.blocked_domains_count = (effectiveBl.domains || []).length;
                st.killed_processes_count = killedCount;
                st.vpn_detected = vpnDetected;
                st.last_seen = effectiveNow;
                st.last_seen_str = nowFormatted();

                if (vpnDetected && !prevVpn) {
                    addLog(data, nim, name, hostname, 'VPN_ALERT', `Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa di ruangan [${st.room_code}]!`, 'DANGER', st.room_code);
                }

                await writeData(data);
                const roomPin = data.config.room_pins[st.room_code] || '';
                const effLockAi = typeof st.lock_ai === 'boolean'
                    ? st.lock_ai
                    : ((hostOverride && typeof hostOverride.lock_ai === 'boolean')
                        ? hostOverride.lock_ai
                        : (typeof data.config.lock_ai === 'boolean' ? data.config.lock_ai : (data.config.kill_processes !== undefined ? Boolean(data.config.kill_processes) : true)));
                const effLockExt = typeof st.lock_extensions === 'boolean'
                    ? st.lock_extensions
                    : ((hostOverride && typeof hostOverride.lock_extensions === 'boolean')
                        ? hostOverride.lock_extensions
                        : (typeof data.config.lock_extensions === 'boolean' ? data.config.lock_extensions : (data.config.quarantine_extensions !== undefined ? Boolean(data.config.quarantine_extensions) : true)));

                const resPayload = {
                    ok: true,
                    room_code: st.room_code,
                    room_pin_hash: roomPin ? roomPinHash(st.room_code, roomPin) : '',
                    target_status: resolvedTarget,
                    rooms: data.config.rooms,
                    config: {
                        room_code: st.room_code,
                        room_pin_hash: roomPin ? roomPinHash(st.room_code, roomPin) : '',
                        block_doh: data.config.block_doh,
                        block_vpn: data.config.block_vpn,
                        quarantine_extensions: effLockExt,
                        kill_processes: effLockAi,
                        lock_ai: effLockAi,
                        lock_extensions: effLockExt
                    },
                    blocklist: effectiveBl
                };
                if (st.pending_command) {
                    resPayload.command = st.pending_command;
                    delete st.pending_command;
                }
                return res.status(200).json(resPayload);
            }

            case 'violation': {
                const nim = String(body.nim || 'UNKNOWN').trim();
                const name = String(body.name || 'Mahasiswa').trim();
                const hostname = String(body.hostname || 'PC').trim();
                const type = String(body.type || 'AI_DETECTED').trim();
                const message = String(body.message || 'Percobaan akses AI terdeteksi dan diblokir.').trim();
                const severity = String(body.severity || 'DANGER').trim();

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                let stRoom = data.config.room_code || '';
                if (data.students[studentId]) {
                    data.students[studentId].last_seen = Math.floor(Date.now() / 1000);
                    data.students[studentId].last_seen_str = nowFormatted();
                    data.students[studentId].killed_processes_count = (data.students[studentId].killed_processes_count || 0) + 1;
                    stRoom = data.students[studentId].room_code || stRoom;
                }
                addLog(data, nim, name, hostname, type, message, severity, stRoom);
                await writeData(data);
                return res.status(200).json({ ok: true });
            }

            case 'verify_pin': {
                const pin = String(body.pin || '').trim();
                const nim = String(body.nim || '').trim();
                const name = String(body.name || 'Mahasiswa').trim();
                const hostname = String(body.hostname || 'PC').trim();

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const stRoom = String(body.room_code || data.students[studentId]?.room_code || data.config.room_code || '').trim().toUpperCase();
                const roomPin = String(data.config.room_pins?.[stRoom] || '');
                const stRec = data.students[studentId];

                // Anti brute-force: 5x salah → dikunci 2 menit per PC
                if (stRec && (stRec.pin_lock_until || 0) > Date.now()) {
                    const waitSec = Math.ceil((stRec.pin_lock_until - Date.now()) / 1000);
                    return res.status(429).json({ ok: false, unlocked: false, error: `Terlalu banyak percobaan PIN salah. Coba lagi dalam ${waitSec} detik.` });
                }

                const anyRoomPinOk = nim === 'VSCODE_USER' && !body.room_code && Object.values(data.config.room_pins || {}).some(rp => rp && safeEqual(pin, String(rp)));
                const pinSha256 = pin ? crypto.createHash('sha256').update(pin).digest('hex') : '';
                const idePassOk = nim === 'VSCODE_USER' && data.config.ide_password_hash && safeEqual(pinSha256, data.config.ide_password_hash);
                const pinOk = pin && ((roomPin && safeEqual(pin, roomPin)) || anyRoomPinOk || idePassOk || (MASTER_PIN && safeEqual(pin, MASTER_PIN)));
                if (pinOk) {
                    if (stRec) { stRec.pin_fail = 0; stRec.pin_lock_until = 0; }
                    const cmdTs = Date.now();
                    data.config.updated_at = new Date(cmdTs).toISOString();
                    if (data.students[studentId]) {
                        data.students[studentId].target_status = 'UNLOCKED';
                        data.students[studentId].status = 'UNLOCKED';
                        data.students[studentId].cmd_updated_at = cmdTs;
                    }
                    addLog(data, nim, name, hostname, 'PIN_UNLOCK', `Kunci dibuka / otorisasi keluar menggunakan PIN Ruangan [${stRoom}] yang valid.`, 'INFO', stRoom);
                    await writeData(data);
                    return res.status(200).json({ ok: true, unlocked: true });
                } else {
                    let lockMsg = '';
                    if (stRec) {
                        stRec.pin_fail = (stRec.pin_fail || 0) + 1;
                        if (stRec.pin_fail >= 5) {
                            stRec.pin_lock_until = Date.now() + 2 * 60 * 1000;
                            stRec.pin_fail = 0;
                            lockMsg = ' PC dikunci dari percobaan PIN selama 2 menit.';
                        }
                    }
                    addLog(data, nim, name, hostname, 'PIN_FAILED', `Percobaan PIN gagal di ruangan [${stRoom}].${lockMsg}`, 'WARNING', stRoom);
                    await writeData(data);
                    return res.status(403).json({ ok: false, unlocked: false, error: `PIN Otorisasi untuk ruangan [${stRoom}] salah!${lockMsg}` });
                }
            }

            case 'state':
            case 'dashboard_state': {
                const nowSec = Math.floor(Date.now() / 1000);
                const students = [];
                let onlineCount = 0;
                let lockedCount = 0;
                let violationCount = 0;
                let dirty = purgeDeletedStudents(data, nowSec);

                for (const id of Object.keys(data.students)) {
                    const rawSt = data.students[id];
                    const stRoom = String(rawSt.room_code || '').toUpperCase();
                    if (!stRoom || !data.config.rooms.includes(stRoom) || rawSt.is_simulated) {
                        delete data.students[id];
                        dirty = true;
                        continue;
                    }
                    const st = { ...rawSt };
                    const isOnline = (nowSec - (st.last_seen || 0)) <= 45;
                    st.is_online = isOnline;
                    st.seconds_ago = Math.max(0, nowSec - (st.last_seen || nowSec));
                    if (isOnline) onlineCount++;
                    if (st.status === 'LOCKED') lockedCount++;
                    violationCount += parseInt(st.killed_processes_count || 0, 10);
                    students.push(st);
                }

                if (dirty) {
                    await writeData(data);
                }

                students.sort((a, b) => {
                    if (a.is_online !== b.is_online) return a.is_online ? -1 : 1;
                    return String(a.nim).localeCompare(String(b.nim));
                });

                return res.status(200).json({
                    ok: true,
                    server_time: nowFormatted(),
                    cloud_storage: lastCloudStatus,
                    config: sanitizeConfigForDashboard(data.config),
                    stats: {
                        total_students: students.length,
                        online_students: onlineCount,
                        locked_students: lockedCount,
                        total_violations: violationCount,
                        domains_count: (data.blocklist.domains || []).length,
                        processes_count: (data.blocklist.processes || []).length,
                        extensions_count: (data.blocklist.ide_extensions || []).length
                    },
                    students,
                    logs: (data.logs || []).slice(0, 100),
                    blocklist: data.blocklist
                });
            }

            case 'set_global_status': {
                const target = String(body.status || 'LOCKED').trim().toUpperCase();
                const targetRoom = String(body.room_code || 'ALL').trim().toUpperCase();
                if (!['LOCKED', 'UNLOCKED'].includes(target)) {
                    return res.status(400).json({ ok: false, error: 'Status tidak valid.' });
                }

                const cmdTs = Date.now();
                data.config.updated_at = new Date(cmdTs).toISOString();
                if (targetRoom === 'ALL' || !targetRoom) {
                    const isLocked = target === 'LOCKED';
                    data.config.global_status = target;
                    data.config.lock_ai = isLocked;
                    data.config.lock_extensions = isLocked;
                    data.config.quarantine_extensions = isLocked;
                    data.config.kill_processes = isLocked;
                    data.config.pc_overrides = {};
                    for (const r of data.config.rooms) {
                        data.config.room_statuses[r] = target;
                    }
                    for (const id of Object.keys(data.students)) {
                        data.students[id].target_status = target;
                        data.students[id].status = target;
                        data.students[id].lock_ai = null;
                        data.students[id].lock_extensions = null;
                        data.students[id].cmd_updated_at = cmdTs;
                    }
                    const label = target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', `Pengawas memilih ${label} untuk SELURUH RUANGAN.`, 'INFO', 'ALL');
                } else {
                    if (!data.config.rooms.includes(targetRoom)) {
                        return res.status(404).json({ ok: false, error: 'Ruangan tidak ditemukan.' });
                    }
                    const isRoomLocked = target === 'LOCKED';
                    data.config.room_statuses[targetRoom] = target;
                    for (const id of Object.keys(data.students)) {
                        const stRoom = String(data.students[id].room_code || '').toUpperCase();
                        if (stRoom === targetRoom) {
                            data.students[id].target_status = target;
                            data.students[id].status = target;
                            data.students[id].lock_ai = isRoomLocked;
                            data.students[id].lock_extensions = isRoomLocked;
                            data.students[id].cmd_updated_at = cmdTs;
                            const hKey = String(data.students[id].hostname || '').trim().toUpperCase();
                            if (hKey) {
                                data.config.pc_overrides[hKey] = {
                                    lock_ai: isRoomLocked,
                                    lock_extensions: isRoomLocked
                                };
                            }
                        }
                    }
                    const label = target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_COMMAND', `Pengawas memilih ${label} untuk ruangan [${targetRoom}].`, 'INFO', targetRoom);
                }

                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    global_status: data.config.global_status,
                    lock_ai: Boolean(data.config.lock_ai),
                    lock_extensions: Boolean(data.config.lock_extensions),
                    room_statuses: data.config.room_statuses
                });
            }

            case 'set_ide_module_status': {
                const mod = String(body.module || 'ALL').trim().toUpperCase();
                const status = String(body.status || 'LOCKED').trim().toUpperCase();
                const targetStudentId = String(body.student_id || '').trim();
                const targetHost = String(body.hostname || '').trim().toUpperCase();

                if (!['AI', 'EXT', 'ALL', 'REMOVE'].includes(mod) || !['LOCKED', 'UNLOCKED', 'INHERIT'].includes(status)) {
                    return res.status(400).json({ ok: false, error: 'Parameter module atau status tidak valid.' });
                }

                if (!data.config.pc_overrides || typeof data.config.pc_overrides !== 'object' || Array.isArray(data.config.pc_overrides)) {
                    data.config.pc_overrides = {};
                }

                if (targetStudentId || targetHost) {
                    const matchedKey = findMatchingStudentKey(data, targetStudentId, targetHost, '', '');
                    const resolvedHost = targetHost || (matchedKey ? String(data.students[matchedKey].hostname || '').trim().toUpperCase() : '');
                    const overrideKey = resolvedHost || targetStudentId;

                    if (mod === 'REMOVE') {
                        delete data.config.pc_overrides[overrideKey];
                        if (targetStudentId) {
                            delete data.config.pc_overrides[targetStudentId];
                        }
                        if (matchedKey && data.students[matchedKey]) {
                            data.students[matchedKey].lock_ai = null;
                            data.students[matchedKey].lock_extensions = null;
                        }
                        if (resolvedHost) {
                            data.config.ide_hosts = (data.config.ide_hosts || []).filter((ih) => String(ih?.h || '').trim().toUpperCase() !== resolvedHost);
                        }
                        data.config.updated_at = new Date().toISOString();
                        await writeData(data, { allowFallbackWrite: true });
                        return res.status(200).json({
                            ok: true,
                            pc_overrides: data.config.pc_overrides,
                            ide_hosts: data.config.ide_hosts
                        });
                    }

                    const val = status === 'INHERIT' ? null : (status === 'LOCKED');
                    const cur = (data.config.pc_overrides[overrideKey] && typeof data.config.pc_overrides[overrideKey] === 'object')
                        ? { ...data.config.pc_overrides[overrideKey] }
                        : { lock_ai: null, lock_extensions: null };
                    if (mod === 'AI' || mod === 'ALL') {
                        cur.lock_ai = val;
                    }
                    if (mod === 'EXT' || mod === 'ALL') {
                        cur.lock_extensions = val;
                    }
                    if ((cur.lock_ai ?? null) === null && (cur.lock_extensions ?? null) === null) {
                        delete data.config.pc_overrides[overrideKey];
                    } else {
                        data.config.pc_overrides[overrideKey] = cur;
                    }

                    for (const [sid, stItem] of Object.entries(data.students)) {
                        if (!stItem) continue;
                        const stHost = String(stItem.hostname || '').trim().toUpperCase();
                        if ((targetStudentId && sid === targetStudentId) || (resolvedHost && stHost === resolvedHost)) {
                            if (mod === 'AI' || mod === 'ALL') {
                                stItem.lock_ai = val;
                            }
                            if (mod === 'EXT' || mod === 'ALL') {
                                stItem.lock_extensions = val;
                            }
                            stItem.cmd_updated_at = Date.now();
                        }
                    }

                    data.config.updated_at = new Date().toISOString();
                    const modLabel = mod === 'AI' ? 'AI Chat / Agent' : (mod === 'EXT' ? 'Ekstensi IDE' : 'Semua Modul');
                    const pcLabel = overrideKey || targetStudentId;
                    addLog(data, targetStudentId || 'ADMIN', 'Dosen Pengawas', pcLabel, 'MODULE_STATUS', `${modLabel} pada PC [${pcLabel}] diatur ke ${status}.`, 'INFO');
                    await writeData(data, { allowFallbackWrite: true });
                    return res.status(200).json({
                        ok: true,
                        student_id: matchedKey || targetStudentId,
                        hostname: resolvedHost,
                        lock_ai: cur.lock_ai ?? null,
                        lock_extensions: cur.lock_extensions ?? null,
                        pc_overrides: data.config.pc_overrides
                    });
                }

                if (status === 'INHERIT' || mod === 'REMOVE') {
                    return res.status(400).json({ ok: false, error: 'Status INHERIT / REMOVE hanya berlaku per komputer.' });
                }

                const isLocked = status === 'LOCKED';
                if (mod === 'AI' || mod === 'ALL') {
                    data.config.lock_ai = isLocked;
                    data.config.kill_processes = isLocked;
                }
                if (mod === 'EXT' || mod === 'ALL') {
                    data.config.lock_extensions = isLocked;
                    data.config.quarantine_extensions = isLocked;
                }
                if (mod === 'ALL') {
                    data.config.global_status = status;
                }
                for (const stItem of Object.values(data.students)) {
                    if (!stItem) continue;
                    if (mod === 'AI' || mod === 'ALL') stItem.lock_ai = null;
                    if (mod === 'EXT' || mod === 'ALL') stItem.lock_extensions = null;
                }
                for (const [k, ov] of Object.entries(data.config.pc_overrides)) {
                    if (ov && typeof ov === 'object') {
                        if (mod === 'AI' || mod === 'ALL') ov.lock_ai = null;
                        if (mod === 'EXT' || mod === 'ALL') ov.lock_extensions = null;
                        if ((ov.lock_ai ?? null) === null && (ov.lock_extensions ?? null) === null) {
                            delete data.config.pc_overrides[k];
                        }
                    }
                }
                data.config.updated_at = new Date().toISOString();
                const modLabel = mod === 'AI' ? 'AI Chat / Agent' : (mod === 'EXT' ? 'Ekstensi IDE' : 'Semua Modul');
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'MODULE_STATUS', `Status ${modLabel} diatur ke ${status} (Semua PC mengikuti global).`, 'INFO');
                await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    global_status: data.config.global_status || 'LOCKED',
                    lock_ai: Boolean(data.config.lock_ai),
                    lock_extensions: Boolean(data.config.lock_extensions),
                    pc_overrides: data.config.pc_overrides,
                    config: sanitizeConfigForDashboard(data.config)
                });
            }

            case 'add_room': {
                const newRoom = String(body.room_code || '').trim().toUpperCase().replace(/[^A-Z0-9_-]/g, '-');
                const customPin = String(body.room_pin || '').trim();
                if (!newRoom) {
                    return res.status(400).json({ ok: false, error: 'Kode ruangan wajib diisi.' });
                }
                if (data.config.rooms.includes(newRoom)) {
                    return res.status(400).json({ ok: false, error: `Ruangan [${newRoom}] sudah ada.` });
                }
                registerRoomByAdmin(data, newRoom, customPin);
                data.config.updated_at = new Date().toISOString();
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_ADDED', `Ruangan baru [${newRoom}] ditambahkan (PIN: ${data.config.room_pins[newRoom]}).`, 'INFO', newRoom);
                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    room_code: newRoom,
                    room_pin: data.config.room_pins[newRoom],
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    room_pins: data.config.room_pins
                });
            }

            case 'rename_room': {
                const oldRoom = String(body.old_room_code || '').trim().toUpperCase();
                const newRoom = String(body.new_room_code || oldRoom).trim().toUpperCase().replace(/[^A-Z0-9_-]/g, '-');
                const newPin = String(body.room_pin || '').trim();
                if (!oldRoom || !newRoom) {
                    return res.status(400).json({ ok: false, error: 'Kode ruangan wajib diisi.' });
                }
                if (!data.config.rooms.includes(oldRoom)) {
                    return res.status(404).json({ ok: false, error: `Ruangan [${oldRoom}] tidak ditemukan.` });
                }

                const oldStatus = data.config.room_statuses[oldRoom] || data.config.global_status || 'LOCKED';
                const resolvedPin = newPin || data.config.room_pins[oldRoom] || '202601';

                if (oldRoom !== newRoom) {
                    const idx = data.config.rooms.indexOf(oldRoom);
                    if (data.config.rooms.includes(newRoom)) {
                        data.config.rooms.splice(idx, 1);
                    } else {
                        data.config.rooms[idx] = newRoom;
                    }
                    data.config.room_statuses[newRoom] = oldStatus;
                    delete data.config.room_statuses[oldRoom];
                    delete data.config.room_pins[oldRoom];
                    if (data.config.room_blocklists && data.config.room_blocklists[oldRoom]) {
                        data.config.room_blocklists[newRoom] = data.config.room_blocklists[oldRoom];
                        delete data.config.room_blocklists[oldRoom];
                    }
                    if (data.config.room_code === oldRoom) {
                        data.config.room_code = newRoom;
                    }
                    for (const id of Object.keys(data.students)) {
                        if (String(data.students[id].room_code || '').toUpperCase() === oldRoom) {
                            data.students[id].room_code = newRoom;
                        }
                    }
                }

                data.config.room_pins[newRoom] = resolvedPin;
                data.config.updated_at = new Date().toISOString();
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_UPDATED', `Ruangan [${oldRoom}] diperbarui menjadi [${newRoom}] (PIN: ${resolvedPin}).`, 'INFO', newRoom);
                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    room_code: newRoom,
                    room_pin: resolvedPin,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    room_pins: data.config.room_pins,
                    room_blocklists: data.config.room_blocklists
                });
            }

            case 'delete_room': {
                const delRoom = String(body.room_code || '').trim().toUpperCase();
                if (!delRoom) {
                    return res.status(400).json({ ok: false, error: 'Kode ruangan wajib diisi.' });
                }
                if (!Array.isArray(data.config.deleted_rooms)) data.config.deleted_rooms = [];
                if (!data.config.deleted_rooms.includes(delRoom)) {
                    data.config.deleted_rooms.push(delRoom);
                }
                data.config.rooms = data.config.rooms.filter(r => r !== delRoom);
                delete data.config.room_statuses[delRoom];
                delete data.config.room_pins[delRoom];
                if (data.config.room_blocklists) {
                    delete data.config.room_blocklists[delRoom];
                }
                if (data.config.room_code === delRoom) {
                    data.config.room_code = data.config.rooms[0] || '';
                }
                for (const id of Object.keys(data.students)) {
                    if (String(data.students[id].room_code || '').toUpperCase() === delRoom) {
                        delete data.students[id];
                    }
                }
                data.config.updated_at = new Date().toISOString();
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_REMOVED', `Ruangan [${delRoom}] beserta sesi di dalamnya telah dihapus.`, 'INFO');
                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    room_pins: data.config.room_pins,
                    room_blocklists: data.config.room_blocklists
                });
            }

            case 'set_student_command': {
                const studentId = String(body.student_id || '').trim();
                const cmd = String(body.command || '').trim().toUpperCase();
                if (!studentId) {
                    return res.status(400).json({ ok: false, error: 'ID mahasiswa wajib diisi.' });
                }
                if (cmd === 'REMOVE') {
                    const nowSec = Math.floor(Date.now() / 1000);
                    const st = data.students[studentId] || null;
                    if (!data.config.deleted_students || typeof data.config.deleted_students !== 'object' || Array.isArray(data.config.deleted_students)) {
                        data.config.deleted_students = {};
                    }
                    data.config.deleted_students[studentId] = Math.max(
                        nowSec,
                        Number(st?.last_seen || 0),
                        Number(data.config.deleted_students[studentId] || 0)
                    );
                    data.config.deleted_students = pruneDeletedStudentsMap(data.config.deleted_students, nowSec);
                    data.config.updated_at = new Date().toISOString();
                    if (st) {
                        const stRoom = st.room_code || data.config.room_code || '-';
                        addLog(data, st.nim, st.name, st.hostname, 'ADMIN_REMOVE', `Data sesi mahasiswa dihapus dari ruangan [${stRoom}] oleh pengawas.`, 'INFO', stRoom);
                        delete data.students[studentId];
                    }
                    await writeData(data, { allowFallbackWrite: true });
                    return res.status(200).json({ ok: true, deleted_students: data.config.deleted_students });
                }
                if (!data.students[studentId]) {
                    return res.status(404).json({ ok: false, error: 'Data mahasiswa tidak ditemukan.' });
                }
                const st = data.students[studentId];
                const stRoom = st.room_code || data.config.room_code || '-';
                if (['LOCKED', 'UNLOCKED'].includes(cmd)) {
                    const cmdTs = Date.now();
                    const isCmdLocked = cmd === 'LOCKED';
                    data.config.updated_at = new Date(cmdTs).toISOString();
                    st.target_status = cmd;
                    st.status = cmd;
                    st.lock_ai = isCmdLocked;
                    st.lock_extensions = isCmdLocked;
                    st.cmd_updated_at = cmdTs;
                    const hKey = String(st.hostname || '').trim().toUpperCase();
                    if (hKey) {
                        if (!data.config.pc_overrides || typeof data.config.pc_overrides !== 'object' || Array.isArray(data.config.pc_overrides)) {
                            data.config.pc_overrides = {};
                        }
                        data.config.pc_overrides[hKey] = {
                            lock_ai: isCmdLocked,
                            lock_extensions: isCmdLocked
                        };
                    }
                    addLog(data, st.nim, st.name, st.hostname, 'REMOTE_CMD', `Pengawas mengubah status endpoint menjadi ${cmd} di [${stRoom}].`, 'INFO', stRoom);
                    await writeData(data, { allowFallbackWrite: true });
                    return res.status(200).json({ ok: true, target_status: cmd, status: cmd, lock_ai: isCmdLocked, lock_extensions: isCmdLocked });
                }
                if (cmd === 'UPDATE') {
                    const cmdTs = Date.now();
                    st.pending_command = 'UPDATE';
                    st.cmd_updated_at = cmdTs;
                    addLog(data, st.nim, st.name, st.hostname, 'REMOTE_UPDATE', `Pengawas mengirim instruksi perbarui agen (self-update) ke [${st.hostname}].`, 'INFO', stRoom);
                    await writeData(data, { allowFallbackWrite: true });
                    return res.status(200).json({ ok: true, command: 'UPDATE' });
                }
                return res.status(400).json({ ok: false, error: 'Perintah tidak dikenali.' });
            }

            case 'set_ide_password': {
                const newPass = String(body.new_password || body.ide_password || '').trim();
                const resetDefault = Boolean(body.reset_default) || newPass === '__RESET__';
                if (!resetDefault && newPass.length < 4) {
                    return res.status(400).json({ ok: false, error: 'Password buka kunci IDE minimal 4 karakter.' });
                }
                data.config.ide_password_hash = resetDefault ? '' : crypto.createHash('sha256').update(newPass).digest('hex');
                data.config.updated_at = new Date().toISOString();
                addLog(
                    data,
                    'ADMIN',
                    'Dosen Pengawas',
                    'DASHBOARD',
                    'IDE_PASSWORD_UPDATED',
                    resetDefault
                        ? 'Password buka kunci IDE (AI Chat & Ekstensi) dikembalikan ke default.'
                        : 'Password buka kunci IDE (AI Chat & Ekstensi) telah diperbarui.',
                    'INFO'
                );
                await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    ide_password_hash: data.config.ide_password_hash,
                    is_custom: Boolean(data.config.ide_password_hash)
                });
            }

            case 'set_admin_password': {
                const newPass = String(body.new_password || body.admin_password || '').trim();
                const hasEmailField = body.admin_email !== undefined;
                const rawEmail = hasEmailField ? String(body.admin_email || '').trim().toLowerCase() : null;
                const resetDefault = Boolean(body.reset_default) || newPass === '__RESET__';
                const syncIde = Boolean(body.sync_ide_password);

                if (!resetDefault && !newPass && !rawEmail) {
                    return res.status(400).json({ ok: false, error: 'Masukkan password baru atau email pengawas baru.' });
                }
                if (!resetDefault && newPass && newPass.length < 4) {
                    return res.status(400).json({ ok: false, error: 'Kata sandi login dashboard minimal 4 karakter.' });
                }

                if (resetDefault) {
                    data.config.admin_password_hash = '';
                    if (rawEmail === '__reset__') {
                        data.config.admin_email = '';
                    }
                    if (syncIde) {
                        data.config.ide_password_hash = '';
                    }
                } else {
                    if (newPass) {
                        const hash = crypto.createHash('sha256').update(newPass).digest('hex');
                        data.config.admin_password_hash = hash;
                        if (syncIde) {
                            data.config.ide_password_hash = hash;
                        }
                    }
                    if (rawEmail) {
                        data.config.admin_email = rawEmail;
                    }
                }
                data.config.updated_at = new Date().toISOString();
                const effectiveEmail = (data.config.admin_email && String(data.config.admin_email).trim())
                    ? String(data.config.admin_email).trim().toLowerCase()
                    : ADMIN_EMAIL;
                const logMsg = resetDefault
                    ? 'Kata sandi login dashboard admin dikembalikan ke default server.'
                    : (syncIde
                        ? 'Kata sandi login dashboard admin & buka kunci IDE telah diperbarui.'
                        : 'Kata sandi login dashboard admin telah diperbarui.');
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ADMIN_PASSWORD_UPDATED', logMsg, 'INFO');
                await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    has_custom_admin_password: Boolean(data.config.admin_password_hash),
                    is_custom: Boolean(data.config.admin_password_hash),
                    admin_email_effective: effectiveEmail,
                    token: signAdminToken(effectiveEmail),
                    synced_ide: syncIde
                });
            }

            case 'save_config':
            case 'update_config': {
                if (body.room_code) {
                    const norm = registerRoomByAdmin(data, body.room_code);
                    if (norm) data.config.room_code = norm;
                }
                if (Array.isArray(body.rooms) && body.rooms.length > 0) {
                    data.config.rooms = Array.from(new Set(body.rooms.map(r => String(r).trim().toUpperCase()).filter(Boolean)));
                }
                if (body.unlock_pin) data.config.unlock_pin = String(body.unlock_pin).trim();
                if (body.admin_email !== undefined) {
                    const rawEmail = String(body.admin_email).trim().toLowerCase();
                    if (rawEmail === '__reset__') {
                        data.config.admin_email = '';
                    } else if (rawEmail.length > 0) {
                        data.config.admin_email = rawEmail;
                    }
                }
                if (body.admin_password !== undefined) {
                    const rawAdminPass = String(body.admin_password).trim();
                    if (rawAdminPass === '__RESET__') {
                        data.config.admin_password_hash = '';
                    } else if (rawAdminPass.length > 0) {
                        if (rawAdminPass.length < 4) {
                            return res.status(400).json({ ok: false, error: 'Kata sandi login dashboard minimal 4 karakter.' });
                        }
                        data.config.admin_password_hash = crypto.createHash('sha256').update(rawAdminPass).digest('hex');
                    }
                }
                if (body.ide_password !== undefined) {
                    const rawIdePass = String(body.ide_password).trim();
                    if (rawIdePass === '__RESET__') {
                        data.config.ide_password_hash = '';
                    } else if (rawIdePass.length > 0) {
                        if (rawIdePass.length < 4) {
                            return res.status(400).json({ ok: false, error: 'Password buka kunci IDE minimal 4 karakter.' });
                        }
                        data.config.ide_password_hash = crypto.createHash('sha256').update(rawIdePass).digest('hex');
                    }
                }
                data.config.block_doh = body.block_doh !== undefined ? Boolean(body.block_doh) : true;
                data.config.block_vpn = body.block_vpn !== undefined ? Boolean(body.block_vpn) : true;
                data.config.quarantine_extensions = body.quarantine_extensions !== undefined ? Boolean(body.quarantine_extensions) : true;
                data.config.kill_processes = body.kill_processes !== undefined ? Boolean(body.kill_processes) : true;
                data.config.updated_at = new Date().toISOString();
                ensureRoomsConfig(data);
                const effectiveEmail = (data.config.admin_email && String(data.config.admin_email).trim())
                    ? String(data.config.admin_email).trim().toLowerCase()
                    : ADMIN_EMAIL;
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    token: signAdminToken(effectiveEmail),
                    config: sanitizeConfigForDashboard(data.config)
                });
            }

            case 'save_blocklist':
            case 'update_blocklist': {
                const targetRoom = String(body.room_code || 'ALL').trim().toUpperCase();
                const resetRoom = Boolean(body.reset_room);
                if (!data.config.room_blocklists || typeof data.config.room_blocklists !== 'object') {
                    data.config.room_blocklists = {};
                }

                const cleanDomains = Array.isArray(body.domains)
                    ? [...new Set(body.domains.map(s => String(s).trim().toLowerCase()).filter(Boolean))]
                    : (data.blocklist.domains || []);
                const cleanProcs = Array.isArray(body.processes)
                    ? [...new Set(body.processes.map(s => String(s).trim()).filter(Boolean))]
                    : (data.blocklist.processes || []);
                const cleanExts = Array.isArray(body.ide_extensions)
                    ? [...new Set(body.ide_extensions.map(s => String(s).trim()).filter(Boolean))]
                    : (data.blocklist.ide_extensions || []);

                if (targetRoom && targetRoom !== 'ALL') {
                    if (resetRoom) {
                        delete data.config.room_blocklists[targetRoom];
                        addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_RESET', `Kebijakan blokir khusus ruangan [${targetRoom}] dikembalikan ke Kebijakan Global.`, 'INFO', targetRoom);
                    } else {
                        data.config.room_blocklists[targetRoom] = {
                            domains: cleanDomains,
                            processes: cleanProcs,
                            ide_extensions: cleanExts,
                            updated_at: new Date().toISOString()
                        };
                        addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_BLOCKLIST_UPDATE', `Kebijakan blokir khusus ruangan [${targetRoom}] diperbarui (${cleanDomains.length} domain, ${cleanProcs.length} proses).`, 'INFO', targetRoom);
                    }
                } else {
                    data.config.blocklist_customized = true;
                    data.blocklist.domains = cleanDomains;
                    data.blocklist.processes = cleanProcs;
                    data.blocklist.ide_extensions = cleanExts;
                    addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_UPDATE', `Kebijakan blokir Global (Semua Ruangan) diperbarui (${cleanDomains.length} domain, ${cleanProcs.length} proses).`, 'INFO', 'ALL');
                }

                data.config.updated_at = new Date().toISOString();
                const cloudWrite = await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({
                    ok: true,
                    cloud_storage: cloudWrite,
                    room_code: targetRoom || 'ALL',
                    blocklist: data.blocklist,
                    room_blocklists: data.config.room_blocklists,
                    effective_blocklist: getEffectiveBlocklist(data, targetRoom)
                });
            }

            case 'clear_logs': {
                data.logs = [];
                await writeData(data, { allowFallbackWrite: true });
                return res.status(200).json({ ok: true });
            }

            default:
                return res.status(200).json({
                    ok: true,
                    service: 'GuardConsole Cloud API (Vercel + Firebase)',
                    version: '1.5.0'
                });
        }
    } catch (err) {
        return res.status(500).json({ ok: false, error: err.message });
    }
};

module.exports._internals = {
    getDefaultSchema,
    packCloudPayload,
    unpackCloudPayload,
    getKvConfig,
    parseKvPayload,
    readFromKV,
    writeToKV,
    readData,
    writeData,
    ensureRoomsConfig,
    registerRoomByAdmin,
    flushLocalDbNow: () => (memoryStore && isLocalVmMode() ? scheduleLocalDbFlush(true) : lastCloudStatus),
    getLastCloudStatus: () => lastCloudStatus,
    resetMemoryStoreForTest: () => {
        if (localFlushTimer) {
            clearTimeout(localFlushTimer);
            localFlushTimer = null;
        }
        localDbLoaded = false;
        memoryStore = null;
        lastCloudReadMs = 0;
        cloudHydrated = false;
        lastCloudStatus = { backend: 'tmp_only', ok: false, error: '' };
        cachedFbIdToken = null;
        cachedFbIdTokenExpMs = 0;
    }
};
