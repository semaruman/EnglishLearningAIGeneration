import { guardRoute } from '../core/router-guard.js';
import { mountNav } from '../components/nav.js';
import { mountAppShell } from '../components/shell.js';
import { mountLanguageOrb } from '../components/languageOrb.js';
import { statisticsApi } from '../api/statisticsApi.js';
import { el, setChildren, $ } from '../utils/dom.js';
import { getStatusCount, clampPercent, plural } from '../utils/format.js';
import { progressBar } from '../components/progressBar.js';
import { skeletonBlock } from '../components/loading.js';
import { toast } from '../components/toast.js';
import { appState } from '../core/appState.js';
import { attachCardTilt, mountScrollReveal } from '../utils/motion.js';

if (!guardRoute()) {
  /* redirected */
} else {
  mountAppShell({ tilt: false });
  mountNav();
  init();
}

function timeGreeting() {
  const h = new Date().getHours();
  if (h < 12) return 'Good morning';
  if (h < 18) return 'Good afternoon';
  return 'Good evening';
}

async function init() {
  const root = $('#dashboard-root');
  setChildren(root, [
    el('div', { className: 'skeleton-panel mb-6' }, [
      skeletonBlock({ className: 'h-8 w-48 mb-3' }),
      skeletonBlock({ className: 'h-16 w-full max-w-md mb-4' }),
      skeletonBlock({ className: 'h-10 w-64' }),
    ]),
    el('div', { className: 'grid grid-cols-2 lg:grid-cols-4 gap-3' }, [
      skeletonBlock({ className: 'h-28' }),
      skeletonBlock({ className: 'h-28' }),
      skeletonBlock({ className: 'h-28' }),
      skeletonBlock({ className: 'h-28' }),
    ]),
  ]);

  try {
    const stats = await statisticsApi.get();
    renderDashboard(root, stats);
  } catch (err) {
    toast(err.message || 'Failed to load statistics', 'error');
    setChildren(root, [
      el('div', { className: 'glass-panel p-8 text-center' }, [
        el('p', { className: 'font-display text-xl mb-2', text: 'Something went wrong' }),
        el('p', { className: 'text-muted', text: err.message || 'Could not load dashboard.' }),
        el('button', {
          type: 'button',
          className: 'btn btn-primary mt-5',
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
  const name = user?.userName || 'learner';
  const greeting = `${timeGreeting()}, ${name}.`;

  const orbHost = el('div', { className: 'orb-host', id: 'language-orb' });

  const statCards = [
    { label: 'Total words', value: total, tone: 'dash-card--vocab' },
    { label: 'Mastered', value: mastered, tone: 'dash-card--mastery' },
    { label: 'Learning', value: learning, tone: 'dash-card--learning' },
    { label: 'New', value: neu, tone: 'dash-card--new' },
  ];

  const reviewGoal = Math.max(due, 1);
  const reviewPct = due > 0 ? clampPercent((Math.min(due, reviewGoal) / reviewGoal) * 100) : 100;

  setChildren(root, [
    el('section', { className: 'hero-stage hero-reveal mb-8' }, [
      el('div', { className: 'hero-copy' }, [
        el('div', { className: 'hero-badge', html: '<span aria-hidden="true">✦</span> Your English. Reimagined.' }),
        el('p', { className: 'section-eyebrow', text: greeting }),
        el('h1', {
          className: 'hero-title font-display',
          html: '<span>Learn English.</span><span>Feel the progress.</span>',
        }),
        el('p', {
          className: 'hero-sub',
          text: due
            ? `You have ${plural(due, 'word', 'words')} due for review. Build vocabulary, practice reading, and stay consistent.`
            : 'Build vocabulary, improve reading practice, and become confident with every session.',
        }),
        el('div', { className: 'hero-cta' }, [
          el('a', { href: 'learn.html', className: 'btn btn-primary btn-lg', text: 'Start learning' }),
          el('a', { href: 'practice.html', className: 'btn btn-secondary btn-lg', text: 'Practice reading' }),
        ]),
      ]),
      orbHost,
    ]),

    el(
      'div',
      { className: 'grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4 mb-6 stagger' },
      statCards.map((s) =>
        el('article', {
          className: `stat-card glass-panel tilt-card p-4 sm:p-5 ${s.tone} fade-up reveal-on-scroll`,
        }, [
          el('p', { className: 'text-xs sm:text-sm font-medium text-muted', text: s.label }),
          el('p', {
            className: 'font-display text-3xl sm:text-4xl font-semibold mt-1 tabular-nums',
            text: String(s.value),
          }),
        ]),
      ),
    ),

    el('div', { className: 'cockpit-grid mb-6' }, [
      el('section', { className: 'glass-panel tilt-card p-5 sm:p-7 reveal-on-scroll' }, [
        el('p', { className: 'section-eyebrow', text: 'English progress' }),
        el('h2', { className: 'font-display text-2xl mb-4', text: 'Mastery overview' }),
        progressBar({
          value: masteryPct,
          max: 100,
          label: 'Mastery progress',
          showPercent: true,
        }),
        el('p', {
          className: 'text-sm text-muted mt-4',
          text: total
            ? `${mastered} of ${plural(total, 'word', 'words')} mastered · ${known} known`
            : 'Add words to start building your vocabulary.',
        }),
      ]),

      el('section', { className: 'glass-panel tilt-card p-5 sm:p-7 flex flex-col items-center justify-center reveal-on-scroll' }, [
        el('p', { className: 'section-eyebrow self-start w-full', text: 'Review focus' }),
        el('div', {
          className: 'progress-ring my-4',
          style: { '--ring-pct': String(due === 0 ? 100 : Math.min(reviewPct, 100)) },
          role: 'img',
          'aria-label': due ? `${due} words due for review` : 'No words due for review',
        }, [
          el('div', { className: 'progress-ring__inner' }, [
            el('p', {
              className: 'font-display text-3xl tabular-nums',
              text: String(due),
            }),
            el('p', { className: 'text-xs text-muted mt-0.5', text: due ? 'due today' : 'all clear' }),
          ]),
        ]),
        el('p', {
          className: 'text-sm text-muted text-center',
          text: due ? 'Due for spaced repetition today.' : 'You are all caught up.',
        }),
      ]),
    ]),

    el('div', { className: 'grid sm:grid-cols-2 gap-4 mb-8' }, [
      el('article', { className: 'glass-panel tilt-card p-5 reveal-on-scroll' }, [
        el('p', { className: 'text-sm font-medium text-muted', text: 'Words to review' }),
        el('p', {
          className: 'font-display text-3xl font-semibold mt-1 tabular-nums',
          text: String(due),
        }),
        el('p', {
          className: 'text-sm text-muted mt-1',
          text: due ? 'Ready for a focused learning session.' : 'Nothing waiting in the queue.',
        }),
        el('a', { href: 'learn.html', className: 'btn btn-secondary btn-sm mt-4', text: 'Open Learn' }),
      ]),
      el('article', { className: 'glass-panel tilt-card p-5 reveal-on-scroll' }, [
        el('p', { className: 'text-sm font-medium text-muted', text: 'Practice sessions' }),
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
        el('a', { href: 'practice.html', className: 'btn btn-secondary btn-sm mt-4', text: 'Open Practice' }),
      ]),
    ]),

    el('section', { className: 'flex flex-wrap gap-3 reveal-on-scroll' }, [
      el('a', { href: 'learn.html', className: 'btn btn-primary btn-lg', text: 'Start learning' }),
      el('a', { href: 'vocabulary.html', className: 'btn btn-secondary btn-lg', text: 'Vocabulary' }),
      el('a', { href: 'library.html', className: 'btn btn-ghost btn-lg', text: 'Browse library' }),
    ]),
  ]);

  mountLanguageOrb(orbHost).catch(() => {});
  attachCardTilt(root);
  mountScrollReveal(root);
}
