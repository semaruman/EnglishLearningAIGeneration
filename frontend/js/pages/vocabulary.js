import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { vocabularyApi } from '../api/vocabularyApi.js';
import { wordsApi } from '../api/wordsApi.js';
import { $, el, debounce } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { openModalDialog, closeModal } from '../components/modal.js';
import { vocabularyCard } from '../components/vocabularyCard.js';
import { renderWordList } from '../components/wordList.js';
import { button } from '../components/button.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initVocabulary();
}

const state = {
  status: '',
  search: '',
  page: 1,
  pageSize: 12,
  loading: false,
};

async function initVocabulary() {
  const searchInput = $('#vocab-search');
  const addForm = $('#add-by-text-form');
  const filters = document.querySelectorAll('[data-status-filter]');

  filters.forEach((chip) => {
    chip.addEventListener('click', () => {
      filters.forEach((c) => c.classList.remove('is-active'));
      chip.classList.add('is-active');
      state.status = chip.dataset.statusFilter || '';
      state.page = 1;
      loadList();
    });
  });

  searchInput?.addEventListener(
    'input',
    debounce((e) => {
      state.search = e.target.value.trim();
      state.page = 1;
      loadList();
    }, 350),
  );

  addForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const input = $('#add-word-text');
    const wordText = input?.value?.trim();
    if (!wordText) {
      toast('Enter a word', 'error');
      return;
    }
    const btn = $('#add-word-btn');
    btn.disabled = true;
    try {
      const result = await wordsApi.addByText(wordText);
      const existed = result.alreadyExisted ?? result.AlreadyExisted;
      toast(
        existed
          ? `"${result.wordText || wordText}" is already in your vocabulary`
          : `Added "${result.wordText || wordText}"`,
        existed ? 'info' : 'success',
      );
      input.value = '';
      state.page = 1;
      await loadList();
    } catch (err) {
      toast(err.message || 'Could not add word', 'error');
    } finally {
      btn.disabled = false;
    }
  });

  await loadList();
}

async function loadList() {
  const listRoot = $('#vocab-list');
  state.loading = true;
  renderWordList(listRoot, {
    loading: true,
    items: [],
  });

  try {
    const data = await vocabularyApi.list({
      status: state.status || undefined,
      search: state.search || undefined,
      page: state.page,
      pageSize: state.pageSize,
    });

    const items = data?.items ?? data?.Items ?? [];
    const page = data?.page ?? data?.Page ?? state.page;
    const totalPages = data?.totalPages ?? data?.TotalPages ?? 1;

    state.loading = false;
    renderWordList(listRoot, {
      items,
      page,
      totalPages,
      emptyTitle: 'No vocabulary words',
      emptyDescription: state.search || state.status
        ? 'Try a different search or filter.'
        : 'Add words from the library, a set, or by typing below.',
      emptyActionLabel: 'Browse library',
      onEmptyAction: () => {
        window.location.href = 'library.html';
      },
      onOpen: (word) => openWordDetail(word),
      onDelete: (word) => confirmRemove(word),
      onPageChange: (p) => {
        state.page = p;
        loadList();
      },
      showStatus: true,
    });

    const countEl = $('#vocab-count');
    if (countEl) {
      const total = data?.totalCount ?? data?.TotalCount ?? items.length;
      countEl.textContent = `${total} word${total === 1 ? '' : 's'}`;
    }
  } catch (err) {
    state.loading = false;
    toast(err.message || 'Failed to load vocabulary', 'error');
    renderWordList(listRoot, {
      items: [],
      emptyTitle: 'Could not load vocabulary',
      emptyDescription: err.message,
      emptyActionLabel: 'Retry',
      onEmptyAction: () => loadList(),
    });
  }
}

function openWordDetail(word) {
  openModalDialog({
    title: 'Word details',
    body: vocabularyCard({
      word,
      inVocabulary: true,
      onRemove: async (w) => {
        await removeWord(w);
        closeModal();
      },
    }),
  });
}

async function confirmRemove(word) {
  const text = word.wordText || word.WordText || 'this word';
  const footer = el('div', { className: 'flex gap-2' }, [
    button({
      label: 'Cancel',
      variant: 'secondary',
      size: 'sm',
      onClick: () => closeModal(),
    }),
    button({
      label: 'Remove',
      variant: 'danger',
      size: 'sm',
      onClick: async () => {
        await removeWord(word);
        closeModal();
      },
    }),
  ]);

  openModalDialog({
    title: 'Remove word?',
    body: el('p', {
      className: 'text-sm text-muted',
      text: `Remove “${text}” from your vocabulary? Progress for this word will be lost.`,
    }),
    footer,
  });
}

async function removeWord(word) {
  const id = word.wordId || word.WordId || word.id || word.Id;
  try {
    await vocabularyApi.remove(id);
    toast('Removed from vocabulary', 'success');
    await loadList();
  } catch (err) {
    toast(err.message || 'Failed to remove', 'error');
  }
}
