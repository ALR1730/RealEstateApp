import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api/v1';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  paramsSerializer: {
    serialize: (params) => {
      const qs = new URLSearchParams();
      Object.entries(params || {}).forEach(([key, value]) => {
        if (Array.isArray(value)) {
          value.forEach((item) => qs.append(key, String(item)));
        } else if (value !== undefined && value !== null && value !== '') {
          qs.append(key, String(value));
        }
      });
      return qs.toString();
    },
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('realestate_jwt_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      if (!error.config.url.includes('/account/authenticate')) {
        localStorage.removeItem('realestate_jwt_token');
        localStorage.removeItem('realestate_user');
        window.dispatchEvent(new Event('auth:logout'));
      }
    }
    return Promise.reject(error);
  }
);
