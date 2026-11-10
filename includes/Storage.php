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
                'room_name' => 'Ujian Pemrograman & Algoritma (Tanpa AI)',
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
                    'api.githubcopilot.com', 'copilot-proxy.githubusercontent.com', 'copilot-telemetry.githubusercontent.com', 'default.exp-tas.com',
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
                'processes' => [
                    // AI-Native IDEs & Desktop AI Clients
                    'Cursor', 'Windsurf', 'Trae', 'PearAI', 'Void',
                    'ChatGPT', 'Claude', 'Copilot', 'Perplexity',
                    // Local / Offline LLM Runners
                    'ollama', 'ollama_llama_server', 'ollama app',
                    'LM Studio', 'lm-studio',
                    'jan', 'Jan',
                    'gpt4all', 'GPT4All',
                    'llama-server', 'llama-cli', 'koboldcpp', 'text-generation-webui', 'anythingllm', 'AnythingLLM', 'msty', 'Msty'
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
        return is_array($data) ? $data : self::getDefaultSchema();
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

    public static function addLog(&$data, $nim, $name, $hostname, $type, $message, $severity = 'INFO') {
        if (!isset($data['logs']) || !is_array($data['logs'])) {
            $data['logs'] = [];
        }
        array_unshift($data['logs'], [
            'id' => uniqid('log_'),
            'timestamp' => date('Y-m-d H:i:s'),
            'nim' => $nim,
            'name' => $name,
            'hostname' => $hostname,
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
