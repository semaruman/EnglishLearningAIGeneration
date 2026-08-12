import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { wordSetsApi } from '../api/wordSetsApi.js';
import { $, el, setChildren } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { loadingSpinner } from '../components/loading.js';
import { emptyState } from '../components/emptyState.js';
import { formatDate } from '../utils/format.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountNav();
  initWordSets();
}

async function initWordSets() {
  const root = $('#word-sets-root');
  setChildren(root, [loadingSpinner({ label: 'Loading word sets…', large: true })]);

  try {
    const data = await wordSetsApi.list();
    const items = Array.isArray(data) ? data : data?.items ?? data?.Items ?? [];

    if (!items.length) {
      setChildren(root, [
        emptyState({
          title: 'No word sets yet',
          description: 'Curated sets will appear here when available.',
          icon: 'book',
        }),
      ]);
      return;
    }

    setChildren(
      root,
      el(
        'div',
        { className: 'grid sm:grid-cols-2 lg:grid-cols-3 gap-4 stagger' },
        items.map((set) => renderSetCard(set)),
      ),
    );
  } catch (err) {
    toast(err.message || 'Failed to load sets', 'error');
    setChildren(root, [
      emptyState({
        title: 'Could not load sets',
        description: err.message,
        actionLabel: 'Retry',
        onAction: () => initWordSets(),
      }),
    ]);
  }
}

function renderSetCard(set) {
  const id = set.id || set.Id;
  const name = set.name || set.Name || 'Word set';
  const description = set.description || set.Description || '';
  const level = set.level || set.Level || '';
  const category = set.category || set.Category || '';
  const wordCount = set.wordCount ?? set.WordCount ?? 0;
  const created = set.createdAt || set.CreatedAt;

  return el(
    'a',
    {
      href: `word-set.html?id=${encodeURIComponent(id)}`,
      className: 'word-card glass-panel p-5 block no-underline text-[var(--ink)] fade-up',
      'aria-label': `Open set ${name}`,
    },
    [
      el('div', { className: 'flex items-start justify-between gap-2 mb-3' }, [
        el('h2', { className: 'font-display text-xl font-semibold', text: name }),
        level
          ? el('span', { className: 'status-pill status-new shrink-0', text: level })
          : null,
      ]),
      description
        ? el('p', { className: 'text-sm text-muted line-clamp-3 mb-4', text: description })
        : el('p', { className: 'text-sm text-muted mb-4', text: 'Open to browse words.' }),
      el('div', { className: 'flex flex-wrap gap-2 text-xs text-muted' }, [
        category ? el('span', { className: 'status-pill status-learning', text: category }) : null,
        el('span', { text: `${wordCount} words` }),
        created ? el('span', { text: formatDate(created) }) : null,
      ]),
    ],
  );
}
