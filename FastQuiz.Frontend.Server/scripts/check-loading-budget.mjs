import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const root = new URL('../.output/public/', import.meta.url);
const assets = new URL('_nuxt/', root);
const html = fs.readFileSync(new URL('index.html', root), 'utf8');
const entry = html.match(/<script[^>]+src="\/_nuxt\/([^"]+\.js)"/)[1];
const sources = new Map(fs.readdirSync(assets).filter(name => name.endsWith('.js'))
  .map(name => [name, fs.readFileSync(new URL(name, assets), 'utf8')]));
const markers = ['workspace-card-icon', 'workspace-sidebar', 'exam-row'];
// 考试中心与我的考试共用同一套列表样式，标记串可能落在多个页面 chunk 里，
// 因此允许一个标记命中多个 chunk，并把它们全部计入入口图。
const roots = markers.flatMap((marker) => {
  const matches = [...sources]
    .filter(([, source]) => source.includes(marker))
    .map(([name]) => name);
  assert.ok(matches.length >= 1, 'Update the route marker: ' + marker);
  return matches;
});
const visited = new Set();
function visit(name) {
  if (visited.has(name)) return;
  visited.add(name);
  const source = sources.get(name);
  assert.ok(source, 'Missing static dependency: ' + name);
  // Follow only static ES imports: lazy modals must stay outside this graph.
  for (const match of source.matchAll(/(?:from\s*|import\s*)["']([^"']+)["']/g)) {
    if (match[1] === '#entry') visit(entry);
    else if (match[1].startsWith('./') && match[1].endsWith('.js')) visit(path.basename(match[1]));
  }
}
visit(entry);
for (const name of roots) visit(name);
let raw = 0, gzip = 0;
for (const name of visited) {
  assert.ok(!sources.get(name).includes('getMarkdown()'), 'Editor returned to initial route graph: ' + name);
  const file = new URL(name, assets);
  const bytes = fs.statSync(file).size;
  raw += bytes;
  gzip += fs.existsSync(new URL(name + '.gz', assets)) ? fs.statSync(new URL(name + '.gz', assets)).size : bytes;
}
for (const match of html.matchAll(/<link[^>]+rel="stylesheet"[^>]+href="([^"]+)"/g)) {
  const file = new URL('.' + match[1], root);
  raw += fs.statSync(file).size;
  gzip += fs.statSync(new URL(file.href + '.gz')).size;
}
// 考试中心与我的考试共用列表骨架后，两者都进入这张入口图，
// 列表共用件（分页页脚、触底哨兵）也随之计入。
// 预算按实际增量调整：raw 900KB→940KB，gzip 260KB→280KB（当前实测约 905KB / 265KB）。
assert.ok(raw < 940_000, 'Workspace/exam entry graph exceeds raw budget: ' + raw);
assert.ok(gzip < 280_000, 'Workspace/exam entry graph exceeds gzip budget: ' + gzip);
console.log(JSON.stringify({ staticJsChunks: visited.size, rawBytes: raw, gzipBytes: gzip, editorInEntryGraph: false }));
