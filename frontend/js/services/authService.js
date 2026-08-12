import { appState } from '../core/appState.js';
import { authApi } from '../api/authApi.js';

export const authService = {
  async login(email, password) {
    const result = await authApi.login(email, password);
    appState.setAuth({
      token: result.token,
      email: result.email,
      userName: result.userName,
      userId: result.userId,
    });
    return result;
  },

  async register(email, userName, password) {
    const result = await authApi.register(email, userName, password);
    appState.setAuth({
      token: result.token,
      email: result.email,
      userName: result.userName,
      userId: result.userId,
    });
    return result;
  },

  async refreshMe() {
    const user = await authApi.me();
    appState.setUser({
      userId: user.userId,
      email: user.email,
      userName: user.userName,
    });
    return user;
  },

  logout() {
    appState.clear();
    window.location.replace('login.html');
  },
};
