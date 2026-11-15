import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaWmsSeparacaoPicking = () => {
  const [ordens, setOrdens] = useState([]);
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarOrdensSeparacao();
  }, []);

  const carregarOrdensSeparacao = async () => {
    try {
      const response = await ApiCentral.get('/wms/separacao');
      setOrdens(response.data || []);
    } catch (error) {
      console.error('Erro ao carregar ordens de separação:', error);
    }
  };

  const iniciarSeparacao = async (id) => {
    setCarregando(true);
    try {
      await ApiCentral.post(`/wms/separacao/${id}/iniciar`);
      setMensagem('Ordem de picking iniciada com sucesso!');
      carregarOrdensSeparacao();
    } catch (error) {
      setMensagem('Erro ao iniciar separação: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  const concluirSeparacao = async (id) => {
    setCarregando(true);
    try {
      await ApiCentral.post(`/wms/separacao/${id}/concluir`);
      setMensagem('Separação concluída e estoque baixado com sucesso!');
      carregarOrdensSeparacao();
    } catch (error) {
      setMensagem('Erro ao concluir separação: ' + (error.response?.data?.erro || error.message));
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
            WMS — SEPARAÇÃO DE PEDIDOS (PICKING)
          </h1>
          <p className="text-sm text-[#a0a0a0]">Gestão de separação, conferência de itens e expedição logística</p>
        </div>
        <button
          onClick={carregarOrdensSeparacao}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          ↻ Sincronizar WMS
        </button>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
        <h2 className="text-lg font-bold mb-4">Ordens de Separação em Aberto</h2>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-white">
            <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
              <tr>
                <th className="p-3">ID Ordem</th>
                <th className="p-3">Pedido / Destino</th>
                <th className="p-3">Status Picking</th>
                <th className="p-3">Itens Separados</th>
                <th className="p-3 text-right">Ações Operacionais</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[#2a2a2a]">
              {ordens.length === 0 ? (
                <tr>
                  <td colSpan="5" className="p-4 text-center text-[#a0a0a0]">Nenhuma ordem de separação pendente no momento.</td>
                </tr>
              ) : (
                ordens.map((o) => (
                  <tr key={o.id} className="hover:bg-[#101114]">
                    <td className="p-3 font-mono font-bold text-[#c9a227]">#{o.id}</td>
                    <td className="p-3 font-mono">{o.pedidoId || 'Pedido Balcão'}</td>
                    <td className="p-3">
                      <span className="px-2 py-1 rounded text-[10px] font-mono bg-amber-950 text-amber-400 border border-amber-800">
                        {o.status || 'AGUARDANDO'}
                      </span>
                    </td>
                    <td className="p-3 font-mono">{o.itensConferidos || 0} / {o.totalItens || 0}</td>
                    <td className="p-3 text-right space-x-2">
                      <button
                        onClick={() => iniciarSeparacao(o.id)}
                        disabled={carregando}
                        className="bg-[#2a2a2a] text-[#c9a227] px-3 py-1.5 rounded text-xs font-mono hover:bg-[#3a3a3a] transition-colors"
                      >
                        Iniciar
                      </button>
                      <button
                        onClick={() => concluirSeparacao(o.id)}
                        disabled={carregando}
                        className="bg-[#c9a227] text-black px-3 py-1.5 rounded text-xs font-bold font-mono hover:bg-[#e8d5a3] transition-colors"
                      >
                        Concluir
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default TelaWmsSeparacaoPicking;