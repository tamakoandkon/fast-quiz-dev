import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import ts from 'typescript';
import { createPinia, defineStore, setActivePinia } from 'pinia';
import { computed, ref } from 'vue';

const deferred = () => {
  let resolve, reject;
  const promise = new Promise((yes, no) => { resolve = yes; reject = no; });
  return { promise, resolve, reject };
};

function load(relative, globals, imports = {}) {
  const source = readFileSync(new URL('../app/' + relative, import.meta.url), 'utf8');
  const code = ts.transpileModule(source, {
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
  }).outputText;
  const exports = {};
  vm.runInNewContext(code, {
    exports, console: { error() {} }, ...globals,
    require(name) {
      if (name === 'vue') return { ref, computed };
      if (name === 'pinia') return { defineStore };
      assert.ok(name in imports, 'Unexpected import: ' + name);
      return imports[name];
    },
  });
  return exports;
}

function session(profileList) {
  setActivePinia(createPinia());
  const token = ref('session-a');
  const state = new Map();
  const useState = (key, init) => {
    if (!state.has(key)) state.set(key, ref(init()));
    return state.get(key);
  };
  const { useUserStore } = load('stores/user.ts', {
    useNuxtApp: () => ({ $api: { auth: { profileList } } }),
    useCookie: () => token, useState,
  });
  return { store: useUserStore(), token, state, useState };
}

test('profile initialization and concurrent callers share one request', async () => {
  const response = deferred();
  let calls = 0;
  const { store } = session(() => { calls++; return response.promise; });
  const requests = [store.init(), store.init(), store.fetchCurrentUser()];
  assert.equal(calls, 1);
  response.resolve({ data: { id: 'a', username: 'Alice' } });
  await Promise.all(requests);
  await store.init();
  assert.equal(calls, 1);
  assert.equal(store.user.username, 'Alice');
});

test('explicit refresh updates a cached profile', async () => {
  let calls = 0;
  const { store } = session(async () => ({ data: { id: 'a', username: 'name-' + ++calls } }));
  await store.init();
  await store.fetchCurrentUser(true);
  assert.equal(calls, 2);
  assert.equal(store.user.username, 'name-2');
});

test('logout clears permissions and ignores a late successful response', async () => {
  const response = deferred();
  const { store, token, state } = session(() => response.promise);
  state.get('workspace-permissions').value = ['user'];
  state.get('workspace-permissions-tenant').value = 'tenant-a';
  const request = store.init();
  store.logout();
  response.resolve({ data: { id: 'a', username: 'Alice' } });
  await request;
  assert.equal(store.user, null);
  assert.equal(token.value, null);
  assert.equal(state.get('workspace-permissions').value.length, 0);
  assert.equal(state.get('workspace-permissions-tenant').value, '');
});

test('old account response cannot replace a new login', async () => {
  const old = deferred();
  let calls = 0;
  const { store } = session(() => ++calls === 1 ? old.promise : Promise.resolve({ data: { id: 'b', username: 'Bob' } }));
  const oldRequest = store.init();
  await store.setToken('session-b');
  old.resolve({ data: { id: 'a', username: 'Alice' } });
  await oldRequest;
  assert.equal(store.user.id, 'b');
});

test('old account failure cannot log out a new account', async () => {
  const old = deferred();
  let calls = 0;
  const { store, token } = session(() => ++calls === 1 ? old.promise : Promise.resolve({ data: { id: 'b', username: 'Bob' } }));
  const oldRequest = store.init();
  await store.setToken('session-b');
  old.reject(new Error('expired old session'));
  await oldRequest;
  assert.equal(store.user.id, 'b');
  assert.equal(token.value, 'session-b');
});

test('same-token re-login is protected from a previous session generation', async () => {
  const old = deferred();
  let calls = 0;
  const { store } = session(() => ++calls === 1 ? old.promise : Promise.resolve({ data: { id: 'a', username: 'Updated' } }));
  const oldRequest = store.init();
  store.logout();
  await store.setToken('session-a');
  old.resolve({ data: { id: 'a', username: 'Old' } });
  await oldRequest;
  assert.equal(store.user.username, 'Updated');
});

test('anonymous initialization makes no profile request', async () => {
  let calls = 0;
  const { store, token } = session(async () => { calls++; });
  token.value = null;
  await store.init();
  assert.equal(calls, 0);
});

test('current profile failure clears session and permits a new login', async () => {
  let calls = 0;
  const { store, token } = session(async () => {
    if (++calls === 1) throw new Error('expired');
    return { data: { id: 'b', username: 'Bob' } };
  });
  await store.init();
  assert.equal(token.value, null);
  await store.setToken('session-b');
  assert.equal(store.user.id, 'b');
});

function apiPlugin(fetch) {
  let intercept;
  let rejectResponse;
  let redirects = 0;
  class Api {
    instance = { interceptors: {
      request: { use(fn) { intercept = fn; } },
      response: { use(_success, failure) { rejectResponse = failure; } },
    } };
  }
  const plugin = load('plugins/api.ts', {
    defineNuxtPlugin: (fn) => fn, fetch, AbortSignal,
    useCookie: () => ({ value: 'test-token' }), navigateTo() { redirects++; },
  }, { '~/utils/backEnd': { Api } }).default;
  return {
    result: plugin(), intercept: (config = {}) => intercept(config),
    rejectResponse: (error) => rejectResponse(error), redirects: () => redirects,
  };
}

test('API plugin renders synchronously while requests await runtime override', async () => {
  const config = deferred();
  const api = apiPlugin(() => config.promise);
  assert.equal(typeof api.result.then, 'undefined');
  assert.ok(api.result.provide.api);
  let completed = false;
  const request = api.intercept({ headers: {} }).then(value => { completed = true; return value; });
  await Promise.resolve();
  assert.equal(completed, false);
  config.resolve({ ok: true, json: async () => ({ apiBaseUrl: '/custom-api' }) });
  const result = await request;
  assert.equal(result.baseURL, '/custom-api');
  assert.equal(result.headers.Authorization, 'Bearer test-token');
});

test('missing runtime configuration falls back to same-origin API', async () => {
  const api = apiPlugin(async () => ({ ok: false }));
  assert.equal((await api.intercept({ headers: {} })).baseURL, '/api');
});

test('invalid configuration and network errors retain safe default', async () => {
  for (const fetch of [
    async () => ({ ok: true, json: async () => ({ apiBaseUrl: 123 }) }),
    async () => { throw new Error('offline'); },
  ]) {
    const api = apiPlugin(fetch);
    assert.equal((await api.intercept({ headers: {} })).baseURL, '/api');
  }
});

test('old unauthorized request cannot redirect the current session', async () => {
  const api = apiPlugin(async () => ({ ok: true, json: async () => ({}) }));
  const error = { response: { status: 401 }, config: { headers: { Authorization: 'Bearer old-token' } } };
  await assert.rejects(api.rejectResponse(error), value => value === error);
  assert.equal(api.redirects(), 0);
});

test('current unauthorized request redirects and still rejects for caller cleanup', async () => {
  const api = apiPlugin(async () => ({ ok: true, json: async () => ({}) }));
  const error = { response: { status: 401 }, config: { headers: { Authorization: 'Bearer test-token' } } };
  await assert.rejects(api.rejectResponse(error), value => value === error);
  assert.equal(api.redirects(), 1);
});

test('permissions from a previous token are discarded', async () => {
  const response = deferred();
  const { token, useState } = session(async () => ({}));
  const { useWorkspaceAccess } = load('composables/useWorkspaceAccess.ts', {
    useRoute: () => ({ params: { tenantId: 'tenant-a' } }),
    useNuxtApp: () => ({ $api: { tenant: { rolePermissionMeList: () => response.promise } } }),
    useCookie: () => token, useState, computed,
  });
  const access = useWorkspaceAccess();
  const pending = access.loadPermissions();
  token.value = 'session-b';
  response.resolve({ data: ['user'] });
  await pending;
  assert.equal(access.can('user'), false);
});

test('permissions from a previous tenant are discarded', async () => {
  const response = deferred();
  const { token, useState } = session(async () => ({}));
  const route = { params: { tenantId: ref('tenant-a') } };
  const { useWorkspaceAccess } = load('composables/useWorkspaceAccess.ts', {
    useRoute: () => ({ params: { get tenantId() { return route.params.tenantId.value; } } }),
    useNuxtApp: () => ({ $api: { tenant: { rolePermissionMeList: () => response.promise } } }),
    useCookie: () => token, useState, computed,
  });
  const access = useWorkspaceAccess();
  const pending = access.loadPermissions();
  route.params.tenantId.value = 'tenant-b';
  response.resolve({ data: ['user'] });
  await pending;
  assert.equal(access.can('user'), false);
});
