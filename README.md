# Genesis_Altivon_Sys

ERP multiempresarial GenesisGest.Net (Nexum Altivon) v1.1.5 — estrutura de pastas e arquivos conforme "Nova Estrutura Genesis_Altivon_Sys".

## Estado desta entrega

Esta é a **estrutura**. Os arquivos de código existem nos caminhos definidos, declaram apenas tipos vazios e **não implementam funcionalidade**: nenhum endpoint, serviço, tela ou integração está pronto. As anotações entre parênteses da estrutura original (por exemplo, "(Master Data)") não viraram parte dos nomes das pastas.

## Pontos de partida técnicos

| Item | Valor |
|---|---|
| .NET | `net10.0`, projeto `GenesisAltivonSys.csproj` (biblioteca, `TreatWarningsAsErrors`) |
| Java | 21, `pom.xml` compila `BackEnd`, `Infraestrutura` e `Compartilhado` com `-Xlint:all` e `failOnWarning` |
| Front-End | React, react-router-dom, Tailwind CSS 3.4 (`tailwind.config.js` em ESM, pois o `package.json` usa `"type": "module"`) |

Não há `Program.cs`, host ASP.NET, `index.html` nem projeto de testes na estrutura enviada; eles precisam ser definidos antes de executar a aplicação.

## Comandos de validação local

```
dotnet build GenesisAltivonSys.sln
mvn -q compile
npm install
```

## Checklist (check_list_genesis_altivon_sys.md)

| Item | Requisito | Status |
|---|---|---|
| CH-01 | Solution única (.sln) + pom.xml | Parcial: .sln, .csproj e pom.xml criados. Build não executado (sem dotnet/Maven no ambiente). |
| CH-02 | Front-End React + Tailwind, roteamento, interceptadores | Parcial: package.json, tailwind.config.js e pastas criados. Sem roteamento nem interceptador implementados. |
| CH-03 | Banco canônico MySQL com tenant_id | Pendente: pasta Infraestrutura/BancoDeDados criada, sem esquema. |
| CH-04 | Docker Compose (MySQL, Redis, MinIO, Serilog/OpenTelemetry) | Pendente: docker-compose não consta na estrutura enviada. |
| CH-05 | Sessão híbrida JWT/Refresh/MFA | Pendente. |
| CH-06 | Isolamento multi-tenant | Pendente. |
| CH-07 | RBAC Back-End e Front-End | Pendente. |
| CH-08 | Entidade auditável (Sys_AuditableEntity) | Pendente. |
| CH-09 | Formulários Master Data com releitura da API | Pendente. |
| CH-10 | Catálogo omnicanal e busca unificada | Pendente. |
| CH-11 | Fluxo transacional PDV/Checkout | Pendente. |
| CH-12 | Campanhas, cupons e leads com consentimento LGPD | Pendente. |
| CH-13 | Compras em três vias | Pendente. |
| CH-14 | WMS e Kardex | Pendente. |
| CH-15 | FICO corporativo | Pendente. |
| CH-16 | Fechamento contábil e DRE | Pendente. |
| CH-17 | Mensageria fiscal (NF-e/NFC-e/CT-e) | Pendente. |
| CH-18 | Marketplaces e Dropshipping | Pendente. |
| CH-19 | Assistentes de IA Yara e Sophia | Pendente. |
| CH-20 | Cobertura de testes ≥ 70% | Pendente: nenhum projeto de teste consta na estrutura enviada. |
| CH-21 | Sem 'Sucesso' baseado em estado local | Pendente. |
| CH-22 | Observabilidade e erros rastreáveis | Pendente. |
| CH-23 | Cutover, restore-test e rollback | Pendente. |
