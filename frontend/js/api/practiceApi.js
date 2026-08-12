import { apiClient } from './apiClient.js';

export const practiceApi = {
  generate({ topic, difficulty, length }) {
    return apiClient.post('/practice/generate', { topic, difficulty, length });
  },

  history({ page = 1, pageSize = 10 } = {}) {
    return apiClient.get('/practice/history', { query: { page, pageSize } });
  },
};
