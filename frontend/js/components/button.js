import { el } from '../utils/dom.js';

/**
 * @param {{
 *   label: string,
 *   variant?: 'primary'|'secondary'|'ghost'|'danger'|'know'|'know-well'|'dont-know',
 *   size?: 'sm'|'md'|'lg',
 *   type?: string,
 *   disabled?: boolean,
 *   className?: string,
 *   onClick?: (e: Event) => void,
 *   attrs?: Record<string, any>
 * }} opts
 */
export function button({
  label,
  variant = 'primary',
  size = 'md',
  type = 'button',
  disabled = false,
  className = '',
  onClick,
  attrs = {},
} = {}) {
  const sizeClass = size === 'sm' ? 'btn-sm' : size === 'lg' ? 'btn-lg' : '';
  const variantClass = {
    primary: 'btn-primary',
    secondary: 'btn-secondary',
    ghost: 'btn-ghost',
    danger: 'btn-danger',
    know: 'btn-know',
    'know-well': 'btn-know-well',
    'dont-know': 'btn-dont-know',
  }[variant] || 'btn-primary';

  return el('button', {
    type,
    className: `btn ${variantClass} ${sizeClass} ${className}`.trim(),
    disabled,
    text: label,
    onClick,
    ...attrs,
  });
}
