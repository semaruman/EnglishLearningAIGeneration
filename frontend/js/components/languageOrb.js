import { el } from '../utils/dom.js';
import { canUseWebGL, isLowPowerDevice, prefersReducedMotion } from '../utils/motion.js';

const WORDS = ['HELLO', 'LEARN', 'SPEAK', 'LISTEN', 'WRITE', 'FLUENT', 'ENGLISH'];

function createFallback(host) {
  const fallback = el('div', {
    className: 'orb-fallback',
    'aria-hidden': 'true',
  }, [
    el('div', { className: 'orb-fallback__ring' }),
    el('div', { className: 'orb-fallback__ring orb-fallback__ring--2' }),
    el('span', { className: 'orb-fallback__word', text: 'LEARN' }),
    el('span', { className: 'orb-fallback__word', text: 'SPEAK' }),
    el('span', { className: 'orb-fallback__word', text: 'FLUENT' }),
    el('span', { className: 'orb-fallback__word', text: 'ENGLISH' }),
  ]);
  host.appendChild(fallback);
  return () => fallback.remove();
}

/**
 * Mount Language Orb into host element.
 * Uses Three.js when available; otherwise CSS fallback.
 * @returns {Promise<() => void>} cleanup
 */
export async function mountLanguageOrb(host, { interactive = true } = {}) {
  if (!host) return () => {};

  host.classList.add('orb-host');
  host.setAttribute('aria-hidden', 'true');

  const use3d = canUseWebGL() && !isLowPowerDevice() && !prefersReducedMotion();

  if (!use3d) {
    return createFallback(host);
  }

  try {
    const THREE = await import('https://cdn.jsdelivr.net/npm/three@0.160.1/build/three.module.js');
    return createThreeOrb(host, THREE, { interactive });
  } catch (err) {
    console.warn('LanguageOrb: falling back to CSS', err);
    return createFallback(host);
  }
}

function createThreeOrb(host, THREE, { interactive }) {
  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(42, 1, 0.1, 100);
  camera.position.z = 4.2;

  const renderer = new THREE.WebGLRenderer({
    antialias: true,
    alpha: true,
    powerPreference: 'high-performance',
  });
  renderer.setClearColor(0x000000, 0);
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.75));
  host.appendChild(renderer.domElement);

  const root = new THREE.Group();
  scene.add(root);

  // Soft lights
  const ambient = new THREE.AmbientLight(0x8eb4ff, 0.55);
  scene.add(ambient);

  const key = new THREE.PointLight(0x5ba3ff, 1.4, 12);
  key.position.set(2.2, 1.8, 3);
  scene.add(key);

  const fill = new THREE.PointLight(0x4ecdc9, 0.7, 10);
  fill.position.set(-2.4, -1.2, 2);
  scene.add(fill);

  const cursorLight = new THREE.PointLight(0xffffff, 0.55, 8);
  cursorLight.position.set(0, 0, 3.2);
  scene.add(cursorLight);

  // Glass sphere
  const sphereGeo = new THREE.SphereGeometry(1.15, 48, 48);
  const sphereMat = new THREE.MeshPhysicalMaterial({
    color: 0xc5d8ff,
    transparent: true,
    opacity: 0.28,
    roughness: 0.18,
    metalness: 0.08,
    clearcoat: 1,
    clearcoatRoughness: 0.1,
    reflectivity: 0.6,
  });
  const sphere = new THREE.Mesh(sphereGeo, sphereMat);
  root.add(sphere);

  // Soft luminous shell
  const glow = new THREE.Mesh(
    new THREE.SphereGeometry(1.28, 32, 32),
    new THREE.MeshBasicMaterial({
      color: 0x5ba3ff,
      transparent: true,
      opacity: 0.07,
      side: THREE.BackSide,
    }),
  );
  root.add(glow);

  // Inner glow core
  const core = new THREE.Mesh(
    new THREE.SphereGeometry(0.42, 24, 24),
    new THREE.MeshBasicMaterial({
      color: 0x5ba3ff,
      transparent: true,
      opacity: 0.22,
    }),
  );
  root.add(core);

  // Rings
  const ringMat = new THREE.MeshBasicMaterial({
    color: 0xffffff,
    transparent: true,
    opacity: 0.22,
    side: THREE.DoubleSide,
  });
  const ring1 = new THREE.Mesh(new THREE.TorusGeometry(1.45, 0.012, 12, 96), ringMat);
  ring1.rotation.x = Math.PI / 2.4;
  root.add(ring1);

  const ring2 = new THREE.Mesh(
    new THREE.TorusGeometry(1.65, 0.01, 12, 96),
    new THREE.MeshBasicMaterial({
      color: 0x4ecdc9,
      transparent: true,
      opacity: 0.18,
      side: THREE.DoubleSide,
    }),
  );
  ring2.rotation.x = Math.PI / 3.2;
  ring2.rotation.y = 0.4;
  root.add(ring2);

  // Particles
  const particleCount = 48;
  const positions = new Float32Array(particleCount * 3);
  for (let i = 0; i < particleCount; i++) {
    const r = 0.55 + Math.random() * 0.85;
    const theta = Math.random() * Math.PI * 2;
    const phi = Math.acos(2 * Math.random() - 1);
    positions[i * 3] = r * Math.sin(phi) * Math.cos(theta);
    positions[i * 3 + 1] = r * Math.sin(phi) * Math.sin(theta);
    positions[i * 3 + 2] = r * Math.cos(phi);
  }
  const pGeo = new THREE.BufferGeometry();
  pGeo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
  const particles = new THREE.Points(
    pGeo,
    new THREE.PointsMaterial({
      color: 0xcfe0ff,
      size: 0.035,
      transparent: true,
      opacity: 0.75,
      depthWrite: false,
    }),
  );
  root.add(particles);

  // Floating word sprites (canvas textures)
  const wordGroup = new THREE.Group();
  root.add(wordGroup);
  WORDS.forEach((word, i) => {
    const sprite = makeWordSprite(THREE, word);
    const angle = (i / WORDS.length) * Math.PI * 2;
    const radius = 0.55 + (i % 3) * 0.12;
    sprite.position.set(
      Math.cos(angle) * radius,
      Math.sin(angle * 1.3) * 0.35,
      Math.sin(angle) * radius * 0.7,
    );
    sprite.userData = { angle, radius, speed: 0.15 + (i % 4) * 0.03, phase: i };
    wordGroup.add(sprite);
  });

  function resize() {
    const w = host.clientWidth || 360;
    const h = host.clientHeight || w;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h, false);
  }

  resize();
  const ro = new ResizeObserver(resize);
  ro.observe(host);

  let raf = 0;
  let running = true;
  let pointerX = 0;
  let pointerY = 0;
  let targetRotY = 0;
  let targetRotX = 0;

  const onPointer = (e) => {
    if (!interactive) return;
    const rect = host.getBoundingClientRect();
    pointerX = ((e.clientX - rect.left) / rect.width) * 2 - 1;
    pointerY = ((e.clientY - rect.top) / rect.height) * 2 - 1;
    targetRotY = pointerX * 0.35;
    targetRotX = pointerY * 0.22;
    cursorLight.position.x = pointerX * 2.2;
    cursorLight.position.y = -pointerY * 1.8;
  };

  if (interactive) {
    window.addEventListener('pointermove', onPointer, { passive: true });
  }

  const clock = new THREE.Clock();

  const animate = () => {
    if (!running) return;
    raf = requestAnimationFrame(animate);
    const t = clock.getElapsedTime();

    root.rotation.y += (targetRotY - root.rotation.y) * 0.04;
    root.rotation.x += (targetRotX - root.rotation.x) * 0.04;
    root.position.y = Math.sin(t * 0.55) * 0.06;

    ring1.rotation.z = t * 0.12;
    ring2.rotation.z = -t * 0.08;
    particles.rotation.y = t * 0.05;
    core.scale.setScalar(1 + Math.sin(t * 1.2) * 0.04);

    wordGroup.children.forEach((sprite) => {
      const { angle, radius, speed, phase } = sprite.userData;
      const a = angle + t * speed;
      sprite.position.x = Math.cos(a) * radius;
      sprite.position.z = Math.sin(a) * radius * 0.75;
      sprite.position.y = Math.sin(t * 0.7 + phase) * 0.28;
      sprite.material.opacity = 0.45 + Math.sin(t + phase) * 0.2;
    });

    renderer.render(scene, camera);
  };

  animate();

  // Pause when offscreen
  const io = new IntersectionObserver(
    ([entry]) => {
      if (entry.isIntersecting && !running) {
        running = true;
        clock.start();
        animate();
      } else if (!entry.isIntersecting && running) {
        running = false;
        cancelAnimationFrame(raf);
      }
    },
    { threshold: 0.05 },
  );
  io.observe(host);

  return () => {
    running = false;
    cancelAnimationFrame(raf);
    io.disconnect();
    ro.disconnect();
    if (interactive) window.removeEventListener('pointermove', onPointer);
    sphereGeo.dispose();
    sphereMat.dispose();
    pGeo.dispose();
    renderer.dispose();
    if (renderer.domElement.parentNode) renderer.domElement.remove();
  };
}

function makeWordSprite(THREE, text) {
  const canvas = document.createElement('canvas');
  canvas.width = 256;
  canvas.height = 64;
  const ctx = canvas.getContext('2d');
  ctx.clearRect(0, 0, 256, 64);
  ctx.font = '600 28px Inter, system-ui, sans-serif';
  ctx.fillStyle = 'rgba(230, 240, 255, 0.92)';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(text, 128, 32);

  const texture = new THREE.CanvasTexture(canvas);
  texture.needsUpdate = true;
  const material = new THREE.SpriteMaterial({
    map: texture,
    transparent: true,
    opacity: 0.65,
    depthWrite: false,
  });
  const sprite = new THREE.Sprite(material);
  sprite.scale.set(0.85, 0.22, 1);
  return sprite;
}
