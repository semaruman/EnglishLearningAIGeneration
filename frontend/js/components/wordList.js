import { el, setChildren } from '../utils/dom.js';
import { wordCard } from './wordCard.js';
import { emptyState } from './emptyState.js';
import { loadingSpinner } from './loading.js';
import { pagination } from './pagination.js';

/**
 * Render a paginated word list into a container.
 */
export function renderWordList(container, {
  items = [],
  loading = false,
  page = 1,
  totalPages = 1,
  emptyTitle = 'No words yet',
  emptyDescription = '',
  emptyActionLabel,
  onEmptyAction,
  onOpen,
  onDelete,
  onPageChange,
  showStatus = true,
} = {}) {
  if (loading) {
    setChildren(container, [loadingSpinner({ label: 'Loading words…', large: true })]);
    return;
  }

  if (!items.length) {
    setChildren(container, [
      emptyState({
        title: emptyTitle,
        description: emptyDescription,
        actionLabel: emptyActionLabel,
        onAction: onEmptyAction,
        icon: 'search',
      }),
    ]);
    return;
  }

  const list = el(
    'div',
    { className: 'grid gap-3 stagger' },
    items.map((word) => wordCard({ word, onOpen, onDelete, showStatus })),
  );

  const pager = pagination({ page, totalPages, onChange: onPageChange });
  setChildren(container, [list, pager]);
}
