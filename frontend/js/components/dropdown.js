import { el } from '../utils/dom.js';

/**
 * Native select styled as glass field.
 * @param {{
 *   id: string,
 *   label: string,
 *   options: Array<{ value: string, label: string }>,
 *   value?: string,
 *   name?: string,
 *   required?: boolean,
 *   onChange?: (e: Event) => void
 * }} opts
 */
export function selectField({
  id,
  label,
  options = [],
  value = '',
  name,
  required = false,
  onChange,
} = {}) {
  const select = el(
    'select',
    {
      id,
      name: name || id,
      className: 'select-field',
      required,
      'aria-label': label,
      onChange,
    },
    options.map((opt) =>
      el('option', {
        value: opt.value,
        text: opt.label,
        selected: String(opt.value) === String(value),
      }),
    ),
  );

  return el('div', { className: 'w-full' }, [
    el('label', { className: 'input-label', for: id, text: label }),
    select,
  ]);
}

/**
 * Custom dropdown (optional alternative).
 */
export function dropdown({
  id,
  label,
  options = [],
  value = '',
  onChange,
} = {}) {
  let current = value || options[0]?.value || '';
  const trigger = el('button', {
    type: 'button',
    id,
    className: 'select-field text-left flex items-center justify-between gap-2',
    'aria-haspopup': 'listbox',
    'aria-expanded': 'false',
    'aria-label': label,
  });

  const labelSpan = el('span', { text: options.find((o) => o.value === current)?.label || 'Select' });
  trigger.append(
    labelSpan,
    el('span', {
      className: 'text-muted',
      html: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m6 9 6 6 6-6"/></svg>`,
    }),
  );

  const menu = el(
    'div',
    { className: 'dropdown-menu', role: 'listbox' },
    options.map((opt) =>
      el('button', {
        type: 'button',
        className: `dropdown-item ${opt.value === current ? 'is-selected' : ''}`,
        role: 'option',
        'aria-selected': opt.value === current,
        text: opt.label,
        onClick: (e) => {
          e.stopPropagation();
          current = opt.value;
          labelSpan.textContent = opt.label;
          root.classList.remove('is-open');
          trigger.setAttribute('aria-expanded', 'false');
          $$items().forEach((btn) => {
            const selected = btn.textContent === opt.label;
            btn.classList.toggle('is-selected', selected);
            btn.setAttribute('aria-selected', selected ? 'true' : 'false');
          });
          onChange?.(current);
        },
      }),
    ),
  );

  function $$items() {
    return Array.from(menu.querySelectorAll('.dropdown-item'));
  }

  const root = el('div', { className: 'dropdown w-full' }, [
    el('label', { className: 'input-label', for: id, text: label }),
    trigger,
    menu,
  ]);

  trigger.addEventListener('click', () => {
    const open = root.classList.toggle('is-open');
    trigger.setAttribute('aria-expanded', open ? 'true' : 'false');
  });

  document.addEventListener('click', (e) => {
    if (!root.contains(e.target)) {
      root.classList.remove('is-open');
      trigger.setAttribute('aria-expanded', 'false');
    }
  });

  return {
    root,
    getValue: () => current,
    setValue: (v) => {
      current = v;
      const opt = options.find((o) => o.value === v);
      if (opt) labelSpan.textContent = opt.label;
    },
  };
}
