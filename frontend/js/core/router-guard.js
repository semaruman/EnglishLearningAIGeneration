import { appState } from './appState.js';

const AUTH_PAGES = new Set(['login.html', 'register.html']);

function currentPage() {
  const parts = window.location.pathname.split('/');
  return parts[parts.length - 1] || 'index.html';
}

/**
 * Redirect unauthenticated users away from protected pages,
 * and authenticated users away from login/register.
 */
export function guardRoute({ requireAuth = true } = {}) {
  const page = currentPage();
  const authed = appState.isAuthenticated();

  if (requireAuth && !authed) {
    const next = encodeURIComponent(page);
    window.location.replace(`login.html?next=${next}`);
    return false;
  }

  if (!requireAuth && authed && AUTH_PAGES.has(page)) {
    window.location.replace('index.html');
    return false;
  }

  return true;
}
