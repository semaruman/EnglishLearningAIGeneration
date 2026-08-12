import { el } from '../utils/dom.js';
import { statusClass, statusLabel, difficultyLabel } from '../utils/format.js';
import { button } from './button.js';

/**
 * Vocabulary / library word card.
 * @param {{
 *   word: object,
 *   onOpen?: (word: object) => void,
 *   onDelete?: (word: object) => void,
 *   showStatus?: boolean
 * }} opts
 */
export function wordCard({ word, onOpen, onDelete, showStatus = true } = {}) {
  const text = word.wordText || word.WordText || '';
  const pos = word.partOfSpeech || word.PartOfSpeech || '';
  const translation = word.translation || word.Translation || '';
  const status = word.status ?? word.Status;
  const difficulty = word.difficultyLevel ?? word.DifficultyLevel;

  const actions = el('div', { className: 'flex items-center gap-2 shrink-0' });
  if (showStatus && status !== undefined && status !== null) {
    actions.appendChild(
      el('span', {
        className: `status-pill ${statusClass(status)}`,
        text: statusLabel(status),
      }),
    );
  }
  if (onDelete) {
    actions.appendChild(
      button({
        label: 'Remove',
        variant: 'danger',
        size: 'sm',
        onClick: (e) => {
          e.stopPropagation();
          onDelete(word);
        },
        attrs: { 'aria-label': `Remove ${text} from vocabulary` },
      }),
    );
  }

  return el(
    'article',
    {
      className: 'word-card glass-panel p-4 flex items-start justify-between gap-3 fade-up',
      tabindex: onOpen ? '0' : undefined,
      role: onOpen ? 'button' : undefined,
      'aria-label': onOpen ? `Open details for ${text}` : undefined,
      onClick: onOpen ? () => onOpen(word) : undefined,
      onKeydown: onOpen
        ? (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              e.preventDefault();
              onOpen(word);
            }
          }
        : undefined,
    },
    [
      el('div', { className: 'min-w-0' }, [
        el('div', { className: 'flex items-baseline gap-2 flex-wrap' }, [
          el('h3', { className: 'font-display text-lg font-semibold truncate', text }),
          pos ? el('span', { className: 'text-xs text-muted font-medium', text: pos }) : null,
          difficulty !== undefined && difficulty !== null
            ? el('span', {
                className: 'text-xs px-2 py-0.5 rounded-full bg-white/50 text-muted',
                text: difficultyLabel(difficulty),
              })
            : null,
        ]),
        translation
          ? el('p', { className: 'text-sm text-muted mt-1 truncate', text: translation })
          : null,
      ]),
      actions,
    ],
  );
}
