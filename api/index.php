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
                $normalizedRoom = Storage::registerRoomIfNeeded($data, $roomCode);

                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $existing = $data['students'][$studentId] ?? null;

                $roomDefaultState = $data['config']['room_statuses'][$normalizedRoom] ?? $data['config']['global_status'] ?? 'LOCKED';
                $desiredState = $existing['target_status'] ?? $roomDefaultState;

                $data['students'][$studentId] = [
                    'id' => $studentId,
                    'nim' => $nim,
                    'name' => $name,
                    'hostname' => $hostname,
                    'room_code' => $normalizedRoom,
                    'ip' => $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1',
                    'os_info' => $osInfo,
                    'version' => $clientVersion,
                    'status' => $existing['status'] ?? $desiredState,
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
                    "PC Mahasiswa terhubung ke ruangan [{$normalizedRoom}] ({$osInfo}). Status target: {$desiredState}",
                    'INFO',
                    $normalizedRoom
                );

                return [
                    'ok' => true,
                    'student_id' => $studentId,
                    'config' => [
                        'room_code' => $normalizedRoom,
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

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $currentStatus, $quarantinedCount, $quarantinedList, $killedCount, $vpnDetected, $jsonInput) {
                Storage::ensureRoomsConfig($data);
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $globalTarget = $data['config']['global_status'] ?? 'LOCKED';

                if (!isset($data['students'][$studentId])) {
                    $defaultRoom = Storage::registerRoomIfNeeded($data, $jsonInput['room_code'] ?? $data['config']['room_code'] ?? 'KUIS-AI-01');
                    $data['students'][$studentId] = [
                        'id' => $studentId,
                        'nim' => $nim,
                        'name' => $name,
                        'hostname' => $hostname,
                        'room_code' => $defaultRoom,
                        'ip' => $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1',
                        'os_info' => 'Windows',
                        'version' => '1.0.0',
                        'target_status' => $data['config']['room_statuses'][$defaultRoom] ?? $globalTarget,
                        'joined_at' => date('Y-m-d H:i:s')
                    ];
                }

                $student = &$data['students'][$studentId];
                if (empty($student['room_code'])) {
                    $student['room_code'] = $data['config']['room_code'] ?? 'KUIS-AI-01';
                }
                Storage::registerRoomIfNeeded($data, $student['room_code']);
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
                    Storage::addLog($data, $nim, $name, $hostname, 'VPN_ALERT', "Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa di [{$student['room_code']}]!", 'DANGER', $student['room_code']);
                }

                $targetStatus = $student['target_status'] ?? ($data['config']['room_statuses'][$student['room_code']] ?? $globalTarget);

                return [
                    'ok' => true,
                    'target_status' => $targetStatus,
                    'config' => [
                        'room_code' => $student['room_code'],
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
                $stRoom = $data['config']['room_code'] ?? 'KUIS-AI-01';
                if (isset($data['students'][$studentId])) {
                    $data['students'][$studentId]['last_seen'] = time();
                    $data['students'][$studentId]['last_seen_str'] = date('Y-m-d H:i:s');
                    $data['students'][$studentId]['killed_processes_count'] = ($data['students'][$studentId]['killed_processes_count'] ?? 0) + 1;
                    $stRoom = $data['students'][$studentId]['room_code'] ?? $stRoom;
                }
                Storage::addLog($data, $nim, $name, $hostname, $type, $message, $severity, $stRoom);
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
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $stRoom = $data['students'][$studentId]['room_code'] ?? ($data['config']['room_code'] ?? 'KUIS-AI-01');
                if ($pin !== '' && hash_equals($validPin, $pin)) {
                    if (isset($data['students'][$studentId])) {
                        $data['students'][$studentId]['target_status'] = 'UNLOCKED';
                        $data['students'][$studentId]['status'] = 'UNLOCKED';
                    }
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_UNLOCK', 'Kunci dibuka menggunakan PIN Dosen/Pengawas yang valid.', 'INFO', $stRoom);
                    return ['ok' => true, 'unlocked' => true];
                } else {
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_FAILED', "Percobaan membuka kunci gagal (PIN salah: {$pin}).", 'WARNING', $stRoom);
                    return ['ok' => false, 'unlocked' => false, 'error' => 'PIN Pengawas salah!'];
                }
            });

            respond($result, $result['ok'] ? 200 : 403);
            break;

        case 'blocklist':
            $data = Storage::read();
            Storage::ensureRoomsConfig($data);
            respond([
                'ok' => true,
                'config' => [
                    'room_code' => $data['config']['room_code'],
                    'rooms' => $data['config']['rooms'],
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
            Storage::ensureRoomsConfig($data);
            $now = time();
            $students = [];
            $onlineCount = 0;
            $lockedCount = 0;
            $violationCount = 0;

            foreach (($data['students'] ?? []) as $id => $st) {
                if (empty($st['room_code'])) {
                    $st['room_code'] = $data['config']['room_code'] ?? 'KUIS-AI-01';
                }
                Storage::registerRoomIfNeeded($data, $st['room_code']);
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
            $targetRoom = strtoupper(trim($jsonInput['room_code'] ?? 'ALL'));
            if (!in_array($target, ['LOCKED', 'UNLOCKED'], true)) {
                respond(['ok' => false, 'error' => 'Status tidak valid.'], 400);
            }

            $result = Storage::update(function (&$data) use ($target, $targetRoom) {
                Storage::ensureRoomsConfig($data);
                $data['config']['updated_at'] = date('c');

                if ($targetRoom === 'ALL' || $targetRoom === '') {
                    $data['config']['global_status'] = $target;
                    foreach ($data['config']['rooms'] as $r) {
                        $data['config']['room_statuses'][$r] = $target;
                    }
                    foreach ($data['students'] as $id => &$st) {
                        $st['target_status'] = $target;
                        $st['status'] = $target;
                        if (!empty($st['is_simulated'])) {
                            $st['last_seen'] = time();
                        }
                    }
                    $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', "Pengawas memilih {$label} untuk SELURUH RUANGAN.", 'INFO', 'ALL');
                } else {
                    Storage::registerRoomIfNeeded($data, $targetRoom);
                    $data['config']['room_statuses'][$targetRoom] = $target;
                    foreach ($data['students'] as $id => &$st) {
                        $stRoom = strtoupper($st['room_code'] ?? $data['config']['room_code'] ?? 'KUIS-AI-01');
                        if ($stRoom === $targetRoom) {
                            $st['target_status'] = $target;
                            $st['status'] = $target;
                            if (!empty($st['is_simulated'])) {
                                $st['last_seen'] = time();
                            }
                        }
                    }
                    $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_COMMAND', "Pengawas memilih {$label} untuk ruangan [{$targetRoom}].", 'INFO', $targetRoom);
                }

                return ['ok' => true, 'global_status' => $data['config']['global_status'], 'room_statuses' => $data['config']['room_statuses']];
            });

            respond($result);
            break;

        case 'add_room':
            $newRoom = strtoupper(preg_replace('/[^a-zA-Z0-9_-]/', '-', trim($jsonInput['room_code'] ?? '')));
            if ($newRoom === '') {
                respond(['ok' => false, 'error' => 'Kode ruangan wajib diisi.'], 400);
            }
            $result = Storage::update(function (&$data) use ($newRoom) {
                Storage::registerRoomIfNeeded($data, $newRoom);
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_ADDED', "Ruangan baru [{$newRoom}] ditambahkan ke sistem.", 'INFO', $newRoom);
                return ['ok' => true, 'room_code' => $newRoom, 'rooms' => $data['config']['rooms'], 'room_statuses' => $data['config']['room_statuses']];
            });
            respond($result);
            break;

        case 'rename_room':
            $oldRoom = strtoupper(trim($jsonInput['old_room_code'] ?? ''));
            $newRoom = strtoupper(preg_replace('/[^a-zA-Z0-9_-]/', '-', trim($jsonInput['new_room_code'] ?? '')));
            if ($oldRoom === '' || $newRoom === '') {
                respond(['ok' => false, 'error' => 'Kode ruangan lama dan baru wajib diisi.'], 400);
            }
            $result = Storage::update(function (&$data) use ($oldRoom, $newRoom) {
                Storage::ensureRoomsConfig($data);
                if ($oldRoom === $newRoom) {
                    return ['ok' => true, 'room_code' => $newRoom, 'rooms' => $data['config']['rooms'], 'room_statuses' => $data['config']['room_statuses']];
                }
                $oldStatus = $data['config']['room_statuses'][$oldRoom] ?? ($data['config']['global_status'] ?? 'LOCKED');
                $idx = array_search($oldRoom, $data['config']['rooms'], true);
                if ($idx !== false) {
                    if (in_array($newRoom, $data['config']['rooms'], true)) {
                        array_splice($data['config']['rooms'], $idx, 1);
                    } else {
                        $data['config']['rooms'][$idx] = $newRoom;
                    }
                } elseif (!in_array($newRoom, $data['config']['rooms'], true)) {
                    $data['config']['rooms'][] = $newRoom;
                }
                $data['config']['rooms'] = array_values(array_unique($data['config']['rooms']));
                $data['config']['room_statuses'][$newRoom] = $oldStatus;
                unset($data['config']['room_statuses'][$oldRoom]);
                if (($data['config']['room_code'] ?? '') === $oldRoom) {
                    $data['config']['room_code'] = $newRoom;
                }
                foreach ($data['students'] as &$st) {
                    if (strtoupper($st['room_code'] ?? '') === $oldRoom) {
                        $st['room_code'] = $newRoom;
                    }
                }
                unset($st);
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_RENAMED', "Kode ruangan [{$oldRoom}] diubah menjadi [{$newRoom}].", 'INFO', $newRoom);
                return ['ok' => true, 'room_code' => $newRoom, 'rooms' => $data['config']['rooms'], 'room_statuses' => $data['config']['room_statuses']];
            });
            respond($result, $result['ok'] ? 200 : 400);
            break;

        case 'delete_room':
            $delRoom = strtoupper(trim($jsonInput['room_code'] ?? ''));
            $result = Storage::update(function (&$data) use ($delRoom) {
                Storage::ensureRoomsConfig($data);
                if ($delRoom === '' || count($data['config']['rooms']) <= 1) {
                    return ['ok' => false, 'error' => 'Minimal harus terdapat 1 ruangan aktif.'];
                }
                $data['config']['rooms'] = array_values(array_filter($data['config']['rooms'], function ($r) use ($delRoom) {
                    return $r !== $delRoom;
                }));
                unset($data['config']['room_statuses'][$delRoom]);
                if (($data['config']['room_code'] ?? '') === $delRoom) {
                    $data['config']['room_code'] = $data['config']['rooms'][0];
                }
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_REMOVED', "Ruangan [{$delRoom}] dihapus dari daftar ruangan.", 'INFO');
                return ['ok' => true, 'rooms' => $data['config']['rooms'], 'room_statuses' => $data['config']['room_statuses']];
            });
            respond($result, $result['ok'] ? 200 : 400);
            break;

        case 'set_student_command':
            $studentId = trim($jsonInput['student_id'] ?? '');
            $cmd = strtoupper(trim($jsonInput['command'] ?? ''));

            $result = Storage::update(function (&$data) use ($studentId, $cmd) {
                if (!isset($data['students'][$studentId])) {
                    return ['ok' => false, 'error' => 'Data mahasiswa tidak ditemukan.'];
                }
                $st = &$data['students'][$studentId];
                $stRoom = $st['room_code'] ?? ($data['config']['room_code'] ?? 'KUIS-AI-01');
                if ($cmd === 'REMOVE') {
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'ADMIN_REMOVE', "Data sesi mahasiswa dihapus dari ruangan [{$stRoom}] oleh pengawas.", 'INFO', $stRoom);
                    unset($data['students'][$studentId]);
                    return ['ok' => true];
                }
                if (in_array($cmd, ['LOCKED', 'UNLOCKED'], true)) {
                    $st['target_status'] = $cmd;
                    $st['status'] = $cmd;
                    if (!empty($st['is_simulated'])) {
                        $st['last_seen'] = time();
                    }
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'REMOTE_CMD', "Pengawas mengubah status endpoint menjadi {$cmd} di [{$stRoom}].", 'INFO', $stRoom);
                    return ['ok' => true, 'target_status' => $cmd, 'status' => $cmd];
                }
                return ['ok' => false, 'error' => 'Perintah tidak dikenali.'];
            });

            respond($result);
            break;

        case 'update_config':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                Storage::ensureRoomsConfig($data);
                if (!empty($jsonInput['room_code'])) {
                    $data['config']['room_code'] = Storage::registerRoomIfNeeded($data, $jsonInput['room_code']);
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
            $reqRoom = strtoupper(trim($jsonInput['room_code'] ?? ''));
            $result = Storage::update(function (&$data) use ($reqRoom) {
                Storage::ensureRoomsConfig($data);
                $assignedRoom = ($reqRoom !== '' && $reqRoom !== 'ALL')
                    ? Storage::registerRoomIfNeeded($data, $reqRoom)
                    : ($data['config']['rooms'][array_rand($data['config']['rooms'])] ?? 'KUIS-AI-01');

                $samples = [
                    ['nim' => '240101001', 'name' => 'Budi Santoso', 'host' => 'LAPTOP-BUDI01', 'ext' => ['vscode.builtin-copilot (chat.disableAIFeatures=true)', 'github.copilot-chat-0.22.1', 'saoudrizwan.claude-dev-3.1.0']],
                    ['nim' => '240101002', 'name' => 'Siti Aminah', 'host' => 'PC-SITI-LAB2', 'ext' => ['vscode.builtin-copilot (chat.disableAIFeatures=true)', 'codeium.codeium-1.18.0']],
                    ['nim' => '240101003', 'name' => 'Reza Rahadian', 'host' => 'ROG-STRIX-REZA', 'ext' => ['vscode.builtin-copilot (chat.disableAIFeatures=true)', 'tabnine.tabnine-vscode-3.190.0']]
                ];
                $pick = $samples[array_rand($samples)];
                $id = $pick['nim'];
                $target = $data['config']['room_statuses'][$assignedRoom] ?? ($data['config']['global_status'] ?? 'LOCKED');

                $data['students'][$id] = [
                    'id' => $id,
                    'nim' => $pick['nim'],
                    'name' => $pick['name'],
                    'hostname' => $pick['host'],
                    'room_code' => $assignedRoom,
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

                Storage::addLog($data, $pick['nim'], $pick['name'], $pick['host'], 'EXTENSION_QUARANTINE', 'Mengkarantina ' . count($pick['ext']) . ' ekstensi AI VS Code di [' . $assignedRoom . ']: ' . implode(', ', $pick['ext']), 'WARNING', $assignedRoom);
                Storage::addLog($data, $pick['nim'], $pick['name'], $pick['host'], 'PROCESS_KILLED', 'Menghentikan paksa proses AI terlarang di [' . $assignedRoom . ']: Cursor.exe & ollama.exe (Port 11434)', 'DANGER', $assignedRoom);

                return ['ok' => true, 'simulated' => $pick, 'room_code' => $assignedRoom];
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
