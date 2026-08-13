/**
 * Motion helpers: reduced-motion, device capability, scroll reveal, card tilt.
 */

export function prefersReducedMotion() {
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

export function isLowPowerDevice() {
  const cores = navigator.hardwareConcurrency || 4;
  const mem = navigator.deviceMemory || 4;
  const mobile = /Mobi|Android|iPhone|iPad/i.test(navigator.userAgent);
  return prefersReducedMotion() || cores <= 4 || mem <= 4 || (mobile && window.innerWidth < 900);
}

export function canUseWebGL() {
  try {
    const canvas = document.createElement('canvas');
    return !!(
      canvas.getContext('webgl') ||
      canvas.getContext('experimental-webgl')
    );
  } catch {
    return false;
  }
}

/**
 * Soft pointer parallax for an element (CSS transform).
 * Returns a cleanup function.
 */
export function attachParallax(target, { strength = 12, max = 18 } = {}) {
  if (!target || prefersReducedMotion()) return () => {};

  let raf = 0;
  let cx = 0;
  let cy = 0;
  let tx = 0;
  let ty = 0;

  const onMove = (e) => {
    const rect = target.getBoundingClientRect();
    const x = (e.clientX - rect.left) / rect.width - 0.5;
    const y = (e.clientY - rect.top) / rect.height - 0.5;
    tx = Math.max(-max, Math.min(max, x * strength));
    ty = Math.max(-max, Math.min(max, y * strength));
    if (!raf) raf = requestAnimationFrame(tick);
  };

  const tick = () => {
    cx += (tx - cx) * 0.08;
    cy += (ty - cy) * 0.08;
    target.style.transform = `translate3d(${cx}px, ${cy}px, 0)`;
    if (Math.abs(tx - cx) > 0.05 || Math.abs(ty - cy) > 0.05) {
      raf = requestAnimationFrame(tick);
    } else {
      raf = 0;
    }
  };

  const onLeave = () => {
    tx = 0;
    ty = 0;
    if (!raf) raf = requestAnimationFrame(tick);
  };

  window.addEventListener('pointermove', onMove, { passive: true });
  target.addEventListener('pointerleave', onLeave);

  return () => {
    window.removeEventListener('pointermove', onMove);
    target.removeEventListener('pointerleave', onLeave);
    if (raf) cancelAnimationFrame(raf);
  };
}

/**
 * Light 3D tilt on cards (max ~5deg).
 */
export function attachCardTilt(root = document, selector = '.tilt-card') {
  if (prefersReducedMotion() || isLowPowerDevice()) return () => {};

  const cleanups = [];

  const bind = (card) => {
    let raf = 0;
    let rx = 0;
    let ry = 0;
    let trx = 0;
    let try_ = 0;

    const onMove = (e) => {
      const rect = card.getBoundingClientRect();
      const x = (e.clientX - rect.left) / rect.width - 0.5;
      const y = (e.clientY - rect.top) / rect.height - 0.5;
      try_ = x * -6;
      trx = y * 5;
      if (!raf) raf = requestAnimationFrame(tick);
    };

    const tick = () => {
      rx += (trx - rx) * 0.12;
      ry += (try_ - ry) * 0.12;
      card.style.transform = `perspective(900px) rotateX(${rx}deg) rotateY(${ry}deg) translateY(-2px)`;
      if (Math.abs(trx - rx) > 0.05 || Math.abs(try_ - ry) > 0.05) {
        raf = requestAnimationFrame(tick);
      } else {
        raf = 0;
      }
    };

    const onLeave = () => {
      trx = 0;
      try_ = 0;
      if (!raf) raf = requestAnimationFrame(tick);
    };

    card.addEventListener('pointermove', onMove);
    card.addEventListener('pointerleave', onLeave);
    cleanups.push(() => {
      card.removeEventListener('pointermove', onMove);
      card.removeEventListener('pointerleave', onLeave);
      if (raf) cancelAnimationFrame(raf);
    });
  };

  root.querySelectorAll(selector).forEach(bind);

  return () => cleanups.forEach((fn) => fn());
}

/**
 * IntersectionObserver reveal for .reveal-on-scroll
 */
export function mountScrollReveal(root = document) {
  if (prefersReducedMotion()) {
    root.querySelectorAll('.reveal-on-scroll').forEach((el) => el.classList.add('is-visible'));
    return () => {};
  }

  const nodes = root.querySelectorAll('.reveal-on-scroll');
  if (!nodes.length) return () => {};

  const io = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('is-visible');
          io.unobserve(entry.target);
        }
      });
    },
    { rootMargin: '0px 0px -8% 0px', threshold: 0.12 },
  );

  nodes.forEach((n) => io.observe(n));
  return () => io.disconnect();
}
