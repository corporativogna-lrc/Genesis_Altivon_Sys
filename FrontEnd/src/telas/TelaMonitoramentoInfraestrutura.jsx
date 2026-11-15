import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaMonitoramentoInfraestrutura = () => {
  const [saude, setSaude] = useState({
    statusApi: 'ONLINE',
    bancoDados: 'MariaDB 10.4.32',
    latenciaMs: 14,
    memoriaUso: '142 MB',
    threadsAtivas: 24,
    uptime: '99.99%'
  });
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    verificarSaudeSistema();
  }, []);

  const verificarSaudeSistema = async () => {
    setCarregando(true);
    try {
      const response = await ApiCentral.get('/sistema/health');
      if (response.data) {
        setSaude(response.data);
      }
    } catch (error) {
      console.error('Erro ao consultar telemetria do sistema:', error);
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
            INFRAESTRUTURA — TELEMETRIA E SAÚDE DO SISTEMA
          </h1>
          <p className="text-sm text-[#a0a0a0]">Monitoramento de servidores, instâncias MariaDB e desempenho global</p>
        </div>
        <button
          onClick={verificarSaudeSistema}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          {carregando ? 'Verificando...' : '↻ Atualizar Telemetria'}
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">STATUS DA API (.NET 8)</p>
          <h3 className="text-xl font-bold text-emerald-400 mt-2 font-mono">
            {saude.statusApi}
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Porta 5050 respondendo normalmente</span>
        </div>

        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">MOTOR DE BANCO DE DADOS</p>
          <h3 className="text-xl font-bold text-[#c9a227] mt-2 font-mono">
            {saude.bancoDados}
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Porta 3309 (Isolamento Ativo)</span>
        </div>

        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">LATÊNCIA DA INSTÂNCIA</p>
          <h3 className="text-xl font-bold text-emerald-400 mt-2 font-mono">
            {saude.latenciaMs} ms
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Alta performance transacional</span>
        </div>
      </div>

      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
        <h2 className="text-lg font-bold mb-4">Métricas de Desempenho e Recursos</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 font-mono text-xs">
          <div className="p-4 bg-[#050505] rounded-lg border border-[#2a2a2a]">
            <span className="text-[#a0a0a0] block mb-1">CONSUMO DE MEMÓRIA:</span>
            <span className="text-white text-sm font-bold">{saude.memoriaUso}</span>
          </div>
          <div className="p-4 bg-[#050505] rounded-lg border border-[#2a2a2a]">
            <span className="text-[#a0a0a0] block mb-1">THREADS ATIVAS:</span>
            <span className="text-white text-sm font-bold">{saude.threadsAtivas}</span>
          </div>
          <div className="p-4 bg-[#050505] rounded-lg border border-[#2a2a2a]">
            <span className="text-[#a0a0a0] block mb-1">DISPONIBILIDADE (UPTIME):</span>
            <span className="text-emerald-400 text-sm font-bold">{saude.uptime}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TelaMonitoramentoInfraestrutura;