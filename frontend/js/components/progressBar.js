import { el } from '../utils/dom.js';
import { clampPercent } from '../utils/format.js';

/**
 * @param {{ value: number, max?: number, label?: string, showPercent?: boolean }} opts
 */
export function progressBar({ value = 0, max = 100, label, showPercent = true } = {}) {
  const pct = max > 0 ? clampPercent((value / max) * 100) : 0;

  return el('div', { className: 'w-full', role: 'group', 'aria-label': label || 'Progress' }, [
    (label || showPercent)
      ? el('div', { className: 'flex items-center justify-between mb-2 text-sm' }, [
          label ? el('span', { className: 'font-medium text-[var(--ink)]', text: label }) : el('span'),
          showPercent ? el('span', { className: 'text-muted tabular-nums', text: `${pct}%` }) : null,
        ])
      : null,
    el('div', {
      className: 'progress-track',
      role: 'progressbar',
      'aria-valuenow': pct,
      'aria-valuemin': '0',
      'aria-valuemax': '100',
      'aria-label': label || 'Progress',
    }, [
      el('div', { className: 'progress-fill', style: { width: `${pct}%` } }),
    ]),
  ]);
}
