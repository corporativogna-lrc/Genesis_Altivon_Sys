# Genesis_Altivon_Sys (Nexum Altivon) v1.1.5

Plataforma ERP Corporativa Multi-Tenant de alta performance projetada para operação integrada de ponta a ponta (Varejo Omnicanal, Logística WMS/TMS, Supply Chain SCM, Controladoria FICO, Emissão Fiscal SEFAZ e Governança GRC/IAM).

---

## 1. Arquitetura e Stack Tecnológica

O ecossistema adota uma arquitetura modular desacoplada, garantindo isolamento estrito por `tenant_id`, rastreabilidade auditável e processamento transacional resiliente:

* **Back-End Core & Minimal APIs:** C# (.NET 8) — Endpoints RESTful, regras de negócio transacionais, emissão fiscal e políticas RBAC.
* **Serviços de Processamento & Integração:** Java 21 (Spring / Maven) — Motores de apuração, filas assíncronas e conectores externos.
* **Front-End Corporativo:** React.js + Tailwind CSS — Interface executiva (identidade visual Nexum Altivon: `#050505` / `#1a1a1a` com acentos em `#c9a227`), roteamento protegido e interceptadores JWT.
* **Banco de Dados Relacional Canônico:** MariaDB / MySQL — Esquema unificado multi-tenant (`GenesisAltivonDb`) com controle de concorrência (`row_version`) e auditoria imutável.
* **Infraestrutura & Observabilidade:** Docker Compose, Redis (Cache/Filas), MinIO (Storage S3), Serilog e OpenTelemetry.

---

## 2. Topologia do Repositório

```text
Genesis_Altivon_Sys/
├── BackEnd/
│   ├── AssistentesInteligenciaArtificial/   # Agentes Yara (Comercial) e Sophia (Administrativo)
│   ├── CadastrosCorporativos/               # Master Data (Clientes, Fornecedores, Produtos, Filiais)
│   ├── ComercialCrmPortal/                  # Funil CRM, E-Commerce Vitrine e Campanhas LGPD
│   ├── EstoqueLogistica/                    # WMS (Saldos/Picking), Kardex e TMS (Fretes/Entregas)
│   ├── FinanceiroContabil/                  # FICO (Contas a Pagar/Receber, Tesouraria e DRE)
│   ├── FiscalTributario/                    # Emissão NF-e / NFC-e / CT-e e Apuração SPED
│   ├── FluxoTrabalho/                       # Motor de Workflow e Alçadas de Aprovação
│   ├── GestaoCompras/                       # SCM (Cotações RFQ, Pedidos e Importação XML)
│   ├── GovernancaRiscosConformidade/        # IAM (JWT, MFA TOTP, RBAC) e Trilha de Auditoria
│   ├── IntegracoesExternas/                 # Marketplaces (ML, Amazon, Shopee) e Ads APIs
│   ├── JuridicoProjetos/                    # Gestão de Contratos e Portfólio WBS/ESG
│   ├── ManufaturaOperacoes/                 # MES (Ordens de Produção, BOM e Manutenção OEE)
│   ├── OperacaoPDV/                         # Frente de Caixa Omnicanal, Caixas e Devoluções
│   └── RecursosHumanos/                     # HCM (Departamento Pessoal, Folha e eSocial)
├── Compartilhado/                           # Contratos SLA, Enumeradores e Respostas Padronizadas
├── FrontEnd/                                # Aplicação SPA React.js + Tailwind CSS
├── Infraestrutura/                          # Contexto EF Core, Segurança JWT/DPAPI, MinIO e Telemetria
├── GenesisAltivonSys.sln                    # Solution .NET
├── pom.xml                                  # Build Maven (Java 21)
└── docker-compose.yml                       # Orquestração de serviços locais



# .NET / C#
bin/
obj/
*.user
*.suo
*.cache
.vs/

# Java / Maven
target/
.mvn/
*.jar
*.war

# Node / React FrontEnd
node_modules/
FrontEnd/node_modules/
FrontEnd/meu-app-react/node_modules/
build/
FrontEnd/build/
dist/
.env
.env.local
npm-debug.log*

# Seguranca, Chaves Criptograficas e Dados Locais (CRITICO)
StorageDados/
*.db
*.sqlite
*.sqlite3
configuracoes.local.json

# Arquivos compactados e residuos acidentais
*.zip
cd
docker
where

# Sistema Operacional / IDEs
.DS_Store
Thumbs.db
.idea/
.vscode/



3. Módulos Funcionais ImplementadosDomínioMódulo / TelaCapacidades PrincipaisExecutivoTelaPainelCockpitTelemetria de API, status do banco MariaDB, latência e indicadores consolidados.Segurança (IAM)TelaMfaDesafio / TelaGestaoUsuariosIamAutenticação em duas etapas (TOTP), gestão de operadores e perfis RBAC multi-tenant.Varejo (PDV)TelaPdvOmnicanal / TelaHistoricoVendasPdvOperação de balcão, baixa atômica de estoque e auditoria de cupons por caixa.Logística (WMS)TelaGestaoEstoqueWms / TelaWmsSeparacaoPickingControle de saldos físicos, separação de pedidos (picking) e expedição.Compras (SCM)TelaGestaoComprasRegistro de pedidos de compra e importação automatizada de XML de entrada.Financeiro (FICO)TelaContasPagarReceber / TelaPainelFinanceiroAvancadoTítulos a pagar/receber, conciliação e fluxo de caixa gerencial.Fiscal (SEFAZ)TelaEmissaoFiscal / TelaPainelNotasFiscaisEmissão, transmissão e monitoramento de NF-e (Modelo 55) e NFC-e (Modelo 65).GovernançaTelaAuditoriaSistema / TelaConfiguracoesTenantTrilha de auditoria transacional, parâmetros SEFAZ por empresa e webhooks.InfraestruturaTelaMonitoramentoInfraestruturaHealth checks em tempo real da API .NET 8, memória, threads e uptime.4. Execução em Ambiente LocalPré-requisitos.NET 8 SDKNode.js 18+ e NPMMariaDB 10.4+ ou MySQL 8+ (Porta padrão configurada: 3309)


---

### Comandos de Execução e Validação (Publicar o `README.md` no GitHub)
```bat
cd /d D:\Genesis_Altivon_Sys
notepad README.md




Checklist Auxiliar de Conferência
Item	Status	Validação Prática
1. Push inicial realizado no GitHub	Concluído	Commit 4fd04cb publicado com sucesso em corporativogna-lrc/Genesis_Altivon_Sys.
2. Blindagem de dados sensíveis (StorageDados/)	Concluído	Chaves DPAPI e compactados removidos do histórico e bloqueados via .gitignore.
3. Documentação arquitetural (README.md)	Concluído	Estruturada para exibição na página inicial do repositório no GitHub.





Checklist Auxiliar de Conferência
Item	Status	Validação Prática
1. Abrir o README.md no Bloco de Notas	Pendente	Executar notepad README.md dentro de D:\Genesis_Altivon_Sys.
2. Substituir o conteúdo e salvar (Ctrl + S)	Pendente	Garantir que apenas o conteúdo Markdown fique salvo no arquivo.
3. Enviar alteração para o GitHub (git push)	Pendente	Confirmar a atualização visualizando a página [https://github.com/corporativogna-lrc/Genesis_Altivon_Sys](https://github.com/corporativogna-lrc/Genesis_Altivon_Sys).