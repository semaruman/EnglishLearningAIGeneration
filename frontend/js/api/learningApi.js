import { apiClient } from './apiClient.js';

export const learningApi = {
  startSession() {
    return apiClient.post('/learning/session');
  },

  next(sessionId) {
    return apiClient.get('/learning/next', { query: { sessionId } });
  },

  answer(wordId, answer, sessionId) {
    return apiClient.post(`/learning/${wordId}/answer`, { answer, sessionId });
  },
};
