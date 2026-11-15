import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import GerenciadorSessao from '../servicos/GerenciadorSessaoMfa';

export const InterceptorAutenticacao = ({ children }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const [verificado, setVerificado] = useState(false);

  useEffect(() => {
    const rotasPublicas = ['/login', '/recuperar-senha', '/portal'];
    const ehPublica = rotasPublicas.some(rota => location.pathname.startsWith(rota));

    const sessaoAtiva = GerenciadorSessao.verificarSessaoAtiva();

    if (!sessaoAtiva && !ehPublica) {
      navigate('/login', { replace: true });
    } else {
      setVerificado(true);
    }
  }, [location, navigate]);

  if (!verificado) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-[#050505] text-[#c9a227]">
        <div className="text-center">
          <div className="inline-block w-8 h-8 border-4 border-[#c9a227] border-t-transparent rounded-full animate-spin mb-4"></div>
          <p className="font-mono text-sm tracking-wider">CARREGANDO NEXPERP SYS...</p>
        </div>
      </div>
    );
  }

  return children;
};

export default InterceptorAutenticacao;