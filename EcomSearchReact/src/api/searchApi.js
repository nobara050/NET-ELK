import { request } from './apiClient';

export function fullTextSearch(q, size = 10) {
  return request('/api/search/fulltext', {
    method: 'GET',
    params: { q, size }
  });
}

export function filterSearch({ category, brand, minPrice, maxPrice }) {
  return request('/api/search/filter', {
    method: 'GET',
    params: { category, brand, minPrice, maxPrice }
  });
}

export function advancedSearch(params) {
  return request('/api/search/advanced', {
    method: 'GET',
    params
  });
}

export function dynamicSearch(conditions, size = 10) {
  return request('/api/search/dynamic', {
    method: 'POST',
    body: JSON.stringify({ conditions, size })
  });
}

export function autocomplete(prefix, size = 5) {
  return request('/api/search/autocomplete', {
    method: 'GET',
    params: { prefix, size }
  });
}

export function fuzzySearch(q, size = 5) {
  return request('/api/search/fuzzy', {
    method: 'GET',
    params: { q, size }
  });
}

export function getStats() {
  return request('/api/search/stats', {
    method: 'GET'
  });
}
