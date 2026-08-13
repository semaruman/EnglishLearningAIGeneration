import { mountAmbientBackground } from './ambientBackground.js';
import { attachCardTilt, mountScrollReveal } from '../utils/motion.js';

/**
 * Global visual shell: ambient lights, page enter, tilt, scroll reveal.
 * Call once from each page entry.
 */
export function mountAppShell({ tilt = true } = {}) {
  mountAmbientBackground();

  const main = document.querySelector('.page-main, .auth-shell');
  if (main && !main.classList.contains('page-enter')) {
    main.classList.add('page-enter');
  }

  // Defer interaction helpers until DOM content for the page is ready
  requestAnimationFrame(() => {
    if (tilt) attachCardTilt(document);
    mountScrollReveal(document);
  });
}
