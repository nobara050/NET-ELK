export async function request(endpoint, options = {}) {
  const { params, ...customConfig } = options;

  let url = endpoint;
  if (params) {
    const searchParams = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        searchParams.append(key, value);
      }
    });
    const queryString = searchParams.toString();
    if (queryString) {
      url += (url.includes('?') ? '&' : '?') + queryString;
    }
  }

  const config = {
    headers: {
      'Content-Type': 'application/json',
      ...customConfig.headers
    },
    ...customConfig
  };

  const response = await fetch(url, config);

  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get('content-type');
  const isJson = contentType && contentType.includes('application/json');
  const data = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const errorMessage = typeof data === 'object' && data?.message
      ? data.message
      : typeof data === 'string' && data
      ? data
      : `HTTP ${response.status}: ${response.statusText}`;
    throw new Error(errorMessage);
  }

  return data;
}
