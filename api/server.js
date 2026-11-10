/**
 * GuardConsole — Vercel Serverless API (Node.js)
 * Kompatibel penuh dengan aplikasi Desktop GuardAgent (.exe) dan Web Dashboard.
 * Mendukung penyimpanan otomatis di /tmp/database.json (Serverless Warm State)
 * serta mendukung penuh Vercel KV / Upstash Redis (jika KV_REST_API_URL diset).
 */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const TMP_DB_PATH = '/tmp/aiblocker_database.json';

function getDefaultSchema() {
    return {
        config: {
            room_code: 'KUIS-AI-01',
            room_name: 'Ujian Pemrograman & Algoritma (Tanpa AI)',
            unlock_pin: '202699',
            global_status: 'LOCKED',
            block_doh: true,
            block_vpn: true,
            quarantine_extensions: true,
            kill_processes: true,
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
                'cohere.com', 'api.cohere.ai'
            ],
            processes: [
                'Cursor', 'Windsurf', 'Trae', 'PearAI', 'Void',
                'ChatGPT', 'Claude', 'Copilot', 'Perplexity',
                'ollama', 'ollama_llama_server', 'ollama app',
                'LM Studio', 'lm-studio', 'jan', 'Jan', 'gpt4all', 'GPT4All',
                'llama-server', 'llama-cli', 'koboldcpp', 'text-generation-webui', 'anythingllm', 'AnythingLLM', 'msty', 'Msty'
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

async function readData() {
    // 1. Jika Upstash Redis / Vercel KV dikonfigurasi via Environment Variable
    if (process.env.KV_REST_API_URL && process.env.KV_REST_API_TOKEN) {
        try {
            const resp = await fetch(`${process.env.KV_REST_API_URL}/get/aiblocker_db`, {
                headers: { Authorization: `Bearer ${process.env.KV_REST_API_TOKEN}` }
            });
            const json = await resp.json();
            if (json && json.result) {
                return typeof json.result === 'string' ? JSON.parse(json.result) : json.result;
            }
        } catch (e) {
            console.error('KV Read Error:', e);
        }
    }

    // 2. Fallback ke /tmp/aiblocker_database.json & Memory
    try {
        if (fs.existsSync(TMP_DB_PATH)) {
            const raw = fs.readFileSync(TMP_DB_PATH, 'utf8');
            memoryStore = JSON.parse(raw);
            return memoryStore;
        }
    } catch (e) {}

    if (!memoryStore) {
        memoryStore = getDefaultSchema();
        try {
            fs.writeFileSync(TMP_DB_PATH, JSON.stringify(memoryStore, null, 2), 'utf8');
        } catch (e) {}
    }
    return memoryStore;
}

async function writeData(data) {
    memoryStore = data;
    try {
        fs.writeFileSync(TMP_DB_PATH, JSON.stringify(data, null, 2), 'utf8');
    } catch (e) {}

    if (process.env.KV_REST_API_URL && process.env.KV_REST_API_TOKEN) {
        try {
            await fetch(`${process.env.KV_REST_API_URL}/set/aiblocker_db`, {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${process.env.KV_REST_API_TOKEN}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(JSON.stringify(data))
            });
        } catch (e) {
            console.error('KV Write Error:', e);
        }
    }
}

function nowFormatted() {
    const d = new Date();
    return d.toISOString().replace('T', ' ').substring(0, 19);
}

function addLog(data, nim, name, hostname, type, message, severity = 'INFO') {
    if (!Array.isArray(data.logs)) data.logs = [];
    data.logs.unshift({
        id: 'log_' + crypto.randomBytes(4).toString('hex'),
        timestamp: nowFormatted(),
        nim,
        name,
        hostname,
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
    const action = req.query.action || body.action || '';
    const clientIp = (req.headers['x-forwarded-for'] || req.socket?.remoteAddress || '127.0.0.1').split(',')[0].trim();

    try {
        const data = await readData();
        if (!data.students || Array.isArray(data.students)) data.students = {};

        switch (action) {
            case 'join':
            case 'handshake': {
                const nim = String(body.nim || '').trim();
                const name = String(body.name || '').trim();
                const hostname = String(body.hostname || 'PC-CLIENT').trim();
                const roomCode = String(body.room_code || '').trim();
                const osInfo = String(body.os_info || 'Windows').trim();
                const version = String(body.version || '1.2.0').trim();

                if (!nim || !name) {
                    return res.status(400).json({ ok: false, error: 'NIM dan Nama Mahasiswa wajib diisi.' });
                }

                const expectedRoom = data.config.room_code || 'KUIS-AI-01';
                if (roomCode && roomCode.toLowerCase() !== expectedRoom.toLowerCase()) {
                    return res.status(403).json({ ok: false, error: `Kode Ruangan tidak sesuai. Gunakan kode: ${expectedRoom}` });
                }

                const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                const existing = data.students[studentId] || null;
                const desiredState = existing?.target_status || data.config.global_status || 'LOCKED';
                const nowSec = Math.floor(Date.now() / 1000);

                data.students[studentId] = {
                    id: studentId,
                    nim,
                    name,
                    hostname,
                    ip: clientIp,
                    os_info: osInfo,
                    version,
                    status: existing?.status || desiredState,
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

                addLog(data, nim, name, hostname, 'JOIN', `PC Mahasiswa terhubung ke ruangan (${osInfo}). Status target: ${desiredState}`, 'INFO');
                await writeData(data);

                return res.status(200).json({
                    ok: true,
                    student_id: studentId,
                    config: {
                        room_code: data.config.room_code,
                        room_name: data.config.room_name,
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
                const currentStatus = String(body.status || 'LOCKED').trim();
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

                if (!data.students[studentId]) {
                    data.students[studentId] = {
                        id: studentId,
                        nim,
                        name,
                        hostname,
                        ip: clientIp,
                        os_info: 'Windows',
                        version: '1.2.0',
                        target_status: globalTarget,
                        joined_at: nowFormatted()
                    };
                }

                const st = data.students[studentId];
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
                    addLog(data, nim, name, hostname, 'VPN_ALERT', 'Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa!', 'DANGER');
                }

                await writeData(data);

                return res.status(200).json({
                    ok: true,
                    target_status: st.target_status || globalTarget,
                    config: {
                        room_code: data.config.room_code,
                        room_name: data.config.room_name,
                        block_doh: data.config.block_doh,
                        block_vpn: data.config.block_vpn,
                        quarantine_extensions: data.config.quarantine_extensions,
                        kill_processes: data.config.kill_processes
                    }
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
                if (data.students[studentId]) {
                    data.students[studentId].last_seen = Math.floor(Date.now() / 1000);
                    data.students[studentId].last_seen_str = nowFormatted();
                    data.students[studentId].killed_processes_count = (data.students[studentId].killed_processes_count || 0) + 1;
                }
                addLog(data, nim, name, hostname, type, message, severity);
                await writeData(data);
                return res.status(200).json({ ok: true });
            }

            case 'verify_pin': {
                const pin = String(body.pin || '').trim();
                const nim = String(body.nim || '').trim();
                const name = String(body.name || 'Mahasiswa').trim();
                const hostname = String(body.hostname || 'PC').trim();
                const validPin = String(data.config.unlock_pin || '202699');

                if (pin && pin === validPin) {
                    const studentId = nim.replace(/[^a-zA-Z0-9_-]/g, '_');
                    if (data.students[studentId]) {
                        data.students[studentId].target_status = 'UNLOCKED';
                        data.students[studentId].status = 'UNLOCKED';
                    }
                    addLog(data, nim, name, hostname, 'PIN_UNLOCK', 'Kunci dibuka menggunakan PIN Dosen/Pengawas yang valid.', 'INFO');
                    await writeData(data);
                    return res.status(200).json({ ok: true, unlocked: true });
                } else {
                    addLog(data, nim, name, hostname, 'PIN_FAILED', `Percobaan membuka kunci gagal (PIN salah: ${pin}).`, 'WARNING');
                    await writeData(data);
                    return res.status(403).json({ ok: false, unlocked: false, error: 'PIN Pengawas salah!' });
                }
            }

            case 'dashboard_state': {
                const nowSec = Math.floor(Date.now() / 1000);
                const students = [];
                let onlineCount = 0;
                let lockedCount = 0;
                let violationCount = 0;

                for (const id of Object.keys(data.students)) {
                    const st = { ...data.students[id] };
                    const isSim = Boolean(st.is_simulated);
                    const isOnline = isSim ? true : ((nowSec - (st.last_seen || 0)) <= 25);
                    st.is_online = isOnline;
                    st.seconds_ago = isSim ? 1 : Math.max(0, nowSec - (st.last_seen || nowSec));
                    if (isOnline) onlineCount++;
                    if (st.status === 'LOCKED' || st.status === 'DRY_RUN') lockedCount++;
                    violationCount += parseInt(st.killed_processes_count || 0, 10);
                    students.push(st);
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
                if (!['LOCKED', 'UNLOCKED'].includes(target)) {
                    return res.status(400).json({ ok: false, error: 'Status tidak valid.' });
                }
                data.config.global_status = target;
                data.config.updated_at = new Date().toISOString();
                for (const id of Object.keys(data.students)) {
                    data.students[id].target_status = target;
                    data.students[id].status = target;
                }
                const label = target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', `Pengawas memilih ${label} untuk seluruh mahasiswa.`, 'INFO');
                await writeData(data);
                return res.status(200).json({ ok: true, global_status: target });
            }

            case 'set_student_command': {
                const studentId = String(body.student_id || '').trim();
                const cmd = String(body.command || '').trim().toUpperCase();
                if (!data.students[studentId]) {
                    return res.status(404).json({ ok: false, error: 'Data mahasiswa tidak ditemukan.' });
                }
                const st = data.students[studentId];
                if (cmd === 'REMOVE') {
                    addLog(data, st.nim, st.name, st.hostname, 'ADMIN_REMOVE', 'Data sesi mahasiswa dihapus dari daftar oleh pengawas.', 'INFO');
                    delete data.students[studentId];
                    await writeData(data);
                    return res.status(200).json({ ok: true });
                }
                if (['LOCKED', 'UNLOCKED'].includes(cmd)) {
                    st.target_status = cmd;
                    st.status = cmd;
                    addLog(data, st.nim, st.name, st.hostname, 'REMOTE_CMD', `Pengawas mengubah status endpoint menjadi ${cmd}.`, 'INFO');
                    await writeData(data);
                    return res.status(200).json({ ok: true, target_status: cmd, status: cmd });
                }
                return res.status(400).json({ ok: false, error: 'Perintah tidak dikenali.' });
            }

            case 'update_config': {
                if (body.room_code) data.config.room_code = String(body.room_code).trim();
                if (body.room_name) data.config.room_name = String(body.room_name).trim();
                if (body.unlock_pin) data.config.unlock_pin = String(body.unlock_pin).trim();
                data.config.block_doh = body.block_doh !== undefined ? Boolean(body.block_doh) : true;
                data.config.block_vpn = body.block_vpn !== undefined ? Boolean(body.block_vpn) : true;
                data.config.quarantine_extensions = body.quarantine_extensions !== undefined ? Boolean(body.quarantine_extensions) : true;
                data.config.kill_processes = body.kill_processes !== undefined ? Boolean(body.kill_processes) : true;
                data.config.updated_at = new Date().toISOString();
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                await writeData(data);
                return res.status(200).json({ ok: true, config: data.config });
            }

            case 'update_blocklist': {
                if (Array.isArray(body.domains)) {
                    data.blocklist.domains = [...new Set(body.domains.map(s => String(s).trim()).filter(Boolean))];
                }
                if (Array.isArray(body.processes)) {
                    data.blocklist.processes = [...new Set(body.processes.map(s => String(s).trim()).filter(Boolean))];
                }
                if (Array.isArray(body.ide_extensions)) {
                    data.blocklist.ide_extensions = [...new Set(body.ide_extensions.map(s => String(s).trim()).filter(Boolean))];
                }
                addLog(data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_UPDATE', 'Daftar domain/proses/ekstensi AI yang diblokir telah diperbarui.', 'INFO');
                await writeData(data);
                return res.status(200).json({ ok: true, blocklist: data.blocklist });
            }

            case 'simulate_student': {
                const samples = [
                    { nim: '240101001', name: 'Budi Santoso', host: 'LAPTOP-BUDI01', ext: ['github.copilot-1.250.0', 'github.copilot-chat-0.22.1', 'saoudrizwan.claude-dev-3.1.0'] },
                    { nim: '240101002', name: 'Siti Aminah', host: 'PC-SITI-LAB2', ext: ['codeium.codeium-1.18.0', 'continue.continue-0.9.2'] },
                    { nim: '240101003', name: 'Reza Rahadian', host: 'ROG-STRIX-REZA', ext: ['github.copilot-1.250.0', 'tabnine.tabnine-vscode-3.190.0'] }
                ];
                const pick = samples[Math.floor(Math.random() * samples.length)];
                const id = pick.nim;
                const target = data.config.global_status || 'LOCKED';
                const nowSec = Math.floor(Date.now() / 1000);

                data.students[id] = {
                    id,
                    nim: pick.nim,
                    name: pick.name,
                    hostname: pick.host,
                    ip: '192.168.1.' + Math.floor(10 + Math.random() * 230),
                    os_info: 'Windows 11 Pro 64-bit',
                    version: '1.2.0',
                    status: target,
                    target_status: target,
                    is_simulated: true,
                    quarantined_count: pick.ext.length,
                    quarantined_list: pick.ext,
                    blocked_domains_count: (data.blocklist.domains || []).length,
                    killed_processes_count: 1,
                    vpn_detected: false,
                    joined_at: nowFormatted(),
                    last_seen: nowSec,
                    last_seen_str: nowFormatted()
                };

                addLog(data, pick.nim, pick.name, pick.host, 'EXTENSION_QUARANTINE', `Mengkarantina ${pick.ext.length} ekstensi AI VS Code: ${pick.ext.join(', ')}`, 'WARNING');
                addLog(data, pick.nim, pick.name, pick.host, 'PROCESS_KILLED', 'Menghentikan paksa proses AI terlarang: Cursor.exe & ollama.exe (Port 11434)', 'DANGER');
                await writeData(data);
                return res.status(200).json({ ok: true, simulated: pick });
            }

            case 'clear_logs': {
                data.logs = [];
                await writeData(data);
                return res.status(200).json({ ok: true });
            }

            default:
                return res.status(200).json({
                    ok: true,
                    service: 'GuardConsole Cloud API (Vercel)',
                    version: '1.2.0'
                });
        }
    } catch (err) {
        return res.status(500).json({ ok: false, error: err.message });
    }
};
