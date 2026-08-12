import { apiClient } from './apiClient.js';

export const wordSetsApi = {
  list() {
    return apiClient.get('/word-sets');
  },

  getById(id) {
    return apiClient.get(`/word-sets/${id}`);
  },

  addToVocabulary(id) {
    return apiClient.post(`/word-sets/${id}/add`);
  },
};
