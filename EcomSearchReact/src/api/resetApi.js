import { request } from './apiClient';

export function resetAll() {
  return request('/api/reset', {
    method: 'POST'
  });
}
