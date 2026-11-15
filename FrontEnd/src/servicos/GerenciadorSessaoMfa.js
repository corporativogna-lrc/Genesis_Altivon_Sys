import ApiCentral from './ClienteApiCentral';

export const GerenciadorSessao = {
  async autenticar(email, senha) {
    try {
      const response = await ApiCentral.post('/iam/login', { email, senha });
      if (response.data && response.data.token) {
        localStorage.setItem('genesis_token_jwt', response.data.token);
        if (response.data.tenantId) localStorage.setItem('genesis_tenant_id', response.data.tenantId);
        return { sucesso: true, dados: response.data };
      }
      return { sucesso: false, erro: 'Token não retornado.' };
    } catch (error) {
      return { sucesso: false, erro: error.response?.data?.erro || 'Falha na autenticação.' };
    }
  },
  logout() {
    localStorage.removeItem('genesis_token_jwt');
    localStorage.removeItem('genesis_tenant_id');
    window.location.href = '/login';
  },
  verificarSessaoAtiva() {
    return !!localStorage.getItem('genesis_token_jwt');
  },
  obterTenantAtual() {
    return localStorage.getItem('genesis_tenant_id') || '';
  },
  definirTenantAtual(tenantId) {
    localStorage.setItem('genesis_tenant_id', tenantId);
  }
};

export default GerenciadorSessao;