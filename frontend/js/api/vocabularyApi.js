import { apiClient } from './apiClient.js';

export const vocabularyApi = {
  list({ status, search, page = 1, pageSize = 20 } = {}) {
    return apiClient.get('/vocabulary', { query: { status, search, page, pageSize } });
  },

  get(wordId) {
    return apiClient.get(`/vocabulary/${wordId}`);
  },

  add(wordId) {
    return apiClient.post('/vocabulary', { wordId });
  },

  remove(wordId) {
    return apiClient.delete(`/vocabulary/${wordId}`);
  },
};
