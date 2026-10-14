# Testes Automatizados

## Visão geral

O projeto utiliza testes automatizados para validar as regras de domínio, os serviços de aplicação, a segurança e o comportamento HTTP da API. A suíte é dividida entre testes unitários e testes de integração.

No levantamento atual, existem 123 testes unitários, 31 testes de integração e quatro testes Terraform.

> **Resultado verificado em 01/09/2026:** o build concluiu sem avisos; os 123 testes unitários, 31 testes de integração e quatro testes Terraform passaram.

## Organização

Os testes estão distribuídos em dois projetos:

| Projeto | Responsabilidade |
| --- | --- |
| `TechChallenge.Tests` | Testes unitários do domínio, validadores, serviços de aplicação e componentes de segurança |
| `TechChallenge.IntegrationTests` | Testes da API, autenticação, autorização, persistência e fluxos completos entre endpoints |

As principais ferramentas utilizadas são:

- **xUnit:** estrutura e execução dos testes;
- **Moq:** criação de dependências simuladas nos testes unitários;
- **FluentAssertions:** escrita das verificações de resultado;
- **WebApplicationFactory:** inicialização da API em memória nos testes de integração;
- **SQLite em memória:** banco isolado utilizado durante os testes de integração;
- **Coverlet:** coleta de cobertura de código.

## Testes unitários

Os testes unitários executam regras isoladas, substituindo repositórios e outras dependências por objetos simulados quando necessário.

### Domínio e Ordem de Serviço

Os principais cenários cobertos são:

- transições válidas e inválidas da máquina de estados;
- registro da data de finalização;
- atribuição da OS e limite de uma OS ativa por mecânico;
- registro e validação do diagnóstico;
- quantidades, cálculo, estoque e envio do orçamento;
- aprovação e reprovação do orçamento;
- retorno de uma OS reprovada para diagnóstico;
- finalização, entrega, cancelamento e exclusão;
- listagem geral, filtro por estado e listagem para a oficina;
- geração do código de acompanhamento e cálculo do tempo médio de execução;
- notificações internas na criação, nas transições e na falta de estoque.

### Cadastros e validações

Os testes verificam:

- criação de cliente com CPF ou CNPJ e rejeição de documento duplicado ou inválido;
- criação de veículo e vínculo com cliente existente;
- validação e normalização de placas antigas e Mercosul;
- rejeição de placa duplicada, ano e valores inválidos;
- criação de usuário, login duplicado e vínculo com funcionário.

### Autenticação e segurança

São validados:

- login com credenciais válidas e inválidas;
- bloqueio de usuário inativo;
- criação e rotação de refresh tokens;
- rejeição de refresh token inexistente ou revogado;
- geração e verificação de hash PBKDF2;
- geração das claims do access token;
- geração e hash de refresh tokens.

## Testes de integração

Os testes de integração inicializam a aplicação por meio de `WebApplicationFactory<Program>` e exercitam os endpoints com requisições HTTP reais dentro do processo de teste.

Durante a execução:

- o ambiente da aplicação é alterado para `Testing`;
- PostgreSQL é substituído por SQLite em memória;
- o esquema é recriado para manter os testes isolados;
- a autenticação JWT é substituída por um esquema de teste;
- o perfil padrão é `Administrador`, podendo ser alterado por cabeçalho nos testes de autorização.

Os fluxos atualmente cobertos incluem:

- CRUD e validação de clientes;
- CRUD e validação de funcionários;
- CRUD e validação de serviços;
- CRUD e validação de produtos do inventário;
- CRUD e validação de veículos;
- criação e listagem de usuários;
- login e rotação de refresh token;
- retorno `Unauthorized` para senha incorreta;
- execução do ciclo principal da OS até a entrega;
- acompanhamento público da OS e métrica de tempo médio;
- bloqueio de operações quando o perfil não possui autorização;
- respostas padronizadas com `ProblemDetails`.
- entrada, consulta e baixa de Estoque;
- rejeição de saldo negativo e retorno 404;
- concorrência otimista do saldo;
- abertura da OS com serviços e produtos;
- rota exclusiva de status;
- ordenação da fila operacional por prioridade e antiguidade.
- aprovação externa autenticada, idempotência, conflito e persistência da outbox;
- reserva concorrente e confirmação de mensagens pendentes da outbox.

> Os testes de integração validam a aplicação com Entity Framework e SQLite em memória. Eles não substituem testes específicos de compatibilidade e operação com uma instância real do PostgreSQL.

As categorias são exercitadas pelos fluxos de produto, serviço, veículo e OS. O Estoque possui testes unitários e de integração específicos.

### Saúde e infraestrutura local

Três testes HTTP verificam acesso anônimo a `/health/live` e `/health/ready` e que
uma dependência indisponível retorna 503 no readiness sem afetar o liveness. Esses
testes utilizam o esquema JWT real sem fornecer token, em vez da autenticação fictícia.

```bash
terraform -chdir=infra/environments/local init
terraform -chdir=infra/environments/local fmt -check -recursive
terraform -chdir=infra/environments/local validate
terraform -chdir=infra/environments/local test
kubectl kustomize k8s/base
kubectl kustomize k8s/overlays/docker-local
kubectl kustomize k8s/tests
```

Esses comandos validam Terraform, quatro cenários com providers mockados e a
renderização Kustomize sem acessar o cluster. A execução real da imagem,
migrations no startup, API, volume e métricas segue o [guia local](../infra/README.md).

## Como executar

Para executar toda a suíte a partir da raiz do repositório:

```bash
dotnet test TechChallenge.slnx
```

Para executar somente os testes unitários:

```bash
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj
```

Para executar somente os testes de integração:

```bash
dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj
```

Para executar sem restaurar novamente as dependências:

```bash
dotnet test TechChallenge.slnx --no-restore
```

## Cobertura de código

O Coverlet está configurado nos dois projetos de teste. Para gerar a cobertura no formato padrão do coletor:

```bash
dotnet test TechChallenge.slnx \
  --collect:"XPlat Code Coverage"
```

Também é possível gerar relatórios no formato OpenCover para análise no SonarQube:

```bash
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../../.sonar/coverage/unit/

dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../../.sonar/coverage/integration/
```

A meta do projeto é manter pelo menos 80% de cobertura nos fluxos críticos. A
medição atual é registrada no
[Relatório de Análise de Vulnerabilidades](relatorio-vulnerabilidades.md).

## Integração contínua

O repositório possui workflows separados para testes unitários e de integração:

- `.github/workflows/unit-tests.yml`;
- `.github/workflows/integration-tests.yml`.

Ambos executam em `push` e `pull_request` para a branch `main`, além do acionamento manual. O workflow de imagem executa restore, build e as duas suítes antes de publicar `latest` e uma tag imutável com os 12 primeiros caracteres do SHA no GHCR. O SHA completo permanece no rótulo OCI da imagem. Depois da publicação, o workflow promove essa tag em `k8s/overlays/docker-local/kustomization.yaml`; o commit automático não inicia outra execução. No ambiente acadêmico, Terraform e Kustomize concluem o deploy no cluster local do Docker Desktop.

## Regressões corrigidas na auditoria

| Área | Resultado |
| --- | --- |
| Entidade `Estoque` e EF Core | Construtor, migration, índice único e concorrência corrigidos |
| Serviços de OS e estoque | Persistência dos itens e mocks corrigidos |
| Refresh token | Datas, revogação do token anterior e rotação corrigidas |
| Atualizações de entidades | Alteração feita por métodos de domínio, sem substituir entidades rastreadas |
| Consultas de usuário | Normalização alterada para expressão traduzível pelo EF Core |
| Novos endpoints | Estoque, categorias, status, abertura com itens e prioridade cobertos |

Comandos usados na verificação:

```bash
dotnet restore TechChallenge.slnx
dotnet build TechChallenge.slnx --no-restore
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj --no-build --no-restore
dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj --no-build --no-restore
```

## Situação atual

A estrutura de testes está verde. A próxima ampliação deve cobrir:

- estados alternativos da OS pela API, incluindo reprovação, retorno para diagnóstico e cancelamento;
- autorização dos demais endpoints e perfis;
- execução com PostgreSQL real;
- fluxos futuros de reserva antecipada de estoque, pagamento e recibo.
