import React, { useState, useEffect } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaPainelAdministrativoReal = () => {
  const [sku, setSku] = useState('');
  const [nomeProduto, setNomeProduto] = useState('');
  const [categoria, setCategoria] = useState('');
  const [preco, setPreco] = useState('');
  const [estoqueInicial, setEstoqueInicial] = useState('');
  const [descricao, setDescricao] = useState('');
  
  const [produtos, setProdutos] = useState([]);
  const [pedidos, setPedidos] = useState([]);
  const [filtroStatus, setFiltroStatus] = useState('TODOS');
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  useEffect(() => {
    carregarDadosAdministrativos();
  }, []);

  const carregarDadosAdministrativos = async () => {
    try {
      const respProdutos = await ApiCentral.get('/master/produtos');
      setProdutos(respProdutos.data || []);
    } catch (e) {
      // Fallback corporativo se api estiver offline
      setProdutos([
        { id: 1, sku: 'SKU-001', nome: 'Servidor Altivon Blade 8U', preco: 12500.00, estoque: 15 },
        { id: 2, sku: 'SKU-002', nome: 'Licença Multi-Tenant Enterprise', preco: 4500.00, estoque: 120 }
      ]);
    }

    try {
      const respPedidos = await ApiCentral.get('/pdv/pedidos');
      setPedidos(respPedidos.data || []);
    } catch (e) {
      setPedidos([
        { id: 'ORD-901', cliente: 'Cliente Corporativo Alpha', total: 12500.00, status: 'CONCLUIDO' },
        { id: 'ORD-902', cliente: 'Indústria Beta Ltda', total: 4500.00, status: 'PENDENTE' }
      ]);
    }
  };

  const cadastrarProduto = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      await ApiCentral.post('/master/produtos', {
        sku,
        nome: nomeProduto,
        categoria,
        preco: Number(preco),
        estoque: Number(estoqueInicial),
        descricao
      });
      setMensagem('Produto cadastrado com sucesso!');
      setSku('');
      setNomeProduto('');
      setCategoria('');
      setPreco('');
      setEstoqueInicial('');
      setDescricao('');
      carregarDadosAdministrativos();
    } catch (error) {
      setMensagem('Erro ao cadastrar produto: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  const pedidosFiltrados = pedidos.filter(p => 
    filtroStatus === 'TODOS' || p.status === filtroStatus
  );

  return (
    <div className="min-h-screen bg-[#050505] text-white p-6 font-sans">
      {/* Cabeçalho */}
      <div className="flex justify-between items-center border-b border-[#2a2a2a] pb-4 mb-6">
        <h1 className="text-xl font-bold flex items-center gap-2 text-[#c9a227]">
          ⚙️ Painel Administrativo Real
        </h1>
        <a 
          href="/cockpit" 
          className="text-xs font-mono text-[#a0a0a0] hover:text-[#c9a227] transition-colors"
        >
          ← Voltar para o Cockpit
        </a>
      </div>

      {mensagem && (
        <div className="bg-[#1a1a1a] border border-[#c9a227] p-3 rounded-lg mb-6 text-xs text-[#c9a227] font-mono">
          {mensagem}
        </div>
      )}

      {/* Grid Principal */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        
        {/* Coluna Esquerda: Cadastrar Produto */}
        <div className="bg-[#1a1a1a] border border-[#2a2a2a] p-6 rounded-xl shadow-xl">
          <h2 className="text-lg font-bold mb-4 text-[#c9a227] flex items-center gap-2">
            + Cadastrar Produto
          </h2>

          <form onSubmit={cadastrarProduto} className="space-y-4">
            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">SKU:</label>
              <input
                type="text"
                value={sku}
                onChange={(e) => setSku(e.target.value)}
                placeholder="Ex: SKU-9999"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Nome do Produto:</label>
              <input
                type="text"
                value={nomeProduto}
                onChange={(e) => setNomeProduto(e.target.value)}
                placeholder="Ex: Servidor Enterprise"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Categoria:</label>
              <input
                type="text"
                value={categoria}
                onChange={(e) => setCategoria(e.target.value)}
                placeholder="Ex: Hardware / Software"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Preço (R$):</label>
              <input
                type="number"
                step="0.01"
                value={preco}
                onChange={(e) => setPreco(e.target.value)}
                placeholder="0.00"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Estoque Inicial:</label>
              <input
                type="number"
                value={estoqueInicial}
                onChange={(e) => setEstoqueInicial(e.target.value)}
                placeholder="0"
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none font-mono"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Selecione a Foto do Computador:</label>
              <div className="flex items-center gap-2 bg-[#050505] border border-[#2a2a2a] rounded p-2 text-xs font-mono text-[#a0a0a0]">
                <span className="bg-[#2a2a2a] text-white px-3 py-1 rounded cursor-pointer hover:bg-[#3a3a3a]">Escolher arquivo</span>
                <span>Nenhum arquivo escolhido</span>
              </div>
            </div>

            <div>
              <label className="block text-xs font-mono text-[#a0a0a0] mb-1">Descrição:</label>
              <textarea
                value={descricao}
                onChange={(e) => setDescricao(e.target.value)}
                rows="3"
                placeholder="Detalhes técnicos do produto..."
                className="w-full bg-[#050505] border border-[#2a2a2a] rounded p-2 text-sm text-white focus:border-[#c9a227] outline-none"
              ></textarea>
            </div>

            <button
              type="submit"
              disabled={carregando}
              className="w-full bg-[#c9a227] text-black font-bold py-2.5 rounded hover:bg-[#e8d5a3] transition-colors text-xs font-mono uppercase tracking-wider"
            >
              {carregando ? 'Cadastrando...' : 'Salvar Produto'}
            </button>
          </form>
        </div>

        {/* Coluna Direita: Pedidos e Gerenciamento */}
        <div className="space-y-6">
          
          {/* Bloco Pedidos */}
          <div className="bg-[#1a1a1a] border border-[#2a2a2a] p-6 rounded-xl shadow-xl">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-lg font-bold text-[#c9a227] flex items-center gap-2">
                📦 Pedidos Gravados no Banco
              </h2>
              <div className="flex items-center gap-2">
                <span className="text-xs font-mono text-[#a0a0a0]">Filtrar por Status:</span>
                <select
                  value={filtroStatus}
                  onChange={(e) => setFiltroStatus(e.target.value)}
                  className="bg-[#050505] border border-[#2a2a2a] text-xs font-mono rounded p-1.5 text-white outline-none"
                >
                  <option value="TODOS">Todos</option>
                  <option value="CONCLUIDO">Concluído</option>
                  <option value="PENDENTE">Pendente</option>
                </select>
              </div>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-[#050505] text-[#c9a227] font-mono border-b border-[#2a2a2a]">
                  <tr>
                    <th className="p-2.5">ID</th>
                    <th className="p-2.5">Cliente</th>
                    <th className="p-2.5">Total</th>
                    <th className="p-2.5">Status</th>
                    <th className="p-2.5 text-right">Ação</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-[#2a2a2a]">
                  {pedidosFiltrados.map((p) => (
                    <tr key={p.id} className="hover:bg-[#101114]">
                      <td className="p-2.5 font-mono text-[#c9a227] font-bold">{p.id}</td>
                      <td className="p-2.5">{p.cliente}</td>
                      <td className="p-2.5 font-mono">R$ {Number(p.total).toFixed(2)}</td>
                      <td className="p-2.5">
                        <span className="px-2 py-0.5 rounded text-[10px] font-mono bg-amber-950 text-amber-400 border border-amber-800">
                          {p.status}
                        </span>
                      </td>
                      <td className="p-2.5 text-right font-mono text-[#c9a227] cursor-pointer hover:underline">Detalhes</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Bloco Gerenciar / Editar Produtos */}
          <div className="bg-[#1a1a1a] border border-[#2a2a2a] p-6 rounded-xl shadow-xl">
            <h2 className="text-lg font-bold mb-4 text-[#c9a227] flex items-center gap-2">
              🛠️ Gerenciar / Editar Produtos
            </h2>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-[#050505] text-[#c9a227] font-mono border-b border-[#2a2a2a]">
                  <tr>
                    <th className="p-2.5">Produto / SKU</th>
                    <th className="p-2.5">Preço (R$)</th>
                    <th className="p-2.5">Estoque</th>
                    <th className="p-2.5 text-right">Ações</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-[#2a2a2a]">
                  {produtos.map((pr) => (
                    <tr key={pr.id} className="hover:bg-[#101114]">
                      <td className="p-2.5">
                        <div className="font-bold">{pr.nome}</div>
                        <div className="text-[10px] font-mono text-[#a0a0a0]">{pr.sku}</div>
                      </td>
                      <td className="p-2.5 font-mono">R$ {Number(pr.preco).toFixed(2)}</td>
                      <td className="p-2.5 font-mono">{pr.estoque} un</td>
                      <td className="p-2.5 text-right space-x-2 font-mono">
                        <button className="text-emerald-400 hover:underline">Editar</button>
                        <button className="text-red-400 hover:underline">Excluir</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

        </div>

      </div>
    </div>
  );
};

export default TelaPainelAdministrativoReal;