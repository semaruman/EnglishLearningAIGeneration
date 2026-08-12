import { guardRoute } from '../core/router-guard.js';
import { authService } from '../services/authService.js';
import { $ } from '../utils/dom.js';
import { toast } from '../components/toast.js';

if (!guardRoute({ requireAuth: false })) {
  /* redirected */
} else {
  const form = $('#login-form');
  const submitBtn = $('#login-submit');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = $('#email')?.value?.trim();
    const password = $('#password')?.value;

    if (!email || !password) {
      toast('Enter email and password', 'error');
      return;
    }

    submitBtn.disabled = true;
    submitBtn.textContent = 'Signing in…';

    try {
      await authService.login(email, password);
      toast('Welcome back!', 'success');
      const params = new URLSearchParams(window.location.search);
      const next = params.get('next');
      const safe = next && !next.includes('://') && next.endsWith('.html') ? next : 'index.html';
      window.location.replace(safe);
    } catch (err) {
      toast(err.message || 'Login failed', 'error');
      submitBtn.disabled = false;
      submitBtn.textContent = 'Sign in';
    }
  });
}
