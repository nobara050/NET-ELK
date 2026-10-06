import { request } from './apiClient';

export function triggerBusinessError(message, orderId) {
  return request('/api/diagnostics/trigger-error', {
    method: 'POST',
    params: { message, orderId }
  });
}

export function simulateException(serviceName) {
  return request('/api/diagnostics/simulate-exception', {
    method: 'POST',
    params: { serviceName }
  });
}

export function triggerBurstErrors(count = 5) {
  return request('/api/diagnostics/trigger-burst-errors', {
    method: 'POST',
    params: { count }
  });
}
