import { el } from '../utils/dom.js';

/**
 * Mount animated ambient light blobs behind the UI.
 * Safe to call once per page.
 */
export function mountAmbientBackground(host = document.body) {
  if (document.querySelector('.ambient-stage')) return;

  const stage = el('div', {
    className: 'ambient-stage',
    'aria-hidden': 'true',
  }, [
    el('div', { className: 'ambient-blob ambient-blob--a' }),
    el('div', { className: 'ambient-blob ambient-blob--b' }),
    el('div', { className: 'ambient-blob ambient-blob--c' }),
    el('div', { className: 'ambient-blob ambient-blob--d' }),
  ]);

  host.insertBefore(stage, host.firstChild);
}
