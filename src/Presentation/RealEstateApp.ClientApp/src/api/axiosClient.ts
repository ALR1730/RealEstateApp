import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5196/api/v1';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para inyectar token JWT Bearer en cada petición
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('realestate_jwt_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});

// Interceptor de respuesta para manejo centralizado de 401
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Si la petición no es de login y recibe 401, limpiar sesión
      if (!error.config.url.includes('/account/authenticate')) {
        localStorage.removeItem('realestate_jwt_token');
        localStorage.removeItem('realestate_user');
      }
    }
    return Promise.reject(error);
  }
);
