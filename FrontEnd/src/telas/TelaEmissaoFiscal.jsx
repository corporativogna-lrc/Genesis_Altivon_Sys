import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaEmissaoFiscal = () => {
  const [documentos, setDocumentos] = useState([]);
  const [modelo, setModelo] = useState('65'); // 65 = NFCe, 55 = NFe
  const [numero, setNumero] = useState('');
  const [serie, setSerie] = useState('1');
  const [valorTotalNota, setValorTotalNota] = useState('');
  const [mensagem, setMensagem] = useState('');

  useEffect(() => {
    carregarDocumentos();
  }, []);

  const carregarDocumentos = async () => {
    try {
      const response = await ApiCentral.get('/fiscal/documentos');
      setDocumentos(response.data);
    } catch (error) {
      console.error('Erro ao carregar documentos fiscais:', error);
    }
  };

  const emitirDocumento = async (e) => {
    e.preventDefault();
    try {
      await ApiCentral.post('/fiscal/documentos', {
        modelo,
        numero: Number(numero),
        serie: Number(serie),
        valorTotalNota: Number(valorTotalNota)
      });
      setMensagem('Documento fiscal emitido e autorizado com sucesso!');
      setNumero('');
      setValorTotalNota('');
      carregarDocumentos();
    } catch (error) {
      setMensagem('Erro ao emitir nota: ' + (error.response?.data?.erro || error.message));
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      <div className="border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
          FISCAL — EMISSÃO DE NOTAS FISCAIS (NFE / NFCE)
        </h1>
        <p className="text-sm text-[#a0a0a0]">Gestão de documentos fiscais eletrônicos e homologação SEFAZ</p>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-sm text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        {/* Formulário de Emissão */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-1">
          <h2 className="text-lg font-bold mb-4">Emitir Nova Nota</h2>

          <form onSubmit={emitirDocumento} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Modelo Fiscal:</label>
              <select
                value={modelo}
                onChange={(e) => setModelo(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              >
                <option value="65">NFC-e (Modelo 65 - Consumidor)</option>
                <option value="55">NF-e (Modelo 55 - Eletrônica)</option>
              </select>
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Número do Documento:</label>
              <input
                type="number"
                value={numero}
                onChange={(e) => setNumero(e.target.value)}
                placeholder="Ex: 1045"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Série:</label>
              <input
                type="number"
                value={serie}
                onChange={(e) => setSerie(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1">Valor Total da Nota (R$):</label>
              <input
                type="number"
                step="0.0001"
                value={valorTotalNota}
                onChange={(e) => setValorTotalNota(e.target.value)}
                placeholder="Ex: 250.00"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors"
            >
              EMITIR & AUTORIZAR SEFAZ
            </button>
          </form>
        </div>

        {/* Listagem de Notas Emitidas */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Notas Fiscais Emitidas</h2>

          <div className="overflow-x-auto max-h-[450px] overflow-y-auto">
            <table className="w-full text-left text-xs text-white">
              <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
                <tr>
                  <th className="p-3">Modelo</th>
                  <th className="p-3">Número / Série</th>
                  <th className="p-3">Status SEFAZ</th>
                  <th className="p-3">Valor Total</th>
                  <th className="p-3">Chave de Acesso</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2a2a2a]">
                {documentos.length === 0 ? (
                  <tr>
                    <td colSpan="5" className="p-4 text-center text-[#a0a0a0]">Nenhum documento fiscal emitido.</td>
                  </tr>
                ) : (
                  documentos.map((d) => (
                    <tr key={d.id} className="hover:bg-[#101114]">
                      <td className="p-3 font-mono font-bold text-[#c9a227]">{d.modelo}</td>
                      <td className="p-3 font-mono">{d.numero} / {d.serie}</td>
                      <td className="p-3">
                        <span className="px-2 py-1 rounded text-[10px] font-mono bg-emerald-950 text-emerald-400 border border-emerald-800">
                          {d.statusSefaz}
                        </span>
                      </td>
                      <td className="p-3 font-mono font-medium">R$ {Number(d.valorTotalNota).toFixed(2)}</td>
                      <td className="p-3 font-mono text-[10px] text-[#a0a0a0] truncate max-w-[200px]">{d.chaveAcesso || 'N/A'}</td>
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

export default TelaEmissaoFiscal;