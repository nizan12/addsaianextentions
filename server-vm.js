const http = require('http');
const fs = require('fs');
const path = require('path');
const { URL } = require('url');

function loadEnvFile(filePath) {
    try {
        if (!fs.existsSync(filePath)) return;
        const rawText = fs.readFileSync(filePath, 'utf8').replace(/^\uFEFF/, '');
        const lines = rawText.split(/\r?\n/);
        for (const rawLine of lines) {
            const line = rawLine.trim();
            if (!line || line.startsWith('#')) continue;
            const eqIdx = line.indexOf('=');
            if (eqIdx === -1) continue;
            const key = line.slice(0, eqIdx).trim();
            if (!key || Object.prototype.hasOwnProperty.call(process.env, key)) continue;
            let val = line.slice(eqIdx + 1).trim();
            if ((val.startsWith('"') && val.endsWith('"')) || (val.startsWith("'") && val.endsWith("'"))) {
                val = val.slice(1, -1);
            }
            process.env[key] = val;
        }
    } catch (e) {
        console.error('[server-vm] Gagal membaca file .env:', e.message);
    }
}

loadEnvFile(process.env.GUARD_ENV_FILE || path.join(__dirname, '.env'));
process.env.STORAGE_MODE = process.env.STORAGE_MODE || 'local';

const apiHandler = require('./api/server.js');

const ROOT_DIR = __dirname;
const ASSETS_DIR = path.join(ROOT_DIR, 'assets');
const DIST_DIR = path.join(ROOT_DIR, 'dist');
const INDEX_HTML_PATH = path.join(ROOT_DIR, 'index.html');

const HOST = process.env.HOST || '127.0.0.1';
const PORT = parseInt(process.env.PORT || '3000', 10);
const MAX_BODY_BYTES = 5 * 1024 * 1024;

const MIME_TYPES = {
    '.html': 'text/html; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.js': 'application/javascript; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.svg': 'image/svg+xml',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.ico': 'image/x-icon',
    '.woff2': 'font/woff2',
    '.exe': 'application/octet-stream',
    '.bat': 'application/octet-stream'
};

function enhanceResponse(res) {
    res.status = (code) => {
        res.statusCode = code;
        return res;
    };
    res.json = (obj) => {
        if (!res.headersSent && !res.getHeader('Content-Type')) {
            res.setHeader('Content-Type', 'application/json; charset=utf-8');
        }
        res.end(JSON.stringify(obj));
        return res;
    };
    res.send = (val) => {
        if (val === undefined || val === null) {
            res.end();
            return res;
        }
        if (Buffer.isBuffer(val) || typeof val === 'string') {
            res.end(val);
            return res;
        }
        return res.json(val);
    };
    return res;
}

function readRequestBody(req, maxBytes = MAX_BODY_BYTES) {
    return new Promise((resolve, reject) => {
        const chunks = [];
        let total = 0;
        let finished = false;
        req.on('data', (chunk) => {
            if (finished) return;
            total += chunk.length;
            if (total > maxBytes) {
                finished = true;
                const err = new Error('Payload Too Large');
                err.statusCode = 413;
                reject(err);
                req.destroy();
                return;
            }
            chunks.push(chunk);
        });
        req.on('end', () => {
            if (!finished) {
                finished = true;
                resolve(Buffer.concat(chunks).toString('utf8'));
            }
        });
        req.on('error', (err) => {
            if (!finished) {
                finished = true;
                reject(err);
            }
        });
    });
}

function resolveSafePath(baseDir, rawRelPath) {
    let decoded;
    try {
        decoded = decodeURIComponent(rawRelPath);
    } catch (e) {
        return null;
    }
    const cleanRel = decoded.replace(/^\/+/, '').replace(/\\/g, '/');
    if (!cleanRel || cleanRel.includes('\0')) return null;
    const baseResolved = path.resolve(baseDir);
    const fullResolved = path.resolve(baseResolved, cleanRel);
    if (!fullResolved.startsWith(baseResolved + path.sep)) {
        return null;
    }
    return fullResolved;
}

function serveStaticFile(req, res, filePath, options = {}) {
    fs.stat(filePath, (err, stat) => {
        if (err || !stat.isFile()) {
            res.statusCode = 404;
            res.setHeader('Content-Type', 'text/plain; charset=utf-8');
            res.end('Not Found');
            return;
        }
        const ext = path.extname(filePath).toLowerCase();
        const contentType = options.contentType || MIME_TYPES[ext] || 'application/octet-stream';
        res.statusCode = 200;
        res.setHeader('Content-Type', contentType);
        res.setHeader('Content-Length', String(stat.size));
        if (options.cacheControl) {
            res.setHeader('Cache-Control', options.cacheControl);
        }
        if (options.attachment) {
            const safeName = path.basename(filePath).replace(/"/g, '');
            res.setHeader('Content-Disposition', `attachment; filename="${safeName}"`);
        }
        if (req.method === 'HEAD') {
            res.end();
            return;
        }
        const stream = fs.createReadStream(filePath);
        stream.on('error', () => {
            if (!res.headersSent) {
                res.statusCode = 500;
                res.end('File Read Error');
            } else {
                res.destroy();
            }
        });
        stream.pipe(res);
    });
}

const server = http.createServer(async (req, res) => {
    enhanceResponse(res);
    let parsedUrl;
    try {
        parsedUrl = new URL(req.url || '/', `http://${req.headers.host || 'localhost'}`);
    } catch (e) {
        return res.status(400).json({ ok: false, error: 'Bad URL' });
    }
    const pathname = parsedUrl.pathname;

    if (req.method === 'GET' && pathname === '/health') {
        try {
            const state = await apiHandler._internals.readData(false);
            const roomsCount = Array.isArray(state?.config?.rooms) ? state.config.rooms.length : 0;
            const studentsCount = state?.students && typeof state.students === 'object' ? Object.keys(state.students).length : 0;
            return res.status(200).json({
                ok: true,
                mode: process.env.STORAGE_MODE || 'local',
                uptime_sec: Math.floor(process.uptime()),
                memory_mb: Math.round(process.memoryUsage().rss / 1024 / 1024),
                rooms_count: roomsCount,
                students_count: studentsCount
            });
        } catch (err) {
            return res.status(500).json({ ok: false, error: err.message });
        }
    }

    if (pathname === '/api/index.php' || pathname === '/api' || pathname === '/api/server.js') {
        req.query = {};
        for (const [k, v] of parsedUrl.searchParams.entries()) {
            req.query[k] = v;
        }
        if (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH') {
            try {
                const rawBody = await readRequestBody(req);
                const ctype = String(req.headers['content-type'] || '').toLowerCase();
                const trimmed = rawBody.trim();
                if (trimmed && (ctype.includes('application/json') || trimmed.startsWith('{') || trimmed.startsWith('['))) {
                    try {
                        req.body = JSON.parse(trimmed);
                    } catch (e) {
                        req.body = rawBody;
                    }
                } else {
                    req.body = rawBody;
                }
            } catch (err) {
                if (err.statusCode === 413) {
                    return res.status(413).json({ ok: false, error: 'Payload terlalu besar' });
                }
                return res.status(400).json({ ok: false, error: 'Gagal membaca body request' });
            }
        } else {
            req.body = {};
        }
        try {
            await apiHandler(req, res);
        } catch (err) {
            if (!res.headersSent) {
                res.status(500).json({ ok: false, error: err.message || 'Internal Server Error' });
            }
        }
        return;
    }

    if (req.method === 'GET' || req.method === 'HEAD') {
        if (pathname === '/' || pathname === '/index.html') {
            return serveStaticFile(req, res, INDEX_HTML_PATH, {
                contentType: 'text/html; charset=utf-8',
                cacheControl: 'no-cache'
            });
        }
        if (pathname.startsWith('/assets/')) {
            const assetPath = resolveSafePath(ASSETS_DIR, pathname.slice('/assets/'.length));
            if (!assetPath) {
                return res.status(403).send('Forbidden');
            }
            return serveStaticFile(req, res, assetPath, {
                cacheControl: 'public, max-age=3600'
            });
        }
        if (pathname.startsWith('/dist/')) {
            const distPath = resolveSafePath(DIST_DIR, pathname.slice('/dist/'.length));
            if (!distPath) {
                return res.status(403).send('Forbidden');
            }
            return serveStaticFile(req, res, distPath, {
                attachment: true,
                cacheControl: 'public, max-age=0, must-revalidate'
            });
        }
    }

    res.statusCode = 404;
    res.setHeader('Content-Type', 'text/plain; charset=utf-8');
    res.end('Not Found');
});

server.keepAliveTimeout = 65000;
server.headersTimeout = 66000;

function shutdown(signal) {
    console.log(`[server-vm] Menerima ${signal}, menyimpan database lokal dan menutup server...`);
    try {
        apiHandler._internals.flushLocalDbNow();
    } catch (e) {}
    server.close(() => {
        process.exit(0);
    });
    setTimeout(() => process.exit(0), 4000).unref();
}

process.on('SIGINT', () => shutdown('SIGINT'));
process.on('SIGTERM', () => shutdown('SIGTERM'));

server.listen(PORT, HOST, () => {
    console.log(`GuardConsole VM server listening on http://${HOST}:${PORT} (mode=${process.env.STORAGE_MODE})`);
});
