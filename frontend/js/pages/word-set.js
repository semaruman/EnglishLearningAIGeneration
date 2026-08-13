import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { wordSetsApi } from '../api/wordSetsApi.js';
import { $, el, setChildren } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { loadingSpinner } from '../components/loading.js';
import { emptyState } from '../components/emptyState.js';
import { button } from '../components/button.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initWordSet();
}

async function initWordSet() {
  const root = $('#word-set-root');
  const params = new URLSearchParams(window.location.search);
  const id = params.get('id');

  if (!id) {
    setChildren(root, [
      emptyState({
        title: 'Set not found',
        description: 'Missing word set id.',
        actionLabel: 'Back to sets',
        onAction: () => {
          window.location.href = 'word-sets.html';
        },
      }),
    ]);
    return;
  }

  setChildren(root, [loadingSpinner({ label: 'Loading set…', large: true })]);

  try {
    const set = await wordSetsApi.getById(id);
    renderSet(root, set);
  } catch (err) {
    toast(err.message || 'Failed to load set', 'error');
    setChildren(root, [
      emptyState({
        title: 'Could not load set',
        description: err.message,
        actionLabel: 'Back to sets',
        onAction: () => {
          window.location.href = 'word-sets.html';
        },
      }),
    ]);
  }
}

function renderSet(root, set) {
  const id = set.id || set.Id;
  const name = set.name || set.Name || 'Word set';
  const description = set.description || set.Description || '';
  const level = set.level || set.Level || '';
  const category = set.category || set.Category || '';
  const items = set.items || set.Items || [];

  const addBtn = button({
    label: 'Add all to vocabulary',
    variant: 'primary',
    onClick: async () => {
      addBtn.disabled = true;
      addBtn.textContent = 'Adding…';
      try {
        const result = await wordSetsApi.addToVocabulary(id);
        const added = result.addedCount ?? result.AddedCount ?? 0;
        const skipped = result.skippedCount ?? result.SkippedCount ?? 0;
        toast(`Added ${added}, skipped ${skipped} (already owned)`, 'success');
      } catch (err) {
        toast(err.message || 'Failed to add set', 'error');
      } finally {
        addBtn.disabled = false;
        addBtn.textContent = 'Add all to vocabulary';
      }
    },
  });

  setChildren(root, [
    el('div', { className: 'fade-up' }, [
      el('a', {
        href: 'word-sets.html',
        className: 'text-sm font-semibold text-[var(--teal-600)] no-underline inline-flex items-center gap-1 mb-4',
        html: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m15 18-6-6 6-6"/></svg> All sets`,
      }),
      el('header', { className: 'glass-panel p-5 sm:p-6 mb-6' }, [
        el('div', { className: 'flex flex-wrap items-start justify-between gap-4' }, [
          el('div', { className: 'min-w-0' }, [
            el('div', { className: 'flex flex-wrap gap-2 mb-2' }, [
              level ? el('span', { className: 'status-pill status-new', text: level }) : null,
              category ? el('span', { className: 'status-pill status-learning', text: category }) : null,
            ]),
            el('h1', { className: 'font-display text-3xl font-semibold', text: name }),
            description
              ? el('p', { className: 'text-muted mt-2 max-w-2xl', text: description })
              : null,
            el('p', {
              className: 'text-sm text-muted mt-3',
              text: `${items.length} word${items.length === 1 ? '' : 's'}`,
            }),
          ]),
          addBtn,
        ]),
      ]),
      items.length
        ? el(
            'ul',
            { className: 'space-y-2 stagger' },
            items.map((item, index) => {
              const text = item.wordText || item.WordText || '';
              const pos = item.partOfSpeech || item.PartOfSpeech || '';
              const translation = item.translation || item.Translation || '';
              const definition = item.definition || item.Definition || '';
              return el('li', {
                className: 'glass-panel px-4 py-3 fade-up flex gap-3',
              }, [
                el('span', {
                  className: 'text-xs font-bold text-muted tabular-nums pt-1 w-6 shrink-0',
                  text: String(item.order ?? item.Order ?? index + 1),
                }),
                el('div', { className: 'min-w-0' }, [
                  el('div', { className: 'flex items-baseline gap-2 flex-wrap' }, [
                    el('span', { className: 'font-display text-lg font-semibold', text }),
                    pos ? el('span', { className: 'text-xs text-muted', text: pos }) : null,
                  ]),
                  translation
                    ? el('p', { className: 'text-sm text-[var(--ink)] mt-0.5', text: translation })
                    : null,
                  definition
                    ? el('p', { className: 'text-sm text-muted mt-0.5', text: definition })
                    : null,
                ]),
              ]);
            }),
          )
        : emptyState({
            title: 'Empty set',
            description: 'This set has no words yet.',
          }),
    ]),
  ]);
}
