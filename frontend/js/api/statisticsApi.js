import { apiClient } from './apiClient.js';

export const statisticsApi = {
  get() {
    return apiClient.get('/statistics');
  },
};
