<?php
/**
 * AI-Blocker Guard - REST API Endpoint
 * Melayani komunikasi dari Aplikasi Desktop PC Mahasiswa (.exe) dan Web Dashboard Pengawas.
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization, X-Admin-Token, X-Client-Token');
header('Cache-Control: no-store');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

require_once __DIR__ . '/../includes/Storage.php';

$adminEmail = strtolower(trim(getenv('ADMIN_EMAIL') ?: 'admin@guard.edu'));
$adminPassword = (string)(getenv('ADMIN_PASSWORD') ?: 'admin123');
$masterPin = trim(getenv('MASTER_PIN') ?: '202699');
$authSecret = getenv('AUTH_SECRET') ?: hash('sha256', "guard-auth-php|{$adminPassword}|{$masterPin}");

function signAdminToken($email, $secret) {
    $payload = rtrim(strtr(base64_encode(json_encode(['e' => $email, 'x' => time() + 43200])), '+/', '-_'), '=');
    $sig = rtrim(strtr(base64_encode(hash_hmac('sha256', $payload, $secret, true)), '+/', '-_'), '=');
    return "{$payload}.{$sig}";
}

function verifyAdminToken($token, $secret) {
    if (!$token || strpos($token, '.') === false) return null;
    $parts = explode('.', $token, 2);
    if (count($parts) !== 2) return null;
    list($payload, $sig) = $parts;
    $expectedSig = rtrim(strtr(base64_encode(hash_hmac('sha256', $payload, $secret, true)), '+/', '-_'), '=');
    if (!hash_equals($expectedSig, $sig)) return null;
    $json = base64_decode(strtr($payload, '-_', '+/'));
    $obj = json_decode($json, true);
    if (!$obj || !isset($obj['x']) || time() > $obj['x']) return null;
    return $obj;
}

function getRequestToken() {
    $headers = function_exists('getallheaders') ? getallheaders() : [];
    $auth = $headers['Authorization'] ?? $headers['authorization'] ?? ($_SERVER['HTTP_AUTHORIZATION'] ?? '');
    if (preg_match('/^Bearer\s+(.*)$/i', $auth, $m)) return trim($m[1]);
    return trim($headers['X-Admin-Token'] ?? $headers['x-admin-token'] ?? ($_SERVER['HTTP_X_ADMIN_TOKEN'] ?? ''));
}

function roomPinHash($room, $pin) {
    return hash_pbkdf2('sha1', (string)$pin, "guard-pin:" . strtoupper((string)$room), 20000, 64);
}

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

// Perlindungan Aksi Dashboard Admin
$adminActions = [
    'state', 'dashboard_state', 'rehydrate_state', 'sync_from_firebase',
    'set_global_status', 'add_room', 'rename_room', 'delete_room',
    'set_student_command', 'update_config', 'save_config',
    'update_blocklist', 'save_blocklist', 'clear_logs'
];

if (in_array($action, $adminActions, true) && !verifyAdminToken(getRequestToken(), $authSecret)) {
    respond(['ok' => false, 'auth_required' => true, 'error' => 'Sesi Admin tidak valid atau telah berakhir. Silakan login kembali.'], 401);
}

try {
    switch ($action) {
        case 'admin_login':
            $email = strtolower(trim($jsonInput['email'] ?? ''));
            $password = trim($jsonInput['password'] ?? '');

            if (hash_equals($adminEmail, $email) && hash_equals($adminPassword, $password)) {
                respond([
                    'ok' => true,
                    'token' => signAdminToken($adminEmail, $authSecret),
                    'expires_in' => 43200,
                    'user' => [
                        'email' => $adminEmail,
                        'name' => 'Dosen / Pengawas Ujian',
                        'role' => 'ADMIN'
                    ]
                ]);
            }
            usleep(500000);
            respond(['ok' => false, 'error' => 'Email atau Kata Sandi tidak sesuai.'], 401);
            break;

        case 'sync_from_firebase':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                if (isset($jsonInput['config']) && is_array($jsonInput['config'])) {
                    $data['config'] = array_merge($data['config'] ?? [], $jsonInput['config']);
                }
                if (isset($jsonInput['blocklist']) && is_array($jsonInput['blocklist'])) {
                    $data['blocklist'] = array_merge($data['blocklist'] ?? [], $jsonInput['blocklist']);
                    $data['config']['blocklist_customized'] = true;
                }
                Storage::ensureRoomsConfig($data);
                return ['ok' => true];
            });
            respond($result);
            break;

        // ====================================================================
        // ENDPOINT UNTUK APLIKASI DESKTOP MAHASISWA (CLIENT)
        // ====================================================================
        case 'get_rooms':
            $data = Storage::read();
            Storage::ensureRoomsConfig($data);
            respond([
                'ok' => true,
                'rooms' => $data['config']['rooms'],
                'room_statuses' => $data['config']['room_statuses'],
                'default_room' => $data['config']['room_code'] ?? ($data['config']['rooms'][0] ?? 'KUIS-AI-01')
            ]);
            break;

        case 'join':
        case 'handshake':
            $nim = trim($jsonInput['nim'] ?? '');
            $name = trim($jsonInput['name'] ?? '');
            $hostname = trim($jsonInput['hostname'] ?? gethostname());
            $roomCode = strtoupper(trim($jsonInput['room_code'] ?? ''));
            $osInfo = trim($jsonInput['os_info'] ?? 'Windows');
            $clientVersion = trim($jsonInput['version'] ?? '1.4.0');

            if ($nim === '' || $name === '') {
                respond(['ok' => false, 'error' => 'NIM dan Nama Mahasiswa wajib diisi.'], 400);
            }

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $roomCode, $osInfo, $clientVersion) {
                Storage::ensureRoomsConfig($data);
                if ($roomCode === '' || !in_array($roomCode, $data['config']['rooms'], true)) {
                    return [
                        'ok' => false,
                        'error' => "Kode Ruangan [{$roomCode}] tidak ditemukan. Silakan pilih Ruangan yang tersedia terlebih dahulu.",
                        'rooms' => $data['config']['rooms']
                    ];
                }

                $normalizedRoom = $roomCode;
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $existing = $data['students'][$studentId] ?? null;

                $roomDefaultState = $data['config']['room_statuses'][$normalizedRoom] ?? $data['config']['global_status'] ?? 'LOCKED';
                $desiredState = ($existing && ($existing['room_code'] ?? '') === $normalizedRoom && !empty($existing['target_status']))
                    ? $existing['target_status']
                    : $roomDefaultState;
                $roomPin = (string)($data['config']['room_pins'][$normalizedRoom] ?? '');

                $data['students'][$studentId] = [
                    'id' => $studentId,
                    'nim' => $nim,
                    'name' => $name,
                    'hostname' => $hostname,
                    'room_code' => $normalizedRoom,
                    'ip' => !empty($jsonInput['ip']) ? trim((string)$jsonInput['ip']) : ($_SERVER['REMOTE_ADDR'] ?? '127.0.0.1'),
                    'os_info' => $osInfo,
                    'version' => $clientVersion,
                    'status' => $existing['status'] ?? $desiredState,
                    'target_status' => $desiredState,
                    'quarantined_count' => $existing['quarantined_count'] ?? 0,
                    'quarantined_list' => $existing['quarantined_list'] ?? [],
                    'blocked_domains_count' => count($data['blocklist']['domains'] ?? []),
                    'killed_processes_count' => $existing['killed_processes_count'] ?? 0,
                    'vpn_detected' => false,
                    'pin_fail' => $existing['pin_fail'] ?? 0,
                    'pin_lock_until' => $existing['pin_lock_until'] ?? 0,
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
                    'room_code' => $normalizedRoom,
                    'room_pin_hash' => $roomPin !== '' ? roomPinHash($normalizedRoom, $roomPin) : '',
                    'target_status' => $desiredState,
                    'rooms' => $data['config']['rooms'],
                    'config' => [
                        'room_code' => $normalizedRoom,
                        'room_pin_hash' => $roomPin !== '' ? roomPinHash($normalizedRoom, $roomPin) : '',
                        'target_status' => $desiredState,
                        'block_doh' => $data['config']['block_doh'],
                        'block_vpn' => $data['config']['block_vpn'],
                        'quarantine_extensions' => $data['config']['quarantine_extensions'],
                        'kill_processes' => $data['config']['kill_processes']
                    ],
                    'blocklist' => Storage::getEffectiveBlocklist($data, $normalizedRoom)
                ];
            });

            respond($result, $result['ok'] ? 200 : 400);
            break;

        case 'heartbeat':
            $nim = trim($jsonInput['nim'] ?? '');
            $name = trim($jsonInput['name'] ?? 'Mahasiswa');
            $hostname = trim($jsonInput['hostname'] ?? 'PC');
            $currentStatus = trim($jsonInput['status'] ?? 'LOCKED');
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
                $reqRoom = strtoupper(trim($jsonInput['room_code'] ?? ''));

                if ($reqRoom === '' || !in_array($reqRoom, $data['config']['rooms'], true)) {
                    if (isset($data['students'][$studentId])) {
                        unset($data['students'][$studentId]);
                    }
                    return [
                        'ok' => false,
                        'require_room_selection' => true,
                        'rooms' => $data['config']['rooms'],
                        'target_status' => 'UNLOCKED',
                        'error' => 'Ruangan belum terdaftar atau telah dihapus oleh Admin. Silakan pilih ruangan aktif.'
                    ];
                }

                if (!isset($data['students'][$studentId])) {
                    $data['students'][$studentId] = [
                        'id' => $studentId,
                        'nim' => $nim,
                        'name' => $name,
                        'hostname' => $hostname,
                        'room_code' => $reqRoom,
                        'ip' => !empty($jsonInput['ip']) ? trim((string)$jsonInput['ip']) : ($_SERVER['REMOTE_ADDR'] ?? '127.0.0.1'),
                        'os_info' => 'Windows',
                        'version' => '1.5.0',
                        'target_status' => $data['config']['room_statuses'][$reqRoom] ?? $globalTarget,
                        'joined_at' => date('Y-m-d H:i:s')
                    ];
                }

                $student = &$data['students'][$studentId];
                if ($name !== '') {
                    $student['name'] = $name;
                }
                $prevVpn = $student['vpn_detected'] ?? false;

                $student['status'] = $currentStatus;
                $student['hostname'] = $hostname;
                $student['ip'] = !empty($jsonInput['ip']) ? trim((string)$jsonInput['ip']) : ($student['ip'] ?? ($_SERVER['REMOTE_ADDR'] ?? '127.0.0.1'));
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
                $roomPin = (string)($data['config']['room_pins'][$student['room_code']] ?? '');

                return [
                    'ok' => true,
                    'room_code' => $student['room_code'],
                    'room_pin_hash' => $roomPin !== '' ? roomPinHash($student['room_code'], $roomPin) : '',
                    'target_status' => $targetStatus,
                    'rooms' => $data['config']['rooms'],
                    'config' => [
                        'room_code' => $student['room_code'],
                        'room_pin_hash' => $roomPin !== '' ? roomPinHash($student['room_code'], $roomPin) : '',
                        'block_doh' => $data['config']['block_doh'],
                        'block_vpn' => $data['config']['block_vpn'],
                        'quarantine_extensions' => $data['config']['quarantine_extensions'],
                        'kill_processes' => $data['config']['kill_processes']
                    ],
                    'blocklist' => Storage::getEffectiveBlocklist($data, $student['room_code'])
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
            $reqRoom = strtoupper(trim($jsonInput['room_code'] ?? ''));

            $result = Storage::update(function (&$data) use ($pin, $nim, $name, $hostname, $reqRoom, $masterPin) {
                Storage::ensureRoomsConfig($data);
                $studentId = preg_replace('/[^a-zA-Z0-9_-]/', '_', $nim);
                $stRoom = $reqRoom ?: ($data['students'][$studentId]['room_code'] ?? ($data['config']['room_code'] ?? ''));
                $roomPin = (string)($data['config']['room_pins'][$stRoom] ?? '');

                // Anti brute-force: 5x gagal → lockout 2 menit per PC
                if (isset($data['students'][$studentId]) && ($data['students'][$studentId]['pin_lock_until'] ?? 0) > time()) {
                    $waitSec = (int)($data['students'][$studentId]['pin_lock_until'] - time());
                    return ['ok' => false, 'unlocked' => false, 'error' => "Terlalu banyak percobaan PIN salah. Coba lagi dalam {$waitSec} detik."];
                }

                $pinValid = ($pin !== '' && (($roomPin !== '' && hash_equals($roomPin, $pin)) || ($masterPin !== '' && hash_equals($masterPin, $pin))));
                if ($pinValid) {
                    if (isset($data['students'][$studentId])) {
                        $data['students'][$studentId]['target_status'] = 'UNLOCKED';
                        $data['students'][$studentId]['status'] = 'UNLOCKED';
                        $data['students'][$studentId]['pin_fail'] = 0;
                        $data['students'][$studentId]['pin_lock_until'] = 0;
                    }
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_UNLOCK', "Kunci dibuka / otorisasi keluar menggunakan PIN Ruangan [{$stRoom}] yang valid.", 'INFO', $stRoom);
                    return ['ok' => true, 'unlocked' => true];
                } else {
                    $lockMsg = '';
                    if (isset($data['students'][$studentId])) {
                        $fails = ($data['students'][$studentId]['pin_fail'] ?? 0) + 1;
                        $data['students'][$studentId]['pin_fail'] = $fails;
                        if ($fails >= 5) {
                            $data['students'][$studentId]['pin_lock_until'] = time() + 120;
                            $data['students'][$studentId]['pin_fail'] = 0;
                            $lockMsg = ' PC dikunci dari percobaan PIN selama 2 menit.';
                        }
                    }
                    Storage::addLog($data, $nim, $name, $hostname, 'PIN_FAILED', "Percobaan PIN gagal di ruangan [{$stRoom}].{$lockMsg}", 'WARNING', $stRoom);
                    return ['ok' => false, 'unlocked' => false, 'error' => "PIN Otorisasi untuk ruangan [{$stRoom}] salah!{$lockMsg}"];
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
            $now = time();
            $result = Storage::update(function (&$data) use ($now) {
                Storage::ensureRoomsConfig($data);
                $students = [];
                $onlineCount = 0;
                $lockedCount = 0;
                $violationCount = 0;

                foreach (($data['students'] ?? []) as $id => $st) {
                    $stRoom = strtoupper(trim($st['room_code'] ?? ''));
                    if ($stRoom === '' || !in_array($stRoom, $data['config']['rooms'], true) || !empty($st['is_simulated'])) {
                        unset($data['students'][$id]);
                        continue;
                    }
                    $isOnline = (($now - ($st['last_seen'] ?? 0)) <= 25);
                    $st['is_online'] = $isOnline;
                    $st['seconds_ago'] = max(0, $now - ($st['last_seen'] ?? $now));
                    if ($isOnline) {
                        $onlineCount++;
                    }
                    if (($st['status'] ?? '') === 'LOCKED') {
                        $lockedCount++;
                    }
                    $violationCount += (int)($st['killed_processes_count'] ?? 0);
                    $students[] = $st;
                }

                usort($students, function ($a, $b) {
                    if ($a['is_online'] !== $b['is_online']) {
                        return $a['is_online'] ? -1 : 1;
                    }
                    return strcmp($a['nim'], $b['nim']);
                });

                return [
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
                ];
            });

            respond($result);
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
                    }
                    $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', "Pengawas memilih {$label} untuk SELURUH RUANGAN.", 'INFO', 'ALL');
                } else {
                    if (!in_array($targetRoom, $data['config']['rooms'], true)) {
                        return ['ok' => false, 'error' => 'Ruangan tidak ditemukan.'];
                    }
                    $data['config']['room_statuses'][$targetRoom] = $target;
                    foreach ($data['students'] as $id => &$st) {
                        $stRoom = strtoupper($st['room_code'] ?? '');
                        if ($stRoom === $targetRoom) {
                            $st['target_status'] = $target;
                            $st['status'] = $target;
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
            $customPin = trim($jsonInput['room_pin'] ?? '');
            if ($newRoom === '') {
                respond(['ok' => false, 'error' => 'Kode ruangan wajib diisi.'], 400);
            }
            $result = Storage::update(function (&$data) use ($newRoom, $customPin) {
                Storage::ensureRoomsConfig($data);
                if (in_array($newRoom, $data['config']['rooms'], true)) {
                    return ['ok' => false, 'error' => "Ruangan [{$newRoom}] sudah ada."];
                }
                Storage::registerRoomIfNeeded($data, $newRoom, $customPin);
                $pinAssigned = $data['config']['room_pins'][$newRoom];
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_ADDED', "Ruangan baru [{$newRoom}] ditambahkan (PIN: {$pinAssigned}).", 'INFO', $newRoom);
                return [
                    'ok' => true,
                    'room_code' => $newRoom,
                    'room_pin' => $pinAssigned,
                    'rooms' => $data['config']['rooms'],
                    'room_statuses' => $data['config']['room_statuses'],
                    'room_pins' => $data['config']['room_pins']
                ];
            });
            respond($result, $result['ok'] ? 200 : 400);
            break;

        case 'rename_room':
            $oldRoom = strtoupper(trim($jsonInput['old_room_code'] ?? ''));
            $newRoom = strtoupper(preg_replace('/[^a-zA-Z0-9_-]/', '-', trim($jsonInput['new_room_code'] ?? $oldRoom)));
            $newPin = trim($jsonInput['room_pin'] ?? '');
            if ($oldRoom === '' || $newRoom === '') {
                respond(['ok' => false, 'error' => 'Kode ruangan wajib diisi.'], 400);
            }
            $result = Storage::update(function (&$data) use ($oldRoom, $newRoom, $newPin) {
                Storage::ensureRoomsConfig($data);
                if (!in_array($oldRoom, $data['config']['rooms'], true)) {
                    return ['ok' => false, 'error' => "Ruangan [{$oldRoom}] tidak ditemukan."];
                }
                $oldStatus = $data['config']['room_statuses'][$oldRoom] ?? ($data['config']['global_status'] ?? 'LOCKED');
                $resolvedPin = $newPin !== '' ? $newPin : ($data['config']['room_pins'][$oldRoom] ?? '202601');

                if ($oldRoom !== $newRoom) {
                    $idx = array_search($oldRoom, $data['config']['rooms'], true);
                    if ($idx !== false) {
                        if (in_array($newRoom, $data['config']['rooms'], true)) {
                            array_splice($data['config']['rooms'], $idx, 1);
                        } else {
                            $data['config']['rooms'][$idx] = $newRoom;
                        }
                    }
                    $data['config']['rooms'] = array_values(array_unique($data['config']['rooms']));
                    $data['config']['room_statuses'][$newRoom] = $oldStatus;
                    unset($data['config']['room_statuses'][$oldRoom]);
                    unset($data['config']['room_pins'][$oldRoom]);
                    if (isset($data['config']['room_blocklists'][$oldRoom])) {
                        $data['config']['room_blocklists'][$newRoom] = $data['config']['room_blocklists'][$oldRoom];
                        unset($data['config']['room_blocklists'][$oldRoom]);
                    }
                    if (($data['config']['room_code'] ?? '') === $oldRoom) {
                        $data['config']['room_code'] = $newRoom;
                    }
                    foreach ($data['students'] as &$st) {
                        if (strtoupper($st['room_code'] ?? '') === $oldRoom) {
                            $st['room_code'] = $newRoom;
                        }
                    }
                    unset($st);
                }

                $data['config']['room_pins'][$newRoom] = $resolvedPin;
                $data['config']['updated_at'] = date('c');
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_UPDATED', "Ruangan [{$oldRoom}] diperbarui menjadi [{$newRoom}] (PIN: {$resolvedPin}).", 'INFO', $newRoom);
                return [
                    'ok' => true,
                    'room_code' => $newRoom,
                    'room_pin' => $resolvedPin,
                    'rooms' => $data['config']['rooms'],
                    'room_statuses' => $data['config']['room_statuses'],
                    'room_pins' => $data['config']['room_pins'],
                    'room_blocklists' => $data['config']['room_blocklists'] ?? (object)[]
                ];
            });
            respond($result, $result['ok'] ? 200 : 400);
            break;

        case 'delete_room':
            $delRoom = strtoupper(trim($jsonInput['room_code'] ?? ''));
            $result = Storage::update(function (&$data) use ($delRoom) {
                Storage::ensureRoomsConfig($data);
                if ($delRoom === '') {
                    return ['ok' => false, 'error' => 'Kode ruangan tidak valid.'];
                }
                if (!isset($data['config']['deleted_rooms']) || !is_array($data['config']['deleted_rooms'])) {
                    $data['config']['deleted_rooms'] = [];
                }
                if (!in_array($delRoom, $data['config']['deleted_rooms'], true)) {
                    $data['config']['deleted_rooms'][] = $delRoom;
                }
                $data['config']['rooms'] = array_values(array_filter($data['config']['rooms'], function ($r) use ($delRoom) {
                    return $r !== $delRoom;
                }));
                unset($data['config']['room_statuses'][$delRoom]);
                unset($data['config']['room_pins'][$delRoom]);
                if (isset($data['config']['room_blocklists'][$delRoom])) {
                    unset($data['config']['room_blocklists'][$delRoom]);
                }
                if (($data['config']['room_code'] ?? '') === $delRoom) {
                    $data['config']['room_code'] = $data['config']['rooms'][0] ?? '';
                }
                foreach ($data['students'] as $id => $st) {
                    if (strtoupper($st['room_code'] ?? '') === $delRoom) {
                        unset($data['students'][$id]);
                    }
                }
                $data['config']['updated_at'] = date('c');
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_REMOVED', "Ruangan [{$delRoom}] beserta sesi di dalamnya dihapus.", 'INFO');
                return [
                    'ok' => true,
                    'rooms' => $data['config']['rooms'],
                    'room_statuses' => $data['config']['room_statuses'],
                    'room_pins' => $data['config']['room_pins'],
                    'room_blocklists' => $data['config']['room_blocklists'] ?? (object)[]
                ];
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
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'REMOTE_CMD', "Pengawas mengubah status endpoint menjadi {$cmd} di [{$stRoom}].", 'INFO', $stRoom);
                    return ['ok' => true, 'target_status' => $cmd, 'status' => $cmd];
                }
                return ['ok' => false, 'error' => 'Perintah tidak dikenali.'];
            });

            respond($result);
            break;

        case 'update_config':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                if (!empty($jsonInput['room_code'])) {
                    $data['config']['room_code'] = Storage::registerRoomIfNeeded($data, $jsonInput['room_code']);
                }
                $data['config']['block_doh'] = isset($jsonInput['block_doh']) ? (bool)$jsonInput['block_doh'] : true;
                $data['config']['block_vpn'] = isset($jsonInput['block_vpn']) ? (bool)$jsonInput['block_vpn'] : true;
                $data['config']['quarantine_extensions'] = isset($jsonInput['quarantine_extensions']) ? (bool)$jsonInput['quarantine_extensions'] : true;
                $data['config']['kill_processes'] = isset($jsonInput['kill_processes']) ? (bool)$jsonInput['kill_processes'] : true;
                $data['config']['updated_at'] = date('c');
                Storage::ensureRoomsConfig($data);

                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                return ['ok' => true, 'config' => $data['config']];
            });
            respond($result);
            break;

        case 'update_blocklist':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                Storage::ensureRoomsConfig($data);
                $targetRoom = strtoupper(trim($jsonInput['room_code'] ?? 'ALL'));
                $resetRoom = !empty($jsonInput['reset_room']);

                $domains = isset($jsonInput['domains']) && is_array($jsonInput['domains'])
                    ? array_values(array_unique(array_filter(array_map('strtolower', array_map('trim', $jsonInput['domains'])))))
                    : ($data['blocklist']['domains'] ?? []);
                $procs = isset($jsonInput['processes']) && is_array($jsonInput['processes'])
                    ? array_values(array_unique(array_filter(array_map('trim', $jsonInput['processes']))))
                    : ($data['blocklist']['processes'] ?? []);
                $exts = isset($jsonInput['ide_extensions']) && is_array($jsonInput['ide_extensions'])
                    ? array_values(array_unique(array_filter(array_map('trim', $jsonInput['ide_extensions']))))
                    : ($data['blocklist']['ide_extensions'] ?? []);

                if ($targetRoom !== '' && $targetRoom !== 'ALL') {
                    if ($resetRoom) {
                        unset($data['config']['room_blocklists'][$targetRoom]);
                        Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_RESET', "Kebijakan blokir khusus ruangan [{$targetRoom}] dikembalikan ke Kebijakan Global.", 'INFO', $targetRoom);
                    } else {
                        $data['config']['room_blocklists'][$targetRoom] = [
                            'domains' => $domains,
                            'processes' => $procs,
                            'ide_extensions' => $exts,
                            'updated_at' => date('c')
                        ];
                        Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_BLOCKLIST_UPDATE', "Kebijakan blokir khusus ruangan [{$targetRoom}] diperbarui.", 'INFO', $targetRoom);
                    }
                } else {
                    $data['config']['blocklist_customized'] = true;
                    $data['blocklist']['domains'] = $domains;
                    $data['blocklist']['processes'] = $procs;
                    $data['blocklist']['ide_extensions'] = $exts;
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'BLOCKLIST_UPDATE', 'Kebijakan blokir Global (Semua Ruangan) telah diperbarui.', 'INFO', 'ALL');
                }
                return [
                    'ok' => true,
                    'room_code' => $targetRoom ?: 'ALL',
                    'blocklist' => $data['blocklist'],
                    'room_blocklists' => $data['config']['room_blocklists'],
                    'effective_blocklist' => Storage::getEffectiveBlocklist($data, $targetRoom)
                ];
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
