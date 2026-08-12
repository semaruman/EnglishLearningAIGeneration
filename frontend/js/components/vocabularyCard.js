import { el } from '../utils/dom.js';
import {
  statusClass,
  statusLabel,
  difficultyLabel,
  formatDate,
  formatDateTime,
} from '../utils/format.js';
import { button } from './button.js';

/**
 * Rich vocabulary / word detail body for modals.
 */
export function vocabularyCard({
  word,
  inVocabulary = false,
  onAdd,
  onRemove,
  adding = false,
} = {}) {
  const text = word.wordText || word.WordText || '';
  const pos = word.partOfSpeech || word.PartOfSpeech || '';
  const definition = word.definition || word.Definition || '';
  const translation = word.translation || word.Translation || '';
  const pronunciation = word.pronunciation || word.Pronunciation || word.phonetic || word.Phonetic;
  const example = word.exampleSentence || word.ExampleSentence || '';
  const status = word.status ?? word.Status;
  const difficulty = word.difficultyLevel ?? word.DifficultyLevel;
  const knowledge = word.knowledgeLevel ?? word.KnowledgeLevel;
  const addedAt = word.addedAt || word.AddedAt;
  const nextReview = word.nextReviewAt || word.NextReviewAt;
  const correct = word.correctAnswers ?? word.CorrectAnswers;
  const incorrect = word.incorrectAnswers ?? word.IncorrectAnswers;

  const meta = el('div', { className: 'flex flex-wrap gap-2 mb-4' });
  if (pos) meta.appendChild(el('span', { className: 'status-pill status-learning', text: pos }));
  if (difficulty !== undefined && difficulty !== null) {
    meta.appendChild(
      el('span', {
        className: 'status-pill status-new',
        text: difficultyLabel(difficulty),
      }),
    );
  }
  if (status !== undefined && status !== null) {
    meta.appendChild(
      el('span', {
        className: `status-pill ${statusClass(status)}`,
        text: statusLabel(status),
      }),
    );
  }

  const rows = [
    ['Definition', definition],
    ['Translation', translation],
    pronunciation ? ['Pronunciation', pronunciation] : null,
    example ? ['Example', example] : null,
  ].filter(Boolean);

  const details = el(
    'dl',
    { className: 'space-y-3 text-sm' },
    rows.map(([k, v]) =>
      el('div', {}, [
        el('dt', { className: 'text-muted font-semibold text-xs uppercase tracking-wide', text: k }),
        el('dd', { className: 'mt-0.5 text-[var(--ink)] leading-relaxed', text: v }),
      ]),
    ),
  );

  const stats = [];
  if (knowledge !== undefined && knowledge !== null) {
    stats.push(`Knowledge: ${knowledge}`);
  }
  if (correct !== undefined) stats.push(`Correct: ${correct}`);
  if (incorrect !== undefined) stats.push(`Incorrect: ${incorrect}`);
  if (addedAt) stats.push(`Added: ${formatDate(addedAt)}`);
  if (nextReview) stats.push(`Next review: ${formatDateTime(nextReview)}`);

  const actions = el('div', { className: 'mt-5 flex flex-wrap gap-2' });
  if (inVocabulary) {
    actions.appendChild(
      el('span', {
        className: 'inline-flex items-center gap-1.5 text-sm font-semibold text-[var(--teal-600)] px-3 py-2 rounded-xl bg-teal-50/80',
        html: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6 9 17l-5-5"/></svg> In your vocabulary`,
      }),
    );
    if (onRemove) {
      actions.appendChild(
        button({
          label: 'Remove',
          variant: 'danger',
          size: 'sm',
          onClick: () => onRemove(word),
        }),
      );
    }
  } else if (onAdd) {
    actions.appendChild(
      button({
        label: adding ? 'Adding…' : 'Add to vocabulary',
        disabled: adding,
        onClick: () => onAdd(word),
      }),
    );
  }

  return el('div', { className: 'fade-in' }, [
    el('p', { className: 'font-display text-2xl font-semibold mb-1', text }),
    meta,
    details,
    stats.length
      ? el('p', { className: 'text-xs text-muted mt-4 leading-relaxed', text: stats.join(' · ') })
      : null,
    actions,
  ]);
}
