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

function findMatchingStudentKey(&$data, $studentId, $upperHost, $clientId, $clientIp) {
    $students = $data['students'] ?? [];
    if ($studentId !== '' && isset($students[$studentId])) {
        return $studentId;
    }
    if ($upperHost !== '') {
        foreach ($students as $id => $st) {
            if (strtoupper(trim((string)($st['hostname'] ?? ''))) === $upperHost) {
                return $id;
            }
        }
    }
    if ($clientId !== '') {
        foreach ($students as $id => $st) {
            if (trim((string)($st['client_id'] ?? '')) === $clientId) {
                return $id;
            }
        }
    }
    if ($clientIp !== '' && $clientIp !== '127.0.0.1' && $clientIp !== '::1') {
        $ipMatches = [];
        foreach ($students as $id => $st) {
            if (trim((string)($st['ip'] ?? '')) === $clientIp) {
                $ipMatches[] = $id;
            }
        }
        if (count($ipMatches) === 1) {
            return $ipMatches[0];
        }
    }
    return null;
}

function resolveDeviceIdeLocks(&$data, $params, $clientIp) {
    $globLocked = ($data['config']['global_status'] ?? 'LOCKED') === 'LOCKED';
    $globalAi = isset($data['config']['lock_ai']) ? (bool)$data['config']['lock_ai'] : $globLocked;
    $globalExt = isset($data['config']['lock_extensions']) ? (bool)$data['config']['lock_extensions'] : $globLocked;

    $studentId = trim((string)($params['student_id'] ?? ''));
    $upperHost = strtoupper(trim((string)($params['hostname'] ?? '')));
    $clientId = trim((string)($params['client_id'] ?? ''));

    $matchedKey = findMatchingStudentKey($data, $studentId, $upperHost, $clientId, $clientIp);
    $matchedStudent = $matchedKey !== null ? $data['students'][$matchedKey] : null;
    $resolvedHost = $upperHost ?: strtoupper(trim((string)($matchedStudent['hostname'] ?? '')));

    $pco = $data['config']['pc_overrides'] ?? [];
    $override = null;
    foreach ([$matchedKey, $studentId, $resolvedHost, $clientId] as $cand) {
        if ($cand !== null && $cand !== '' && isset($pco[$cand]) && is_array($pco[$cand])) {
            $override = $pco[$cand];
            break;
        }
    }

    $lockAi = $globalAi;
    if ($matchedStudent && array_key_exists('lock_ai', $matchedStudent) && is_bool($matchedStudent['lock_ai'])) {
        $lockAi = $matchedStudent['lock_ai'];
    } elseif ($override && array_key_exists('lock_ai', $override) && is_bool($override['lock_ai'])) {
        $lockAi = $override['lock_ai'];
    }

    $lockExt = $globalExt;
    if ($matchedStudent && array_key_exists('lock_extensions', $matchedStudent) && is_bool($matchedStudent['lock_extensions'])) {
        $lockExt = $matchedStudent['lock_extensions'];
    } elseif ($override && array_key_exists('lock_extensions', $override) && is_bool($override['lock_extensions'])) {
        $lockExt = $override['lock_extensions'];
    }

    $hostKnown = false;
    foreach (($data['config']['ide_hosts'] ?? []) as $ih) {
        if (is_array($ih) && strtoupper(trim((string)($ih['h'] ?? ''))) === $upperHost && $upperHost !== '') {
            $hostKnown = true;
            break;
        }
    }

    return [
        'lock_ai' => $lockAi,
        'lock_extensions' => $lockExt,
        'global_lock_ai' => $globalAi,
        'global_lock_extensions' => $globalExt,
        'device_known' => ($matchedStudent !== null || $hostKnown),
        'matched_student_id' => $matchedKey,
        'matched_hostname' => $resolvedHost
    ];
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

function sanitizeConfigForDashboard($config, $defaultAdminEmail) {
    $out = $config;
    $out['has_custom_admin_password'] = !empty($out['admin_password_hash']);
    $out['admin_email_effective'] = !empty($out['admin_email']) ? $out['admin_email'] : $defaultAdminEmail;
    unset($out['admin_password_hash'], $out['admin_password']);
    if (empty($out['pc_overrides'])) {
        $out['pc_overrides'] = (object)[];
    }
    if (empty($out['deleted_students'])) {
        $out['deleted_students'] = (object)[];
    }
    return $out;
}

// Perlindungan Aksi Dashboard Admin
$adminActions = [
    'state', 'dashboard_state', 'rehydrate_state', 'sync_from_firebase',
    'set_global_status', 'set_ide_module_status', 'set_ide_password', 'set_admin_password', 'add_room', 'rename_room', 'delete_room',
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

            $authData = Storage::read();
            Storage::ensureRoomsConfig($authData);
            $effectiveAdminEmail = !empty($authData['config']['admin_email'])
                ? strtolower(trim((string)$authData['config']['admin_email']))
                : $adminEmail;
            $customPassHash = trim((string)($authData['config']['admin_password_hash'] ?? ''));
            $effectivePassHash = $customPassHash !== '' ? $customPassHash : hash('sha256', $adminPassword);
            $inputPassHash = hash('sha256', $password);

            if ($email !== '' && $password !== '' && hash_equals($effectiveAdminEmail, $email) && hash_equals($effectivePassHash, $inputPassHash)) {
                respond([
                    'ok' => true,
                    'token' => signAdminToken($effectiveAdminEmail, $authSecret),
                    'expires_in' => 43200,
                    'user' => [
                        'email' => $effectiveAdminEmail,
                        'name' => 'Dosen / Pengawas Ujian',
                        'role' => 'ADMIN'
                    ]
                ]);
            }
            usleep(500000);
            respond(['ok' => false, 'error' => 'Email atau Kata Sandi tidak sesuai.'], 401);
            break;

        case 'rehydrate_state':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                Storage::ensureRoomsConfig($data);
                if (!empty($jsonInput['students']) && is_array($jsonInput['students'])) {
                    $delMap = $data['config']['deleted_students'] ?? [];
                    foreach ($jsonInput['students'] as $st) {
                        $sid = (string)($st['id'] ?? '');
                        $stRoom = strtoupper(trim((string)($st['room_code'] ?? '')));
                        if ($sid !== '' && $stRoom !== '' && in_array($stRoom, $data['config']['rooms'], true) && !isset($data['students'][$sid])) {
                            $delSec = (int)($delMap[$sid] ?? 0);
                            if ($delSec <= 0 || ((int)($st['last_seen'] ?? 0) > $delSec)) {
                                $data['students'][$sid] = $st;
                            }
                        }
                    }
                }
                return ['ok' => true];
            });
            respond($result);
            break;

        case 'sync_from_firebase':
            $result = Storage::update(function (&$data) use ($jsonInput) {
                if (isset($jsonInput['config']) && is_array($jsonInput['config'])) {
                    $incomingCfg = $jsonInput['config'];
                    unset($incomingCfg['admin_password_hash'], $incomingCfg['admin_password'], $incomingCfg['admin_email'], $incomingCfg['has_custom_admin_password'], $incomingCfg['admin_email_effective']);
                    $data['config'] = array_merge($data['config'] ?? [], $incomingCfg);
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

            $result = Storage::update(function (&$data) use ($nim, $name, $hostname, $roomCode, $osInfo, $clientVersion, $jsonInput) {
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

                $nowSec = time();
                $effectiveNow = $nowSec;
                if (!empty($data['config']['deleted_students'][$studentId])) {
                    $prevDel = (int)$data['config']['deleted_students'][$studentId];
                    $effectiveNow = max($nowSec, $prevDel + 1);
                    unset($data['config']['deleted_students'][$studentId]);
                    $data['config']['updated_at'] = date('c');
                }
                $upperHost = strtoupper(trim((string)$hostname));
                $hostOverride = ($upperHost !== '' && isset($data['config']['pc_overrides'][$upperHost]) && is_array($data['config']['pc_overrides'][$upperHost]))
                    ? $data['config']['pc_overrides'][$upperHost]
                    : null;

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
                    'lock_ai' => $existing['lock_ai'] ?? ($hostOverride['lock_ai'] ?? null),
                    'lock_extensions' => $existing['lock_extensions'] ?? ($hostOverride['lock_extensions'] ?? null),
                    'quarantined_count' => $existing['quarantined_count'] ?? 0,
                    'quarantined_list' => $existing['quarantined_list'] ?? [],
                    'blocked_domains_count' => count($data['blocklist']['domains'] ?? []),
                    'killed_processes_count' => $existing['killed_processes_count'] ?? 0,
                    'vpn_detected' => false,
                    'pin_fail' => $existing['pin_fail'] ?? 0,
                    'pin_lock_until' => $existing['pin_lock_until'] ?? 0,
                    'joined_at' => $existing['joined_at'] ?? date('Y-m-d H:i:s'),
                    'last_seen' => $effectiveNow,
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
                $nowSec = time();
                $effectiveNow = $nowSec;
                if (!empty($data['config']['deleted_students'][$studentId])) {
                    $prevDel = (int)$data['config']['deleted_students'][$studentId];
                    $effectiveNow = max($nowSec, $prevDel + 1);
                    unset($data['config']['deleted_students'][$studentId]);
                    $data['config']['updated_at'] = date('c');
                }

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

                $upperHost = strtoupper(trim((string)$hostname));
                $hostOverride = ($upperHost !== '' && isset($data['config']['pc_overrides'][$upperHost]) && is_array($data['config']['pc_overrides'][$upperHost]))
                    ? $data['config']['pc_overrides'][$upperHost]
                    : null;

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
                        'lock_ai' => $hostOverride['lock_ai'] ?? null,
                        'lock_extensions' => $hostOverride['lock_extensions'] ?? null,
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
                $student['last_seen'] = $effectiveNow;
                $student['last_seen_str'] = date('Y-m-d H:i:s');

                if ($vpnDetected && !$prevVpn) {
                    Storage::addLog($data, $nim, $name, $hostname, 'VPN_ALERT', "Terdeteksi jaringan VPN / Tunnel aktif pada PC mahasiswa di [{$student['room_code']}]!", 'DANGER', $student['room_code']);
                }

                $targetStatus = $student['target_status'] ?? ($data['config']['room_statuses'][$student['room_code']] ?? $globalTarget);
                $roomPin = (string)($data['config']['room_pins'][$student['room_code']] ?? '');
                $effLockAi = (array_key_exists('lock_ai', $student) && is_bool($student['lock_ai']))
                    ? $student['lock_ai']
                    : (($hostOverride && array_key_exists('lock_ai', $hostOverride) && is_bool($hostOverride['lock_ai']))
                        ? $hostOverride['lock_ai']
                        : ($data['config']['lock_ai'] ?? $data['config']['kill_processes'] ?? true));
                $effLockExt = (array_key_exists('lock_extensions', $student) && is_bool($student['lock_extensions']))
                    ? $student['lock_extensions']
                    : (($hostOverride && array_key_exists('lock_extensions', $hostOverride) && is_bool($hostOverride['lock_extensions']))
                        ? $hostOverride['lock_extensions']
                        : ($data['config']['lock_extensions'] ?? $data['config']['quarantine_extensions'] ?? true));

                $resPayload = [
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
                        'quarantine_extensions' => $effLockExt,
                        'kill_processes' => $effLockAi,
                        'lock_ai' => $effLockAi,
                        'lock_extensions' => $effLockExt
                    ],
                    'blocklist' => Storage::getEffectiveBlocklist($data, $student['room_code'])
                ];

                if (!empty($student['pending_command'])) {
                    $resPayload['command'] = $student['pending_command'];
                    unset($student['pending_command']);
                }

                return $resPayload;
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

                $anyRoomPinValid = ($pin !== '' && $nim === 'VSCODE_USER' && $reqRoom === '' && !empty($data['config']['room_pins']) && count(array_filter($data['config']['room_pins'], function ($rp) use ($pin) {
                    return $rp !== '' && hash_equals((string)$rp, $pin);
                })) > 0);
                $idePassHash = (string)($data['config']['ide_password_hash'] ?? '');
                $idePassValid = ($pin !== '' && $nim === 'VSCODE_USER' && $idePassHash !== '' && hash_equals($idePassHash, hash('sha256', $pin)));
                $pinValid = ($pin !== '' && (($roomPin !== '' && hash_equals($roomPin, $pin)) || $anyRoomPinValid || $idePassValid || ($masterPin !== '' && hash_equals($masterPin, $pin))));
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

        case 'ide_register':
            $rawHost = trim((string)($jsonInput['hostname'] ?? $_GET['hostname'] ?? ''));
            $upperHost = strtoupper($rawHost);
            $clientId = trim((string)($jsonInput['client_id'] ?? $_GET['client_id'] ?? ''));
            $username = trim((string)($jsonInput['username'] ?? $_GET['username'] ?? 'VS Code User'));
            if ($upperHost === '') {
                respond(['ok' => false, 'error' => 'Hostname diperlukan.'], 400);
            }
            $clientIp = $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1';
            $result = Storage::update(function (&$data) use ($upperHost, $clientId, $username, $clientIp) {
                Storage::ensureRoomsConfig($data);
                $now = time();
                $matchedKey = findMatchingStudentKey($data, '', $upperHost, $clientId, '');
                if ($matchedKey !== null) {
                    $effectiveNow = $now;
                    if (!empty($data['config']['deleted_students'][$matchedKey])) {
                        $prevDel = (int)$data['config']['deleted_students'][$matchedKey];
                        $effectiveNow = max($now, $prevDel + 1);
                        unset($data['config']['deleted_students'][$matchedKey]);
                        $data['config']['updated_at'] = date('c');
                    }
                    $data['students'][$matchedKey]['ide_connected'] = true;
                    $data['students'][$matchedKey]['ide_last_seen'] = $effectiveNow;
                    $data['students'][$matchedKey]['last_seen'] = $effectiveNow;
                    $data['students'][$matchedKey]['last_seen_str'] = date('Y-m-d H:i:s');
                    if ($clientId !== '') {
                        $data['students'][$matchedKey]['client_id'] = $clientId;
                    }
                    return ['ok' => true, 'student_id' => $matchedKey, 'mode' => 'existing_student'];
                }

                if (!empty($data['config']['rooms'])) {
                    $targetRoom = $data['config']['room_code'] ?: $data['config']['rooms'][0];
                    $safeId = 'IDE_' . preg_replace('/[^A-Z0-9_-]/', '_', $upperHost);
                    $effectiveNow = $now;
                    if (!empty($data['config']['deleted_students'][$safeId])) {
                        $prevDel = (int)$data['config']['deleted_students'][$safeId];
                        $effectiveNow = max($now, $prevDel + 1);
                        unset($data['config']['deleted_students'][$safeId]);
                        $data['config']['updated_at'] = date('c');
                    }
                    $roomStatus = $data['config']['room_statuses'][$targetRoom] ?? ($data['config']['global_status'] ?? 'LOCKED');
                    $hostOverride = isset($data['config']['pc_overrides'][$upperHost]) && is_array($data['config']['pc_overrides'][$upperHost])
                        ? $data['config']['pc_overrides'][$upperHost]
                        : null;
                    $existing = $data['students'][$safeId] ?? [];
                    $data['students'][$safeId] = array_merge([
                        'id' => $safeId,
                        'nim' => 'VS CODE',
                        'name' => $username ?: $upperHost,
                        'hostname' => $upperHost,
                        'client_id' => $clientId,
                        'room_code' => $targetRoom,
                        'ip' => $clientIp,
                        'os_info' => 'VS Code IDE',
                        'version' => 'IDE-Sync',
                        'status' => $existing['status'] ?? $roomStatus,
                        'target_status' => $existing['target_status'] ?? $roomStatus,
                        'lock_ai' => $existing['lock_ai'] ?? ($hostOverride['lock_ai'] ?? null),
                        'lock_extensions' => $existing['lock_extensions'] ?? ($hostOverride['lock_extensions'] ?? null),
                        'quarantined_count' => $existing['quarantined_count'] ?? 0,
                        'quarantined_list' => $existing['quarantined_list'] ?? [],
                        'killed_processes_count' => $existing['killed_processes_count'] ?? 0,
                        'vpn_detected' => false,
                        'joined_at' => $existing['joined_at'] ?? date('Y-m-d H:i:s'),
                    ], [
                        'ide_connected' => true,
                        'ide_last_seen' => $effectiveNow,
                        'last_seen' => $effectiveNow,
                        'last_seen_str' => date('Y-m-d H:i:s')
                    ]);
                    return ['ok' => true, 'student_id' => $safeId, 'mode' => 'ide_student'];
                }

                $hosts = [];
                $found = false;
                foreach (($data['config']['ide_hosts'] ?? []) as $ih) {
                    if (is_array($ih) && !empty($ih['h'])) {
                        if (strtoupper($ih['h']) === $upperHost) {
                            $hosts[] = ['h' => $upperHost, 'id' => $clientId, 'ip' => $clientIp, 'ls' => $now];
                            $found = true;
                        } else {
                            $hosts[] = $ih;
                        }
                    }
                }
                if (!$found) {
                    array_unshift($hosts, ['h' => $upperHost, 'id' => $clientId, 'ip' => $clientIp, 'ls' => $now]);
                }
                $data['config']['ide_hosts'] = array_slice($hosts, 0, 20);
                return ['ok' => true, 'hostname' => $upperHost, 'mode' => 'standalone_host'];
            });
            respond($result);
            break;

        case 'blocklist':
            $data = Storage::read();
            Storage::ensureRoomsConfig($data);
            $clientIp = $_SERVER['REMOTE_ADDR'] ?? '127.0.0.1';
            $params = [
                'hostname' => $_GET['hostname'] ?? ($jsonInput['hostname'] ?? ''),
                'student_id' => $_GET['student_id'] ?? ($jsonInput['student_id'] ?? ''),
                'client_id' => $_GET['client_id'] ?? ($jsonInput['client_id'] ?? ''),
            ];
            $resolved = resolveDeviceIdeLocks($data, $params, $clientIp);
            respond([
                'ok' => true,
                'device_known' => $resolved['device_known'],
                'matched_student_id' => $resolved['matched_student_id'],
                'matched_hostname' => $resolved['matched_hostname'],
                'config' => [
                    'room_code' => $data['config']['room_code'],
                    'rooms' => $data['config']['rooms'],
                    'global_status' => $data['config']['global_status'],
                    'lock_ai' => $resolved['lock_ai'],
                    'lock_extensions' => $resolved['lock_extensions'],
                    'global_lock_ai' => $resolved['global_lock_ai'],
                    'global_lock_extensions' => $resolved['global_lock_extensions'],
                    'ide_password_hash' => (string)($data['config']['ide_password_hash'] ?? ''),
                    'quarantine_extensions' => $data['config']['quarantine_extensions'] ?? true,
                    'kill_processes' => $data['config']['kill_processes'] ?? true,
                ],
                'blocklist' => $data['blocklist']
            ]);
            break;

        // ====================================================================
        // ENDPOINT UNTUK WEB DASHBOARD PENGAWAS / DOSEN
        // ====================================================================
        case 'dashboard_state':
            $now = time();
            $result = Storage::update(function (&$data) use ($now, $adminEmail) {
                Storage::ensureRoomsConfig($data);
                $students = [];
                $onlineCount = 0;
                $lockedCount = 0;
                $violationCount = 0;

                $delMap = $data['config']['deleted_students'] ?? [];
                foreach (($data['students'] ?? []) as $id => $st) {
                    $delSec = (int)($delMap[$id] ?? 0);
                    $stRoom = strtoupper(trim($st['room_code'] ?? ''));
                    if (($delSec > 0 && ((int)($st['last_seen'] ?? 0) <= $delSec)) || $stRoom === '' || !in_array($stRoom, $data['config']['rooms'], true) || !empty($st['is_simulated'])) {
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

                $outConfig = sanitizeConfigForDashboard($data['config'], $adminEmail);
                return [
                    'ok' => true,
                    'server_time' => date('Y-m-d H:i:s'),
                    'config' => $outConfig,
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
                    $isGlobLocked = ($target === 'LOCKED');
                    $data['config']['global_status'] = $target;
                    $data['config']['lock_ai'] = $isGlobLocked;
                    $data['config']['lock_extensions'] = $isGlobLocked;
                    $data['config']['quarantine_extensions'] = $isGlobLocked;
                    $data['config']['kill_processes'] = $isGlobLocked;
                    $data['config']['pc_overrides'] = [];
                    foreach ($data['config']['rooms'] as $r) {
                        $data['config']['room_statuses'][$r] = $target;
                    }
                    foreach ($data['students'] as $id => &$st) {
                        $st['target_status'] = $target;
                        $st['status'] = $target;
                        $st['lock_ai'] = null;
                        $st['lock_extensions'] = null;
                    }
                    unset($st);
                    $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'GLOBAL_COMMAND', "Pengawas memilih {$label} untuk SELURUH RUANGAN.", 'INFO', 'ALL');
                } else {
                    if (!in_array($targetRoom, $data['config']['rooms'], true)) {
                        return ['ok' => false, 'error' => 'Ruangan tidak ditemukan.'];
                    }
                    $isRoomLocked = ($target === 'LOCKED');
                    $data['config']['room_statuses'][$targetRoom] = $target;
                    foreach ($data['students'] as $id => &$st) {
                        $stRoom = strtoupper($st['room_code'] ?? '');
                        if ($stRoom === $targetRoom) {
                            $st['target_status'] = $target;
                            $st['status'] = $target;
                            $st['lock_ai'] = $isRoomLocked;
                            $st['lock_extensions'] = $isRoomLocked;
                            $hKey = strtoupper(trim((string)($st['hostname'] ?? '')));
                            if ($hKey !== '') {
                                $data['config']['pc_overrides'][$hKey] = [
                                    'lock_ai' => $isRoomLocked,
                                    'lock_extensions' => $isRoomLocked
                                ];
                            }
                        }
                    }
                    unset($st);
                    $label = $target === 'LOCKED' ? 'MENGUNCI (Aktifkan Blokir AI)' : 'MEMBUKA KUNCI (Pulihkan PC)';
                    Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ROOM_COMMAND', "Pengawas memilih {$label} untuk ruangan [{$targetRoom}].", 'INFO', $targetRoom);
                }

                return [
                    'ok' => true,
                    'global_status' => $data['config']['global_status'],
                    'lock_ai' => $data['config']['lock_ai'] ?? ($data['config']['global_status'] === 'LOCKED'),
                    'lock_extensions' => $data['config']['lock_extensions'] ?? ($data['config']['global_status'] === 'LOCKED'),
                    'room_statuses' => $data['config']['room_statuses']
                ];
            });

            respond($result);
            break;

        case 'set_ide_module_status':
            $module = strtoupper(trim($jsonInput['module'] ?? 'ALL')); // 'AI' | 'EXT' | 'ALL' | 'REMOVE'
            $status = strtoupper(trim($jsonInput['status'] ?? 'LOCKED')); // 'LOCKED' | 'UNLOCKED' | 'INHERIT'
            $targetStudentId = trim((string)($jsonInput['student_id'] ?? ''));
            $targetHost = strtoupper(trim((string)($jsonInput['hostname'] ?? '')));

            if (!in_array($module, ['AI', 'EXT', 'ALL', 'REMOVE'], true) || !in_array($status, ['LOCKED', 'UNLOCKED', 'INHERIT'], true)) {
                respond(['ok' => false, 'error' => 'Parameter module atau status tidak valid.'], 400);
            }

            $result = Storage::update(function (&$data) use ($module, $status, $targetStudentId, $targetHost) {
                Storage::ensureRoomsConfig($data);
                if (!isset($data['config']['pc_overrides']) || !is_array($data['config']['pc_overrides'])) {
                    $data['config']['pc_overrides'] = [];
                }

                if ($targetStudentId !== '' || $targetHost !== '') {
                    $matchedKey = findMatchingStudentKey($data, $targetStudentId, $targetHost, '', '');
                    $resolvedHost = $targetHost;
                    if ($matchedKey !== null && $resolvedHost === '') {
                        $resolvedHost = strtoupper(trim((string)($data['students'][$matchedKey]['hostname'] ?? '')));
                    }
                    $overrideKey = $resolvedHost !== '' ? $resolvedHost : $targetStudentId;

                    if ($module === 'REMOVE') {
                        unset($data['config']['pc_overrides'][$overrideKey]);
                        if ($targetStudentId !== '') {
                            unset($data['config']['pc_overrides'][$targetStudentId]);
                        }
                        if ($matchedKey !== null) {
                            $data['students'][$matchedKey]['lock_ai'] = null;
                            $data['students'][$matchedKey]['lock_extensions'] = null;
                        }
                        if ($resolvedHost !== '') {
                            $data['config']['ide_hosts'] = array_values(array_filter($data['config']['ide_hosts'] ?? [], function ($ih) use ($resolvedHost) {
                                return strtoupper(trim((string)($ih['h'] ?? ''))) !== $resolvedHost;
                            }));
                        }
                        $data['config']['updated_at'] = date('c');
                        return [
                            'ok' => true,
                            'pc_overrides' => empty($data['config']['pc_overrides']) ? (object)[] : $data['config']['pc_overrides'],
                            'ide_hosts' => $data['config']['ide_hosts']
                        ];
                    }

                    $val = ($status === 'INHERIT') ? null : ($status === 'LOCKED');
                    $cur = isset($data['config']['pc_overrides'][$overrideKey]) && is_array($data['config']['pc_overrides'][$overrideKey])
                        ? $data['config']['pc_overrides'][$overrideKey]
                        : ['lock_ai' => null, 'lock_extensions' => null];
                    if ($module === 'AI' || $module === 'ALL') {
                        $cur['lock_ai'] = $val;
                    }
                    if ($module === 'EXT' || $module === 'ALL') {
                        $cur['lock_extensions'] = $val;
                    }
                    if (($cur['lock_ai'] ?? null) === null && ($cur['lock_extensions'] ?? null) === null) {
                        unset($data['config']['pc_overrides'][$overrideKey]);
                    } else {
                        $data['config']['pc_overrides'][$overrideKey] = $cur;
                    }

                    foreach ($data['students'] as $sid => &$stItem) {
                        $stHost = strtoupper(trim((string)($stItem['hostname'] ?? '')));
                        if (($targetStudentId !== '' && $sid === $targetStudentId) || ($resolvedHost !== '' && $stHost === $resolvedHost)) {
                            if ($module === 'AI' || $module === 'ALL') {
                                $stItem['lock_ai'] = $val;
                            }
                            if ($module === 'EXT' || $module === 'ALL') {
                                $stItem['lock_extensions'] = $val;
                            }
                        }
                    }
                    unset($stItem);

                    $data['config']['updated_at'] = date('c');
                    $modLabel = $module === 'AI' ? 'AI Chat / Agent' : ($module === 'EXT' ? 'Ekstensi IDE' : 'Semua Modul');
                    $pcLabel = $overrideKey ?: $targetStudentId;
                    Storage::addLog($data, $targetStudentId ?: 'ADMIN', 'Dosen Pengawas', $pcLabel, 'MODULE_STATUS', "{$modLabel} pada PC [{$pcLabel}] diatur ke {$status}.", 'INFO');
                    return [
                        'ok' => true,
                        'student_id' => $matchedKey ?? $targetStudentId,
                        'hostname' => $resolvedHost,
                        'lock_ai' => $cur['lock_ai'] ?? null,
                        'lock_extensions' => $cur['lock_extensions'] ?? null,
                        'pc_overrides' => empty($data['config']['pc_overrides']) ? (object)[] : $data['config']['pc_overrides']
                    ];
                }

                if ($status === 'INHERIT') {
                    return ['ok' => false, 'error' => 'Status INHERIT hanya berlaku per komputer.'];
                }
                $isLocked = ($status === 'LOCKED');
                if ($module === 'AI' || $module === 'ALL') {
                    $data['config']['lock_ai'] = $isLocked;
                    $data['config']['kill_processes'] = $isLocked;
                }
                if ($module === 'EXT' || $module === 'ALL') {
                    $data['config']['lock_extensions'] = $isLocked;
                    $data['config']['quarantine_extensions'] = $isLocked;
                }
                if ($module === 'ALL') {
                    $data['config']['global_status'] = $status;
                }

                foreach ($data['students'] as &$stItem) {
                    if ($module === 'AI' || $module === 'ALL') {
                        $stItem['lock_ai'] = null;
                    }
                    if ($module === 'EXT' || $module === 'ALL') {
                        $stItem['lock_extensions'] = null;
                    }
                }
                unset($stItem);
                foreach ($data['config']['pc_overrides'] as $k => &$ov) {
                    if ($module === 'AI' || $module === 'ALL') {
                        $ov['lock_ai'] = null;
                    }
                    if ($module === 'EXT' || $module === 'ALL') {
                        $ov['lock_extensions'] = null;
                    }
                    if (($ov['lock_ai'] ?? null) === null && ($ov['lock_extensions'] ?? null) === null) {
                        unset($data['config']['pc_overrides'][$k]);
                    }
                }
                unset($ov);

                $data['config']['updated_at'] = date('c');
                $modLabel = $module === 'AI' ? 'AI Chat / Agent' : ($module === 'EXT' ? 'Ekstensi IDE' : 'Semua Modul');
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'MODULE_STATUS', "Status Global {$modLabel} diatur ke {$status}.", 'INFO');
                return [
                    'ok' => true,
                    'global_status' => $data['config']['global_status'] ?? 'LOCKED',
                    'lock_ai' => isset($data['config']['lock_ai']) ? (bool)$data['config']['lock_ai'] : true,
                    'lock_extensions' => isset($data['config']['lock_extensions']) ? (bool)$data['config']['lock_extensions'] : true,
                    'pc_overrides' => empty($data['config']['pc_overrides']) ? (object)[] : $data['config']['pc_overrides'],
                    'config' => $data['config']
                ];
            });
            respond($result, !empty($result['ok']) ? 200 : 400);
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
            if ($studentId === '') {
                respond(['ok' => false, 'error' => 'ID mahasiswa wajib diisi.'], 400);
            }

            $result = Storage::update(function (&$data) use ($studentId, $cmd) {
                Storage::ensureRoomsConfig($data);
                if ($cmd === 'REMOVE') {
                    $nowSec = time();
                    $stExisting = $data['students'][$studentId] ?? null;
                    if (!isset($data['config']['deleted_students']) || !is_array($data['config']['deleted_students'])) {
                        $data['config']['deleted_students'] = [];
                    }
                    $data['config']['deleted_students'][$studentId] = max(
                        $nowSec,
                        (int)($stExisting['last_seen'] ?? 0),
                        (int)($data['config']['deleted_students'][$studentId] ?? 0)
                    );
                    $data['config']['updated_at'] = date('c');
                    if ($stExisting) {
                        $stRoom = $stExisting['room_code'] ?? ($data['config']['room_code'] ?? 'KUIS-AI-01');
                        Storage::addLog($data, $stExisting['nim'], $stExisting['name'], $stExisting['hostname'], 'ADMIN_REMOVE', "Data sesi mahasiswa dihapus dari ruangan [{$stRoom}] oleh pengawas.", 'INFO', $stRoom);
                        unset($data['students'][$studentId]);
                    }
                    return ['ok' => true, 'deleted_students' => $data['config']['deleted_students']];
                }
                if (!isset($data['students'][$studentId])) {
                    return ['ok' => false, 'error' => 'Data mahasiswa tidak ditemukan.'];
                }
                $st = &$data['students'][$studentId];
                $stRoom = $st['room_code'] ?? ($data['config']['room_code'] ?? 'KUIS-AI-01');
                if (in_array($cmd, ['LOCKED', 'UNLOCKED'], true)) {
                    $isCmdLocked = ($cmd === 'LOCKED');
                    $st['target_status'] = $cmd;
                    $st['status'] = $cmd;
                    $st['lock_ai'] = $isCmdLocked;
                    $st['lock_extensions'] = $isCmdLocked;
                    $hKey = strtoupper(trim((string)($st['hostname'] ?? '')));
                    if ($hKey !== '') {
                        if (!isset($data['config']['pc_overrides']) || !is_array($data['config']['pc_overrides'])) {
                            $data['config']['pc_overrides'] = [];
                        }
                        $data['config']['pc_overrides'][$hKey] = [
                            'lock_ai' => $isCmdLocked,
                            'lock_extensions' => $isCmdLocked
                        ];
                    }
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'REMOTE_CMD', "Pengawas mengubah status endpoint menjadi {$cmd} di [{$stRoom}].", 'INFO', $stRoom);
                    return ['ok' => true, 'target_status' => $cmd, 'status' => $cmd, 'lock_ai' => $isCmdLocked, 'lock_extensions' => $isCmdLocked];
                }
                if ($cmd === 'UPDATE') {
                    $st['pending_command'] = 'UPDATE';
                    $st['cmd_updated_at'] = time();
                    Storage::addLog($data, $st['nim'], $st['name'], $st['hostname'], 'REMOTE_UPDATE', "Pengawas mengirim instruksi perbarui agen (self-update) ke [{$st['hostname']}].", 'INFO', $stRoom);
                    return ['ok' => true, 'command' => 'UPDATE'];
                }
                return ['ok' => false, 'error' => 'Perintah tidak dikenali.'];
            });

            respond($result);
            break;

        case 'set_ide_password':
            $newPass = trim($jsonInput['new_password'] ?? $jsonInput['ide_password'] ?? '');
            $resetDefault = !empty($jsonInput['reset_default']) || $newPass === '__RESET__';
            if (!$resetDefault && strlen($newPass) < 4) {
                respond(['ok' => false, 'error' => 'Password buka kunci IDE minimal 4 karakter.'], 400);
            }
            $result = Storage::update(function (&$data) use ($newPass, $resetDefault) {
                Storage::ensureRoomsConfig($data);
                $data['config']['ide_password_hash'] = $resetDefault ? '' : hash('sha256', $newPass);
                $data['config']['updated_at'] = date('c');
                Storage::addLog(
                    $data,
                    'ADMIN',
                    'Dosen Pengawas',
                    'DASHBOARD',
                    'IDE_PASSWORD_UPDATED',
                    $resetDefault
                        ? 'Password buka kunci IDE (AI Chat & Ekstensi) dikembalikan ke default.'
                        : 'Password buka kunci IDE (AI Chat & Ekstensi) telah diperbarui.',
                    'INFO'
                );
                return [
                    'ok' => true,
                    'ide_password_hash' => $data['config']['ide_password_hash'],
                    'is_custom' => ($data['config']['ide_password_hash'] !== '')
                ];
            });
            respond($result);
            break;

        case 'set_admin_password':
            $newPass = trim((string)($jsonInput['new_password'] ?? $jsonInput['admin_password'] ?? ''));
            $newEmail = isset($jsonInput['admin_email']) ? strtolower(trim((string)$jsonInput['admin_email'])) : null;
            $resetDefault = !empty($jsonInput['reset_default']) || $newPass === '__RESET__';
            $syncIde = !empty($jsonInput['sync_ide_password']);

            if (!$resetDefault && $newPass === '' && ($newEmail === null || $newEmail === '')) {
                respond(['ok' => false, 'error' => 'Masukkan password baru atau email pengawas baru.'], 400);
            }
            if (!$resetDefault && $newPass !== '' && strlen($newPass) < 4) {
                respond(['ok' => false, 'error' => 'Kata sandi login dashboard minimal 4 karakter.'], 400);
            }

            $result = Storage::update(function (&$data) use ($newPass, $newEmail, $resetDefault, $syncIde, $adminEmail, $authSecret) {
                Storage::ensureRoomsConfig($data);
                if ($resetDefault) {
                    $data['config']['admin_password_hash'] = '';
                    if (!empty($newEmail) && $newEmail === '__reset__') {
                        $data['config']['admin_email'] = '';
                    }
                    if ($syncIde) {
                        $data['config']['ide_password_hash'] = '';
                    }
                } else {
                    if ($newPass !== '') {
                        $hash = hash('sha256', $newPass);
                        $data['config']['admin_password_hash'] = $hash;
                        if ($syncIde) {
                            $data['config']['ide_password_hash'] = $hash;
                        }
                    }
                    if ($newEmail !== null && $newEmail !== '') {
                        $data['config']['admin_email'] = $newEmail;
                    }
                }
                $data['config']['updated_at'] = date('c');
                $effectiveEmail = !empty($data['config']['admin_email']) ? $data['config']['admin_email'] : $adminEmail;
                $logMsg = $resetDefault
                    ? 'Kata sandi login dashboard admin dikembalikan ke default server.'
                    : ($syncIde
                        ? 'Kata sandi login dashboard admin & buka kunci IDE telah diperbarui.'
                        : 'Kata sandi login dashboard admin telah diperbarui.');
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'ADMIN_PASSWORD_UPDATED', $logMsg, 'INFO');
                return [
                    'ok' => true,
                    'has_custom_admin_password' => ($data['config']['admin_password_hash'] !== ''),
                    'is_custom' => ($data['config']['admin_password_hash'] !== ''),
                    'admin_email_effective' => $effectiveEmail,
                    'token' => signAdminToken($effectiveEmail, $authSecret),
                    'synced_ide' => $syncIde
                ];
            });
            respond($result);
            break;

        case 'update_config':
            $rawIdePass = isset($jsonInput['ide_password']) ? trim((string)$jsonInput['ide_password']) : null;
            if ($rawIdePass !== null && $rawIdePass !== '' && $rawIdePass !== '__RESET__' && strlen($rawIdePass) < 4) {
                respond(['ok' => false, 'error' => 'Password buka kunci IDE minimal 4 karakter.'], 400);
            }
            $rawAdminPass = isset($jsonInput['admin_password']) ? trim((string)$jsonInput['admin_password']) : null;
            if ($rawAdminPass !== null && $rawAdminPass !== '' && $rawAdminPass !== '__RESET__' && strlen($rawAdminPass) < 4) {
                respond(['ok' => false, 'error' => 'Kata sandi login dashboard minimal 4 karakter.'], 400);
            }
            $rawAdminEmail = isset($jsonInput['admin_email']) ? strtolower(trim((string)$jsonInput['admin_email'])) : null;

            $result = Storage::update(function (&$data) use ($jsonInput, $rawIdePass, $rawAdminPass, $rawAdminEmail, $adminEmail, $authSecret) {
                if (!empty($jsonInput['room_code'])) {
                    $data['config']['room_code'] = Storage::registerRoomIfNeeded($data, $jsonInput['room_code']);
                }
                if ($rawIdePass !== null && $rawIdePass !== '') {
                    $data['config']['ide_password_hash'] = ($rawIdePass === '__RESET__') ? '' : hash('sha256', $rawIdePass);
                }
                if ($rawAdminPass !== null && $rawAdminPass !== '') {
                    $data['config']['admin_password_hash'] = ($rawAdminPass === '__RESET__') ? '' : hash('sha256', $rawAdminPass);
                }
                if ($rawAdminEmail !== null && $rawAdminEmail !== '') {
                    $data['config']['admin_email'] = ($rawAdminEmail === '__reset__') ? '' : $rawAdminEmail;
                }
                $data['config']['block_doh'] = isset($jsonInput['block_doh']) ? (bool)$jsonInput['block_doh'] : true;
                $data['config']['block_vpn'] = isset($jsonInput['block_vpn']) ? (bool)$jsonInput['block_vpn'] : true;
                $data['config']['quarantine_extensions'] = isset($jsonInput['quarantine_extensions']) ? (bool)$jsonInput['quarantine_extensions'] : true;
                $data['config']['kill_processes'] = isset($jsonInput['kill_processes']) ? (bool)$jsonInput['kill_processes'] : true;
                $data['config']['updated_at'] = date('c');
                Storage::ensureRoomsConfig($data);

                $effectiveEmail = !empty($data['config']['admin_email']) ? $data['config']['admin_email'] : $adminEmail;
                Storage::addLog($data, 'ADMIN', 'Dosen Pengawas', 'DASHBOARD', 'CONFIG_UPDATE', 'Konfigurasi ruangan & kebijakan blokir diperbarui.', 'INFO');
                return [
                    'ok' => true,
                    'token' => signAdminToken($effectiveEmail, $authSecret),
                    'config' => sanitizeConfigForDashboard($data['config'], $adminEmail)
                ];
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
