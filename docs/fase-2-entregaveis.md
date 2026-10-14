# Fase 2 - Checklist de Entregáveis

Este checklist acompanha a Fase 2 com atualização local em 01/09/2026. O escopo de infraestrutura acordado usa o cluster já fornecido pelo Docker Desktop: Terraform prepara dependências e banco, e Kustomize publica a aplicação. Criação de cluster e infraestrutura de nuvem não fazem parte deste ambiente.

## Legenda

- [x] Concluído e localizado ou validado no repositório.
- [ ] Pendente, parcial ou bloqueado.

Quando um item estiver parcialmente desenvolvido, ele permanece desmarcado e recebe a indicação **Parcial**. Assim, somente itens realmente prontos aparecem com check.

## Progresso atual

| Área | Estado verificado |
| --- | --- |
| Arquitetura em camadas e testes | Concluído para o escopo da fase |
| Clean Code | Concluído: fluxo assíncrono propagado dos endpoints aos repositórios |
| APIs obrigatórias da OS | Concluído, incluindo webhook e e-mail local |
| Estoque e categorias | Concluído |
| Docker e Docker Compose | Concluído e validado |
| Kubernetes local | Concluído e validado |
| Terraform local | Concluído sobre o cluster Docker Desktop existente |
| CI: build, testes e imagem | Concluído |
| CD para o cluster local | Concluído: Terraform e aplicação por Kustomize validados |
| Vídeo e documento final | Concluído e revisado |

As evidências de infraestrutura e entrega estão em [Validação da infraestrutura local](validacao-infra-local.md). O cluster do Docker Desktop é o ambiente de execução do trabalho; não existe um ambiente remoto a ser acessado pelo runner do GitHub Actions.

## 1. Evolução da aplicação

- [x] Aplicar Clean Code na solução para o escopo da fase.
  - [x] Organização dos casos de uso por feature.
  - [x] Nomes de comandos, validadores e serviços relacionados ao domínio.
  - [x] Substituir `GetAwaiter().GetResult()` por `async/await` de ponta a ponta.
  - [x] Remover helpers duplicados.
  - [x] Resolver avisos de nulabilidade e dependências não inicializadas no build atual.

- [x] Consolidar Clean Architecture ou Arquitetura Hexagonal para o escopo da fase.
  - [x] Projetos separados para API, aplicação, abstrações, domínio e infraestrutura.
  - [x] Repositórios definidos por interfaces na camada de abstrações.
  - [x] Autenticação e persistência implementadas em projetos de infraestrutura.
  - [x] Regras de negócio mantidas no domínio sem referência aos detalhes de persistência.

- [x] Concluir a refatoração das entidades e acessores.
  - [x] Entidades refatoradas com `private set`.
  - [x] Métodos de alteração adicionados às entidades.
  - [x] Corrigir o construtor de `Estoque`, incompatível com o EF Core.
  - [x] Proteger invariantes como saldo não negativo.
  - [x] Padronizar os tipos usados para quantidade.

- [x] Manter testes automatizados verdes para os fluxos críticos.
  - [x] Projeto de testes unitários existente.
  - [x] Projeto de testes de integração existente.
  - [x] 123 testes unitários aprovados em 01/09/2026.
  - [x] Corrigir os 8 testes unitários anteriormente falhos.
  - [x] Corrigir os testes de integração anteriormente falhos; a suíte agora possui 31 testes verdes, incluindo webhook, outbox e decisão pelo e-mail.

## 2. APIs obrigatórias

### Abertura da Ordem de Serviço

- [x] Receber cliente, veículo, serviços e peças na abertura da OS.
  - [x] `POST /api/v1/ordens-servico` existe.
  - [x] A API retorna o identificador único da OS.
  - [x] Cliente e veículo são recebidos por identificador.
  - [x] Incluir serviços no contrato de abertura.
  - [x] Incluir peças/produtos no contrato de abertura.
  - [x] Cliente e veículo são referenciados por seus identificadores existentes.

### Consulta do status

- [x] Disponibilizar uma rota exclusiva para consultar o status da OS.
  - [x] Handler de consulta de status foi criado.
  - [x] Remover o conflito entre os dois handlers `GET /api/v1/ordens-servico/{id}`.
  - [x] Adotar `GET /api/v1/ordens-servico/{id}/status`.
  - [x] Cobrir a consulta de status no fluxo principal de integração.

### Aprovação ou recusa externa

- [x] Criar endpoint para receber aprovação ou recusa de um sistema externo.
  - [x] Aprovação administrativa interna existe.
  - [x] Reprovação administrativa interna existe.
  - [x] Criar callback/webhook externo.
  - [x] Definir autenticação do integrador por API key.
  - [x] Implementar idempotência, correlação e auditoria da decisão.
  - [x] Testar repetição idêntica, chave inválida, conteúdo conflitante e geração da outbox.

### Listagem priorizada

- [x] Ordenar a listagem conforme a prioridade exigida.
  - [x] Finalizadas e entregues são removidas da listagem operacional.
  - [x] `DataCriacao` é usada como critério de desempate.
  - [x] Corrigir a ordenação booleana anteriormente invertida.
  - [x] Garantir a sequência `EmExecucao > AguardandoAprovacao > EmDiagnostico > Recebida`.
  - [x] Exibir as mais antigas primeiro dentro de cada prioridade.
  - [x] Remover reprovadas e canceladas da fila operacional.
  - [x] Adicionar teste de integração para a ordem final.

### Notificação de mudança de status

- [x] Atualizar o cliente por e-mail ou ferramenta equivalente.
  - [x] Notificações internas por log existem.
  - [x] Implementar envio SMTP e caixa de entrada local com Mailpit.
  - [x] Implementar retentativa com espera progressiva e tratamento de falha.
  - [x] Evitar perda de notificação com outbox persistida na mesma transação da OS.
  - [x] Incluir resumo, valor total e links assinados de decisão no e-mail de orçamento.

## 3. Estoque e categorias

- [x] Concluir a entidade e persistência de Estoque.
  - [x] Entidade `Estoque` separada de `Produto`.
  - [x] Interface `IEstoqueRepository` criada.
  - [x] Implementação `EstoqueRepository` criada.
  - [x] Configuração inicial do EF Core criada.
  - [x] Corrigir o binding entre o parâmetro do construtor e a propriedade `ProdutoId`.
  - [x] Criar constraint única para `ProdutoId`.
  - [x] Criar migration com preservação do saldo legado e atualizar o model snapshot.

- [x] Corrigir a entrada de estoque.
  - [x] Caso de uso e validador existem.
  - [x] Rota `POST /api/v1/estoque` existe.
  - [x] Copiar `request.Quantidade` para o comando.
  - [x] Aguardar a persistência assíncrona.
  - [x] Proteger a atualização com token de concorrência otimista.

- [x] Corrigir a consulta de estoque.
  - [x] Caso de uso de consulta existe.
  - [x] Alterar o verbo de `DELETE` para `GET`.
  - [x] Alinhar `produtoId` entre rota, handler e repositório.
  - [x] Retornar 404 quando o produto não possuir estoque.

- [x] Corrigir a baixa manual de estoque.
  - [x] Caso de uso e validador existem.
  - [x] Consultar por `ProdutoId`, não pelo ID do registro de estoque.
  - [x] Impedir saldo negativo na entidade.
  - [x] Retornar `200 OK`, em vez de `201 Created`.
  - [x] Tornar a atualização concorrente-segura com token de versão e resposta HTTP 409 em conflito.

- [x] Corrigir a integração entre Produto, Categoria e Estoque.
  - [x] Categorias de produto, serviço e veículo foram modeladas.
  - [x] Endpoints e casos de uso de categorias foram criados.
  - [x] Incluir `IdCategoria` no request e no mapeamento de criação de produto.
  - [x] Inicializar corretamente `_estoqueRepository` em `AtualizarItemInventarioService`.
  - [x] Criar migrations para todas as categorias.
  - [x] Adicionar testes para Estoque e exercitar Categorias nos fluxos de integração.

Consulte a análise detalhada em [Auditoria do Endpoint de Estoque](auditoria-estoque.md).

## 4. Docker

- [x] Entregar Dockerfile revisado e validado localmente.
  - [x] Dockerfile multi-stage existente.
  - [x] Imagem final configurada com usuário não-root.
  - [x] Copiar `TechChallenge.Infrastructure.Auth.csproj` antes do `dotnet restore` da imagem.
  - [x] Executar build completo da imagem no Docker Desktop.
  - [x] Executar smoke test da imagem no Kubernetes: saúde, Swagger e autenticação.

- [x] Manter Docker Compose para desenvolvimento local.
  - [x] API configurada.
  - [x] PostgreSQL configurado.
  - [x] Mailpit configurado para a demonstração de e-mail.
  - [x] SonarQube configurado.
  - [x] `docker compose config --quiet` validado em 01/09/2026.

## 5. Kubernetes

Manifests reorganizados em base da API e overlay local. Namespace, Secrets e banco pertencem ao Terraform; o guia está em [Infraestrutura local](../infra/README.md).

- [x] Entregar Deployment funcional.
  - [x] Deployment presente em `/k8s`.
  - [x] Base renderiza com `kubectl kustomize k8s/base`.
  - [x] Adicionar readiness probe com consulta ao schema do banco.
  - [x] Adicionar liveness probe independente do banco e startup probe.
  - [x] Definir requests e limits de CPU/memória.

- [x] Entregar Service funcional.
  - [x] Service presente.
  - [x] Alinhar o selector `fiap-tech-challenge-api` aos labels dos pods.
  - [x] Validar endpoints e acesso à API pelo Service.

- [x] Criar ConfigMap.
  - [x] Separar configurações não sensíveis da imagem.
  - [x] Referenciar o ConfigMap no Deployment.

- [x] Criar Secret pelo Terraform.
  - [x] Fornecer conexão do banco e segredo JWT sem versionar valores reais.
  - [x] Referenciar o Secret no Deployment, sem duplicar sua definição.

- [x] Criar HPA por CPU.
  - [x] Definir CPU com alvo de 70% do request e 1 a 3 réplicas.
  - [x] Instalar/validar Metrics Server 0.9.0 no Docker Desktop.
  - [x] Demonstrar escala de 1 para 3 réplicas e retorno a 1 após carga de 120 segundos.

- [x] Disponibilizar banco para o ambiente Kubernetes.
  - [x] Executar PostgreSQL 17 em StatefulSet de uma réplica.
  - [x] Validar PVC, persistência após recriar pod, backup e restauração em banco separado.
  - [x] Executar migrations e seed na inicialização da API, antes de servir HTTP.

- [x] Corrigir o overlay local.
  - [x] Referenciar o namespace criado pelo Terraform, sem duplicar um `namespace.yaml`.
  - [x] Definir imagem do GHCR e tag configurável no overlay, sem script de deploy.
  - [x] Validar manifests com Kustomize e registrar o deploy real anterior com imagem local; conferir limitações da revisão GHCR no relatório de validação.

## 6. Terraform

- [x] Criar o diretório `/infra`.
- [x] Definir providers Kubernetes, Helm e Random, com lockfile versionável.
- [x] Usar explicitamente o cluster existente `docker-desktop`, conforme o escopo local acordado.
- [x] Provisionar o PostgreSQL.
- [x] Usar a rede do cluster, criar Service interno e manter somente o output do namespace.
- [x] Definir state local por desenvolvedor, fora do Git; estado remoto fica para um futuro ambiente compartilhado.
- [x] Documentar `init`, `validate`, `test` e `apply`; o plano é revisado na confirmação do `apply`.
- [x] Validar apply real e plano posterior sem mudanças.

A arquitetura e as limitações estão em [Arquitetura Proposta - Fase 2](arquitetura-fase-2.md). Quatro testes Terraform mockados cobrem os contratos básicos, mas não substituem as validações reais no cluster.

## 7. CI/CD

- [x] Executar build da aplicação em GitHub Actions.
  - [x] Workflow `build.yml` presente.
  - [x] Execução configurada para push na `main` e acionamento manual.

- [x] Executar testes automaticamente como gate da publicação.
  - [x] Workflow de testes unitários presente.
  - [x] Workflow de testes de integração presente.
  - [x] Executar os testes automaticamente em pull request/push.
  - [x] Impedir a publicação da imagem quando qualquer teste falhar.
  - [x] Recuperar e ampliar a suíte: 123 unitários e 31 integrações aprovados.

- [x] Automatizar build e publicação da imagem.
  - [x] Workflow `docker-image.yml` presente.
  - [x] Publicação no GHCR configurada.
  - [x] Executar build e testes antes da publicação no mesmo workflow.
  - [x] Publicar tag imutável com 12 caracteres do SHA e promover somente imagem validada.
  - [x] Atualizar automaticamente o `newTag` do overlay sem disparar outra pipeline.

- [x] Executar o deploy no cluster Kubernetes local.
- [x] Provisionar o banco e as dependências locais com Terraform.
- [x] Executar Terraform com revisão do plano antes do `apply` local.
- [x] Aplicar os manifests com um único `kubectl apply -k` no ambiente local.
- [x] Validar rollout, health checks e smoke test após o deploy.

## 8. README e documentação

- [x] Atualizar a descrição da solução e os objetivos da Fase 2.
- [x] Documentar os componentes da aplicação.
- [x] Documentar a infraestrutura proposta.
- [x] Documentar o fluxo de deploy proposto.
- [x] Manter instruções de execução local.
- [x] Disponibilizar instruções executáveis de deploy Kubernetes local.
- [x] Disponibilizar instruções executáveis de Terraform local.
- [x] Disponibilizar a documentação executável das APIs.
  - [x] Swagger UI gerado em runtime.
  - [x] OpenAPI JSON gerado em runtime.
  - [x] Documentar as URLs locais para execução direta dos cenários.
  - [x] Versionar a [coleção HTTP da demo](demo/fase-2/README.md).

## 9. Vídeo e entrega no portal

- [x] Preparar o [Roteiro do Vídeo da Fase 2](roteiro-video-fase-2.md).
- [x] Gravar vídeo de até 15 minutos.
  - [x] Demonstrar deploy da aplicação.
  - [x] Demonstrar execução do CI/CD.
  - [x] Demonstrar consumo das APIs.
  - [x] Demonstrar escalabilidade automática com HPA.
  - [x] Publicar no YouTube ou Vimeo como público ou não listado.
  - [x] Inserir o link no README e no documento final.

- [x] Preparar a fonte do [Documento de Entrega da Fase 2](entrega-fase-2.md).
- [x] Gerar e revisar o PDF final do portal.
  - [x] Inserir link do repositório.
  - [x] Inserir o desenho da arquitetura realmente implantada.
  - [x] Inserir o link do vídeo.
  - [x] Validar visualmente todas as páginas e links.

- [x] Confirmar que o repositório privado foi compartilhado com `soat-architecture`.

## 10. Bloqueadores prioritários

- [x] Corrigir o modelo EF Core e criar as migrations de Estoque/Categorias.
- [x] Corrigir contratos, rotas, invariantes e testes do Estoque.
- [x] Recuperar os 8 testes unitários e os 20 testes de integração anteriormente falhos.
- [x] Corrigir a rota de status e a ordenação das OS.
- [x] Completar a abertura da OS com serviços e produtos.
- [x] Implementar a integração externa de decisão e notificação.
- [x] Completar e validar Kubernetes no ambiente local.
- [x] Implementar Terraform para o escopo local.
- [x] Concluir o fluxo de entrega da imagem do GHCR ao Kubernetes local.
- [x] Disponibilizar as APIs via Swagger/OpenAPI.
- [x] Gravar o vídeo, inserir o link e gerar o PDF final.
