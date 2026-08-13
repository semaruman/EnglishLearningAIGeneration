import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { learningApi } from '../api/learningApi.js';
import { $, el, setChildren } from '../utils/dom.js';
import { toast } from '../components/toast.js';
import { loadingSpinner } from '../components/loading.js';
import { emptyState } from '../components/emptyState.js';
import { progressBar } from '../components/progressBar.js';
import { button } from '../components/button.js';
import { difficultyLabel, statusLabel } from '../utils/format.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell();
  mountNav();
  initLearn();
}

const state = {
  session: null,
  card: null,
  flipped: false,
  busy: false,
  reviewed: 0,
  correct: 0,
  incorrect: 0,
};

async function initLearn() {
  const root = $('#learn-root');
  setChildren(root, [loadingSpinner({ label: 'Starting session…', large: true })]);

  try {
    state.session = await learningApi.startSession();
    state.reviewed = state.session.wordsReviewed ?? state.session.WordsReviewed ?? 0;
    state.correct = state.session.correctAnswers ?? state.session.CorrectAnswers ?? 0;
    state.incorrect = state.session.incorrectAnswers ?? state.session.IncorrectAnswers ?? 0;
    await loadNext();
  } catch (err) {
    toast(err.message || 'Could not start learning', 'error');
    setChildren(root, [
      emptyState({
        title: 'Unable to start',
        description: err.message,
        actionLabel: 'Retry',
        onAction: () => initLearn(),
        icon: 'spark',
      }),
    ]);
  }
}

async function loadNext() {
  const root = $('#learn-root');
  const sessionId = state.session?.id || state.session?.Id;

  try {
    const card = await learningApi.next(sessionId);
    state.card = card;
    state.flipped = false;

    if (!card) {
      setChildren(root, [
        el('div', { className: 'fade-up max-w-lg mx-auto' }, [
          emptyState({
            title: 'Session complete',
            description: `You reviewed ${state.reviewed} cards (${state.correct} known, ${state.incorrect} to revisit).`,
            actionLabel: 'Back to dashboard',
            onAction: () => {
              window.location.href = 'index.html';
            },
            icon: 'spark',
          }),
          el('div', { className: 'flex justify-center gap-3 mt-4' }, [
            button({
              label: 'Learn more',
              variant: 'secondary',
              onClick: () => initLearn(),
            }),
          ]),
        ]),
      ]);
      return;
    }

    renderCard(root);
  } catch (err) {
    toast(err.message || 'Failed to load card', 'error');
    setChildren(root, [
      emptyState({
        title: 'No card available',
        description: err.message || 'Add more vocabulary words to keep learning.',
        actionLabel: 'Open vocabulary',
        onAction: () => {
          window.location.href = 'vocabulary.html';
        },
      }),
    ]);
  }
}

function renderCard(root) {
  const card = state.card;
  const text = card.wordText || card.WordText || '';
  const pos = card.partOfSpeech || card.PartOfSpeech || '';
  const definition = card.definition || card.Definition || '';
  const translation = card.translation || card.Translation || '';
  const example = card.exampleSentence || card.ExampleSentence || '';
  const pronunciation = card.pronunciation || card.Pronunciation || card.phonetic || card.Phonetic;
  const difficulty = card.difficultyLevel ?? card.DifficultyLevel;
  const status = card.status ?? card.Status;

  const totalAnswers = state.correct + state.incorrect;
  const accuracy = totalAnswers ? Math.round((state.correct / totalAnswers) * 100) : 0;

  const flipCard = el('div', {
    className: `flip-card glass-panel ${state.flipped ? 'is-flipped' : ''}`,
    id: 'flip-card',
  }, [
    el('div', { className: 'flip-face flip-front glass-strong' }, [
      el('p', { className: 'text-xs font-semibold uppercase tracking-wide text-muted mb-3', text: pos || 'Word' }),
      el('h2', { className: 'font-display text-4xl sm:text-5xl font-semibold text-center', text }),
      pronunciation
        ? el('p', { className: 'text-muted mt-3 text-sm', text: pronunciation })
        : null,
      difficulty !== undefined && difficulty !== null
        ? el('p', { className: 'mt-4 text-xs status-pill status-new', text: difficultyLabel(difficulty) })
        : null,
    ]),
    el('div', { className: 'flip-face flip-back glass-strong' }, [
      el('p', { className: 'text-xs font-semibold uppercase tracking-wide text-muted mb-2', text: 'Translation' }),
      el('h2', { className: 'font-display text-3xl font-semibold text-center mb-3', text: translation }),
      definition
        ? el('p', { className: 'text-center text-sm text-[var(--ink)] leading-relaxed max-w-md', text: definition })
        : null,
      example
        ? el('p', {
            className: 'text-center text-sm text-muted italic mt-4 max-w-md',
            text: `“${example}”`,
          })
        : null,
      status !== undefined && status !== null
        ? el('p', {
            className: 'mt-4 text-xs text-muted',
            text: `Status: ${statusLabel(status)}`,
          })
        : null,
    ]),
  ]);

  const actionsFront = el('div', {
    className: `mt-6 flex flex-col items-center gap-3 ${state.flipped ? 'hidden' : ''}`,
    id: 'actions-front',
  }, [
    button({
      label: 'Show translation',
      variant: 'primary',
      size: 'lg',
      onClick: () => {
        state.flipped = true;
        renderCard(root);
      },
      attrs: { 'aria-label': 'Show translation and definition' },
    }),
  ]);

  const actionsBack = el('div', {
    className: `mt-6 grid grid-cols-1 sm:grid-cols-3 gap-2 ${state.flipped ? '' : 'hidden'}`,
    id: 'actions-back',
  }, [
    button({
      label: "I don't know",
      variant: 'dont-know',
      disabled: state.busy,
      onClick: () => submitAnswer(0),
    }),
    button({
      label: 'I know',
      variant: 'know',
      disabled: state.busy,
      onClick: () => submitAnswer(1),
    }),
    button({
      label: 'I know very well',
      variant: 'know-well',
      disabled: state.busy,
      onClick: () => submitAnswer(2),
    }),
  ]);

  setChildren(root, [
    el('div', { className: 'max-w-xl mx-auto fade-up' }, [
      el('header', { className: 'mb-6 text-center sm:text-left' }, [
        el('p', { className: 'section-eyebrow', text: 'Learn' }),
        el('h1', { className: 'font-display text-3xl font-semibold', text: 'Flashcards' }),
      ]),
      el('div', { className: 'glass-panel p-4 mb-5' }, [
        progressBar({
          value: accuracy,
          label: `Session · ${state.reviewed} reviewed`,
          showPercent: true,
        }),
        el('p', {
          className: 'text-xs text-muted mt-2',
          text: `${state.correct} known · ${state.incorrect} to revisit`,
        }),
      ]),
      el('div', { className: 'flip-scene card-enter', id: 'flip-scene' }, [flipCard]),
      actionsFront,
      actionsBack,
    ]),
  ]);
}

async function submitAnswer(answer) {
  if (state.busy || !state.card) return;
  state.busy = true;

  const wordId = state.card.wordId || state.card.WordId;
  const sessionId =
    state.card.sessionId ||
    state.card.SessionId ||
    state.session?.id ||
    state.session?.Id;

  const scene = $('#flip-scene');
  scene?.classList.add('card-exit');

  try {
    const result = await learningApi.answer(wordId, answer, sessionId);
    const session = result.session || result.Session;
    if (session) {
      state.session = session;
      state.reviewed = session.wordsReviewed ?? session.WordsReviewed ?? state.reviewed + 1;
      state.correct = session.correctAnswers ?? session.CorrectAnswers ?? state.correct;
      state.incorrect = session.incorrectAnswers ?? session.IncorrectAnswers ?? state.incorrect;
    } else {
      state.reviewed += 1;
      if (answer === 0) state.incorrect += 1;
      else state.correct += 1;
    }

    window.setTimeout(async () => {
      state.busy = false;
      await loadNext();
    }, 280);
  } catch (err) {
    state.busy = false;
    scene?.classList.remove('card-exit');
    toast(err.message || 'Failed to submit answer', 'error');
  }
}
