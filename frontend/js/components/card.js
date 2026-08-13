import { el } from '../utils/dom.js';

/**
 * @param {{ title?: string, subtitle?: string, children?: Node|Node[], className?: string, onClick?: (e: Event) => void }} opts
 */
export function card({ title, subtitle, children, className = '', onClick } = {}) {
  const header =
    title || subtitle
      ? el('div', { className: 'mb-3' }, [
          title
            ? el('h3', { className: 'font-display text-lg font-semibold text-[var(--ink)]', text: title })
            : null,
          subtitle ? el('p', { className: 'text-sm text-muted mt-0.5', text: subtitle }) : null,
        ])
      : null;

  const body = el('div', { className: 'card-body' });
  const list = Array.isArray(children) ? children : children ? [children] : [];
  list.forEach((c) => body.appendChild(c));

  const node = el(
    onClick ? 'button' : 'div',
    {
      type: onClick ? 'button' : undefined,
      className: `glass-panel tilt-card p-5 ${onClick ? 'text-left w-full cursor-pointer' : ''} ${className}`.trim(),
      onClick,
    },
    [header, body],
  );

  return node;
}
