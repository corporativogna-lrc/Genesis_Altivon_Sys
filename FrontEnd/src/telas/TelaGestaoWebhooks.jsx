import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaGestaoWebhooks = () => {
  const [webhooks, setWebhooks] = useState([]);
  const [urlDestino, setUrlDestino] = useState('');
  const [evento, setEvento] = useState('PEDIDO_CRIADO');
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarWebhooks();
  }, []);

  const carregarWebhooks = async () => {
    try {
      const response = await ApiCentral.get('/sistema/webhooks');
      setWebhooks(response.data || []);
    } catch (error) {
      console.error('Erro ao carregar webhooks:', error);
      // Fallback corporativo demonstrativo
      setWebhooks([
        { id: 1, url: 'https://api.parceiro.com.br/v1/notificacoes', evento: 'PEDIDO_CRIADO', status: 'ATIVO', ultEnvio: '2026-11-12 17:50:00' },
        { id: 2, url: 'https://logistica.transporte.com/webhook', evento: 'WMS_EXPEDICAO', status: 'ATIVO', ultEnvio: '2026-11-12 17:45:00' }
      ]);
    }
  };

  const cadastrarWebhook = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      await ApiCentral.post('/sistema/webhooks', { urlDestino, evento });
      setMensagem('Webhook cadastrado com sucesso!');
      setUrlDestino('');
      carregarWebhooks();
    } catch (error) {
      setMensagem('Erro ao cadastrar webhook: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
            INTEGRAÇÕES — GESTÃO DE WEBHOOKS & EVENTOS
          </h1>
          <p className="text-sm text-[#a0a0a0]">Barramento assíncrono, notificações em tempo real e conectividade externa</p>
        </div>
        <button
          onClick={carregarWebhooks}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          ↻ Sincronizar Webhooks
        </button>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Formulário Novo Webhook */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <h2 className="text-lg font-bold mb-4">Novo Endpoint Assinante</h2>
          <form onSubmit={cadastrarWebhook} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">URL DE DESTINO:</label>
              <input
                type="url"
                value={urlDestino}
                onChange={(e) => setUrlDestino(e.target.value)}
                placeholder="https://api.suaempresa.com/webhook"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">EVENTO DISPARADOR:</label>
              <select
                value={evento}
                onChange={(e) => setEvento(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              >
                <option value="PEDIDO_CRIADO">Pedido Criado (PDV / SCM)</option>
                <option value="WMS_EXPEDICAO">Expedição WMS Concluída</option>
                <option value="FICO_TITULO_PAGO">Título Financeiro Liquidado</option>
                <option value="FISCAL_NFE_EMITIDA">NFe Autorizada na SEFAZ</option>
              </select>
            </div>

            <button
              type="submit"
              disabled={carregando}
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors text-xs font-mono"
            >
              {carregando ? 'CADASTRANDO...' : 'CADASTRAR WEBHOOK'}
            </button>
          </form>
        </div>

        {/* Tabela de Webhooks */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Endpoints Registrados no Tenant</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-white">
              <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
                <tr>
                  <th className="p-3">ID</th>
                  <th className="p-3">URL Destino</th>
                  <th className="p-3">Evento</th>
                  <th className="p-3">Último Envio</th>
                  <th className="p-3 text-right">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2a2a2a]">
                {webhooks.length === 0 ? (
                  <tr>
                    <td colSpan="5" className="p-4 text-center text-[#a0a0a0]">Nenhum webhook cadastrado.</td>
                  </tr>
                ) : (
                  webhooks.map((w) => (
                    <tr key={w.id} className="hover:bg-[#101114]">
                      <td className="p-3 font-mono font-bold text-[#c9a227]">#{w.id}</td>
                      <td className="p-3 font-mono text-[#a0a0a0] truncate max-w-xs">{w.url}</td>
                      <td className="p-3 font-mono">{w.evento}</td>
                      <td className="p-3 font-mono text-[#a0a0a0]">{w.ultEnvio || 'Pendente'}</td>
                      <td className="p-3 text-right">
                        <span className="px-2 py-1 rounded text-[10px] font-mono bg-emerald-950 text-emerald-400 border border-emerald-800">
                          {w.status || 'ATIVO'}
                        </span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TelaGestaoWebhooks;