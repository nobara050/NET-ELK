import { request } from './apiClient';

export function getAllProducts() {
  return request('/api/products', {
    method: 'GET'
  });
}

export function getProductById(id) {
  return request(`/api/products/${id}`, {
    method: 'GET'
  });
}

export function createProduct(product) {
  return request('/api/products', {
    method: 'POST',
    body: JSON.stringify(product)
  });
}

export function updateProduct(id, product) {
  return request(`/api/products/${id}`, {
    method: 'PUT',
    body: JSON.stringify(product)
  });
}

export function deleteProduct(id) {
  return request(`/api/products/${id}`, {
    method: 'DELETE'
  });
}
