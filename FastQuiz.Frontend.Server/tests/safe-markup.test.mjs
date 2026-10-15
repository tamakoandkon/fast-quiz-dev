import assert from 'node:assert/strict';
import { readFileSync, readdirSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import ts from 'typescript';
import createDOMPurify from 'dompurify';
import { marked } from 'marked';
import { JSDOM } from 'jsdom';

const dom = new JSDOM('');
const purifier = createDOMPurify(dom.window);
const source = readFileSync(new URL('../app/utils/safeMarkup.ts', import.meta.url), 'utf8');
const exports = {};
vm.runInNewContext(ts.transpileModule(source, { compilerOptions: {
  module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022,
} }).outputText, { exports, require(name) {
  if (name === 'dompurify') return purifier;
  if (name === 'marked') return { marked };
  throw new Error(name);
} });
const render = exports.safeMarkup;

test('Markdown and legacy HTML remain readable', () => {
  const html = render('**题目**\n\n- 选项一\n- 选项二\n\n<table><tr><td>历史内容</td></tr></table>');
  assert.match(html, /<strong>题目<\/strong>/);
  assert.match(html, /<li>选项一<\/li>/);
  assert.match(html, /<td>历史内容<\/td>/);
  assert.match(render('[文档](https://example.com) `code`'), /href="https:\/\/example.com"/);
});

test('events, scripts, active content and dangerous links are removed after Markdown parsing', () => {
  const attacks = [
    '<img src=x onerror="window.auditExecuted=true">',
    '<svg onload="window.auditExecuted=true"><a href="javascript:alert(1)">x</a></svg>',
    '<script>window.auditExecuted=true</script><iframe srcdoc="<script>alert(1)</script>"></iframe>',
    '[危险](javascript:alert%281%29)',
    '<a href="java&#x73;cript:alert(1)" onclick="alert(1)">link</a>',
    '<math><mtext><img src=x onerror=alert(1)></mtext></math>',
    '<form id="attributes"><input name="__proto__"></form><div style="background:url(javascript:alert(1))">ok</div>',
    '<img src="data:image/svg+xml,<svg onload=alert(1)>">',
  ];
  for (const payload of attacks) {
    const html = render(payload);
    const document = new JSDOM(html).window.document;
    assert.equal(document.querySelector('script,iframe,svg,math,form,input,object,embed,style'), null, payload);
    for (const element of document.querySelectorAll('*')) {
      for (const attr of element.attributes) {
        assert.ok(!/^on|^style$|^id$|^name$/i.test(attr.name), html);
        if (attr.name === 'href' || attr.name === 'src') assert.ok(!/^\s*(javascript|vbscript):/i.test(attr.value), html);
      }
    }
  }
});

test('all nine question and option sinks use the shared safe component', () => {
  let count = 0;
  const directory = new URL('../app/components/QuestionRecord/', import.meta.url);
  for (const name of readdirSync(directory).filter(x => x.endsWith('.vue'))) {
    const file = readFileSync(new URL(name, directory), 'utf8');
    assert.doesNotMatch(file, /v-html/);
    count += [...file.matchAll(/<CommonSafeMarkup\b/g)].length;
  }
  assert.equal(count, 9);
});
