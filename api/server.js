/**
 * GuardConsole — Vercel Serverless API (Node.js) + Firebase Sync Engine
 * Kompatibel penuh dengan aplikasi Desktop GuardAgent (.exe) dan Web Dashboard.
 * Mendukung sinkronisasi otomatis ke Firebase Firestore / Realtime Database (blockai-ba569),
 * Vercel KV, serta fallback penyimpanan lokal /tmp/aiblocker_database.json.
 */

const fs = require('fs');
const os = require('os');
const path = require('path');
const crypto = require('crypto');

const TMP_DB_PATH = path.join(os.tmpdir(), 'aiblocker_database.json');
const FIREBASE_PROJECT_ID = 'blockai-ba569';
const FIRESTORE_DOC_URL = `https://firestore.googleapis.com/v1/projects/${FIREBASE_PROJECT_ID}/databases/(default)/documents/guard_system/main_state`;
const RTDB_URL = process.env.FIREBASE_DB_URL || `https://${FIREBASE_PROJECT_ID}-default-rtdb.firebaseio.com/guard_system.json`;

function getDefaultSchema() {
    return {
        config: {
            room_code: 'LAB-706',
            rooms: ['LAB-702', 'LAB-704', 'LAB-706'],
            deleted_rooms: [],
            room_statuses: {
                'LAB-702': 'LOCKED',
                'LAB-704': 'LOCKED',
                'LAB-706': 'LOCKED'
            },
            room_pins: {
                'LAB-702': '202601',
                'LAB-704': '202602',
                'LAB-706': '202603'
            },
            unlock_pin: '202699',
            admin_email: 'admin@guard.edu',
            admin_password: 'admin123',
            global_status: 'LOCKED',
            block_doh: true,
            block_vpn: true,
            quarantine_extensions: true,
            kill_processes: true,
            blocklist_customized: false,
            updated_at: new Date().toISOString()
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
let firestoreBackoffUntil = 0;
let rtdbBackoffUntil = 0;
let lastFirebaseWriteMs = 0;

async function fetchWithTimeout(url, options = {}, timeoutMs = 800) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);
    try {
        return await fetch(url, { ...options, signal: controller.signal });
    } finally {
        clearTimeout(timer);
    }
}

async function readFromFirebase() {
    const now = Date.now();
    // 1. Coba Cloud Firestore REST API (blockai-ba569) jika tidak sedang backoff
    if (now >= firestoreBackoffUntil) {
        try {
            const res = await fetchWithTimeout(FIRESTORE_DOC_URL, { method: 'GET' }, 800);
            if (res.ok) {
                const doc = await res.json();
                const rawPayload = doc?.fields?.payload?.stringValue;
                if (rawPayload) {
                    return JSON.parse(rawPayload);
                }
            } else if (res.status === 403 || res.status === 404) {
                firestoreBackoffUntil = now + 60000;
            }
        } catch (e) {
            firestoreBackoffUntil = now + 30000;
        }
    }

    // 2. Coba Firebase Realtime Database REST API jika tidak sedang backoff
    if (now >= rtdbBackoffUntil) {
        try {
            const res = await fetchWithTimeout(RTDB_URL, { method: 'GET' }, 800);
            if (res.ok) {
                const data = await res.json();
                if (data && typeof data === 'object' && data.config) {
                    return data;
                }
            } else if (res.status === 401 || res.status === 403 || res.status === 404) {
                rtdbBackoffUntil = now + 120000;
            }
        } catch (e) {
            rtdbBackoffUntil = now + 60000;
        }
    }

    return null;
}

async function writeToFirebase(data, forceSync = false) {
    const now = Date.now();
    if (!forceSync && (now - lastFirebaseWriteMs) < 12000) {
        return;
    }
    lastFirebaseWriteMs = now;

    // 1. Simpan ke Cloud Firestore REST API jika aktif
    if (now >= firestoreBackoffUntil) {
        try {
            const res = await fetchWithTimeout(FIRESTORE_DOC_URL, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    fields: {
                        payload: { stringValue: JSON.stringify(data) },
                        updatedAt: { stringValue: new Date().toISOString() }
                    }
                })
            }, 800);
            if (res.status === 403 || res.status === 404) {
                firestoreBackoffUntil = now + 60000;
            }
        } catch (e) {
            firestoreBackoffUntil = now + 30000;
        }
    }

    // 2. Simpan ke Firebase Realtime Database REST API jika aktif
    if (now >= rtdbBackoffUntil) {
        try {
            const res = await fetchWithTimeout(RTDB_URL, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            }, 800);
            if (res.status === 401 || res.status === 403 || res.status === 404) {
                rtdbBackoffUntil = now + 120000;
            }
        } catch (e) {
            rtdbBackoffUntil = now + 60000;
        }
    }
}

async function readData() {
    // 1. Jika Upstash Redis / Vercel KV dikonfigurasi
    if (process.env.KV_REST_API_URL && process.env.KV_REST_API_TOKEN) {
        try {
            const resp = await fetchWithTimeout(`${process.env.KV_REST_API_URL}/get/aiblocker_db`, {
                headers: { Authorization: `Bearer ${process.env.KV_REST_API_TOKEN}` }
            }, 800);
            const json = await resp.json();
            if (json && json.result) {
                return typeof json.result === 'string' ? JSON.parse(json.result) : json.result;
            }
        } catch (e) {}
    }

    // 2. Cek in-memory / file lokal /tmp/aiblocker_database.json (Warm Lambda < 1ms)
    if (memoryStore && memoryStore.config) {
        return memoryStore;
    }
    try {
        if (fs.existsSync(TMP_DB_PATH)) {
            const raw = fs.readFileSync(TMP_DB_PATH, 'utf8');
            memoryStore = JSON.parse(raw);
            return memoryStore;
        }
    } catch (e) {}

    // 3. Jika cold start, ambil dari Firebase (Firestore / RTDB)
    const fbData = await readFromFirebase();
    if (fbData && fbData.config) {
        memoryStore = fbData;
        try {
            fs.writeFileSync(TMP_DB_PATH, JSON.stringify(memoryStore, null, 2), 'utf8');
        } catch (e) {}
        return memoryStore;
    }

    if (!memoryStore) {
        memoryStore = getDefaultSchema();
        try {
            fs.writeFileSync(TMP_DB_PATH, JSON.stringify(memoryStore, null, 2), 'utf8');
        } catch (e) {}
    }
    return memoryStore;
}

async function writeData(data, forceCloudSync = false) {
    memoryStore = data;
    try {
        fs.writeFileSync(TMP_DB_PATH, JSON.stringify(data, null, 2), 'utf8');
    } catch (e) {}

    // Sinkronkan ke Firebase dengan circuit-breaker cepat agar tidak memblokir heartbeat
    await writeToFirebase(data, forceCloudSync);

    if (process.env.KV_REST_API_URL && process.env.KV_REST_API_TOKEN) {
        try {
            await fetchWithTimeout(`${process.env.KV_REST_API_URL}/set/aiblocker_db`, {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${process.env.KV_REST_API_TOKEN}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(JSON.stringify(data))
            }, 800);
        } catch (e) {}
    }
}

function nowFormatted() {
    const d = new Date();
    return d.toISOString().replace('T', ' ').substring(0, 19);
}

function ensureRoomsConfig(data) {
    if (!Array.isArray(data.config.rooms) || data.config.rooms.length === 0) {
        const defRoom = data.config.room_code || 'LAB-706';
        data.config.rooms = Array.from(new Set(['LAB-702', 'LAB-704', 'LAB-706', defRoom]));
    }
    if (!Array.isArray(data.config.deleted_rooms)) {
        data.config.deleted_rooms = [];
    }
    if (!data.config.room_statuses || typeof data.config.room_statuses !== 'object') {
        data.config.room_statuses = {};
    }
    if (!data.config.room_pins || typeof data.config.room_pins !== 'object') {
        data.config.room_pins = {};
    }
    data.config.rooms.forEach((r, idx) => {
        if (!data.config.room_statuses[r]) {
            data.config.room_statuses[r] = data.config.global_status || 'LOCKED';
        }
        if (!data.config.room_pins[r]) {
            data.config.room_pins[r] = String(202601 + idx);
        }
    });

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
        data.config.room_pins[norm] = String(202600 + data.config.rooms.length);
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

module.exports = async (req, res) => {
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', 'Content-Type, X-Client-Token');

    if (req.method === 'OPTIONS') {
        return res.status(200).end();
    }

    const body = typeof req.body === 'string' ? (() => { try { return JSON.parse(req.body); } catch { return {}; } })() : (req.body || {});
    const action = (req.query && req.query.action) || body.action || '';
    const clientIp = ((req.headers && req.headers['x-forwarded-for']) || req.socket?.remoteAddress || '127.0.0.1').split(',')[0].trim();

    try {
        const data = await readData();
        if (!data.students || Array.isArray(data.students)) data.students = {};
        ensureRoomsConfig(data);

        switch (action) {
            case 'admin_login': {
                const email = String(body.email || '').trim().toLowerCase();
                const password = String(body.password || '').trim();
                const validEmail = String(data.config.admin_email || 'admin@guard.edu').toLowerCase();
                const validPass = String(data.config.admin_password || 'admin123');
                const masterPin = String(data.config.unlock_pin || '202699');

                if ((email === validEmail || email === 'admin' || email === 'pengawas@guard.edu') &&
                    (password === validPass || password === masterPin)) {
                    return res.status(200).json({
                        ok: true,
                        user: {
                            email: email.includes('@') ? email : validEmail,
                            name: 'Dosen / Pengawas Ujian',
                            role: 'ADMIN'
                        }
                    });
                }
                return res.status(401).json({ ok: false, error: 'Email atau Kata Sandi / PIN Pengawas tidak sesuai.' });
            }

            case 'rehydrate_state':
            case 'sync_from_firebase': {
                // Menerima sinkronisasi state dari Firebase client SDK / Admin Browser jika container baru cold-start
                if (body.config && typeof body.config === 'object') {
                    data.config = { ...data.config, ...body.config };
                }
                if (body.blocklist && typeof body.blocklist === 'object') {
                    data.blocklist = { ...data.blocklist, ...body.blocklist };
                    data.config.blocklist_customized = true;
                }
                if (Array.isArray(body.students)) {
                    for (const st of body.students) {
                        if (st && st.id && !data.students[st.id]) {
                            data.students[st.id] = st;
                        }
                    }
                }
                ensureRoomsConfig(data);
                await writeData(data, true);
                return res.status(200).json({ ok: true });
            }

            case 'get_rooms': {
                return res.status(200).json({
                    ok: true,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    default_room: data.config.room_code || data.config.rooms[0] || 'LAB-706'
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

                if (!rawRoom || (data.config.deleted_rooms || []).includes(rawRoom)) {
                    return res.status(400).json({
                        ok: false,
                        error: `Kode Ruangan [${rawRoom || '-'}] tidak ditemukan. Silakan pilih Ruangan yang tersedia terlebih dahulu.`,
                        rooms: data.config.rooms
                    });
                }

                // Jika container baru saja cold-start dan ruangan valid (tidak ada di deleted_rooms), pastikan terdaftar
                if (!data.config.rooms.includes(rawRoom)) {
                    registerRoomByAdmin(data, rawRoom);
                }

                const normalizedRoom = rawRoom;
                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const existing = data.students[studentId] || null;
                const roomDefaultState = data.config.room_statuses[normalizedRoom] || data.config.global_status || 'LOCKED';
                const desiredState = (existing && existing.room_code === normalizedRoom && existing.target_status) ? existing.target_status : roomDefaultState;
                const roomPin = data.config.room_pins[normalizedRoom] || data.config.unlock_pin || '202699';
                const nowSec = Math.floor(Date.now() / 1000);

                data.students[studentId] = {
                    id: studentId,
                    nim,
                    name,
                    hostname,
                    room_code: normalizedRoom,
                    ip: clientIp,
                    os_info: osInfo,
                    version,
                    status: existing?.status === 'UNLOCKED' ? 'UNLOCKED' : desiredState,
                    target_status: desiredState,
                    quarantined_count: existing?.quarantined_count || 0,
                    quarantined_list: existing?.quarantined_list || [],
                    blocked_domains_count: (data.blocklist.domains || []).length,
                    killed_processes_count: existing?.killed_processes_count || 0,
                    vpn_detected: false,
                    joined_at: existing?.joined_at || nowFormatted(),
                    last_seen: nowSec,
                    last_seen_str: nowFormatted()
                };

                addLog(data, nim, name, hostname, 'JOIN', `PC Mahasiswa terhubung ke ruangan [${normalizedRoom}] (${osInfo}). Status: ${desiredState}`, 'INFO', normalizedRoom);
                await writeData(data);

                return res.status(200).json({
                    ok: true,
                    student_id: studentId,
                    rooms: data.config.rooms,
                    config: {
                        room_code: normalizedRoom,
                        room_pin: roomPin,
                        target_status: desiredState,
                        block_doh: data.config.block_doh,
                        block_vpn: data.config.block_vpn,
                        quarantine_extensions: data.config.quarantine_extensions,
                        kill_processes: data.config.kill_processes
                    },
                    blocklist: data.blocklist
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
                const reqRoom = String(body.room_code || '').trim().toUpperCase();
                const deletedRooms = data.config.deleted_rooms || [];

                // Hanya tolak ruangan jika ruangan kosong atau secara eksplisit dihapus oleh Admin
                if (!reqRoom || deletedRooms.includes(reqRoom)) {
                    if (data.students[studentId]) {
                        delete data.students[studentId];
                        await writeData(data);
                    }
                    return res.status(200).json({
                        ok: false,
                        require_room_selection: true,
                        rooms: data.config.rooms,
                        target_status: 'UNLOCKED',
                        error: 'Ruangan telah dihapus oleh Admin. Silakan pilih ruangan aktif.'
                    });
                }

                // Apabila container baru cold-start dan PC mahasiswa sudah aktif di ruangan ini, otomatis pertahankan ruangan tersebut
                if (!data.config.rooms.includes(reqRoom)) {
                    registerRoomByAdmin(data, reqRoom);
                }

                if (!data.students[studentId]) {
                    data.students[studentId] = {
                        id: studentId,
                        nim,
                        name,
                        hostname,
                        room_code: reqRoom,
                        ip: clientIp,
                        os_info: 'Windows',
                        version: '1.5.0',
                        target_status: data.config.room_statuses[reqRoom] || globalTarget,
                        joined_at: nowFormatted()
                    };
                }

                const st = data.students[studentId];
                if (name) st.name = name;
                st.room_code = reqRoom;

                const prevVpn = Boolean(st.vpn_detected);
                st.status = currentStatus;
                st.hostname = hostname;
                st.ip = clientIp;
                st.quarantined_count = quarantinedCount;
                st.quarantined_list = quarantinedList;
                st.killed_processes_count = killedCount;
                st.vpn_detected = vpnDetected;
                st.last_seen = nowSec;
                st.last_seen_str = nowFormatted();

                if (vpnDetected && !prevVpn) {
                    addLog(data, nim, name, hostname, 'VPN_ALERT', `Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa di ruangan [${st.room_code}]!`, 'DANGER', st.room_code);
                }

                await writeData(data);
                const roomPin = data.config.room_pins[st.room_code] || data.config.unlock_pin || '202699';

                return res.status(200).json({
                    ok: true,
                    room_code: st.room_code,
                    room_pin: roomPin,
                    target_status: st.target_status || data.config.room_statuses[st.room_code] || globalTarget,
                    rooms: data.config.rooms,
                    config: {
                        room_code: st.room_code,
                        room_pin: roomPin,
                        block_doh: data.config.block_doh,
                        block_vpn: data.config.block_vpn,
                        quarantine_extensions: data.config.quarantine_extensions,
                        kill_processes: data.config.kill_processes
                    },
                    blocklist: data.blocklist
                });
            }

            case 'violation': {
                const nim = String(body.nim || 'UNKNOWN').trim();
                const name = String(body.name || 'Mahasiswa').trim();
                const hostname = String(body.hostname || 'PC').trim();
                const type = String(body.type || 'AI_DETECTED').trim();
                const message = String(body.message || 'Percobaan akses AI terdeteksi dan diblokir.').trim();
                const severity = String(body.severity || 'DANGER').trim();

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                let stRoom = data.config.room_code || 'LAB-706';
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
                const masterPin = String(data.config.unlock_pin || '202699');

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const stRoom = String(body.room_code || data.students[studentId]?.room_code || data.config.room_code || 'LAB-706').trim().toUpperCase();
                const roomPin = String(data.config.room_pins?.[stRoom] || '');

                if (pin && (pin === roomPin || pin === masterPin)) {
                    if (data.students[studentId]) {
                        data.students[studentId].target_status = 'UNLOCKED';
                        data.students[studentId].status = 'UNLOCKED';
                    }
                    addLog(data, nim, name, hostname, 'PIN_UNLOCK', `Kunci dibuka / otorisasi keluar menggunakan PIN Ruangan [${stRoom}] yang valid.`, 'INFO', stRoom);
                    await writeData(data);
                    return res.status(200).json({ ok: true, unlocked: true });
                } else {
                    addLog(data, nim, name, hostname, 'PIN_FAILED', `Percobaan PIN gagal di ruangan [${stRoom}] (PIN salah: ${pin}).`, 'WARNING', stRoom);
                    await writeData(data);
                    return res.status(403).json({ ok: false, unlocked: false, error: `PIN Otorisasi untuk ruangan [${stRoom}] salah!` });
                }
            }

            case 'state':
            case 'dashboard_state': {
                const nowSec = Math.floor(Date.now() / 1000);
                const students = [];
                let onlineCount = 0;
                let lockedCount = 0;
                let violationCount = 0;
                let dirty = false;
                const deletedRooms = data.config.deleted_rooms || [];

                for (const id of Object.keys(data.students)) {
                    const rawSt = data.students[id];
                    const stRoom = String(rawSt.room_code || '').toUpperCase();
                    if (!stRoom || deletedRooms.includes(stRoom) || rawSt.is_simulated) {
                        delete data.students[id];
                        dirty = true;
                        continue;
                    }
                    if (!data.config.rooms.includes(stRoom)) {
                        registerRoomByAdmin(data, stRoom);
                        dirty = true;
                    }
                    const st = { ...rawSt };
                    const isOnline = (nowSec - (st.last_seen || 0)) <= 40;
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
                    config: data.config,
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

                data.config.updated_at = new Date().toISOString();
                if (targetRoom === 'ALL' || !targetRoom) {
                    data.config.global_status = target;
                    for (const r of data.config.rooms) {
                        data.config.room_statuses[r] = target;
                    }
                    for (const id of Object.keys(data.students)) {
                        data.students[id].target_status = target;
                        data.students[id].status = target;
                    }
                    const label = target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', `Pengawas memilih ${label} untuk SELURUH RUANGAN.`, 'INFO', 'ALL');
                } else {
                    if (!data.config.rooms.includes(targetRoom)) {
                        return res.status(404).json({ ok: false, error: 'Ruangan tidak ditemukan.' });
                    }
                    data.config.room_statuses[targetRoom] = target;
                    for (const id of Object.keys(data.students)) {
                        const stRoom = String(data.students[id].room_code || '').toUpperCase();
                        if (stRoom === targetRoom) {
                            data.students[id].target_status = target;
                            data.students[id].status = target;
                        }
                    }
                    const label = target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_COMMAND', `Pengawas memilih ${label} untuk ruangan [${targetRoom}].`, 'INFO', targetRoom);
                }

                await writeData(data, true);
                return res.status(200).json({ ok: true, global_status: data.config.global_status, room_statuses: data.config.room_statuses });
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
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_ADDED', `Ruangan baru [${newRoom}] ditambahkan (PIN: ${data.config.room_pins[newRoom]}).`, 'INFO', newRoom);
                await writeData(data, true);
                return res.status(200).json({
                    ok: true,
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
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_UPDATED', `Ruangan [${oldRoom}] diperbarui menjadi [${newRoom}] (PIN: ${resolvedPin}).`, 'INFO', newRoom);
                await writeData(data, true);
                return res.status(200).json({
                    ok: true,
                    room_code: newRoom,
                    room_pin: resolvedPin,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    room_pins: data.config.room_pins
                });
            }

            case 'delete_room': {
                const delRoom = String(body.room_code || '').trim().toUpperCase();
                if (!delRoom || data.config.rooms.length <= 1) {
                    return res.status(400).json({ ok: false, error: 'Minimal harus terdapat 1 ruangan aktif.' });
                }
                if (!Array.isArray(data.config.deleted_rooms)) data.config.deleted_rooms = [];
                if (!data.config.deleted_rooms.includes(delRoom)) {
                    data.config.deleted_rooms.push(delRoom);
                }
                data.config.rooms = data.config.rooms.filter(r => r !== delRoom);
                delete data.config.room_statuses[delRoom];
                delete data.config.room_pins[delRoom];
                if (data.config.room_code === delRoom) {
                    data.config.room_code = data.config.rooms[0];
                }
                for (const id of Object.keys(data.students)) {
                    if (String(data.students[id].room_code || '').toUpperCase() === delRoom) {
                        delete data.students[id];
                    }
                }
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_REMOVED', `Ruangan [${delRoom}] beserta sesi di dalamnya telah dihapus.`, 'INFO');
                await writeData(data, true);
                return res.status(200).json({
                    ok: true,
                    rooms: data.config.rooms,
                    room_statuses: data.config.room_statuses,
                    room_pins: data.config.room_pins
                });
            }

            case 'set_student_command': {
                const studentId = String(body.student_id || '').trim();
                const cmd = String(body.command || '').trim().toUpperCase();
                if (!data.students[studentId]) {
                    return res.status(404).json({ ok: false, error: 'Data mahasiswa tidak ditemukan.' });
                }
                const st = data.students[studentId];
                const stRoom = st.room_code || data.config.room_code || 'KUIS-AI-01';
                if (cmd === 'REMOVE') {
                    addLog(data, st.nim, st.name, st.hostname, 'ADMIN_REMOVE', `Data sesi mahasiswa dihapus dari ruangan [${stRoom}] oleh pengawas.`, 'INFO', stRoom);
                    delete data.students[studentId];
                    await writeData(data);
                    return res.status(200).json({ ok: true });
                }
                if (['LOCKED', 'UNLOCKED'].includes(cmd)) {
                    st.target_status = cmd;
                    st.status = cmd;
                    addLog(data, st.nim, st.name, st.hostname, 'REMOTE_CMD', `Pengawas mengubah status endpoint menjadi ${cmd} di [${stRoom}].`, 'INFO', stRoom);
                    await writeData(data);
                    return res.status(200).json({ ok: true, target_status: cmd, status: cmd });
                }
                return res.status(400).json({ ok: false, error: 'Perintah tidak dikenali.' });
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
                if (body.admin_email) data.config.admin_email = String(body.admin_email).trim();
                if (body.admin_password) data.config.admin_password = String(body.admin_password).trim();
                data.config.block_doh = body.block_doh !== undefined ? Boolean(body.block_doh) : true;
                data.config.block_vpn = body.block_vpn !== undefined ? Boolean(body.block_vpn) : true;
                data.config.quarantine_extensions = body.quarantine_extensions !== undefined ? Boolean(body.quarantine_extensions) : true;
                data.config.kill_processes = body.kill_processes !== undefined ? Boolean(body.kill_processes) : true;
                data.config.updated_at = new Date().toISOString();
                ensureRoomsConfig(data);
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                await writeData(data);
                return res.status(200).json({ ok: true, config: data.config });
            }

            case 'save_blocklist':
            case 'update_blocklist': {
                data.config.blocklist_customized = true;
                if (Array.isArray(body.domains)) {
                    data.blocklist.domains = [...new Set(body.domains.map(s => String(s).trim().toLowerCase()).filter(Boolean))];
                }
                if (Array.isArray(body.processes)) {
                    data.blocklist.processes = [...new Set(body.processes.map(s => String(s).trim()).filter(Boolean))];
                }
                if (Array.isArray(body.ide_extensions)) {
                    data.blocklist.ide_extensions = [...new Set(body.ide_extensions.map(s => String(s).trim()).filter(Boolean))];
                }
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_UPDATE', `Daftar blokir diperbarui (${data.blocklist.domains.length} domain, ${data.blocklist.processes.length} proses).`, 'INFO');
                await writeData(data);
                return res.status(200).json({ ok: true, blocklist: data.blocklist });
            }

            case 'clear_logs': {
                data.logs = [];
                await writeData(data);
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
