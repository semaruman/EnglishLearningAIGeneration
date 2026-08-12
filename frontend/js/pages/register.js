import { guardRoute } from '../core/router-guard.js';
import { authService } from '../services/authService.js';
import { $ } from '../utils/dom.js';
import { toast } from '../components/toast.js';

if (!guardRoute({ requireAuth: false })) {
  /* redirected */
} else {
  const form = $('#register-form');
  const submitBtn = $('#register-submit');

  form?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = $('#email')?.value?.trim();
    const userName = $('#userName')?.value?.trim();
    const password = $('#password')?.value;
    const confirm = $('#confirmPassword')?.value;

    if (!email || !userName || !password) {
      toast('Fill in all fields', 'error');
      return;
    }
    if (password !== confirm) {
      toast('Passwords do not match', 'error');
      return;
    }
    if (password.length < 6) {
      toast('Password must be at least 6 characters', 'error');
      return;
    }

    submitBtn.disabled = true;
    submitBtn.textContent = 'Creating account…';

    try {
      await authService.register(email, userName, password);
      toast('Account created!', 'success');
      window.location.replace('index.html');
    } catch (err) {
      toast(err.message || 'Registration failed', 'error');
      submitBtn.disabled = false;
      submitBtn.textContent = 'Create account';
    }
  });
}
