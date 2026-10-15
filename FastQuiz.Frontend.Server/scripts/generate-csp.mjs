import { createHash } from 'node:crypto';
import { mkdir, readFile, readdir, writeFile } from 'node:fs/promises';

// Nuxt's bootstrap is inline. Hash its exact bytes instead of enabling arbitrary inline scripts.
const root = new URL('../.output/public/', import.meta.url);
const pages = (await readdir(root, { recursive: true })).filter(name => name.endsWith('.html'));
const html = (await Promise.all(pages.map(name => readFile(new URL(name, root), 'utf8')))).join('\n');
const hashes = [...html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/gi)]
  .filter(([, attrs, body]) => !/\bsrc\s*=/i.test(attrs) && body.length)
  .map(([, , body]) => `'sha256-${createHash('sha256').update(body).digest('base64')}'`);
const policy = [
  "default-src 'self'",
  `script-src 'self' ${[...new Set(hashes)].join(' ')}`.trim(),
  "style-src 'self' 'unsafe-inline'",
  "img-src 'self' data: https:",
  "font-src 'self' data:",
  // config.json supports a separately hosted API. Narrow this to its origin in production.
  "connect-src 'self' https: http:",
  "object-src 'none'", "base-uri 'self'", "frame-ancestors 'none'", "form-action 'self'",
].join('; ');
const directory = new URL('../.output/security/', import.meta.url);
await mkdir(directory, { recursive: true });
await writeFile(new URL('caddy-csp.conf', directory), `header {\n  Content-Security-Policy "${policy}"\n  X-Content-Type-Options nosniff\n  Referrer-Policy same-origin\n}\n`);
await writeFile(new URL('nginx-csp.conf', directory), `add_header Content-Security-Policy "${policy}" always;\nadd_header X-Content-Type-Options nosniff always;\nadd_header Referrer-Policy same-origin always;\n`);
await writeFile(new URL('policy.txt', directory), policy);
