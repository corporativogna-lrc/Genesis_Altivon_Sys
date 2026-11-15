import React, { useState } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaPdvOmnicanal = () => {
  const [vendaId, setVendaId] = useState(null);
  const [produtoId, setProdutoId] = useState('');
  const [quantidade, setQuantidade] = useState(1);
  const [valorTotal, setValorTotal] = useState(0);
  const [statusVenda, setStatusVenda] = useState('NENHUMA');
  const [mensagem, setMensagem] = useState('');

  const iniciarVenda = async () => {
    try {
      const response = await ApiCentral.post('/pdv/vendas', { clienteId: null });
      setVendaId(response.data.id);
      setStatusVenda(response.data.status);
      setValorTotal(response.data.valorTotal);
      setMensagem('Venda iniciada com sucesso no balcão.');
    } catch (error) {
      setMensagem('Erro ao iniciar venda: ' + (error.response?.data?.erro || error.message));
    }
  };

  const adicionarItem = async (e) => {
    e.preventDefault();
    if (!vendaId) {
      alert('Inicie uma venda primeiro.');
      return;
    }

    try {
      const response = await ApiCentral.post(`/pdv/vendas/${vendaId}/itens`, {
        produtoId,
        quantidade: Number(quantidade)
      });
      setValorTotal(response.data.novoTotalVenda);
      setMensagem(`Item adicionado! Subtotal: R$ ${response.data.subtotalAdicionado}`);
      setProdutoId('');
      setQuantidade(1);
    } catch (error) {
      setMensagem('Erro ao adicionar item: ' + (error.response?.data?.erro || error.message));
    }
  };

  const finalizarVenda = async () => {
    if (!vendaId) return;

    try {
      const response = await ApiCentral.post(`/pdv/vendas/${vendaId}/finalizar`);
      setStatusVenda(response.data.status);
      setMensagem('Venda finalizada com sucesso! Estoque baixado.');
    } catch (error) {
      setMensagem('Erro ao finalizar venda: ' + (error.response?.data?.erro || error.message));
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
          PDV — FRENTE DE CAIXA OMNICANAL
        </h1>
        <p className="text-sm text-[#a0a0a0]">Operação de balcão com baixa automática de estoque via FOR UPDATE</p>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-sm text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Painel de Controle da Venda */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-1">
          <h2 className="text-lg font-bold mb-4">Controle de Caixa</h2>
          
          <div className="mb-4">
            <p className="text-xs text-[#a0a0a0]">Status Atual:</p>
            <span className="inline-block mt-1 px-2.5 py-1 rounded bg-[#050505] text-[#c9a227] font-mono text-xs border border-[#2a2a2a]">
              {statusVenda}
            </span>
          </div>

          <div className="mb-6">
            <p className="text-xs text-[#a0a0a0]">ID da Venda:</p>
            <p className="text-xs font-mono text-white mt-1 break-all">{vendaId || 'Nenhuma venda ativa'}</p>
          </div>

          {!vendaId || statusVenda !== 'ABERTA' ? (
            <button
              onClick={iniciarVenda}
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors"
            >
              INICIAR NOVA VENDA
            </button>
          ) : (
            <button
              onClick={finalizarVenda}
              className="w-full bg-emerald-600 text-white font-bold py-2.5 rounded-lg hover:bg-emerald-500 transition-colors"
            >
              FINALIZAR VENDA & BAIXAR ESTOQUE
            </button>
          )}
        </div>

        {/* Lançamento de Itens */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Adicionar Produtos ao Carrinho</h2>

          <form onSubmit={adicionarItem} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">UUID do Produto:</label>
              <input
                type="text"
                value={produtoId}
                onChange={(e) => setProdutoId(e.target.value)}
                placeholder="Ex: d3b07384-d113-4ec2-..."
                disabled={!vendaId || statusVenda !== 'ABERTA'}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Quantidade:</label>
              <input
                type="number"
                step="0.0001"
                value={quantidade}
                onChange={(e) => setQuantidade(e.target.value)}
                disabled={!vendaId || statusVenda !== 'ABERTA'}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <button
              type="submit"
              disabled={!vendaId || statusVenda !== 'ABERTA'}
              className="bg-[#2a2a2a] text-white font-medium px-4 py-2 rounded-lg hover:bg-[#3a3a3a] transition-colors border border-[#3a3a3a] disabled:opacity-50"
            >
              Adicionar Item
            </button>
          </form>

          <div className="mt-8 pt-6 border-t border-[#2a2a2a] flex justify-between items-center">
            <span className="text-sm text-[#a0a0a0]">VALOR TOTAL DA VENDA:</span>
            <span className="text-2xl font-bold text-[#c9a227] font-mono">
              R$ {Number(valorTotal).toFixed(4)}
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TelaPdvOmnicanal;