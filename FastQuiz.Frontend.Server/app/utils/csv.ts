const FORMULA_PREFIX_PATTERN = /^[\t\r=+\-@]/;

/**
 * Neutralize spreadsheet formula triggers before applying RFC 4180 CSV escaping.
 */
export function escapeCsvCell(value: unknown): string {
  const raw = value === null || value === undefined ? "" : String(value);
  const trimmedStart = raw.replace(/^\s+/, "");
  const safe =
    FORMULA_PREFIX_PATTERN.test(raw) || FORMULA_PREFIX_PATTERN.test(trimmedStart)
      ? `'${raw}`
      : raw;
  return /[",\n\r]/.test(safe) ? `"${safe.replace(/"/g, '""')}"` : safe;
}

export function sanitizeFilename(name: string): string {
  return name.replace(/[\\/:*?"<>|\x00-\x1f]/g, "_");
}
