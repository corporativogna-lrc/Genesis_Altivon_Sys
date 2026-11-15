import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaConfiguracoesTenant = () => {
  const [config, setConfig] = useState({
    nomeEmpresa: '',
    cnpj: '',
    ambienteSefaz: 'HOMOLOGACAO',
    toleranciaAtrasoDias: 5,
    notificarEmailAtiva: true
  });
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarConfiguracoes();
  }, []);

  const carregarConfiguracoes = async () => {
    try {
      const response = await ApiCentral.get('/sistema/configuracoes');
      if (response.data) {
        setConfig(response.data);
      }
    } catch (error) {
      console.error('Erro ao carregar configurações do tenant:', error);
    }
  };

  const salvarConfiguracoes = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      await ApiCentral.put('/sistema/configuracoes', config);
      setMensagem('Configurações do Tenant atualizadas com sucesso!');
    } catch (error) {
      setMensagem('Erro ao salvar configurações: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
          GOVERNANÇA — CONFIGURAÇÕES E PARÂMETROS DO TENANT
        </h1>
        <p className="text-sm text-[#a0a0a0]">Gestão de parâmetros operacionais, fiscais e regras de governança multi-tenant</p>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] max-w-2xl">
        <h2 className="text-lg font-bold mb-4">Parâmetros Gerais</h2>

        <form onSubmit={salvarConfiguracoes} className="space-y-4">
          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">RAZÃO SOCIAL DA EMPRESA:</label>
            <input
              type="text"
              value={config.nomeEmpresa}
              onChange={(e) => setConfig({ ...config, nomeEmpresa: e.target.value })}
              placeholder="Ex: Grupo Nexum Altivon Ltda"
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              required
            />
          </div>

          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">CNPJ:</label>
            <input
              type="text"
              value={config.cnpj}
              onChange={(e) => setConfig({ ...config, cnpj: e.target.value })}
              placeholder="Ex: 00.000.000/0001-00"
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              required
            />
          </div>

          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">AMBIENTE SEFAZ (FISCAL):</label>
            <select
              value={config.ambienteSefaz}
              onChange={(e) => setConfig({ ...config, ambienteSefaz: e.target.value })}
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
            >
              <option value="HOMOLOGACAO">Homologação (Testes)</option>
              <option value="PRODUCAO">Produção (Efetivo)</option>
            </select>
          </div>

          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">TOLERÂNCIA DE ATRASO FINANCEIRO (DIAS):</label>
            <input
              type="number"
              value={config.toleranciaAtrasoDias}
              onChange={(e) => setConfig({ ...config, toleranciaAtrasoDias: Number(e.target.value) })}
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              required
            />
          </div>

          <div className="flex items-center gap-3 pt-2">
            <input
              type="checkbox"
              id="notificar"
              checked={config.notificarEmailAtiva}
              onChange={(e) => setConfig({ ...config, notificarEmailAtiva: e.target.checked })}
              className="w-4 h-4 accent-[#c9a227]"
            />
            <label htmlFor="notificar" className="text-xs font-mono text-[#a0a0a0]">
              Habilitar notificações automáticas por e-mail para títulos vencidos
            </label>
          </div>

          <div className="pt-4">
            <button
              type="submit"
              disabled={carregando}
              className="bg-[#c9a227] text-black font-bold px-6 py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors text-xs font-mono tracking-wider"
            >
              {carregando ? 'SALVANDO...' : 'SALVAR CONFIGURAÇÕES'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TelaConfiguracoesTenant;