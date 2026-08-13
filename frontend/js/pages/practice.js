import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { practiceApi } from '../api/practiceApi.js';
import { wordsApi } from '../api/wordsApi.js';
import { vocabularyApi } from '../api/vocabularyApi.js';
import { $, el, setChildren } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { loadingSpinner } from '../components/loading.js';
import { openModalDialog } from '../components/modal.js';
import { vocabularyCard } from '../components/vocabularyCard.js';
import { button } from '../components/button.js';
import { formatDateTime } from '../utils/format.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initPractice();
}

async function initPractice() {
  const form = $('#practice-form');
  const resultRoot = $('#practice-result');
  const historyRoot = $('#practice-history');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const topic = $('#topic')?.value?.trim();
    const difficulty = $('#difficulty')?.value;
    const length = $('#length')?.value;

    if (!topic) {
      toast('Enter a topic', 'error');
      return;
    }

    const submitBtn = $('#generate-btn');
    submitBtn.disabled = true;
    setChildren(resultRoot, [loadingSpinner({ label: 'Generating practice text…', large: true })]);

    try {
      const result = await practiceApi.generate({ topic, difficulty, length });
      renderResult(resultRoot, result);
      await loadHistory(historyRoot);
    } catch (err) {
      toast(err.message || 'Generation failed', 'error');
      setChildren(resultRoot, [
        el('div', { className: 'glass-panel p-6 text-center text-muted', text: err.message }),
      ]);
    } finally {
      submitBtn.disabled = false;
    }
  });

  $('#generate-another')?.addEventListener('click', () => {
    form?.requestSubmit();
  });

  await loadHistory(historyRoot);
}

function renderResult(root, result) {
  const text = result.generatedText || result.GeneratedText || '';
  const vocab = result.vocabularyUsed || result.VocabularyUsed || [];
  const topic = result.topic || result.Topic || '';
  const difficulty = result.difficulty || result.Difficulty || '';
  const wordCount = result.wordCount ?? result.WordCount ?? 0;

  const vocabSet = new Set(vocab.map((w) => String(w).toLowerCase()));

  const paragraph = el('p', {
    className: 'text-lg leading-relaxed text-[var(--ink)] whitespace-pre-wrap',
    id: 'practice-text',
  });

  // Tokenize roughly into words while preserving punctuation/spacing
  const parts = text.split(/(\s+)/);
  parts.forEach((part) => {
    if (/^\s+$/.test(part) || !part) {
      paragraph.appendChild(document.createTextNode(part));
      return;
    }
    const clean = part.replace(/^[^\p{L}\p{N}']+|[^\p{L}\p{N}']+$/gu, '');
    const isVocab = clean && vocabSet.has(clean.toLowerCase());
    if (isVocab) {
      const btn = el('button', {
        type: 'button',
        className: 'practice-word',
        text: part,
        'aria-label': `Show translation for ${clean}`,
        onClick: () => showWordModal(clean),
      });
      paragraph.appendChild(btn);
    } else {
      paragraph.appendChild(document.createTextNode(part));
    }
  });

  setChildren(root, [
    el('article', { className: 'glass-panel p-5 sm:p-7 fade-up' }, [
      el('div', { className: 'flex flex-wrap items-center justify-between gap-2 mb-4' }, [
        el('div', {}, [
          el('p', { className: 'text-xs font-semibold uppercase tracking-wide text-muted', text: 'Generated text' }),
          el('h2', { className: 'font-display text-xl font-semibold', text: topic }),
        ]),
        el('div', { className: 'flex gap-2 text-xs' }, [
          el('span', { className: 'status-pill status-learning', text: difficulty }),
          el('span', { className: 'status-pill status-new', text: `${wordCount} words` }),
        ]),
      ]),
      paragraph,
      el('p', {
        className: 'text-xs text-muted mt-4',
        text: vocab.length
          ? `Highlighted words are from your vocabulary (${vocab.length}). Tap for translation.`
          : 'No vocabulary words matched this text.',
      }),
      el('div', { className: 'mt-5' }, [
        button({
          label: 'Generate another',
          variant: 'secondary',
          onClick: () => $('#practice-form')?.requestSubmit(),
        }),
      ]),
    ]),
  ]);
}

async function showWordModal(wordText) {
  const modal = openModalDialog({
    title: wordText,
    body: loadingSpinner({ label: 'Looking up…' }),
  });

  try {
    const page = await wordsApi.list({ search: wordText, page: 1, pageSize: 5 });
    const items = page?.items ?? page?.Items ?? [];
    const exact =
      items.find((w) => (w.wordText || w.WordText || '').toLowerCase() === wordText.toLowerCase()) ||
      items[0];

    if (!exact) {
      modal.setBody(
        el('p', {
          className: 'text-sm text-muted',
          text: 'No dictionary entry found for this word.',
        }),
      );
      return;
    }

    let inVocab = false;
    try {
      const vocabPage = await vocabularyApi.list({ search: wordText, page: 1, pageSize: 5 });
      const vItems = vocabPage?.items ?? vocabPage?.Items ?? [];
      inVocab = vItems.some(
        (w) => (w.wordText || w.WordText || '').toLowerCase() === wordText.toLowerCase(),
      );
    } catch {
      /* ignore */
    }

    modal.setBody(
      vocabularyCard({
        word: exact,
        inVocabulary: inVocab,
        onAdd: async (w) => {
          try {
            await vocabularyApi.add(w.id || w.Id);
            toast('Added to vocabulary', 'success');
            modal.close();
          } catch (err) {
            toast(err.message || 'Failed to add', 'error');
          }
        },
      }),
    );
  } catch (err) {
    modal.setBody(el('p', { className: 'text-sm text-red-700', text: err.message }));
  }
}

async function loadHistory(root) {
  if (!root) return;
  setChildren(root, [loadingSpinner({ label: 'Loading history…' })]);
  try {
    const data = await practiceApi.history({ page: 1, pageSize: 5 });
    const items = data?.items ?? data?.Items ?? (Array.isArray(data) ? data : []);

    if (!items.length) {
      setChildren(root, [
        el('p', { className: 'text-sm text-muted', text: 'No practice sessions yet.' }),
      ]);
      return;
    }

    setChildren(
      root,
      el(
        'ul',
        { className: 'space-y-2' },
        items.map((item) => {
          const topic = item.topic || item.Topic || 'Practice';
          const difficulty = item.difficulty || item.Difficulty || '';
          const created = item.createdAt || item.CreatedAt;
          return el('li', {
            className: 'glass-panel px-4 py-3 flex items-center justify-between gap-3',
          }, [
            el('div', { className: 'min-w-0' }, [
              el('p', { className: 'font-semibold truncate', text: topic }),
              el('p', {
                className: 'text-xs text-muted',
                text: `${difficulty} · ${formatDateTime(created)}`,
              }),
            ]),
          ]);
        }),
      ),
    );
  } catch {
    setChildren(root, [
      el('p', { className: 'text-sm text-muted', text: 'History unavailable.' }),
    ]);
  }
}
