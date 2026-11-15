import axios from 'axios';

const ApiCentral = axios.create({
  baseURL: 'http://127.0.0.1:5050/api/v1',
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

export default ApiCentral;