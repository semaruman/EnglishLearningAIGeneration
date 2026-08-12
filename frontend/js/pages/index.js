import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { statisticsApi } from '../api/statisticsApi.js';
import { el, setChildren, $ } from '../utils/dom.js';
import { getStatusCount, clampPercent, plural } from '../utils/format.js';
import { progressBar } from '../components/progressBar.js';
import { loadingSpinner } from '../components/loading.js';
import { toast } from '../components/toast.js';
import { appState } from '../core/appState.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountNav();
  init();
}

async function init() {
  const root = $('#dashboard-root');
  setChildren(root, [loadingSpinner({ label: 'Loading your progress…', large: true })]);

  try {
    const stats = await statisticsApi.get();
    renderDashboard(root, stats);
  } catch (err) {
    toast(err.message || 'Failed to load statistics', 'error');
    setChildren(root, [
      el('div', { className: 'glass-panel p-8 text-center' }, [
        el('p', { className: 'text-muted', text: err.message || 'Could not load dashboard.' }),
        el('button', {
          type: 'button',
          className: 'btn btn-primary mt-4',
          text: 'Retry',
          onClick: () => init(),
        }),
      ]),
    ]);
  }
}

function renderDashboard(root, stats) {
  const total = stats.totalWords ?? stats.TotalWords ?? stats.totalVocabularyWords ?? 0;
  const mastered = stats.masteredWords ?? stats.MasteredWords ?? getStatusCount(stats.wordsByStatus, 'Mastered');
  const learning = stats.learningWords ?? stats.LearningWords ?? getStatusCount(stats.wordsByStatus, 'Learning');
  const known = stats.knownWords ?? stats.KnownWords ?? getStatusCount(stats.wordsByStatus, 'Known');
  const neu = stats.newWords ?? stats.NewWords ?? getStatusCount(stats.wordsByStatus, 'New');
  const due = stats.dueForReviewCount ?? stats.DueForReviewCount ?? stats.wordsReviewedToday ?? stats.WordsReviewedToday ?? 0;
  const practiceCount = stats.practiceSessions ?? stats.PracticeSessions ?? stats.practiceSessionsCount ?? 0;
  const masteryPct = total > 0 ? clampPercent(((mastered + known) / total) * 100) : 0;

  const user = appState.getUser();
  const greeting = user?.userName ? `Welcome back, ${user.userName}` : 'Welcome back';

  const statCards = [
    { label: 'Total words', value: total, tone: 'from-sky-100/80 to-white/60' },
    { label: 'Mastered', value: mastered, tone: 'from-teal-100/80 to-white/60' },
    { label: 'Learning', value: learning, tone: 'from-amber-50/90 to-white/60' },
    { label: 'New', value: neu, tone: 'from-cyan-50/90 to-white/60' },
  ];

  setChildren(root, [
    el('header', { className: 'mb-8 fade-up' }, [
      el('p', { className: 'text-sm font-semibold text-[var(--teal-600)] mb-1', text: 'Dashboard' }),
      el('h1', { className: 'font-display text-3xl sm:text-4xl font-semibold tracking-tight', text: greeting }),
      el('p', {
        className: 'text-muted mt-2 max-w-xl',
        text: 'Track progress, review due words, and jump into a focused session.',
      }),
    ]),

    el(
      'div',
      { className: 'grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 mb-6 stagger' },
      statCards.map((s) =>
        el('article', {
          className: `stat-card glass-panel p-4 sm:p-5 bg-gradient-to-br ${s.tone} fade-up`,
        }, [
          el('p', { className: 'text-xs sm:text-sm font-semibold text-muted', text: s.label }),
          el('p', {
            className: 'font-display text-3xl sm:text-4xl font-semibold mt-1 tabular-nums',
            text: String(s.value),
          }),
        ]),
      ),
    ),

    el('section', { className: 'glass-panel p-5 sm:p-6 mb-6 fade-up' }, [
      progressBar({
        value: masteryPct,
        max: 100,
        label: 'Mastery progress',
        showPercent: true,
      }),
      el('p', {
        className: 'text-sm text-muted mt-3',
        text: total
          ? `${mastered} of ${plural(total, 'word', 'words')} mastered · ${known} known`
          : 'Add words to start building your vocabulary.',
      }),
    ]),

    el('div', { className: 'grid sm:grid-cols-2 gap-4 mb-8' }, [
      el('article', { className: 'glass-panel p-5 fade-up' }, [
        el('p', { className: 'text-sm font-semibold text-muted', text: 'Words to review' }),
        el('p', {
          className: 'font-display text-3xl font-semibold mt-1 tabular-nums',
          text: String(due),
        }),
        el('p', {
          className: 'text-sm text-muted mt-1',
          text: due ? 'Due for spaced repetition today.' : 'You are all caught up.',
        }),
      ]),
      el('article', { className: 'glass-panel p-5 fade-up' }, [
        el('p', { className: 'text-sm font-semibold text-muted', text: 'Practice sessions' }),
        el('p', {
          className: 'font-display text-3xl font-semibold mt-1 tabular-nums',
          text: String(practiceCount),
        }),
        el('p', {
          className: 'text-sm text-muted mt-1',
          text: practiceCount
            ? `${plural(practiceCount, 'session', 'sessions')} completed.`
            : 'Generate reading practice from your words.',
        }),
      ]),
    ]),

    el('section', { className: 'flex flex-wrap gap-3 fade-up' }, [
      el('a', { href: 'learn.html', className: 'btn btn-primary btn-lg', text: 'Start learning' }),
      el('a', { href: 'practice.html', className: 'btn btn-secondary btn-lg', text: 'Practice' }),
      el('a', { href: 'vocabulary.html', className: 'btn btn-ghost btn-lg', text: 'Vocabulary' }),
    ]),
  ]);
}
