import assert from 'node:assert/strict';
import { test } from 'node:test';
import { mkdtemp, mkdir, copyFile, readFile, writeFile, rm } from 'node:fs/promises';
import { execFileSync } from 'node:child_process';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createHash } from 'node:crypto';

test('CSP covers exact inline scripts on all prerendered routes without enabling arbitrary inline scripts', async () => {
  const root=await mkdtemp(join(tmpdir(),'fastquiz-csp-test-'));
  try {
    await mkdir(join(root,'scripts'));
    await mkdir(join(root,'.output/public/tenant/manage'),{recursive:true});
    await copyFile(new URL('../scripts/generate-csp.mjs',import.meta.url),join(root,'scripts/generate-csp.mjs'));
    const scripts=['window.bootstrap="首页";','window.bootstrap="管理";'];
    await writeFile(join(root,'.output/public/index.html'),`<script>${scripts[0]}</script>`);
    await writeFile(join(root,'.output/public/tenant/manage/index.html'),`<script>${scripts[1]}</script><script src="/app.js"></script>`);
    execFileSync(process.execPath,[join(root,'scripts/generate-csp.mjs')]);
    const policy=await readFile(join(root,'.output/security/policy.txt'),'utf8');
    const scriptPolicy=policy.split(';').find(x=>x.trim().startsWith('script-src'));
    assert.doesNotMatch(scriptPolicy, /unsafe-inline|unsafe-eval/);
    for(const script of scripts) assert.ok(scriptPolicy.includes(`'sha256-${createHash('sha256').update(script).digest('base64')}'`));
    assert.match(policy,/object-src 'none'/);
    assert.match(policy,/frame-ancestors 'none'/);
    for(const server of ['nginx','caddy'])assert.ok((await readFile(join(root,`.output/security/${server}-csp.conf`),'utf8')).includes(policy));
  } finally { await rm(root,{recursive:true,force:true}); }
});
