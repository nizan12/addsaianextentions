<?php
/**
 * AI-Blocker Guard - Zero-Config Atomic Storage Engine
 * Menyimpan data sesi ujian, mahasiswa terhubung, log pelanggaran, dan daftar blokir AI.
 */

class Storage {
    private static $dataDir = __DIR__ . '/../storage';
    private static $dbFile = __DIR__ . '/../storage/database.json';

    public static function init() {
        if (!is_dir(self::$dataDir)) {
            mkdir(self::$dataDir, 0777, true);
        }
        if (!file_exists(self::$dbFile)) {
            $defaultData = self::getDefaultSchema();
            file_put_contents(self::$dbFile, json_encode($defaultData, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES), LOCK_EX);
        }
    }

    public static function getDefaultSchema() {
        return [
            'config' => [
                'room_code' => 'KUIS-AI-01',
                'rooms' => ['KUIS-AI-01', 'LAB-01', 'LAB-02', 'LAB-03'],
                'room_statuses' => [
                    'KUIS-AI-01' => 'LOCKED',
                    'LAB-01' => 'LOCKED',
                    'LAB-02' => 'LOCKED',
                    'LAB-03' => 'LOCKED'
                ],
                'unlock_pin' => '202699',
                'admin_password' => 'admin123',
                'global_status' => 'LOCKED', // LOCKED | UNLOCKED
                'block_doh' => true,
                'block_vpn' => true,
                'quarantine_extensions' => true,
                'kill_processes' => true,
                'updated_at' => date('c')
            ],
            'blocklist' => [
                'domains' => [
                    // Major Web AI Assistants
                    'chatgpt.com', 'www.chatgpt.com', 'openai.com', 'www.openai.com', 'api.openai.com', 'auth.openai.com', 'chat.openai.com',
                    'claude.ai', 'www.claude.ai', 'anthropic.com', 'www.anthropic.com', 'api.anthropic.com',
                    'gemini.google.com', 'bard.google.com', 'aistudio.google.com', 'generativelanguage.googleapis.com', 'makersuite.google.com',
                    'copilot.microsoft.com', 'www.copilot.microsoft.com', 'sydney.bing.com', 'edgeservices.bing.com',
                    'deepseek.com', 'www.deepseek.com', 'chat.deepseek.com', 'api.deepseek.com',
                    'perplexity.ai', 'www.perplexity.ai', 'api.perplexity.ai',
                    'poe.com', 'www.poe.com',
                    'grok.com', 'www.grok.com', 'x.ai', 'api.x.ai',
                    'mistral.ai', 'chat.mistral.ai', 'api.mistral.ai',
                    'meta.ai', 'www.meta.ai',
                    'pi.ai', 'inflection.ai',
                    'you.com', 'www.you.com',
                    'HuggingFace.co', 'huggingface.co', 'www.huggingface.co', 'api-inference.huggingface.co',
                    // Coding AI Web & App Builders
                    'blackbox.ai', 'www.blackbox.ai', 'api.blackbox.ai',
                    'phind.com', 'www.phind.com',
                    'v0.dev', 'www.v0.dev', 'v0.app',
                    'bolt.new', 'www.bolt.new',
                    'lovable.dev', 'www.lovable.dev',
                    'replit.com', 'ai.replit.com',
                    'idx.google.com',
                    // IDE AI Backends & Proxies (Copilot, Cursor, Codeium, Tabnine, Supermaven, OpenRouter)
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
                'processes' => [
                    // AI-Native IDEs & Desktop AI Clients
                    'Cursor', 'Windsurf', 'Trae', 'PearAI', 'Void',
                    'ChatGPT', 'Claude', 'Copilot', 'Perplexity',
                    // Local / Offline LLM Runners
                    'ollama', 'ollama_llama_server', 'ollama app',
                    'LM Studio', 'lm-studio',
                    'jan', 'Jan',
                    'gpt4all', 'GPT4All',
                    'llama-server', 'llama-cli', 'koboldcpp', 'text-generation-webui', 'anythingllm', 'AnythingLLM', 'msty', 'Msty',
                    // Roblox Player, Studio, Launcher, Microsoft Store UWP & Bootstrapper
                    'RobloxPlayerBeta', 'RobloxPlayerLauncher', 'RobloxPlayerInstaller',
                    'RobloxStudioBeta', 'RobloxStudioLauncherBeta', 'RobloxStudioInstaller',
                    'RobloxCrashHandler', 'Windows10Universal', 'Roblox', 'Bloxstrap', 'Fishstrap'
                ],
                'ide_extensions' => [
                    // VS Code & JetBrains Extension Prefixes / Keywords
                    'github.copilot',
                    'github.copilot-chat',
                    'codeium.codeium',
                    'continue.continue',
                    'saoudrizwan.claude-dev', // Cline
                    'rooveterinaryinc.roo-cline', // Roo Code
                    'tabnine.tabnine-vscode',
                    'supermaven.supermaven',
                    'blackboxapp.blackbox',
                    'sourcegraph.cody-ai',
                    'amazonwebservices.amazon-q-vscode',
                    'amazonwebservices.aws-toolkit-vscode',
                    'google.geminicodeassist',
                    'googlecloudtools.cloudcode',
                    'codium.codium',
                    ' DanielSanMedium.dscodegpt',
                    'genieai.chatgpt-vscode',
                    'bito.bito',
                    'Pieces.pieces-vscode',
                    'kilocode.kilo-code',
                    'augment.vscode-augment',
                    ' cursor-',
                    'windsurf-'
                ],
                'local_ai_ports' => [
                    11434, // Ollama default API port
                    1234,  // LM Studio default API port
                    1337,  // Jan AI default API port
                    8080,  // LocalAI / llama.cpp common port (checked by process name)
                    5001   // KoboldCPP default port
                ]
            ],
            'students' => [],
            'logs' => []
        ];
    }

    public static function read() {
        self::init();
        $fp = fopen(self::$dbFile, 'r');
        if (!$fp) {
            return self::getDefaultSchema();
        }
        flock($fp, LOCK_SH);
        $content = stream_get_contents($fp);
        flock($fp, LOCK_UN);
        fclose($fp);

        $data = json_decode($content, true);
        if (!is_array($data)) {
            return self::getDefaultSchema();
        }
        self::ensureRoomsConfig($data);
        return $data;
    }

    public static function update(callable $mutator) {
        self::init();
        $fp = fopen(self::$dbFile, 'c+');
        if (!$fp) {
            throw new RuntimeException('Gagal membuka file penyimpanan.');
        }
        flock($fp, LOCK_EX);
        $content = stream_get_contents($fp);
        $data = json_decode($content, true);
        if (!is_array($data)) {
            $data = self::getDefaultSchema();
        }

        $result = $mutator($data);

        ftruncate($fp, 0);
        rewind($fp);
        fwrite($fp, json_encode($data, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES));
        fflush($fp);
        flock($fp, LOCK_UN);
        fclose($fp);

        return $result;
    }

    public static function ensureRoomsConfig(&$data) {
        if (empty($data['config']['rooms']) || !is_array($data['config']['rooms'])) {
            $defRoom = $data['config']['room_code'] ?? 'KUIS-AI-01';
            $data['config']['rooms'] = array_values(array_unique([$defRoom, 'LAB-01', 'LAB-02', 'LAB-03']));
        }
        if (empty($data['config']['room_statuses']) || !is_array($data['config']['room_statuses'])) {
            $data['config']['room_statuses'] = [];
        }
        foreach ($data['config']['rooms'] as $r) {
            if (!isset($data['config']['room_statuses'][$r])) {
                $data['config']['room_statuses'][$r] = $data['config']['global_status'] ?? 'LOCKED';
            }
        }
        $defaults = self::getDefaultSchema()['blocklist'];
        if (isset($data['blocklist'])) {
            $data['blocklist']['domains'] = array_values(array_unique(array_merge($data['blocklist']['domains'] ?? [], $defaults['domains'])));
            $data['blocklist']['processes'] = array_values(array_unique(array_merge($data['blocklist']['processes'] ?? [], $defaults['processes'])));
        }
    }

    public static function registerRoomIfNeeded(&$data, $roomCode) {
        self::ensureRoomsConfig($data);
        $norm = strtoupper(trim($roomCode ?: ($data['config']['room_code'] ?? 'KUIS-AI-01')));
        if (!in_array($norm, $data['config']['rooms'], true)) {
            $data['config']['rooms'][] = $norm;
        }
        if (!isset($data['config']['room_statuses'][$norm])) {
            $data['config']['room_statuses'][$norm] = $data['config']['global_status'] ?? 'LOCKED';
        }
        return $norm;
    }

    public static function addLog(&$data, $nim, $name, $hostname, $type, $message, $severity = 'INFO', $roomCode = '') {
        if (!isset($data['logs']) || !is_array($data['logs'])) {
            $data['logs'] = [];
        }
        array_unshift($data['logs'], [
            'id' => uniqid('log_'),
            'timestamp' => date('Y-m-d H:i:s'),
            'nim' => $nim,
            'name' => $name,
            'hostname' => $hostname,
            'room_code' => $roomCode,
            'type' => $type,
            'message' => $message,
            'severity' => $severity // INFO | WARNING | DANGER
        ]);
        // Simpan maksimal 300 log terbaru agar tetap cepat
        if (count($data['logs']) > 300) {
            $data['logs'] = array_slice($data['logs'], 0, 300);
        }
    }
}
