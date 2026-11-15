import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaContasPagarReceber = () => {
  const [titulos, setTitulos] = useState([]);
  const [tipoTitulo, setTipoTitulo] = useState('RECEBER');
  const [descricao, setDescricao] = useState('');
  const [valorOriginal, setValorOriginal] = useState('');
  const [dataVencimento, setDataVencimento] = useState('');
  const [valorPago, setValorPago] = useState('');
  const [mensagem, setMensagem] = useState('');

  useEffect(() => {
    carregarTitulos();
  }, []);

  const carregarTitulos = async () => {
    try {
      const response = await ApiCentral.get('/fico/titulos');
      setTitulos(response.data);
    } catch (error) {
      console.error('Erro ao carregar títulos financeiros:', error);
    }
  };

  const criarTitulo = async (e) => {
    e.preventDefault();
    try {
      await ApiCentral.post('/fico/titulos', {
        tipoTitulo,
        descricao,
        valorOriginal: Number(valorOriginal),
        dataVencimento: new Date(dataVencimento).toISOString()
      });
      setMensagem('Título financeiro cadastrado com sucesso!');
      setDescricao('');
      setValorOriginal('');
      setDataVencimento('');
      carregarTitulos();
    } catch (error) {
      setMensagem('Erro ao criar título: ' + (error.response?.data?.erro || error.message));
    }
  };

  const baixarTitulo = async (id) => {
    if (!valorPago || Number(valorPago) <= 0) {
      alert('Informe um valor de pagamento válido.');
      return;
    }

    try {
      await ApiCentral.post(`/fico/titulos/${id}/baixar`, {
        valorPago: Number(valorPago),
        dataPagamento: new Date().toISOString()
      });
      setMensagem('Baixa de título realizada com sucesso!');
      setValorPago('');
      carregarTitulos();
    } catch (error) {
      setMensagem('Erro ao baixar título: ' + (error.response?.data?.erro || error.message));
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
          FICO — GESTÃO FINANCEIRA (CONTAS A PAGAR & RECEBER)
        </h1>
        <p className="text-sm text-[#a0a0a0]">Controle de fluxo de caixa, vencimentos e liquidação de títulos</p>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-sm text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        {/* Formulário de Novo Título */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-1">
          <h2 className="text-lg font-bold mb-4">Novo Título FICO</h2>

          <form onSubmit={criarTitulo} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Tipo:</label>
              <select
                value={tipoTitulo}
                onChange={(e) => setTipoTitulo(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
              >
                <option value="RECEBER">CONTAS A RECEBER</option>
                <option value="PAGAR">CONTAS A PAGAR</option>
              </select>
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Descrição:</label>
              <input
                type="text"
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
                placeholder="Ex: Venda Varejo #102 ou Fornecedor X"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Valor Original (R$):</label>
              <input
                type="number"
                step="0.0001"
                value={valorOriginal}
                onChange={(e) => setValorOriginal(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Data de Vencimento:</label>
              <input
                type="date"
                value={dataVencimento}
                onChange={(e) => setDataVencimento(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors"
            >
              CADASTRAR TÍTULO
            </button>
          </form>
        </div>

        {/* Listagem de Títulos */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Títulos Financeiros Registrados</h2>

          <div className="overflow-x-auto max-h-[450px] overflow-y-auto">
            <table className="w-full text-left text-xs text-white">
              <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
                <tr>
                  <th className="p-3">Tipo</th>
                  <th className="p-3">Descrição</th>
                  <th className="p-3">Status</th>
                  <th className="p-3">Valor</th>
                  <th className="p-3">Vencimento</th>
                  <th className="p-3">Ação / Baixa</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2a2a2a]">
                {titulos.length === 0 ? (
                  <tr>
                    <td colSpan="6" className="p-4 text-center text-[#a0a0a0]">Nenhum título cadastrado.</td>
                  </tr>
                ) : (
                  titulos.map((t) => (
                    <tr key={t.id} className="hover:bg-[#101114]">
                      <td className="p-3 font-mono font-bold text-[#c9a227]">{t.tipoTitulo}</td>
                      <td className="p-3">{t.descricao}</td>
                      <td className="p-3">
                        <span className={`px-2 py-1 rounded text-[10px] font-mono ${
                          t.status === 'PAGO' ? 'bg-emerald-950 text-emerald-400 border border-emerald-800' :
                          t.status === 'ATRASADO' ? 'bg-red-950 text-red-400 border border-red-800' :
                          'bg-amber-950 text-amber-400 border border-amber-800'
                        }`}>
                          {t.status}
                        </span>
                      </td>
                      <td className="p-3 font-mono font-medium">R$ {Number(t.valorOriginal).toFixed(2)}</td>
                      <td className="p-3 font-mono">{new Date(t.dataVencimento).toLocaleDateString()}</td>
                      <td className="p-3">
                        {t.status !== 'PAGO' ? (
                          <div className="flex items-center gap-2">
                            <input
                              type="number"
                              step="0.01"
                              placeholder="Valor pago"
                              onChange={(e) => setValorPago(e.target.value)}
                              className="w-24 bg-[#050505] border border-[#2a2a2a] rounded p-1 text-xs text-white outline-none"
                            />
                            <button
                              onClick={() => baixarTitulo(t.id)}
                              className="bg-emerald-600 text-white px-2 py-1 rounded text-xs hover:bg-emerald-500 font-medium"
                            >
                              Baixar
                            </button>
                          </div>
                        ) : (
                          <span className="text-emerald-400 font-mono text-[11px]">Liquidado</span>
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
    </div>
  );
};

export default TelaContasPagarReceber;