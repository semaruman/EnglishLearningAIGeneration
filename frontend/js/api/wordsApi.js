import { apiClient } from './apiClient.js';

export const wordsApi = {
  list({ search, page = 1, pageSize = 20, partOfSpeech, difficulty } = {}) {
    return apiClient.get('/words', {
      query: { search, page, pageSize, partOfSpeech, difficulty },
    });
  },

  getById(id) {
    return apiClient.get(`/words/${id}`);
  },

  addByText(wordText) {
    return apiClient.post('/words', { wordText });
  },
};
