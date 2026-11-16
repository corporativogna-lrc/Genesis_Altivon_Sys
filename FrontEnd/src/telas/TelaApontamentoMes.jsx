import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaApontamentoMes = () => {
  const [ordens, setOrdens] = useState([]);
  const [produtoAcabadoId, setProdutoAcabadoId] = useState('');
  const [linhaProducao, setLinhaProducao] = useState('LINHA-01');
  const [quantidadePlanejada, setQuantidadePlanejada] = useState('');
  const [qtdApontamento, setQtdApontamento] = useState({});
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarOrdens();
  }, []);

  const carregarOrdens = async () => {
    setCarregando(true);
    try {
      const response = await ApiCentral.get('/mes/ordens-producao');
      setOrdens(response.data || []);
    } catch (error) {
      setMensagem('Aviso ao carregar ordens de produção: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  const criarOrdemProducao = async (e) => {
    e.preventDefault();
    setMensagem('');
    try {
      const response = await ApiCentral.post('/mes/ordens-producao', {
        produtoAcabadoId: produtoAcabadoId.trim(),
        linhaProducao: linhaProducao.trim(),
        quantidadePlanejada: Number(quantidadePlanejada)
      });
      setMensagem(`Ordem de Produção ${response.data.codigoOp} criada com sucesso!`);
      setProdutoAcabadoId('');
      setQuantidadePlanejada('');
      carregarOrdens();
    } catch (error) {
      setMensagem('Erro ao criar OP: ' + (error.response?.data?.erro || error.message));
    }
  };

  const iniciarProducao = async (id) => {
    setMensagem('');
    try {
      await ApiCentral.post(`/mes/ordens-producao/${id}/iniciar`);
      setMensagem('Ordem de produção iniciada no chão de fábrica.');
      carregarOrdens();
    } catch (error) {
      setMensagem('Erro ao iniciar OP: ' + (error.response?.data?.erro || error.message));
    }
  };

  const concluirApontamento = async (op) => {
    setMensagem('');
    const quantidadeFinal = Number(qtdApontamento[op.id] || op.quantidadePlanejada);
    try {
      await ApiCentral.post(`/mes/ordens-producao/${op.id}/concluir`, {
        quantidadeProduzida: quantidadeFinal
      });
      setMensagem(`OP ${op.codigoOp} concluída! ${quantidadeFinal} un. creditadas no estoque WMS.`);
      carregarOrdens();
    } catch (error) {
      setMensagem('Erro ao concluir OP: ' + (error.response?.data?.erro || error.message));
    }
  };

  const badgeStatus = (status) => {
    switch (status) {
      case 'CONCLUIDA':
        return 'bg-emerald-950 text-emerald-400 border-emerald-800';
      case 'EM_PRODUCAO':
        return 'bg-amber-950 text-[#c9a227] border-amber-800';
      case 'CANCELADA':
        return 'bg-red-950 text-red-400 border-red-800';
      default:
        return 'bg-zinc-900 text-zinc-300 border-zinc-700';
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
            MES — MANUFATURA & APONTAMENTO DE PRODUÇÃO
          </h1>
          <p className="text-sm text-[#a0a0a0]">
            Planejamento fabril, execução em linha e entrada automática de produto acabado no estoque
          </p>
        </div>
        <button
          onClick={carregarOrdens}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          {carregando ? 'Sincronizando...' : '↻ Atualizar Chão de Fábrica'}
        </button>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      {/* Formulário Nova Ordem de Produção */}
      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] mb-6">
        <h2 className="text-lg font-bold mb-4">Abrir Ordem de Produção (OP)</h2>
        <form onSubmit={criarOrdemProducao} className="grid grid-cols-1 md:grid-cols-4 gap-4 items-end">
          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1">UUID do Produto Acabado:</label>
            <input
              type="text"
              value={produtoAcabadoId}
              onChange={(e) => setProdutoAcabadoId(e.target.value)}
              placeholder="Ex: d3b07384-d113-4ec2..."
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-xs text-white font-mono focus:border-[#c9a227] outline-none"
              required
            />
          </div>

          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1">Linha / Célula Fabril:</label>
            <input
              type="text"
              value={linhaProducao}
              onChange={(e) => setLinhaProducao(e.target.value)}
              placeholder="LINHA-01"
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-xs text-white font-mono focus:border-[#c9a227] outline-none"
              required
            />
          </div>

          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1">Quantidade Planejada:</label>
            <input
              type="number"
              step="0.0001"
              value={quantidadePlanejada}
              onChange={(e) => setQuantidadePlanejada(e.target.value)}
              placeholder="100"
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-xs text-white font-mono focus:border-[#c9a227] outline-none"
              required
            />
          </div>

          <button
            type="submit"
            className="bg-[#c9a227] text-black font-bold py-2.5 px-4 rounded-lg text-xs hover:bg-[#e8d5a3] transition-colors"
          >
            + GERAR ORDEM DE PRODUÇÃO
          </button>
        </form>
      </div>

      {/* Tabela de Ordens de Produção */}
      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
        <h2 className="text-lg font-bold mb-4">Fila de Produção e Apontamentos</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-white">
            <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
              <tr>
                <th className="p-3">Código OP</th>
                <th className="p-3">Linha</th>
                <th className="p-3">Produto ID</th>
                <th className="p-3">Planejado</th>
                <th className="p-3">Produzido</th>
                <th className="p-3">Status</th>
                <th className="p-3 text-right">Apontamento</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[#2a2a2a]">
              {ordens.length === 0 ? (
                <tr>
                  <td colSpan="7" className="p-4 text-center text-[#a0a0a0]">
                    Nenhuma ordem de produção cadastrada.
                  </td>
                </tr>
              ) : (
                ordens.map((op) => (
                  <tr key={op.id} className="hover:bg-[#101114]">
                    <td className="p-3 font-mono font-bold text-[#c9a227]">{op.codigoOp}</td>
                    <td className="p-3 font-mono text-[#a0a0a0]">{op.linhaProducao}</td>
                    <td className="p-3 font-mono text-[#a0a0a0]">{op.produtoAcabadoId}</td>
                    <td className="p-3 font-mono">{Number(op.quantidadePlanejada).toFixed(2)}</td>
                    <td className="p-3 font-mono">{Number(op.quantidadeProduzida).toFixed(2)}</td>
                    <td className="p-3">
                      <span className={`px-2 py-1 rounded text-[10px] font-mono border ${badgeStatus(op.statusOp)}`}>
                        {op.statusOp}
                      </span>
                    </td>
                    <td className="p-3 text-right space-x-2 font-mono">
                      {op.statusOp === 'PLANEJADA' && (
                        <button
                          onClick={() => iniciarProducao(op.id)}
                          className="bg-[#2a2a2a] text-[#c9a227] px-2.5 py-1 rounded hover:bg-[#3a3a3a]"
                        >
                          Iniciar Linha
                        </button>
                      )}
                      {(op.statusOp === 'PLANEJADA' || op.statusOp === 'EM_PRODUCAO') && (
                        <>
                          <input
                            type="number"
                            step="0.01"
                            placeholder={op.quantidadePlanejada}
                            value={qtdApontamento[op.id] || ''}
                            onChange={(e) =>
                              setQtdApontamento({ ...qtdApontamento, [op.id]: e.target.value })
                            }
                            className="w-20 bg-[#050505] border border-[#2a2a2a] rounded px-2 py-1 text-xs text-white text-right"
                          />
                          <button
                            onClick={() => concluirApontamento(op)}
                            className="bg-emerald-900/60 text-emerald-300 border border-emerald-700 px-2.5 py-1 rounded hover:bg-emerald-800/60"
                          >
                            Concluir & Estocar
                          </button>
                        </>
                      )}
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

export default TelaApontamentoMes;