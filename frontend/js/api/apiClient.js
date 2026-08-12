import { appState } from '../core/appState.js';

const BASE_URL = '/api';

export class ApiError extends Error {
  constructor(message, { status, code, data } = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.data = data;
  }
}

function buildUrl(path, query) {
  const url = new URL(
    `${BASE_URL}${path.startsWith('/') ? path : `/${path}`}`,
    window.location.origin,
  );
  if (query && typeof query === 'object') {
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null || value === '') return;
      url.searchParams.set(key, String(value));
    });
  }
  return url.toString();
}

function unwrap(payload) {
  if (payload && typeof payload === 'object' && 'success' in payload) {
    if (payload.success === false) {
      const msg = payload.error?.message || payload.error?.Message || 'Request failed';
      const code = payload.error?.code || payload.error?.Code;
      throw new ApiError(msg, { code, data: payload });
    }
    return 'data' in payload ? payload.data : payload.Data;
  }
  return payload;
}

async function request(path, { method = 'GET', body, query, headers = {}, auth = true } = {}) {
  const finalHeaders = {
    Accept: 'application/json',
    ...headers,
  };

  if (body !== undefined) {
    finalHeaders['Content-Type'] = 'application/json';
  }

  if (auth) {
    const token = appState.getToken();
    if (token) finalHeaders.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(buildUrl(path, query), {
    method,
    headers: finalHeaders,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (response.status === 401) {
    appState.clear();
    const page = window.location.pathname.split('/').pop() || 'index.html';
    if (page !== 'login.html' && page !== 'register.html') {
      window.location.replace(`login.html?next=${encodeURIComponent(page)}`);
    }
    throw new ApiError('Unauthorized', { status: 401 });
  }

  const text = await response.text();
  let payload = null;
  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = text;
    }
  }

  if (!response.ok) {
    const msg =
      payload?.error?.message ||
      payload?.error?.Message ||
      payload?.message ||
      payload?.title ||
      (typeof payload === 'string' ? payload : null) ||
      `Request failed (${response.status})`;
    throw new ApiError(msg, {
      status: response.status,
      code: payload?.error?.code || payload?.error?.Code,
      data: payload,
    });
  }

  if (response.status === 204 || payload === null || payload === '') {
    return null;
  }

  return unwrap(payload);
}

export const apiClient = {
  get: (path, options = {}) => request(path, { ...options, method: 'GET' }),
  post: (path, body, options = {}) => request(path, { ...options, method: 'POST', body }),
  put: (path, body, options = {}) => request(path, { ...options, method: 'PUT', body }),
  delete: (path, options = {}) => request(path, { ...options, method: 'DELETE' }),
};
