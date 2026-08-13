import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { wordsApi } from '../api/wordsApi.js';
import { vocabularyApi } from '../api/vocabularyApi.js';
import { $, debounce } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { openModalDialog } from '../components/modal.js';
import { vocabularyCard } from '../components/vocabularyCard.js';
import { renderWordList } from '../components/wordList.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initLibrary();
}

const state = {
  search: '',
  page: 1,
  pageSize: 12,
  vocabIds: new Set(),
};

async function initLibrary() {
  const searchInput = $('#library-search');
  searchInput?.addEventListener(
    'input',
    debounce((e) => {
      state.search = e.target.value.trim();
      state.page = 1;
      loadList();
    }, 350),
  );

  await refreshVocabIds();
  await loadList();
}

async function refreshVocabIds() {
  try {
    const data = await vocabularyApi.list({ page: 1, pageSize: 200 });
    const items = data?.items ?? data?.Items ?? [];
    state.vocabIds = new Set(
      items.map((w) => String(w.wordId || w.WordId || w.id || w.Id)),
    );
  } catch {
    state.vocabIds = new Set();
  }
}

async function loadList() {
  const listRoot = $('#library-list');
  renderWordList(listRoot, { loading: true, items: [] });

  try {
    const data = await wordsApi.list({
      search: state.search || undefined,
      page: state.page,
      pageSize: state.pageSize,
    });
    const items = data?.items ?? data?.Items ?? [];
    const page = data?.page ?? data?.Page ?? state.page;
    const totalPages = data?.totalPages ?? data?.TotalPages ?? 1;
    const total = data?.totalCount ?? data?.TotalCount ?? items.length;

    const countEl = $('#library-count');
    if (countEl) countEl.textContent = `${total} word${total === 1 ? '' : 's'}`;

    renderWordList(listRoot, {
      items,
      page,
      totalPages,
      showStatus: false,
      emptyTitle: 'No words found',
      emptyDescription: state.search
        ? 'Try another search term.'
        : 'The global word library is empty.',
      onOpen: (word) => openDetail(word),
      onPageChange: (p) => {
        state.page = p;
        loadList();
      },
    });
  } catch (err) {
    toast(err.message || 'Failed to load library', 'error');
    renderWordList(listRoot, {
      items: [],
      emptyTitle: 'Could not load library',
      emptyDescription: err.message,
      emptyActionLabel: 'Retry',
      onEmptyAction: () => loadList(),
    });
  }
}

function openDetail(word) {
  const id = String(word.id || word.Id);
  const inVocabulary = state.vocabIds.has(id);
  let adding = false;

  const modal = openModalDialog({
    title: 'Dictionary',
    body: vocabularyCard({
      word,
      inVocabulary,
      adding,
      onAdd: async (w) => {
        if (adding) return;
        adding = true;
        try {
          await vocabularyApi.add(w.id || w.Id);
          state.vocabIds.add(String(w.id || w.Id));
          toast('Added to vocabulary', 'success');
          modal.setBody(
            vocabularyCard({
              word: w,
              inVocabulary: true,
            }),
          );
        } catch (err) {
          toast(err.message || 'Failed to add', 'error');
          adding = false;
        }
      },
    }),
  });
}
