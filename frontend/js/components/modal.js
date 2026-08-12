import { el } from '../utils/dom.js';

let openModal = null;

/**
 * Open a modal dialog.
 * @param {{ title: string, body: Node|string, footer?: Node|null, onClose?: () => void }} options
 */
export function openModalDialog({ title, body, footer = null, onClose } = {}) {
  closeModal();

  const backdrop = el('div', {
    className: 'modal-backdrop',
    role: 'dialog',
    'aria-modal': 'true',
    'aria-label': title || 'Dialog',
  });

  const closeBtn = el('button', {
    type: 'button',
    className: 'btn btn-ghost btn-sm',
    'aria-label': 'Close dialog',
    html: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 6 6 18M6 6l12 12"/></svg>`,
  });

  const header = el('div', { className: 'flex items-start justify-between gap-3 mb-4' }, [
    el('h2', { className: 'font-display text-xl font-semibold text-[var(--ink)]', text: title || '' }),
    closeBtn,
  ]);

  const bodyWrap = el('div', { className: 'modal-body text-[var(--ink)]' });
  if (typeof body === 'string') bodyWrap.innerHTML = body;
  else if (body) bodyWrap.appendChild(body);

  const panelChildren = [header, bodyWrap];
  if (footer) {
    panelChildren.push(el('div', { className: 'mt-5 flex flex-wrap gap-2 justify-end' }, [footer]));
  }

  const panel = el('div', { className: 'modal-panel glass-strong p-5 sm:p-6' }, panelChildren);
  backdrop.appendChild(panel);
  document.body.appendChild(backdrop);

  const api = {
    backdrop,
    panel,
    setBody(node) {
      bodyWrap.replaceChildren();
      if (typeof node === 'string') bodyWrap.innerHTML = node;
      else if (node) bodyWrap.appendChild(node);
    },
    close() {
      document.removeEventListener('keydown', onKey);
      backdrop.classList.remove('is-open');
      window.setTimeout(() => {
        backdrop.remove();
        if (openModal === api) openModal = null;
        onClose?.();
      }, 220);
    },
  };

  function onKey(e) {
    if (e.key === 'Escape') api.close();
  }

  closeBtn.addEventListener('click', () => api.close());
  backdrop.addEventListener('click', (e) => {
    if (e.target === backdrop) api.close();
  });
  document.addEventListener('keydown', onKey);

  openModal = api;
  requestAnimationFrame(() => backdrop.classList.add('is-open'));
  closeBtn.focus();
  return api;
}

export function closeModal() {
  if (openModal) openModal.close();
}

export const modal = { open: openModalDialog, close: closeModal };
