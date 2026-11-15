# Check-List Clínico Point-to-Point (Genesis_Altivon_Sys)

## I. Governança de Arquitetura e Setup Base
- [ ] **CH-01:** Configurar Solution Única (`.sln`) e `pom.xml` consolidando os projetos C# (API, ERP, Core) e Java (Microsserviços de alto processamento), garantindo build sem erros ou avisos (0 errors / 0 warnings).
- [ ] **CH-02:** Configurar repositório Front-End (React.js + Tailwind CSS) com estrutura modular de roteamento (`react-router`), gerenciamento de estado e interceptadores HTTP seguros.
- [ ] **CH-03:** Estabelecer o esquema de Banco de Dados Canônico (MySQL), unificando as antigas instâncias separadas (nexum e genesis) em um modelo com chaveamento `tenant_id` em 100% das entidades transacionais.
- [ ] **CH-04:** Configurar Docker Compose com MySQL 8, Redis 7 (Cache/Fila), MinIO (Storage S3) e observabilidade (Serilog + OpenTelemetry).

## II. Segurança, Identidade e Isolamento (IAM / GRC)
- [ ] **CH-05:** Implementar sistema de Sessão Híbrida: Login seguro (JWT em memória no front), Refresh Token rotativo (SHA-256 no banco) e MFA (TOTP com segredo cifrado via DPAPI/Data Protection).
- [ ] **CH-06:** Aplicar Isolamento Multi-Tenant estrito em consultas e inserções (bloqueio `FOR UPDATE` e filtro global de `tenant_id` no DbContext / Repositories).
- [ ] **CH-07:** Implementar Matriz RBAC (Role-Based Access Control) granular no Back-End (Policies) e Front-End (Ocultação de componentes).
- [ ] **CH-08:** Ativar Entidade Auditável (`Sys_AuditableEntity`): Soft-delete (`is_deleted`), campos de auditoria (`created_by`, `updated_by`) e concorrência (`row_version` BLOB/Timestamp) para toda escrita.

## III. Master Data, Cadastros e Operação Omnicanal (CRM / PDV)
- [ ] **CH-09:** Desenvolver formulários React para Master Data (Clientes, Fornecedores, Produtos) com releitura independente da API após inserção (sem confirmação visual falsa).
- [ ] **CH-10:** Homologar Catálogo Omnicanal e motor de busca unificado (Código de Barras, QR Code, SKU, Filtro de Publicação) para uso no PDV e E-commerce.
- [ ] **CH-11:** Implementar fluxo transacional do PDV / Checkout: Reserva atômica de número de pedido, baixa de estoque, processamento de pagamento e fila fiscal em uma única transação de banco.
- [ ] **CH-12:** Configurar campanhas, cupons promocionais e captura de leads do CRM com rastreabilidade de consentimento LGPD.

## IV. Supply Chain, Finanças e WMS (SCM / FICO)
- [ ] **CH-13:** Implementar fluxo completo de Compras (Três vias): Solicitação → Cotação (RFQ) → Pedido → Recebimento parcial/total, bloqueando entrada sem pedido aprovado.
- [ ] **CH-14:** Configurar WMS e Kardex: Rastreabilidade por Lote/Série, transferências intercompany e inventário com travas de concorrência.
- [ ] **CH-15:** Concluir FICO Corporativo: Contas a Pagar/Receber, geração de boletos/PDFs, baixa financeira, conciliação bancária por regras de semelhança e fluxo de caixa projetado.
- [ ] **CH-16:** Implementar fechamento contábil e apuração de DRE bloqueando alterações em períodos fechados.

## V. Fiscal, Integrações e IA (OpenAI / Sefaz)
- [ ] **CH-17:** Implementar mensageria Fiscal (NF-e/NFC-e/CT-e) consumindo API ou mensageiro Sefaz com persistência de Chave de Acesso, XML e Protocolo (registro de status de falha sem ocultar incidentes).
- [ ] **CH-18:** Ativar conexões de Marketplaces (Mercado Livre, Amazon, Shopee) e Dropshipping com mapeamento de produtos e Webhooks de eventos idempotentes.
- [ ] **CH-19:** Configurar Assistentes de IA separados: **Yara** (Comercial/Vitrine - leitura restrita, sem dados de pagamento) e **Sophia** (Administrativo - Policy Admin). Credenciais da OpenAI protegidas e cifradas no Back-End.

## VI. Validação, Qualidade e CI/CD (Definition of Done)
- [ ] **CH-20:** Cobertura de Testes: Alcançar no mínimo 70% de cobertura de código nos Services/UseCases (xUnit/JUnit) antes de liberar a pipeline de CI.
- [ ] **CH-21:** Validação Transacional: Nenhuma tela Front-End pode exibir mensagens de "Sucesso" baseadas em estado local. Requer status 200/201 da API e releitura do dado.
- [ ] **CH-22:** Observabilidade e Auto-Update: Garantir logs estruturados de toda exceção sem engolir erros externos (ex: recusa de Sefaz, falha no Gateway de Frete retorna 502 rastreável).
- [ ] **CH-23:** Executar rotina de Cutover: Teste de Stress/Carga, restauração de Backup semanal automatizado (Restore-Test) e simulação de Rollback antes da promoção para ambiente de Produção 24h.




https://github.com/corporativogna-lrc/Genesis_Altivon_Sys - Repositorio Guithub