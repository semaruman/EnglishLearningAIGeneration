import { apiClient } from './apiClient.js';

export const authApi = {
  login(email, password) {
    return apiClient.post('/auth/login', { email, password }, { auth: false });
  },

  register(email, userName, password) {
    return apiClient.post('/auth/register', { email, userName, password }, { auth: false });
  },

  me() {
    return apiClient.get('/auth/me');
  },
};
