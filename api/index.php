<?php
/**
 * AI-Blocker Guard - REST API Endpoint
 * Melayani komunikasi dari Aplikasi Desktop PC Mahasiswa (.exe) dan Web Dashboard Pengawas.
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, X-Client-Token');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

require_once __DIR__ . '/../includes/Storage.php';

$action = $_GET['action'] ?? $_POST['action'] ?? '';
$rawInput = file_get_contents('php://input');
$jsonInput = json_decode($rawInput, true);
if (is_array($jsonInput)) {
    if (empty($action) && isset($jsonInput['action'])) {
        $action = $jsonInput['action'];
    }
} else {
    $jsonInput = $_POST;
}

function respond($data, $code = 200) {
    http_response_code($code);
    echo json_encode($data, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES);
    exit;
}

try {
    switch ($action) {
        // ====================================================================
        // ENDPOINT UNTUK APLIKASI DESKTOP MAHASISWA (CLIENT)
        // ====================================================================
        case 'join':
        case 'handshake':
            $nim = trim($jsonInput['nim'] ?? '');
            $name = trim($jsonInput['name'] ?? '');
            $hostname = trim($jsonInput['hostname'] ?? gethostname());
            $roomCode = trim($jsonInput['room_code'] ?? '');
            $osInfo = trim($jsonInput['os_info'] ?? 'Windows');
            $clientVersion = trim($jsonInput['version'] ?? '1.0.0');

            if ($nim === '' || $name === '') {
                respond(['ok' => false, 'error' => 'NIM dan Nama Mahasiswa wajib diisi.'], 400);
            }

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $roomCode, $osInfo, $clientVersion) {
                $expectedRoom = $data['config']['room_code'] ?? 'KUIS-AI-01';
                if ($roomCode !== '' && strcasecmp($roomCode, $expectedRoom) !== 0) {
                    return ['ok' => false, 'error' => "Kode Ruangan tidak sesuai. Gunakan kode: {$expectedRoom}"];
                }

                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $existing = $data['students'][$studentId] ?? null;

                $desiredState = $existing['target_status'] ?? $data['config']['global_status'] ?? 'LOCKED';

                $data['students'][$studentId] = [
                    'id' => $studentId,
                    'nim' => $nim,
                    'name' => $name,
                    'hostname' => $hostname,
                    'ip' => $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1',
                    'os_info' => $osInfo,
                    'version' => $clientVersion,
                    'status' => $existing['status'] ?? 'CONNECTED',
                    'target_status' => $desiredState,
                    'quarantined_count' => $existing['quarantined_count'] ?? 0,
                    'quarantined_list' => $existing['quarantined_list'] ?? [],
                    'blocked_domains_count' => count($data['blocklist']['domains'] ?? []),
                    'killed_processes_count' => $existing['killed_processes_count'] ?? 0,
                    'vpn_detected' => false,
                    'joined_at' => $existing['joined_at'] ?? date('Y-m-d H:i:s'),
                    'last_seen' => time(),
                    'last_seen_str' => date('Y-m-d H:i:s')
                ];

                Storage::addLog(
                    $data,
                    $nim,
                    $name,
                    $hostname,
                    'JOIN',
                    "PC Mahasiswa terhubung ke ruangan ({$osInfo}). Status target: {$desiredState}",
                    'INFO'
                );

                return [
                    'ok' => true,
                    'student_id' => $studentId,
                    'config' => [
                        'room_code' => $data['config']['room_code'],
                        'room_name' => $data['config']['room_name'],
                        'target_status' => $desiredState,
                        'block_doh' => $data['config']['block_doh'],
                        'block_vpn' => $data['config']['block_vpn'],
                        'quarantine_extensions' => $data['config']['quarantine_extensions'],
                        'kill_processes' => $data['config']['kill_processes']
                    ],
                    'blocklist' => $data['blocklist']
                ];
            });

            respond($result, $result['ok'] ? 200 : 403);
            break;

        case 'heartbeat':
            $nim = trim($jsonInput['nim'] ?? '');
            $name = trim($jsonInput['name'] ?? 'Mahasiswa');
            $hostname = trim($jsonInput['hostname'] ?? 'PC');
            $currentStatus = trim($jsonInput['status'] ?? 'LOCKED'); // LOCKED | UNLOCKED | DRY_RUN
            $quarantinedCount = (int)($jsonInput['quarantined_count'] ?? 0);
            $quarantinedList = $jsonInput['quarantined_list'] ?? [];
            $killedCount = (int)($jsonInput['killed_processes_count'] ?? 0);
            $vpnDetected = (bool)($jsonInput['vpn_detected'] ?? false);

            if ($nim === '') {
                respond(['ok' => false, 'error' => 'NIM diperlukan untuk heartbeat.'], 400);
            }

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $currentStatus, $quarantinedCount, $quarantinedList, $killedCount, $vpnDetected) {
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $globalTarget = $data['config']['global_status'] ?? 'LOCKED';

                if (!isset($data['students'][$studentId])) {
                    $data['students'][$studentId] = [
                        'id' => $studentId,
                        'nim' => $nim,
                        'name' => $name,
                        'hostname' => $hostname,
                        'ip' => $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1',
                        'os_info' => 'Windows',
                        'version' => '1.0.0',
                        'target_status' => $globalTarget,
                        'joined_at' => date('Y-m-d H:i:s')
                    ];
                }

                $student = &$data['students'][$studentId];
                $prevVpn = $student['vpn_detected'] ?? false;

                $student['status'] = $currentStatus;
                $student['hostname'] = $hostname;
                $student['ip'] = $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1';
                $student['quarantined_count'] = $quarantinedCount;
                if (is_array($quarantinedList)) {
                    $student['quarantined_list'] = $quarantinedList;
                }
                $student['killed_processes_count'] = $killedCount;
                $student['vpn_detected'] = $vpnDetected;
                $student['last_seen'] = time();
                $student['last_seen_str'] = date('Y-m-d H:i:s');

                if ($vpnDetected && !$prevVpn) {
                    Storage::addLog($data, $nim, $name, $hostname, 'VPN_ALERT', 'Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa!', 'DANGER');
                }

                $targetStatus = $student['target_status'] ?? $globalTarget;

                return [
                    'ok' => true,
                    'target_status' => $targetStatus,
                    'config' => [
                        'room_code' => $data['config']['room_code'],
                        'room_name' => $data['config']['room_name'],
                        'block_doh' => $data['config']['block_doh'],
                        'block_vpn' => $data['config']['block_vpn'],
                        'quarantine_extensions' => $data['config']['quarantine_extensions'],
                        'kill_processes' => $data['config']['kill_processes']
                    ]
                ];
            });

            respond($result);
            break;

        case 'violation':
            $nim = trim($jsonInput['nim'] ?? 'UNKNOWN');
            $name = trim($jsonInput['name'] ?? 'Mahasiswa');
            $hostname = trim($jsonInput['hostname'] ?? 'PC');
            $type = trim($jsonInput['type'] ?? 'AI_DETECTED');
            $message = trim($jsonInput['message'] ?? 'Percobaan akses AI terdeteksi dan diblokir.');
            $severity = trim($jsonInput['severity'] ?? 'DANGER');

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $type, $message, $severity) {
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                if (isset($data['students'][$studentId])) {
                    $data['students'][$studentId]['last_seen'] = time();
                    $data['students'][$studentId]['last_seen_str'] = date('Y-m-d H:i:s');
                    $data['students'][$studentId]['killed_processes_count'] = ($data['students'][$studentId]['killed_processes_count'] ?? 0) + 1;
                }
                Storage::addLog($data, $nim, $name, $hostname, $type, $message, $severity);
                return ['ok' => true];
            });

            respond($result);
            break;

        case 'verify_pin':
            $pin = trim($jsonInput['pin'] ?? '');
            $nim = trim($jsonInput['nim'] ?? '');
            $name = trim($jsonInput['name'] ?? 'Mahasiswa');
            $hostname = trim($jsonInput['hostname'] ?? 'PC');

            $result = Storage::update(function (&$data) use ($pin, $nim, $name, $hostname) {
                $validPin = (string)($data['config']['unlock_pin'] ?? '202699');
                if ($pin !== '' && hash_equals($validPin, $pin)) {
                    $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                    if (isset($data['students'][$studentId])) {
                        $data['students'][$studentId]['target_status'] = 'UNLOCKED';
                        $data['students'][$studentId]['status'] = 'UNLOCKED';
                    }
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_UNLOCK', 'Kunci dibuka menggunakan PIN Dosen/Pengawas yang valid.', 'INFO');
                    return ['ok' => true, 'unlocked' => true];
                } else {
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_FAILED', "Percobaan membuka kunci gagal (PIN salah: {$pin}).", 'WARNING');
                    return ['ok' => false, 'unlocked' => false, 'error' => 'PIN Pengawas salah!'];
                }
            });

            respond($result, $result['ok'] ? 200 : 403);
            break;

        case 'blocklist':
            $data = Storage::read();
            respond([
                'ok' => true,
                'config' => [
                    'room_code' => $data['config']['room_code'],
                    'room_name' => $data['config']['room_name'],
                    'global_status' => $data['config']['global_status']
                ],
                'blocklist' => $data['blocklist']
            ]);
            break;

        // ====================================================================
        // ENDPOINT UNTUK WEB DASHBOARD PENGAWAS / DOSEN
        // ====================================================================
        case 'dashboard_state':
            $data = Storage::read();
            $now = time();
            $students = [];
            $onlineCount = 0;
            $lockedCount = 0;
            $violationCount = 0;

            foreach (($data['students'] ?? []) as $id => $st) {
                $isSimulated = !empty($st['is_simulated']);
                $isOnline = $isSimulated ? true : (($now - ($st['last_seen'] ?? 0)) <= 25);
                $st['is_online'] = $isOnline;
                $st['seconds_ago'] = $isSimulated ? 1 : max(0, $now - ($st['last_seen'] ?? $now));
                if ($isOnline) {
                    $onlineCount++;
                }
                if (($st['status'] ?? '') === 'LOCKED' || ($st['status'] ?? '') === 'DRY_RUN') {
                    $lockedCount++;
                }
                $violationCount += (int)($st['killed_processes_count'] ?? 0);
                $students[] = $st;
            }

            // Urutkan yang online di atas
            usort($students, function ($a, $b) {
                if ($a['is_online'] !== $b['is_online']) {
                    return $a['is_online'] ? -1 : 1;
                }
                return strcmp($a['nim'], $b['nim']);
            });

            respond([
                'ok' => true,
                'server_time' => date('Y-m-d H:i:s'),
                'config' => $data['config'],
                'stats' => [
                    'total_students' => count($students),
                    'online_students' => $onlineCount,
                    'locked_students' => $lockedCount,
                    'total_violations' => $violationCount,
                    'domains_count' => count($data['blocklist']['domains'] ?? []),
                    'processes_count' => count($data['blocklist']['processes'] ?? []),
                    'extensions_count' => count($data['blocklist']['ide_extensions'] ?? [])
                ],
                'students' => $students,
                'logs' => array_slice($data['logs'] ?? [], 0, 100),
                'blocklist' => $data['blocklist']
            ]);
            break;

        case 'set_global_status':
            $target = strtoupper(trim($jsonInput['status'] ?? 'LOCKED'));
            if (!in_array($target, ['LOCKED', 'UNLOCKED'], true)) {
                respond(['ok' => false, 'error' => 'Status tidak valid.'], 400);
            }

            $result = Storage::update(function (&$data) use ($target) {
                $data['config']['global_status'] = $target;
                $data['config']['updated_at'] = date('c');
                foreach ($data['students'] as $id => &$st) {
                    $st['target_status'] = $target;
                    $st['status'] = $target;
                    if (!empty($st['is_simulated'])) {
                        $st['last_seen'] = time();
                    }
                }
                $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', "Pengawas memilih {$label} untuk seluruh mahasiswa.", 'INFO');
                return ['ok' => true, 'global_status' => $target];
            });

            respond($result);
            break;

        case 'set_student_command':
            $studentId = trim($jsonInput['student_id'] ?? '');
            $cmd = strtoupper(trim($jsonInput['command'] ?? ''));

            $result = Storage::update(function (&$data) use ($studentId, $cmd) {
                if (!isset($data['students'][$studentId])) {
                    return ['ok' => false, 'error' => 'Data mahasiswa tidak ditemukan.'];
                }
                $st = &$data['students'][$studentId];
                if ($cmd === 'REMOVE') {
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'ADMIN_REMOVE', 'Data sesi mahasiswa dihapus dari daftar oleh pengawas.', 'INFO');
                    unset($data['students'][$studentId]);
                    return ['ok' => true];
                }
                if (in_array($cmd, ['LOCKED', 'UNLOCKED'], true)) {
                    $st['target_status'] = $cmd;
                    $st['status'] = $cmd;
                    if (!empty($st['is_simulated'])) {
                        $st['last_seen'] = time();
                    }
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'REMOTE_CMD', "Pengawas mengubah status endpoint menjadi {$cmd}.", 'INFO');
                    return ['ok' => true, 'target_status' => $cmd, 'status' => $cmd];
                }
                return ['ok' => false, 'error' => 'Perintah tidak dikenali.'];
            });

            respond($result);
            break;

        case 'update_config':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                if (!empty($jsonInput['room_code'])) {
                    $data['config']['room_code'] = trim($jsonInput['room_code']);
                }
                if (!empty($jsonInput['room_name'])) {
                    $data['config']['room_name'] = trim($jsonInput['room_name']);
                }
                if (!empty($jsonInput['unlock_pin'])) {
                    $data['config']['unlock_pin'] = trim($jsonInput['unlock_pin']);
                }
                $data['config']['block_doh'] = isset($jsonInput['block_doh']) ? (bool)$jsonInput['block_doh'] : true;
                $data['config']['block_vpn'] = isset($jsonInput['block_vpn']) ? (bool)$jsonInput['block_vpn'] : true;
                $data['config']['quarantine_extensions'] = isset($jsonInput['quarantine_extensions']) ? (bool)$jsonInput['quarantine_extensions'] : true;
                $data['config']['kill_processes'] = isset($jsonInput['kill_processes']) ? (bool)$jsonInput['kill_processes'] : true;
                $data['config']['updated_at'] = date('c');

                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                return ['ok' => true, 'config' => $data['config']];
            });
            respond($result);
            break;

        case 'update_blocklist':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                if (isset($jsonInput['domains']) && is_array($jsonInput['domains'])) {
                    $data['blocklist']['domains'] = array_values(array_unique(array_filter(array_map('trim', $jsonInput['domains']))));
                }
                if (isset($jsonInput['processes']) && is_array($jsonInput['processes'])) {
                    $data['blocklist']['processes'] = array_values(array_unique(array_filter(array_map('trim', $jsonInput['processes']))));
                }
                if (isset($jsonInput['ide_extensions']) && is_array($jsonInput['ide_extensions'])) {
                    $data['blocklist']['ide_extensions'] = array_values(array_unique(array_filter(array_map('trim', $jsonInput['ide_extensions']))));
                }
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_UPDATE', 'Daftar domain/proses/ekstensi AI yang diblokir telah diperbarui.', 'INFO');
                return ['ok' => true, 'blocklist' => $data['blocklist']];
            });
            respond($result);
            break;

        case 'simulate_student':
            // Fitur simulasi untuk demonstrasi langsung di dashboard
            $result = Storage::update(function (&$data) {
                $samples = [
                    ['nim' => '240101001', 'name' => 'Budi Santoso', 'host' => 'LAPTOP-BUDI01', 'ext' => ['github.copilot-1.250.0', 'github.copilot-chat-0.22.1', 'saoudrizwan.claude-dev-3.1.0']],
                    ['nim' => '240101002', 'name' => 'Siti Aminah', 'host' => 'PC-SITI-LAB2', 'ext' => ['codeium.codeium-1.18.0', 'continue.continue-0.9.2']],
                    ['nim' => '240101003', 'name' => 'Reza Rahadian', 'host' => 'ROG-STRIX-REZA', 'ext' => ['github.copilot-1.250.0', 'tabnine.tabnine-vscode-3.190.0']]
                ];
                $pick = $samples[array_rand($samples)];
                $id = $pick['nim'];
                $target = $data['config']['global_status'] ?? 'LOCKED';

                $data['students'][$id] = [
                    'id' => $id,
                    'nim' => $pick['nim'],
                    'name' => $pick['name'],
                    'hostname' => $pick['host'],
                    'ip' => '192.168.1.' . rand(10, 240),
                    'os_info' => 'Windows 11 Pro 64-bit',
                    'version' => '1.2.0',
                    'status' => $target,
                    'target_status' => $target,
                    'is_simulated' => true,
                    'quarantined_count' => count($pick['ext']),
                    'quarantined_list' => $pick['ext'],
                    'blocked_domains_count' => count($data['blocklist']['domains']),
                    'killed_processes_count' => 1,
                    'vpn_detected' => false,
                    'joined_at' => date('Y-m-d H:i:s'),
                    'last_seen' => time(),
                    'last_seen_str' => date('Y-m-d H:i:s')
                ];

                Storage::addLog($data, $pick['nim'], $pick['name'], $pick['host'], 'EXTENSION_QUARANTINE', 'Mengkarantina ' . count($pick['ext']) . ' ekstensi AI VS Code: ' . implode(', ', $pick['ext']), 'WARNING');
                Storage::addLog($data, $pick['nim'], $pick['name'], $pick['host'], 'PROCESS_KILLED', 'Menghentikan paksa proses AI terlarang: Cursor.exe & ollama.exe (Port 11434)', 'DANGER');

                return ['ok' => true, 'simulated' => $pick];
            });
            respond($result);
            break;

        case 'clear_logs':
            $result = Storage::update(function (&$data) {
                $data['logs'] = [];
                return ['ok' => true];
            });
            respond($result);
            break;

        default:
            respond([
                'ok' => true,
                'service' => 'AI-Blocker Guard API Server',
                'version' => '1.0.0',
                'endpoints' => ['join', 'heartbeat', 'violation', 'verify_pin', 'blocklist', 'dashboard_state']
            ]);
    }
} catch (Throwable $e) {
    respond(['ok' => false, 'error' => $e->getMessage()], 500);
}
