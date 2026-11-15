import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaGestaoCompras = () => {
  const [compras, setCompras] = useState([]);
  const [fornecedor, setFornecedor] = useState('');
  const [valorTotal, setValorTotal] = useState('');
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarOrdensCompras();
  }, []);

  const carregarOrdensCompras = async () => {
    try {
      const response = await ApiCentral.get('/scm/compras');
      setCompras(response.data || []);
    } catch (error) {
      console.error('Erro ao carregar ordens de compra:', error);
    }
  };

  const criarOrdemCompra = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      await ApiCentral.post('/scm/compras', {
        fornecedor,
        valorTotal: Number(valorTotal)
      });
      setMensagem('Ordem de Compra criada com sucesso!');
      setFornecedor('');
      setValorTotal('');
      carregarOrdensCompras();
    } catch (error) {
      setMensagem('Erro ao criar ordem de compra: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  const importarXmlNfe = async (id) => {
    setCarregando(true);
    try {
      await ApiCentral.post(`/scm/compras/${id}/importar-xml`);
      setMensagem('XML da NFe de entrada importado e estoque atualizado com sucesso!');
      carregarOrdensCompras();
    } catch (error) {
      setMensagem('Erro ao importar XML: ' + (error.response?.data?.erro || error.message));
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
            SCM — GESTÃO DE COMPRAS & XML DE FORNECEDORES
          </h1>
          <p className="text-sm text-[#a0a0a0]">Controle de suprimentos, cotações, pedidos e integração fiscal de entrada</p>
        </div>
        <button
          onClick={carregarOrdensCompras}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          ↻ Sincronizar SCM
        </button>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Formulário Nova Compra */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <h2 className="text-lg font-bold mb-4">Nova Ordem de Compra</h2>
          <form onSubmit={criarOrdemCompra} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">FORNECEDOR:</label>
              <input
                type="text"
                value={fornecedor}
                onChange={(e) => setFornecedor(e.target.value)}
                placeholder="Ex: Distribuidora Altivon Ltda"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">VALOR TOTAL (R$):</label>
              <input
                type="number"
                step="0.01"
                value={valorTotal}
                onChange={(e) => setValorTotal(e.target.value)}
                placeholder="Ex: 5400.00"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <button
              type="submit"
              disabled={carregando}
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors text-xs font-mono"
            >
              {carregando ? 'REGISTRANDO...' : 'REGISTRAR PEDIDO DE COMPRA'}
            </button>
          </form>
        </div>

        {/* Tabela de Ordens */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Ordens de Compra e Notas de Entrada</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-white">
              <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
                <tr>
                  <th className="p-3">ID</th>
                  <th className="p-3">Fornecedor</th>
                  <th className="p-3">Valor Total</th>
                  <th className="p-3">Status SCM</th>
                  <th className="p-3 text-right">Ação / XML</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2a2a2a]">
                {compras.length === 0 ? (
                  <tr>
                    <td colSpan="5" className="p-4 text-center text-[#a0a0a0]">Nenhuma ordem de compra cadastrada.</td>
                  </tr>
                ) : (
                  compras.map((c) => (
                    <tr key={c.id} className="hover:bg-[#101114]">
                      <td className="p-3 font-mono font-bold text-[#c9a227]">#{c.id}</td>
                      <td className="p-3 font-medium">{c.fornecedor}</td>
                      <td className="p-3 font-mono">R$ {Number(c.valorTotal).toFixed(2)}</td>
                      <td className="p-3">
                        <span className="px-2 py-1 rounded text-[10px] font-mono bg-blue-950 text-blue-400 border border-blue-800">
                          {c.status || 'PENDENTE'}
                        </span>
                      </td>
                      <td className="p-3 text-right">
                        <button
                          onClick={() => importarXmlNfe(c.id)}
                          disabled={carregando}
                          className="bg-[#2a2a2a] text-[#c9a227] px-3 py-1.5 rounded text-xs font-mono hover:bg-[#3a3a3a] transition-colors"
                        >
                          Importar XML NFe
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
    </div>
  );
};

export default TelaGestaoCompras;