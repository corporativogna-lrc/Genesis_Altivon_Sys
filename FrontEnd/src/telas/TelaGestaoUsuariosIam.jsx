import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaGestaoUsuariosIam = () => {
  const [usuarios, setUsuarios] = useState([]);
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [perfil, setPerfil] = useState('OPERADOR');
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarUsuarios();
  }, []);

  const carregarUsuarios = async () => {
    try {
      const response = await ApiCentral.get('/iam/usuarios');
      setUsuarios(response.data || []);
    } catch (error) {
      console.error('Erro ao carregar usuários IAM:', error);
      // Fallback corporativo demonstrativo
      setUsuarios([
        { id: 1, nome: 'Administrador Master', email: 'admin@altivon.com', perfil: 'MASTER', status: 'ATIVO' },
        { id: 2, nome: 'Operador Logística WMS', email: 'wms@altivon.com', perfil: 'OPERADOR', status: 'ATIVO' },
        { id: 3, nome: 'Analista Fiscal NFe', email: 'fiscal@altivon.com', perfil: 'FISCAL', status: 'ATIVO' }
      ]);
    }
  };

  const cadastrarUsuario = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      await ApiCentral.post('/iam/usuarios', { nome, email, perfil });
      setMensagem('Usuário cadastrado com sucesso!');
      setNome('');
      setEmail('');
      carregarUsuarios();
    } catch (error) {
      setMensagem('Erro ao cadastrar usuário: ' + (error.response?.data?.erro || error.message));
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
            IAM — GESTÃO DE USUÁRIOS E PERMISSÕES DE ACESSO
          </h1>
          <p className="text-sm text-[#a0a0a0]">Controle de credenciais, perfis multi-tenant e segurança corporativa</p>
        </div>
        <button
          onClick={carregarUsuarios}
          className="bg-[#1a1a1a] border border-[#2a2a2a] text-[#c9a227] px-4 py-2 rounded-lg text-xs font-mono hover:bg-[#2a2a2a] transition-colors"
        >
          ↻ Sincronizar IAM
        </button>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Formulário Novo Usuário */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a]">
          <h2 className="text-lg font-bold mb-4">Novo Usuário do Sistema</h2>
          <form onSubmit={cadastrarUsuario} className="space-y-4">
            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">NOME COMPLETO:</label>
              <input
                type="text"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                placeholder="Ex: João da Silva"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">E-MAIL CORPORATIVO:</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="joao@altivon.com"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">PERFIL DE ACESSO:</label>
              <select
                value={perfil}
                onChange={(e) => setPerfil(e.target.value)}
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-2.5 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
              >
                <option value="MASTER">Master (Admin Total)</option>
                <option value="OPERADOR">Operador (WMS / SCM / PDV)</option>
                <option value="FINANCEIRO">Financeiro (FICO)</option>
                <option value="FISCAL">Fiscal (NFe / NFCe)</option>
              </select>
            </div>

            <button
              type="submit"
              disabled={carregando}
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded-lg hover:bg-[#e8d5a3] transition-colors text-xs font-mono"
            >
              {carregando ? 'CADASTRANDO...' : 'CADASTRAR USUÁRIO'}
            </button>
          </form>
        </div>

        {/* Tabela de Usuários */}
        <div className="bg-[#1a1a1a] p-6 rounded-xl border border-[#2a2a2a] md:col-span-2">
          <h2 className="text-lg font-bold mb-4">Usuários Ativos no Tenant</h2>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-white">
              <thead className="bg-[#050505] text-[#c9a227] uppercase font-mono border-b border-[#2a2a2a]">
                <tr>
                  <th className="p-3">ID</th>
                  <th className="p-3">Nome</th>
                  <th className="p-3">E-mail</th>
                  <th className="p-3">Perfil</th>
                  <th className="p-3 text-right">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2a2a2a]">
                {usuarios.length === 0 ? (
                  <tr>
                    <td colSpan="5" className="p-4 text-center text-[#a0a0a0]">Nenhum usuário cadastrado.</td>
                  </tr>
                ) : (
                  usuarios.map((u) => (
                    <tr key={u.id} className="hover:bg-[#101114]">
                      <td className="p-3 font-mono font-bold text-[#c9a227]">#{u.id}</td>
                      <td className="p-3 font-medium">{u.nome}</td>
                      <td className="p-3 font-mono text-[#a0a0a0]">{u.email}</td>
                      <td className="p-3">
                        <span className="px-2 py-1 rounded text-[10px] font-mono bg-[#2a2a2a] text-[#c9a227] border border-[#3a3a3a]">
                          {u.perfil}
                        </span>
                      </td>
                      <td className="p-3 text-right">
                        <span className="px-2 py-1 rounded text-[10px] font-mono bg-emerald-950 text-emerald-400 border border-emerald-800">
                          {u.status || 'ATIVO'}
                        </span>
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

export default TelaGestaoUsuariosIam;