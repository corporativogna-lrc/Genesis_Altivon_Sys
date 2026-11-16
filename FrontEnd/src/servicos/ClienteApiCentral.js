import axios from 'axios';

const baseURL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
  ? 'http://127.0.0.1:5050/api/v1'
  : 'https://api.nexumaltivon.com.br/api/v1';

const ApiCentral = axios.create({
  baseURL: baseURL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
});

ApiCentral.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('genesis_token_jwt');
    const tenantId = localStorage.getItem('genesis_tenant_id');
    if (token) config.headers['Authorization'] = `Bearer ${token}`;
    if (tenantId) config.headers['X-Tenant-Id'] = tenantId;
    return config;
  },
  (error) => Promise.reject(error)
);

ApiCentral.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('genesis_token_jwt');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default ApiCentral;