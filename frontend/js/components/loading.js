import { el } from '../utils/dom.js';

export function loadingSpinner({ label = 'Loading…', large = false } = {}) {
  return el('div', {
    className: 'flex flex-col items-center justify-center gap-3 py-10',
    role: 'status',
    'aria-live': 'polite',
    'aria-label': label,
  }, [
    el('div', { className: `spinner ${large ? 'spinner-lg' : ''}`, 'aria-hidden': 'true' }),
    el('p', { className: 'text-sm text-muted', text: label }),
  ]);
}

export function skeletonBlock({ className = 'h-24 w-full' } = {}) {
  return el('div', { className: `skeleton ${className}`, 'aria-hidden': 'true' });
}

export function glassSkeleton({ rows = 3 } = {}) {
  return el('div', { className: 'skeleton-panel space-y-3', role: 'status', 'aria-label': 'Loading' }, [
    skeletonBlock({ className: 'h-6 w-1/3' }),
    ...Array.from({ length: rows }, (_, i) =>
      skeletonBlock({ className: i === rows - 1 ? 'h-16 w-2/3' : 'h-20 w-full' }),
    ),
  ]);
}

export function inlineSpinner() {
  return el('span', {
    className: 'spinner inline-block align-middle',
    style: { width: '1.1rem', height: '1.1rem', borderWidth: '2px' },
    'aria-hidden': 'true',
  });
}
