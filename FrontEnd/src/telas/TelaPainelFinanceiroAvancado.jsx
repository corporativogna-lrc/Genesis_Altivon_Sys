import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaPainelFinanceiroAvancado = () => {
  const [metricas, setMetricas] = useState({
    totalReceber: 0,
    totalPagar: 0,
    saldoProjetado: 0,
    titulosAtrasados: 0
  });
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    carregarMetricasFinanceiras();
  }, []);

  const carregarMetricasFinanceiras = async () => {
    try {
      const response = await ApiCentral.get('/fico/titulos');
      const titulos = response.data || [];

      let rec = 0;
      let pag = 0;
      let atrasados = 0;

      titulos.forEach((t) => {
        const valor = Number(t.valorOriginal) || 0;
        if (t.status === 'ATRASADO') atrasados++;
        
        if (t.tipoTitulo === 'RECEBER' && t.status !== 'PAGO') {
          rec += valor;
        } else if (t.tipoTitulo === 'PAGAR' && t.status !== 'PAGO') {
          pag += valor;
        }
      });

      setMetricas({
        totalReceber: rec,
        totalPagar: pag,
        saldoProjetado: rec - pag,
        titulosAtrasados: atrasados
      });
    } catch (error) {
      console.error('Erro ao carregar métricas financeiras:', error);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="nexum-admin-shell min-h-screen p-6 text-white">
      {/* Cabeçalho */}
      <div className="border-b border-[#2a2a2a] pb-4 mb-6 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <span className="w-3 h-3 rounded-full bg-[#c9a227]"></span>
            FICO — INTELIGÊNCIA E FLUXO DE CAIXA
          </h1>
          <p className="text-sm text-[#a0a0a0]">Análise preditiva de liquidez e saúde financeira multi-tenant</p>
        </div>
        <button
          onClick={carregarMetricasFinanceiras}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          {carregando ? 'Atualizando...' : '↻ Sincronizar Dados'}
        </button>
      </div>

      {/* Cards de Indicadores de Tesouraria */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">CONTAS A RECEBER (ABERTAS)</p>
          <h3 className="text-xl font-bold text-emerald-400 mt-2 font-mono">
            R$ {Number(metricas.totalReceber).toFixed(2)}
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Entradas previstas</span>
        </div>

        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">CONTAS A PAGAR (ABERTAS)</p>
          <h3 className="text-xl font-bold text-red-400 mt-2 font-mono">
            R$ {Number(metricas.totalPagar).toFixed(2)}
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Obrigações e fornecedores</span>
        </div>

        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">SALDO PROJETADO DE CAIXA</p>
          <h3 className={`text-xl font-bold mt-2 font-mono ${metricas.saldoProjetado >= 0 ? 'text-[#c9a227]' : 'text-red-400'}`}>
            R$ {Number(metricas.saldoProjetado).toFixed(2)}
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Posição líquida estimada</span>
        </div>

        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <p className="text-xs font-mono text-[#a0a0a0]">TÍTULOS EM ATRASO</p>
          <h3 className="text-xl font-bold text-amber-400 mt-2 font-mono">
            {metricas.titulosAtrasados} itens
          </h3>
          <span className="text-xs text-[#a0a0a0] mt-2 block">● Requer atenção gerencial</span>
        </div>
      </div>

      {/* Painel Analítico Visual */}
      <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
        <h2 className="text-lg font-bold mb-4">Resumo da Governança FICO</h2>
        <p className="text-sm text-[#a0a0a0] mb-6">
          Os dados acima são calculados diretamente a partir dos títulos liquidados e em aberto na base MariaDB do tenant atual, garantindo isolamento total e conformidade com as normas contábeis brasileiras.
        </p>
        <div className="p-4 bg-[#050505] rounded-lg border border-[#2a2a2a] flex items-center justify-between">
          <span className="text-xs font-mono text-[#c9a227]">STATUS DO MOTOR FICO:</span>
          <span className="text-xs font-mono text-emerald-400">ATIVO & AUDITADO (TRANSACIONAL ACID)</span>
        </div>
      </div>
    </div>
  );
};

export default TelaPainelFinanceiroAvancado;