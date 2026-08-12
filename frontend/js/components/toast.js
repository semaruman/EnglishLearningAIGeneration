import { el } from '../utils/dom.js';

let host;

function ensureHost() {
  if (!host) {
    host = document.getElementById('toast-host');
    if (!host) {
      host = el('div', { id: 'toast-host', className: 'toast-host', 'aria-live': 'polite' });
      document.body.appendChild(host);
    }
  }
  return host;
}

/**
 * @param {string} message
 * @param {'success'|'error'|'info'} [type]
 * @param {number} [duration]
 */
export function toast(message, type = 'info', duration = 3200) {
  const node = el('div', {
    className: `toast toast-${type}`,
    role: 'status',
    text: message,
  });
  ensureHost().appendChild(node);
  window.setTimeout(() => {
    node.style.opacity = '0';
    node.style.transform = 'translateY(-4px)';
    node.style.transition = 'opacity 0.2s ease, transform 0.2s ease';
    window.setTimeout(() => node.remove(), 220);
  }, duration);
  return node;
}

export const showToast = toast;
