const TOKEN_KEY = 'el_token';
const USER_KEY = 'el_user';

/**
 * Shared client-side app state (token + user).
 */
export const appState = {
  getToken() {
    return localStorage.getItem(TOKEN_KEY);
  },

  setToken(token) {
    if (token) localStorage.setItem(TOKEN_KEY, token);
    else localStorage.removeItem(TOKEN_KEY);
  },

  getUser() {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  },

  setUser(user) {
    if (user) localStorage.setItem(USER_KEY, JSON.stringify(user));
    else localStorage.removeItem(USER_KEY);
  },

  isAuthenticated() {
    return Boolean(this.getToken());
  },

  clear() {
    this.setToken(null);
    this.setUser(null);
  },

  setAuth({ token, email, userName, userId }) {
    this.setToken(token);
    this.setUser({ email, userName, userId });
  },
};
