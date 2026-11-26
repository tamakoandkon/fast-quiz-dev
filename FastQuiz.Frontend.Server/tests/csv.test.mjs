import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
import ts from 'typescript';

const source = readFileSync(new URL('../app/utils/csv.ts', import.meta.url), 'utf8');
const exports = {};
vm.runInNewContext(
  ts.transpileModule(source, {
    compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022 },
  }).outputText,
  { exports },
);

const { escapeCsvCell, sanitizeFilename } = exports;

test('escapeCsvCell neutralizes spreadsheet formula triggers while preserving safe values', () => {
  for (const payload of ['=CMD|"/c calc"!A0', '+SUM(1,2)', '-1+1', '@SUM(1,1)', '\t=1', '\r=1', '  =1']) {
    const escaped = escapeCsvCell(payload);
    const unquoted = escaped.startsWith('"') ? escaped.slice(1, -1).replace(/""/g, '"') : escaped;
    assert.ok(unquoted.startsWith("'"), `Expected formula prefix quote for ${payload}, got ${escaped}`);
  }
  assert.equal(escapeCsvCell('普通文本'), '普通文本');
  assert.equal(escapeCsvCell(95), '95');
  assert.equal(escapeCsvCell(null), '');
  assert.equal(escapeCsvCell('a, "b"'), '"a, ""b"""');
});

test('sanitizeFilename replaces path separators and control characters', () => {
  assert.equal(sanitizeFilename('../../evil:name?.csv'), '.._.._evil_name_.csv');
});
