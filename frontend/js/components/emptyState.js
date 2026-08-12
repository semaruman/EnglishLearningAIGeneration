import { el } from '../utils/dom.js';
import { button } from './button.js';

const ICONS = {
  book: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>`,
  search: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg>`,
  spark: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6"><path d="M12 3v3M12 18v3M3 12h3M18 12h3M5.6 5.6l2.1 2.1M16.3 16.3l2.1 2.1M18.4 5.6l-2.1 2.1M7.7 16.3l-2.1 2.1"/><circle cx="12" cy="12" r="3"/></svg>`,
};

/**
 * @param {{ title: string, description?: string, actionLabel?: string, onAction?: () => void, icon?: keyof typeof ICONS }} opts
 */
export function emptyState({
  title,
  description = '',
  actionLabel,
  onAction,
  icon = 'book',
} = {}) {
  return el('div', { className: 'empty-state glass-panel fade-in' }, [
    el('div', { html: ICONS[icon] || ICONS.book, 'aria-hidden': 'true' }),
    el('h3', { className: 'font-display text-xl font-semibold mb-1', text: title }),
    description
      ? el('p', { className: 'text-muted text-sm max-w-sm mx-auto mb-4', text: description })
      : null,
    actionLabel && onAction
      ? button({ label: actionLabel, onClick: onAction, className: 'mx-auto' })
      : null,
  ]);
}
