import React, { useState, useEffect } from 'react';
import GerenciadorSessao from '../servicos/GerenciadorSessaoMfa';

export const SeletorTenantMultiempresa = () => {
  const [tenantAtual, setTenantAtual] = useState('');

  useEffect(() => {
    const tenantSalvo = GerenciadorSessao.obterTenantAtual();
    if (tenantSalvo) {
      setTenantAtual(tenantSalvo);
    }
  }, []);

  const lidarComMudancaTenant = (e) => {
    const novoTenant = e.target.value;
    setTenantAtual(novoTenant);
    GerenciadorSessao.definirTenantAtual(novoTenant);
    // Recarrega a página para atualizar o contexto global das requisições
    window.location.reload();
  };

  return (
    <div className="flex items-center gap-2 bg-[#1a1a1a] px-3 py-1.5 rounded-lg border border-[#2a2a2a]">
      <span className="text-xs font-mono text-[#c9a227] tracking-wider">TENANT ID:</span>
      <input
        type="text"
        value={tenantAtual}
        onChange={lidarComMudancaTenant}
        placeholder="Informe o UUID do Tenant"
        className="bg-[#050505] text-xs text-white px-2 py-1 rounded border border-[#2a2a2a] focus:border-[#c9a227] outline-none w-64 font-mono"
      />
    </div>
  );
};

export default SeletorTenantMultiempresa;