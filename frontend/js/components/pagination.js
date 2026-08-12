import { el } from '../utils/dom.js';

/**
 * @param {{ page: number, totalPages: number, onChange: (page: number) => void }} opts
 */
export function pagination({ page = 1, totalPages = 1, onChange } = {}) {
  if (totalPages <= 1) {
    return el('div', { className: 'hidden', 'aria-hidden': 'true' });
  }

  const pages = [];
  const windowSize = 5;
  let start = Math.max(1, page - Math.floor(windowSize / 2));
  let end = Math.min(totalPages, start + windowSize - 1);
  start = Math.max(1, end - windowSize + 1);

  for (let i = start; i <= end; i += 1) pages.push(i);

  const nav = el('nav', {
    className: 'flex items-center justify-center gap-1.5 mt-6',
    'aria-label': 'Pagination',
  });

  const prev = el('button', {
    type: 'button',
    className: 'page-btn',
    disabled: page <= 1,
    'aria-label': 'Previous page',
    text: '‹',
    onClick: () => onChange?.(page - 1),
  });

  const next = el('button', {
    type: 'button',
    className: 'page-btn',
    disabled: page >= totalPages,
    'aria-label': 'Next page',
    text: '›',
    onClick: () => onChange?.(page + 1),
  });

  nav.append(
    prev,
    ...pages.map((p) =>
      el('button', {
        type: 'button',
        className: `page-btn ${p === page ? 'is-active' : ''}`,
        'aria-label': `Page ${p}`,
        'aria-current': p === page ? 'page' : undefined,
        text: String(p),
        onClick: () => onChange?.(p),
      }),
    ),
    next,
  );

  return nav;
}
