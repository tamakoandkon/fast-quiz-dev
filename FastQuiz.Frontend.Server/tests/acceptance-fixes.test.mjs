import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import ts from 'typescript';
import * as Vue from 'vue';

function load(relative, globals = {}, imports = {}) {
  const source = readFileSync(new URL('../app/' + relative, import.meta.url), 'utf8');
  const code = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 } }).outputText;
  const exports = {};
  vm.runInNewContext(code, { exports, console: { error() {} }, ...globals, require(name) {
    if (name === 'vue') return Vue;
    assert.ok(name in imports, 'Unexpected import: ' + name);
    return imports[name];
  } });
  return exports;
}

const statuses = { Pending: 'Pending', InProgress: 'InProgress', Submitted: 'Submitted', Graded: 'Graded' };
const { quizRecordLink, quizRecordName } = load('utils/quizRecords.ts', {}, { './backEnd': { QuizAttemptStatus: statuses } });
test('graded records always open the complete answer page, including for reviewers', () => {
  for (const reviewer of [false, true]) assert.equal(quizRecordLink('tenant', { id: 'attempt', quizTemplateId: 'template', status: 'Graded' }, reviewer), '/tenant/tenant/quiz-attempt/attempt/result');
});
test('only a reviewer can open a submitted record for manual review', () => {
  const row = { id: 'attempt', quizTemplateId: 'template', status: 'Submitted' };
  assert.equal(quizRecordLink('tenant', row, false), undefined);
  assert.equal(quizRecordLink('tenant', row, true), '/tenant/tenant/dash/quiz-template/template/review/attempt');
});
test('unfinished or incomplete records have no result link', () => {
  for (const status of ['Pending', 'InProgress', undefined]) assert.equal(quizRecordLink('tenant', { id: 'attempt', status }, true), undefined);
  assert.equal(quizRecordLink('', { id: 'attempt', status: 'Graded' }, true), undefined);
  assert.equal(quizRecordLink('tenant', { status: 'Graded' }, true), undefined);
});
test('display uses the account name with a safe missing-account fallback', () => {
  assert.equal(quizRecordName({ assignId: 'id', assignUsername: 'Alice' }), 'Alice');
  assert.equal(quizRecordName({ assignId: 'removed-id' }), 'removed-id');
  assert.equal(quizRecordName({}), '未知考生');
});

const tick = async () => { await Vue.nextTick(); await new Promise(resolve => setImmediate(resolve)); };
const deferred = () => { let resolve; const promise = new Promise(yes => { resolve = yes; }); return { promise, resolve }; };
function modal(api, tenant = '', open = false) {
  const scope = Vue.effectScope();
  const tenantId = Vue.ref(tenant), enabled = Vue.ref(open);
  const { useQuestionGroups } = load('composables/useQuestionGroups.ts', { useNuxtApp: () => ({ $api: { tenant: { questionGroupList: api } } }) });
  const result = scope.run(() => useQuestionGroups(tenantId, enabled));
  return { scope, tenantId, enabled, ...result };
}
test('closed modals and blank tenants make no background API request', async t => {
  let calls = 0;
  const m = modal(async () => { calls++; return { data: [] }; });
  t.after(() => m.scope.stop());
  m.enabled.value = true; await tick();
  m.tenantId.value = ' '; await tick();
  m.enabled.value = false; m.tenantId.value = 'tenant'; await tick();
  assert.equal(calls, 0);
  m.enabled.value = true; await tick();
  assert.equal(calls, 1);
});
test('an open modal loads when a valid tenant arrives after mount', async t => {
  const calls = [];
  const m = modal(async tenant => { calls.push(tenant); return { data: [{ id: 'g' }] }; }, '', true);
  t.after(() => m.scope.stop());
  m.tenantId.value = 'tenant'; await tick();
  assert.deepEqual(calls, ['tenant']);
  assert.equal(m.groups.value[0].id, 'g');
});
test('late replies from another tenant never populate the current modal', async t => {
  const old = deferred();
  const m = modal(tenant => tenant === 'old' ? old.promise : Promise.resolve({ data: [{ id: 'new-group' }] }), 'old', true);
  t.after(() => m.scope.stop());
  m.tenantId.value = 'new'; await tick();
  old.resolve({ data: [{ id: 'old-group' }] }); await tick();
  assert.equal(m.groups.value[0].id, 'new-group');
  assert.equal(m.loadingGroups.value, false);
});
test('closing the modal discards its pending response and reopening reloads', async t => {
  const old = deferred(); let calls = 0;
  const m = modal(() => ++calls === 1 ? old.promise : Promise.resolve({ data: [{ id: 'fresh' }] }), 'tenant', true);
  t.after(() => m.scope.stop());
  m.enabled.value = false; await tick();
  old.resolve({ data: [{ id: 'stale' }] }); await tick();
  assert.equal(m.groups.value.length, 0);
  m.enabled.value = true; await tick();
  assert.equal(calls, 2); assert.equal(m.groups.value[0].id, 'fresh');
});
test('pagination uses a consistent 200-item page and the last cursor', async t => {
  const calls = [];
  const batch = Array.from({ length: 200 }, (_, i) => ({ id: String(i) }));
  const m = modal(async (tenant, query) => { calls.push({ tenant, ...query }); return { data: calls.length === 1 ? batch : [{ id: 'last' }] }; }, 'tenant', true);
  t.after(() => m.scope.stop()); await tick();
  assert.equal(m.groups.value.length, 201);
  assert.equal(calls[1].Cursor, '199'); assert.equal(calls[1].Count, 200);
});
test('a repeated cursor terminates without exposing partial/stale data', async t => {
  let calls = 0;
  const batch = Array.from({ length: 200 }, () => ({ id: 'same' }));
  const m = modal(async () => { calls++; return { data: batch }; }, 'tenant', true);
  t.after(() => m.scope.stop()); await tick();
  assert.equal(calls, 2); assert.equal(m.groups.value.length, 0); assert.equal(m.loadingGroups.value, false);
});
test('failed requests clear loading and component disposal ignores late replies', async () => {
  const failed = modal(async () => { throw new Error('failed'); }, 'tenant', true);
  await tick(); assert.equal(failed.loadingGroups.value, false); failed.scope.stop();
  const response = deferred(); const disposed = modal(() => response.promise, 'tenant', true);
  disposed.scope.stop(); response.resolve({ data: [{ id: 'late' }] }); await tick();
  assert.equal(disposed.groups.value.length, 0);
});
