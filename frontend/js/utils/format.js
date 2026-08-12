const STATUS_LABELS = {
  0: 'New',
  1: 'Learning',
  2: 'Known',
  3: 'Mastered',
  New: 'New',
  Learning: 'Learning',
  Known: 'Known',
  Mastered: 'Mastered',
};

const STATUS_CLASS = {
  0: 'status-new',
  1: 'status-learning',
  2: 'status-known',
  3: 'status-mastered',
  New: 'status-new',
  Learning: 'status-learning',
  Known: 'status-known',
  Mastered: 'status-mastered',
};

const DIFFICULTY_LABELS = {
  0: 'A1',
  1: 'A2',
  2: 'B1',
  3: 'B2',
  4: 'C1',
  5: 'C2',
  A1: 'A1',
  A2: 'A2',
  B1: 'B1',
  B2: 'B2',
  C1: 'C1',
  C2: 'C2',
};

export function statusLabel(status) {
  return STATUS_LABELS[status] ?? String(status ?? '—');
}

export function statusClass(status) {
  return STATUS_CLASS[status] ?? 'status-new';
}

export function difficultyLabel(level) {
  return DIFFICULTY_LABELS[level] ?? String(level ?? '—');
}

export function formatDate(value) {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function formatDateTime(value) {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function plural(n, one, many) {
  return `${n} ${n === 1 ? one : many}`;
}

export function clampPercent(value) {
  if (!Number.isFinite(value)) return 0;
  return Math.max(0, Math.min(100, Math.round(value)));
}

export function getStatusCount(wordsByStatus, key) {
  if (!wordsByStatus) return 0;
  if (typeof wordsByStatus[key] === 'number') return wordsByStatus[key];
  const alt = { New: 0, Learning: 1, Known: 2, Mastered: 3 };
  if (typeof wordsByStatus[alt[key]] === 'number') return wordsByStatus[alt[key]];
  return 0;
}
