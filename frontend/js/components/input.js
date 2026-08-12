import { el } from '../utils/dom.js';

/**
 * @param {{
 *   id: string,
 *   label: string,
 *   type?: string,
 *   name?: string,
 *   placeholder?: string,
 *   value?: string,
 *   required?: boolean,
 *   autocomplete?: string,
 *   className?: string,
 *   hint?: string,
 *   onInput?: (e: Event) => void
 * }} opts
 */
export function inputField({
  id,
  label,
  type = 'text',
  name,
  placeholder = '',
  value = '',
  required = false,
  autocomplete,
  className = '',
  hint,
  onInput,
} = {}) {
  const input = el('input', {
    id,
    name: name || id,
    type,
    className: `input-field ${className}`.trim(),
    placeholder,
    value,
    required,
    autocomplete,
    'aria-label': label,
    onInput,
  });

  return el('div', { className: 'w-full' }, [
    el('label', { className: 'input-label', for: id, text: label }),
    input,
    hint ? el('p', { className: 'text-xs text-muted mt-1.5', text: hint }) : null,
  ]);
}

/**
 * Textarea field with label.
 */
export function textareaField({
  id,
  label,
  name,
  placeholder = '',
  value = '',
  required = false,
  rows = 3,
  className = '',
  onInput,
} = {}) {
  const ta = el('textarea', {
    id,
    name: name || id,
    className: `input-field resize-y min-h-[5rem] ${className}`.trim(),
    placeholder,
    required,
    rows,
    'aria-label': label,
    onInput,
  });
  ta.value = value;

  return el('div', { className: 'w-full' }, [
    el('label', { className: 'input-label', for: id, text: label }),
    ta,
  ]);
}
