import { el, setChildren } from '../utils/dom.js';

const NAV_ITEMS = [
  {
    href: 'index.html',
    label: 'Dashboard',
    short: 'Home',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="m3 10 9-7 9 7"/><path d="M5 10v10a1 1 0 0 0 1 1h4v-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v5h4a1 1 0 0 0 1-1V10"/></svg>`,
  },
  {
    href: 'vocabulary.html',
    label: 'Vocabulary',
    short: 'Words',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/></svg>`,
  },
  {
    href: 'learn.html',
    label: 'Learn',
    short: 'Learn',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M12 20h9"/><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>`,
  },
  {
    href: 'practice.html',
    label: 'Practice',
    short: 'Practice',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M12 3v3M12 18v3M3 12h3M18 12h3"/><circle cx="12" cy="12" r="4"/></svg>`,
  },
  {
    href: 'library.html',
    label: 'Library',
    short: 'Library',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M4 4h6v16H4zM14 4h6v16h-6z"/></svg>`,
  },
  {
    href: 'word-sets.html',
    label: 'Sets',
    short: 'Sets',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><rect x="3" y="3" width="7" height="7" rx="1.5"/><rect x="14" y="3" width="7" height="7" rx="1.5"/><rect x="3" y="14" width="7" height="7" rx="1.5"/><rect x="14" y="14" width="7" height="7" rx="1.5"/></svg>`,
  },
  {
    href: 'profile.html',
    label: 'Profile',
    short: 'Profile',
    icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="12" cy="8" r="4"/><path d="M4 20a8 8 0 0 1 16 0"/></svg>`,
  },
];

function currentPage() {
  const parts = window.location.pathname.split('/');
  return parts[parts.length - 1] || 'index.html';
}

function isActive(href, page) {
  if (href === 'index.html') return page === 'index.html' || page === '' || page === 'frontend';
  if (href === 'word-sets.html') return page === 'word-sets.html' || page === 'word-set.html';
  return page === href;
}

function linkNode(item, { mobile = false } = {}) {
  const page = currentPage();
  const active = isActive(item.href, page);
  return el('a', {
    href: item.href,
    className: `nav-link ${active ? 'is-active' : ''}`,
    'aria-current': active ? 'page' : undefined,
    'aria-label': item.label,
    html: `${item.icon}<span>${mobile ? item.short : item.label}</span>`,
  });
}

/**
 * Mount desktop top nav + mobile bottom nav into #app-nav and #bottom-nav.
 */
export function mountNav() {
  const topHost = document.getElementById('app-nav');
  const bottomHost = document.getElementById('bottom-nav');

  if (topHost) {
    const brand = el('a', {
      href: 'index.html',
      className: 'brand-mark text-xl text-[var(--ink)] no-underline shrink-0',
      'aria-label': 'LexiFlow home',
      text: 'LexiFlow',
    });

    const links = el(
      'nav',
      { className: 'flex items-center gap-0.5 overflow-x-auto', 'aria-label': 'Main' },
      NAV_ITEMS.map((item) => linkNode(item)),
    );

    setChildren(
      topHost,
      [
        el('div', {
          className: 'desktop-nav glass-nav w-full items-center justify-between px-6 lg:px-10',
        }, [brand, links]),
      ],
    );
  }

  if (bottomHost) {
    setChildren(
      bottomHost,
      [
        el(
          'nav',
          {
            className: 'mobile-bottom-nav glass-bottom-nav items-stretch justify-around px-1',
            'aria-label': 'Mobile main',
          },
          NAV_ITEMS.map((item) => linkNode(item, { mobile: true })),
        ),
      ],
    );
  }
}
