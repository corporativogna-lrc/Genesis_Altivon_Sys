import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { InterceptorAutenticacao } from './componentes/InterceptorAutenticacao';

// Importação das Telas do ERP
import TelaPainelCockpit from './telas/TelaPainelCockpit';
import TelaPdvOmnicanal from './telas/TelaPdvOmnicanal';
import TelaHistoricoVendasPdv from './telas/TelaHistoricoVendasPdv';
import TelaGestaoEstoqueWms from './telas/TelaGestaoEstoqueWms';
import TelaWmsSeparacaoPicking from './telas/TelaWmsSeparacaoPicking';
import TelaLogisticaEntregas from './telas/TelaLogisticaEntregas';
import TelaApontamentoMes from './telas/TelaApontamentoMes';
import TelaGestaoCompras from './telas/TelaGestaoCompras';
import TelaContasPagarReceber from './telas/TelaContasPagarReceber';
import TelaEmissaoFiscal from './telas/TelaEmissaoFiscal';
import TelaPainelNotasFiscais from './telas/TelaPainelNotasFiscais';
import TelaPainelFinanceiroAvancado from './telas/TelaPainelFinanceiroAvancado';
import TelaAuditoriaSistema from './telas/TelaAuditoriaSistema';
import TelaConfiguracoesTenant from './telas/TelaConfiguracoesTenant';
import TelaGestaoUsuariosIam from './telas/TelaGestaoUsuariosIam';
import TelaMonitoramentoInfraestrutura from './telas/TelaMonitoramentoInfraestrutura';
import TelaGestaoWebhooks from './telas/TelaGestaoWebhooks';
import TelaPainelAdministrativoReal from './telas/TelaPainelAdministrativoReal';
import TelaLogin from './telas/TelaLogin';

function App() {
  return (
    <Router>
      <InterceptorAutenticacao>
        <Routes>
          <Route path="/" element={<Navigate to="/cockpit" replace />} />
          <Route path="/cockpit" element={<TelaPainelCockpit />} />
          <Route path="/pdv" element={<TelaPdvOmnicanal />} />
          <Route path="/pdv/historico" element={<TelaHistoricoVendasPdv />} />
          <Route path="/wms" element={<TelaGestaoEstoqueWms />} />
          <Route path="/wms/separacao" element={<TelaWmsSeparacaoPicking />} />
          <Route path="/logistica/entregas" element={<TelaLogisticaEntregas />} />
          <Route path="/mes/apontamento" element={<TelaApontamentoMes />} />
          <Route path="/compras" element={<TelaGestaoCompras />} />
          <Route path="/financeiro" element={<TelaContasPagarReceber />} />
          <Route path="/financeiro-avancado" element={<TelaPainelFinanceiroAvancado />} />
          <Route path="/fiscal" element={<TelaEmissaoFiscal />} />
          <Route path="/fiscal/notas" element={<TelaPainelNotasFiscais />} />
          <Route path="/auditoria" element={<TelaAuditoriaSistema />} />
          <Route path="/configuracoes" element={<TelaConfiguracoesTenant />} />
          <Route path="/iam/usuarios" element={<TelaGestaoUsuariosIam />} />
          <Route path="/infraestrutura" element={<TelaMonitoramentoInfraestrutura />} />
          <Route path="/webhooks" element={<TelaGestaoWebhooks />} />
          <Route path="/admin-real" element={<TelaPainelAdministrativoReal />} />
          <Route path="*" element={<Navigate to="/cockpit" replace />} />
		  <Route path="/login" element={<TelaLogin />} />
        </Routes>
      </InterceptorAutenticacao>
    </Router>
  );
}

export default App;