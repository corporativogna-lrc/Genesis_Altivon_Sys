import React, { useState } from 'react';
import ApiCentral from '../servicos/ClienteApiCentral';

export const TelaMfaDesafio = ({ usuarioId, aoAutenticarComSucesso }) => {
  const [codigoOtp, setCodigoOtp] = useState('');
  const [mensagem, setMensagem] = useState('');
  const [carregando, setCarregando] = useState(false);

  const validarMfa = async (e) => {
    e.preventDefault();
    setCarregando(true);
    setMensagem('');

    try {
      const response = await ApiCentral.post('/iam/mfa/validar', {
        usuarioId,
        codigo: codigoOtp
      });

      if (response.data && response.data.token) {
        localStorage.setItem('genesis_token_jwt', response.data.token);
        if (response.data.tenantId) {
          localStorage.setItem('genesis_tenant_id', response.data.tenantId);
        }
        setMensagem('MFA validado com sucesso! Entrando...');
        setTimeout(() => {
          if (aoAutenticarComSucesso) aoAutenticarComSucesso();
          else window.location.href = '/cockpit';
        }, 1000);
      }
    } catch (error) {
      setMensagem('Erro no MFA: ' + (error.response?.data?.erro || error.message));
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#050505] flex items-center justify-center p-4">
      <div className="bg-[#1a1a1a] border border-[#2a2a2a] p-8 rounded-xl max-w-md w-full text-white shadow-2xl">
        <div className="text-center mb-6">
          <div className="w-3 h-3 rounded-full bg-[#c9a227] inline-block mb-2"></div>
          <h1 className="text-xl font-bold tracking-wider">VERIFICAÇÃO EM DUAS ETAPAS</h1>
          <p className="text-xs text-[#a0a0a0] mt-1">Informe o código gerado pelo seu aplicativo autenticador (TOTP)</p>
        </div>

        {mensagem && (
          <div className="bg-[#050505] border border-[#c9a227] p-3 rounded-lg mb-4 text-xs text-[#c9a227] font-mono text-center">
            {mensagem}
          </div>
        )}

        <form onSubmit={validarMfa} className="space-y-4">
          <div>
            <label className="block text-xs text-[#a0a0a0] mb-1 font-mono">CÓDIGO DE 6 DÍGITOS:</label>
            <input
              type="text"
              maxLength="6"
              value={codigoOtp}
              onChange={(e) => setCodigoOtp(e.target.value)}
              placeholder="000000"
              className="w-full bg-[#050505] border border-[#2a2a2a] rounded-lg p-3 text-center text-xl text-[#c9a227] tracking-widest font-mono focus:border-[#c9a227] outline-none"
              required
            />
          </div>

          <button
            type="submit"
            disabled={carregando}
            className="w-full bg-[#c9a227] text-black font-bold py-3 rounded-lg hover:bg-[#e8d5a3] transition-colors disabled:opacity-50 text-sm tracking-wider"
          >
            {carregando ? 'VALIDANDO...' : 'CONFIRMAR AUTENTICAÇÃO'}
          </button>
        </form>

        <div className="mt-6 text-center">
          <span className="text-[10px] text-[#a0a0a0] font-mono">GENESIS ALTIVON SYS — SEGURANÇA IAM</span>
        </div>
      </div>
    </div>
  );
};

export default TelaMfaDesafio;