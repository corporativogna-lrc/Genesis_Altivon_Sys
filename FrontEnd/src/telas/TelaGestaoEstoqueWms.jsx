import React, { useState } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaGestaoEstoqueWms = () => {
  const [produtoId, setProdutoId] = useState('');
  const [tipoMovimento, setTipoMovimento] = useState('ENTRADA');
  const [quantidade, setQuantidade] = useState('');
  const [observacao, setObservacao] = useState('');
  const [historico, setHistorico] = useState([]);
  const [mensagem, setMensagem] = useState('');

  const registrarMovimentacao = async (e) => {
    e.preventDefault();
    try {
      const response = await ApiCentral.post('/wms/movimentacoes', {
        produtoId,
        tipoMovimento,
        quantidade: Number(quantidade),
        observacao
      });
      setMensagem(`Movimentação WMS registrada com sucesso! ID: ${response.data.id}`);
      setQuantidade('');
      setObservacao('');
      buscarHistorico(produtoId);
    } catch (error) {
      setMensagem('Erro ao movimentar estoque: ' + (error.response?.data?.erro || error.message));
    }
  };

  const buscarHistorico = async (idProduto) => {
    if (!idProduto) return;
    try {
      const response = await ApiCentral.get(`/wms/movimentacoes/produto/${idProduto}`);
      setHistorico(response.data);
    } catch (error) {
      console.error('Erro ao buscar histórico:', error);
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
          WMS — CONTROLE DE ESTOQUE E LOGÍSTICA
        </h1>
        <p className="text-sm text-[#a0a0a0]">Rastreabilidade de saldos físicos com bloqueio transacional</p>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-sm text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Formulário de Movimentação */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <h2 className="text-lg font-bold mb-4">Nova Movimentação Física</h2>

          <form onSubmit={registrarMovimentacao} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">UUID do Produto:</label>
              <input
                type="text"
                value={produtoId}
                onChange={(e) => {
                  setProdutoId(e.target.value);
                  buscarHistorico(e.target.value);
                }}
                placeholder="Ex: d3b07384-d113-4ec2-..."
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Tipo de Movimento:</label>
              <select
                value={tipoMovimento}
                onChange={(e) => setTipoMovimento(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
              >
                <option value="ENTRADA">ENTRADA</option>
                <option value="SAIDA">SAÍDA</option>
                <option value="AJUSTE">AJUSTE DE INVENTÁRIO</option>
              </select>
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Quantidade:</label>
              <input
                type="number"
                step="0.0001"
                value={quantidade}
                onChange={(e) => setQuantidade(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Observação / Justificativa:</label>
              <input
                type="text"
                value={observacao}
                onChange={(e) => setObservacao(e.target.value)}
                placeholder="Ex: Recebimento de NF de compra 1234"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors"
            >
              REGISTRAR MOVIMENTAÇÃO WMS
            </button>
          </form>
        </div>

        {/* Histórico do Produto */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <h2 className="text-lg font-bold mb-4">Histórico de Movimentações</h2>
          
          {historico.length === 0 ? (
            <p className="text-sm text-[#a0a0a0]">Informe o UUID do produto para carregar o kardex.</p>
          ) : (
            <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2">
              {historico.map((m) => (
                <div key={m.id} className="bg-[#050505] p-3 rounded-lg border border-[#2a2a2a] text-xs">
                  <div className="flex justify-between font-mono mb-1">
                    <span className="text-[#c9a227] font-bold">{m.tipoMovimento}</span>
                    <span className="text-[#a0a0a0]">{new Date(m.createdAt).toLocaleString()}</span>
                  </div>
                  <p className="text-white">Qtd: <span className="font-mono">{m.quantidade}</span></p>
                  <p className="text-[#a0a0a0] mt-1">Obs: {m.observacao}</p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TelaGestaoEstoqueWms;