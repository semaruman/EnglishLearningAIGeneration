import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { authService } from '../services/authService.js';
import { appState } from '../core/appState.js';
import { $, el, setChildren } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { loadingSpinner } from '../components/loading.js';
import { button } from '../components/button.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initProfile();
}

async function initProfile() {
  const root = $('#profile-root');
  setChildren(root, [loadingSpinner({ label: 'Loading profile…' })]);

  let user = appState.getUser();
  try {
    user = await authService.refreshMe();
  } catch (err) {
    if (!user) {
      toast(err.message || 'Failed to load profile', 'error');
    }
  }

  if (!user) {
    setChildren(root, [
      el('div', { className: 'glass-panel p-6 text-center text-muted', text: 'Unable to load profile.' }),
    ]);
    return;
  }

  const initial = (user.userName || user.email || '?').charAt(0).toUpperCase();

  setChildren(root, [
    el('div', { className: 'max-w-lg mx-auto fade-up' }, [
      el('header', { className: 'mb-6' }, [
        el('p', { className: 'section-eyebrow', text: 'Profile' }),
        el('h1', { className: 'font-display text-3xl font-semibold', text: 'Your account' }),
      ]),
      el('section', { className: 'glass-panel tilt-card p-6 sm:p-8' }, [
        el('div', { className: 'flex items-center gap-4 mb-6' }, [
          el('div', {
            className:
              'w-16 h-16 rounded-2xl flex items-center justify-center font-display text-2xl font-semibold text-white',
            style: {
              background: 'linear-gradient(135deg, #5ba3ff, #4ecdc9)',
              boxShadow: '0 12px 32px rgba(91, 163, 255, 0.28)',
            },
            'aria-hidden': 'true',
            text: initial,
          }),
          el('div', {}, [
            el('p', {
              className: 'font-display text-2xl font-semibold',
              text: user.userName || 'Learner',
            }),
            el('p', { className: 'text-muted text-sm', text: user.email || '' }),
          ]),
        ]),
        el('dl', { className: 'space-y-3 text-sm mb-8' }, [
          row('Username', user.userName || '—'),
          row('Email', user.email || '—'),
          row('User ID', user.userId || '—'),
        ]),
        button({
          label: 'Log out',
          variant: 'danger',
          onClick: () => authService.logout(),
          attrs: { 'aria-label': 'Log out of AI English Learning' },
        }),
      ]),
    ]),
  ]);
}

function row(label, value) {
  return el('div', {
    className: 'flex justify-between gap-4 py-2 border-b border-white/40',
  }, [
    el('dt', { className: 'text-muted font-semibold', text: label }),
    el('dd', { className: 'text-right break-all', text: value }),
  ]);
}
