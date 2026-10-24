# Tech Challenge Fase 3 — Aplicação Principal (Kubernetes)

Repositório **4 de 4** da Fase 3 da Pós-Tech FIAP. Contém a API REST da oficina
mecânica, executada em AWS EKS atrás de um API Gateway, com autenticação
emitida por uma Function Serverless e persistência em PostgreSQL gerenciado.

Os quatro repositórios da Fase 3:

| # | Repositório | Responsabilidade |
| --- | --- | --- |
| 1 | Lambda (Function Serverless) | Validação de CPF e emissão de JWT RS256 |
| 2 | Infraestrutura Kubernetes (Terraform) | Cluster EKS, Metrics Server, RBAC, ECR, OIDC |
| 3 | Infraestrutura do Banco Gerenciado (Terraform) | PostgreSQL (RDS/Aurora), VPC, Security Groups |
| 4 | **Aplicação principal executando em Kubernetes** (este) | API .NET, imagem Docker, manifests, CI/CD |

## Propósito
# Tech Challenge - Oficina Mecânica - Fase 3

API desenvolvida para a **Pós-Tech FIAP**. O sistema administra clientes,
veículos, serviços, produtos, estoque e todo o ciclo de vida das Ordens de
Serviço (OS).

Na Fase 3 a solução passa a operar na nuvem: autenticação de clientes por CPF
em uma Function serverless, API Gateway, banco de dados gerenciado, cluster
Kubernetes com escalabilidade, infraestrutura como código, entrega por GitOps e
observabilidade com OpenTelemetry. O projeto foi dividido em quatro
repositórios, cada um com sua pipeline:

| Repositório | Responsabilidade |
| --- | --- |
| [tech-challenge-lambda](https://github.com/tech-challenge-SOAT16/tech-challenge-lambda) | Function serverless (AWS Lambda + API Gateway) que valida o CPF, consulta o cliente e emite o JWT |
| [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s) | Terraform da VPC, EKS e ECR; Argo CD, Kong e stack de observabilidade por GitOps |
| [tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database) | Terraform do RDS PostgreSQL, Secrets Manager, bootstrap do banco e Secret da API |
| **tech-challenge-application** (este) | API .NET, testes, imagem Docker, manifests Kustomize e instrumentação |

O histórico das fases anteriores está no repositório legado
[fiap-tech-challenge](https://github.com/louistx/fiap-tech-challenge).

## Entregáveis da Fase 3 neste repositório

| Área | Estado |
| --- | --- |
| Monitoramento (traces, métricas, logs JSON, correlação, dashboards e alertas) | Concluído e validado localmente |
| Métricas de negócio da OS (volume, tempo por status, falhas, integrações) | Concluído |
| Manifests Kubernetes para EKS (probes, HPA, PDB, segurança) | Concluído; overlay de homologação pendente |
| CI: build, testes unitários e de integração | Concluído |
| CD por GitOps (pipeline grava a tag; Argo CD aplica) | Pendente: ajuste do `deploy.yml` |
| Autenticação de clientes com o JWT da Lambda | Concluído e validado com a Lambda local: área do cliente em `/api/v1/cliente` |

Pendências detalhadas e responsáveis: [docs/ToDo.md](docs/ToDo.md).

## Funcionalidades principais

- cadastro de clientes, veículos, funcionários, serviços e produtos;
- categorias de produto, serviço e veículo;
- autenticação de funcionários (JWT, refresh token e autorização por perfil);
- área do cliente autenticado por CPF (JWT da Lambda) com as próprias OS;
- abertura da OS com cliente, veículo, serviços e produtos;
- diagnóstico, orçamento, aprovação, execução, finalização e entrega;
- aprovação ou recusa externa com API key, correlação e idempotência;
- notificação de mudanças de status por e-mail com outbox, retentativa, resumo
  do orçamento e botões assinados para aprovação ou recusa;
- consulta exclusiva de status e acompanhamento público da OS;
- fila operacional priorizada por estado e data;
- entrada, consulta e baixa de estoque sem permitir saldo negativo;
- concorrência otimista nas alterações de estoque;
- cálculo do tempo médio de execução das Ordens de Serviço.

Fluxo principal da OS:

Este repositório entrega:

- a imagem Docker da API publicada no Amazon ECR;
- os manifests Kustomize (`k8s/base` + `k8s/overlays/aws-eks`) que descrevem o
  Deployment, Service ClusterIP, ServiceAccount, PDB, HPA e ConfigMap;
- a pipeline GitHub Actions que, a cada push em `homologacao` ou `main`,
  valida, publica a imagem no ECR e aplica os manifests no cluster EKS
  correspondente;
- a aplicação configurada como **OAuth 2.0 Resource Server**, validando apenas
  tokens RS256 emitidos pela Function Serverless (OIDC discovery/JWKS).

A terminação TLS, o roteamento público e o rate limit ficam no API Gateway. A
aplicação só é alcançada por integração privada (VPC Link) na porta 8080.

## Arquitetura específica deste repositório

```mermaid
flowchart LR
    Dev[Desenvolvedor] -->|push main / homologacao| GH[GitHub]
    GH --> CI[GitHub Actions<br/>deploy.yml]
    CI -->|OIDC AssumeRole| AWS[(AWS IAM)]
    CI -->|build e push| ECR[(Amazon ECR<br/>techchallenge-application:SHA)]
    CI -->|kubectl apply -k| EKS[(Amazon EKS<br/>namespace techchallenge)]

    subgraph EKS[AWS EKS - namespace techchallenge]
        direction TB
        Deploy[Deployment<br/>techchallenge-api<br/>HPA 2-10 réplicas]
        SA[ServiceAccount]
        SVC[Service ClusterIP :8080]
        PDB[PodDisruptionBudget<br/>minAvailable 1]
        CM[ConfigMap<br/>techchallenge-api-config]
        SEC[Secret<br/>techchallenge-api-secrets]
        Deploy --> SVC
        Deploy --> SA
        Deploy --> CM
        Deploy --> SEC
        PDB --> Deploy
    end

    APIGW[API Gateway] -->|VPC Link| SVC
    Deploy --> RDS[(PostgreSQL<br/>RDS gerenciado)]
    Deploy -.OIDC JWKS.-> LAMBDA[Function Serverless<br/>emissor JWT RS256]
```

Fluxo de rollout:

```mermaid
sequenceDiagram
    participant Dev as Desenvolvedor
    participant GH as GitHub Actions
    participant ECR as Amazon ECR
    participant EKS as EKS techchallenge-api

    Dev->>GH: push em main ou homologacao
    GH->>GH: restore, build, unit + integration tests
    GH->>GH: kustomize build k8s/base e overlay
    GH->>GH: AssumeRole via OIDC
    GH->>ECR: docker build e push :SHA
    GH->>EKS: kustomize edit set image + kubectl apply -k
    EKS->>EKS: pod novo com startupProbe (migrations + seed)
    EKS->>EKS: readinessProbe libera tráfego
    GH->>EKS: rollout status --timeout 300s
```

## Tecnologias

| Camada | Tecnologia |
| --- | --- |
| API | .NET 10, ASP.NET Core Minimal APIs, FluentValidation, Swashbuckle/OpenAPI |
| Persistência | Entity Framework Core 10, Npgsql, PostgreSQL |
| Autenticação | JWT Bearer RS256 (resource server), OIDC discovery/JWKS |
| Build | Docker multi-stage, usuário não-root (UID 1654) |
| Orquestração | Kubernetes 1.30, Kustomize 5.4 |
| Nuvem | AWS EKS, ECR, IAM OIDC |
| CI/CD | GitHub Actions, aws-actions/configure-aws-credentials, OIDC |
| Qualidade | xUnit (unit + integration), kubeconform |
### Visão de nuvem

```mermaid
flowchart LR
    Cliente[Cliente / sistema externo]
    subgraph AWS
        APIGW[API Gateway HTTP]
        Lambda[Lambda de autenticação por CPF]
        subgraph VPC
            NLB[NLB]
            subgraph EKS[EKS]
                Kong[Kong Gateway]
                API[techchallenge-api<br/>homolog e produção]
                Argo[Argo CD]
                OTel[OpenTelemetry Collector]
                Obs[Prometheus, Loki, Tempo e Grafana]
                Mailpit[Mailpit]
            end
            RDS[(RDS PostgreSQL)]
        end
        ECR[ECR]
        SM[Secrets Manager]
    end
    GH[GitHub Actions]

    Cliente -->|POST /auth/login, /auth/verify| APIGW --> Lambda
    Lambda --> RDS
    Lambda --> SM
    Cliente -->|Bearer JWT da Lambda| NLB --> Kong --> API
    Funcionario[Funcionário] -->|login da API| NLB
    API --> RDS
    API -->|SMTP| Mailpit
    API -->|OTLP| OTel --> Obs
    GH -->|imagem| ECR
    GH -->|tag no overlay| Repo[(Git: overlays)]
    Argo -->|lê| Repo
    Argo -->|sincroniza| API
    ECR --> API
```

### Camadas da aplicação

O projeto utiliza um monólito organizado em camadas, com o domínio isolado dos
detalhes de API, autenticação e persistência.

```mermaid
flowchart LR
    Client[Cliente ou sistema consumidor] --> API[ASP.NET Core API]
    API --> Application[Casos de uso]
    Application --> Domain[Domínio]
    Application --> Contracts[Abstrações]
    Infrastructure[Persistência, autenticação, notificações e observabilidade] --> Contracts
    Infrastructure --> Database[(PostgreSQL)]
    Infrastructure --> Mailpit[SMTP]
    Infrastructure --> Collector[OpenTelemetry Collector]
```

| Projeto | Responsabilidade |
| --- | --- |
| `TechChallenge.Api` | Endpoints, contratos HTTP, Swagger, saúde e middleware |
| `TechChallenge.Application` | Casos de uso, comandos e validações |
| `TechChallenge.Application.Abstractions` | Interfaces, portas e métricas de negócio |
| `TechChallenge.Domain` | Entidades, estados e regras de negócio |
| `TechChallenge.Infrastructure.*` | PostgreSQL, EF Core, autenticação, SMTP/outbox, OpenTelemetry e injeção de dependências |
| `TechChallenge.Tests` | Testes unitários |
| `TechChallenge.IntegrationTests` | Testes de integração HTTP |

Tecnologias principais: **.NET 10**, ASP.NET Core Minimal APIs, Entity Framework
Core, PostgreSQL, JWT (RS256 para clientes, HS256 para funcionários), OpenTelemetry, FluentValidation,
xUnit, Docker, Kubernetes (EKS), Kustomize, Argo CD, Kong e GitHub Actions.

## APIs

Prefixo: `/api/v1`. Swagger UI disponível em `/swagger` quando a aplicação
está em execução. Documento OpenAPI JSON em `/openapi/v1.json`.

- Local: [http://localhost:5020/swagger](http://localhost:5020/swagger)
- Docker Compose: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- Kubernetes via `kubectl port-forward`: [http://localhost:18080/swagger](http://localhost:18080/swagger)
- Produção: `https://<api-gateway>/swagger` (URL definida pelo repositório do
  API Gateway)
- local: [http://localhost:5020/swagger](http://localhost:5020/swagger);
- Docker: [http://localhost:8080/swagger](http://localhost:8080/swagger);
- Kubernetes (EKS ou kind, via Kong): `http://localhost:8000/swagger`;
- OpenAPI JSON: `/openapi/v1.json`.

Rotas centrais:

| Método | Rota | Finalidade |
| --- | --- | --- |
| `POST` | `/api/v1/ordens-servico` | Abrir OS com cliente, veículo, serviços e produtos |
| `GET` | `/api/v1/ordens-servico/{id}/status` | Consultar o status da OS |
| `GET` | `/api/v1/ordens-servico/oficina` | Fila operacional priorizada |
| `GET` | `/api/v1/ordens-servico/acompanhamento/{codigo}` | Acompanhamento público |
| `POST` | `/api/v1/integracoes/orcamentos/respostas` | Webhook de aprovação/recusa externa |
| `POST` | `/api/v1/estoque` | Adicionar quantidade ao estoque |
| `GET` | `/api/v1/estoque/{produtoId}` | Consultar saldo por produto |
| `PUT` | `/api/v1/estoque` | Efetuar baixa no estoque |
| `GET` | `/health/live` | Liveness probe (Kubernetes) |
| `GET` | `/health/ready` | Readiness probe (Kubernetes) |

Autenticação: em Production a API valida apenas JWT RS256 do emissor configurado
em `Jwt__Authority`. O webhook externo usa o cabeçalho `X-Integration-Key` (API
key independente, não é JWT).
Os CRUDs de clientes, veículos, funcionários, serviços, produtos, categorias e
usuários estão documentados no Swagger. Por padrão, as rotas exigem autenticação;
login, refresh, OpenAPI, saúde e acompanhamento público são exceções explícitas.
O webhook externo não usa o JWT interno: ele exige o cabeçalho
`X-Integration-Key`. A coleção de requisições fica em
[docs/demo](docs/demo/fase-3/monitoramento.http) (arquivos `.http`, importáveis no Postman).

## Execução local

### Pré-requisitos

## Execução local (desenvolvimento)

Pré-requisitos: .NET SDK 10, PostgreSQL, Git, Docker Desktop (opcional).

```bash
git clone https://github.com/tech-challenge-SOAT16/tech-challenge-application.git
cd tech-challenge-application
```

Configure as variáveis mínimas (via user-secrets ou variáveis de ambiente):

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=TechChallenge;Username=postgres;Password=SUA_SENHA"
export Jwt__EnableLocalIssuance="true"
export Jwt__LocalSigningKey="chave-local-somente-dev-com-32-caracteres"
export ApprovalLinks__SigningKey="chave-independente-com-32-caracteres"
export Seed__AdminPassword="SenhaAdmin123"
```

`Jwt__EnableLocalIssuance=true` só é aceito fora de `Production`; a aplicação
emite HS256 nesse modo para facilitar testes locais. Em Production, a emissão
local é bloqueada e apenas RS256 externo é aceito.

Compile e execute:

```bash
dotnet restore TechChallenge.slnx
dotnet build TechChallenge.slnx --no-restore
dotnet run --project src/TechChallenge.Api/TechChallenge.Api.csproj
```

A API sobe em `http://localhost:5020`. Migrations e seed são aplicados no
startup, protegidos por `pg_advisory_lock` para suportar deploy com múltiplas
réplicas sem race.

### Docker Compose local

Útil para testar a imagem antes do push:
Suba a API, o PostgreSQL, o Mailpit e a stack de monitoramento:

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  up --build
```

Expõe API em `http://localhost:8080`, Mailpit em `http://localhost:8025`,
PostgreSQL em `localhost:5432`.
A API ficará em `http://localhost:8080`, o Mailpit em
`http://localhost:8025`, o Grafana em `http://localhost:3000` e o PostgreSQL em
`localhost:5432`. Para incluir a Lambda de autenticação por CPF e a área do
cliente, adicione `-f docker-compose/docker-compose.lambda.yml`; o passo a passo
completo está em [docs/demo/fase-3](docs/demo/fase-3/README.md). A coleção de
requisições pronta para apresentação está em
[docs/demo/fase-2](docs/demo/fase-2/README.md).

Para encerrar:

## Deploy em Kubernetes (AWS EKS)

### Fluxo automático (CI/CD)
O Dockerfile usa múltiplos estágios e usuário não-root. A pipeline publica a
imagem `linux/amd64` no Amazon ECR com a tag formada pelos 12 primeiros
caracteres do SHA. No Docker Desktop em Apple Silicon, essa imagem é executada
por emulação.

## Monitoramento

A API exporta traces, métricas e logs via OpenTelemetry. O compose inclui o
`grafana/otel-lgtm` (Collector, Prometheus, Loki, Tempo e Grafana) com os
dashboards e alertas já provisionados. Grafana em `http://localhost:3000`,
sem login. Detalhes em [docs/monitoramento.md](docs/monitoramento.md);
requisições para gerar dados em
[docs/demo/fase-3/monitoramento.http](docs/demo/fase-3/monitoramento.http).

`.github/workflows/deploy.yml`:

| Branch | GitHub Environment | Alvo |
| --- | --- | --- |
| `homologacao` | `homologacao` | Cluster/Role/ECR de homologação |
| `main` | `production` | Cluster/Role/ECR de produção |
```bash
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj
dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj
```

Cada ambiente carrega seus próprios secrets/variables:

| Nome | Tipo | Exemplo |
| --- | --- | --- |
| `AWS_ROLE_ARN` | Secret | `arn:aws:iam::123456789012:role/gh-actions-techchallenge-deploy` |
| `AWS_REGION` | Variable | `us-east-1` |
| `EKS_CLUSTER_NAME` | Variable | `techchallenge-eks` ou `techchallenge-eks-hml` |
- 126 testes unitários aprovados;
- 36 testes de integração aprovados;
- build Release sem erros ou avisos.

Etapas do workflow:

1. `validate`: restore, build Release, testes unitários e de integração,
   render de `kustomize build k8s/base` e `kustomize build k8s/overlays/aws-eks`.
2. `deploy` (só após `validate` verde): AssumeRole via OIDC, login no ECR,
   `docker build` + push com tag imutável `${SHA::12}` e também `latest`,
   `aws eks update-kubeconfig`, `kustomize edit set image` com a URI do ECR,
   `kubectl apply -k`, `kubectl rollout status --timeout=300s`, resumo.

`.github/workflows/manifest-validation.yml` roda em todo PR direcionado a
`main`/`homologacao` e valida base + overlay com `kubeconform`, sem exigir
credencial da AWS.

### Deploy manual

```bash
aws eks update-kubeconfig --name <cluster> --region <region>
kubectl kustomize k8s/overlays/aws-eks   # inspeção
kubectl apply -k k8s/overlays/aws-eks
kubectl -n techchallenge rollout status deployment/techchallenge-api --timeout=300s
kubectl -n techchallenge get deploy,pod,svc,hpa,pdb
```

### Pré-requisitos de infraestrutura

Este repositório **não** cria o cluster, a Role IAM, o ECR, o namespace nem o
Secret. Esses recursos são provisionados pelos outros três repositórios da
Fase 3. Antes de disparar o pipeline, é preciso ter:

- cluster EKS acessível pela Role do GitHub Actions (OIDC trust) com permissão
  `eks:DescribeCluster`;
- repositório ECR `techchallenge-application` com permissão de push para a
  Role do pipeline e pull para os nós/Fargate do cluster;
- namespace `techchallenge` criado;
- Metrics Server instalado (necessário para o HPA);
- Secret Kubernetes `techchallenge-api-secrets` no namespace, com as chaves:
  - `ConnectionStrings__DefaultConnection` (TLS para o RDS);
  - `ApprovalLinks__SigningKey` (≥ 32 caracteres, independente do JWT);
  - `IntegracaoExterna__ApiKey` (webhook externo);
  - `Seed__AdminPassword` (seed administrativo);
  - `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`.

Nenhuma chave privada RSA entra neste repositório. A API busca somente as
chaves públicas via OIDC/JWKS do emissor configurado em `Jwt__Authority`.

## Segurança
## Kubernetes

Este repositório mantém só os manifests da API: `k8s/base` (Deployment,
Service, ServiceAccount, ConfigMap e PDB) e o overlay `k8s/overlays/aws-eks`
(imagem do ECR, configuração de produção e HPA de 2 a 10 réplicas por CPU e
memória). Cluster, namespaces, Kong, observabilidade e o Argo CD ficam no
[tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s);
banco e o Secret `techchallenge-api-secrets` ficam no
[tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database).

O Argo CD lê os overlays da branch `main` deste repositório e aplica a API em
`techchallenge` (produção) e, quando o overlay `k8s/overlays/aws-eks-homolog`
existir, em `techchallenge-homolog` (homologação). Sem AWS, o
mesmo fluxo roda num cluster kind pelo `terraform/local` do repositório de
infraestrutura. Detalhes dos manifests em [k8s/README.md](k8s/README.md).

- Imagem Docker multi-stage, usuário não-root (`runAsUser: 1654`);
- `readOnlyRootFilesystem: true`, `/tmp` em `emptyDir`;
- `capabilities.drop: [ALL]`, `seccompProfile: RuntimeDefault`,
  `allowPrivilegeEscalation: false`;
- `automountServiceAccountToken: false`;
- validação JWT restrita a RS256 em Production; HS256 bloqueado;
- segredos consumidos por `envFrom.secretRef`, sem versionamento no Git;
- respostas de erro padronizadas pelo `ExceptionMiddleware`.

## Alta disponibilidade e escalabilidade

- HPA 2 a 10 réplicas, com alvo de 70% CPU e 80% memória;
- PodDisruptionBudget `minAvailable: 1` protege drains de nó;
- `topologySpreadConstraints` distribui réplicas por AZ e por nó
  (`ScheduleAnyway` para não bloquear clusters pequenos);
- RollingUpdate com `maxUnavailable: 0`, `maxSurge: 1`;
- `terminationGracePeriodSeconds: 60` para encerrar requisições em voo;
- startup, readiness e liveness probes em `/health/live` e `/health/ready`;
- advisory lock PostgreSQL (`pg_advisory_lock(727292024)`) serializa migrations
  e seed quando o HPA inicia com 2+ réplicas simultâneas.

## Estrutura do repositório

```text
src/
├── TechChallenge.Api/                       # Minimal APIs + Dockerfile
├── TechChallenge.Application/               # casos de uso
├── TechChallenge.Application.Abstractions/  # portas
├── TechChallenge.Domain/                    # entidades e regras
├── TechChallenge.Infrastructure.Auth/       # JWT RS256/HS256 e policies
├── TechChallenge.Infrastructure.Database/   # EF Core, migrations, seed
├── TechChallenge.Infrastructure.IoC/        # composição de dependências
└── TechChallenge.Infrastructure.Notifications/
tests/
├── TechChallenge.Tests/                     # xUnit (unitários)
└── TechChallenge.IntegrationTests/          # xUnit (integração HTTP)
k8s/
├── base/
│   └── api/{serviceaccount,configmap,deployment,service,pdb,kustomization}.yaml
└── overlays/
    └── aws-eks/{configmap-patch,hpa,kustomization}.yaml
.github/workflows/
├── deploy.yml               # Deploy main/homologacao -> EKS
├── manifest-validation.yml  # kustomize build + kubeconform em PR
├── build.yml
├── unit-tests.yml
├── integration-tests.yml
└── discord-notifications.yml
```
| Workflow | Responsabilidade |
| --- | --- |
| `build.yml` | Restore e build |
| `unit-tests.yml` | Testes unitários em push e PR para `main` |
| `integration-tests.yml` | Testes de integração em push e PR para `main` |
| `deploy.yml` | Build, testes, publicação da imagem no ECR e deploy no EKS |
| `discord-notifications.yml` | Aviso de PR e push no Discord (ignorado sem o webhook) |

Fluxo alvo da Fase 3 (GitOps): o `deploy.yml` publica a imagem no ECR e grava a
nova tag no overlay do ambiente, com commit direto na `main` feito pelo
`github-actions[bot]`. O Argo CD detecta a mudança e faz o rollout. Hoje o
`deploy.yml` ainda aplica os manifests com `kubectl`; a troca está registrada em
[docs/ToDo.md](docs/ToDo.md).

Detalhes dos manifests e do contrato do Secret: [k8s/README.md](k8s/README.md).

## Testes

```bash
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj
dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj
```

As duas suítes também são executadas pelo workflow `deploy.yml` antes de
qualquer publicação de imagem.
- clientes: JWT RS256 da Function Serverless, validado por OIDC discovery/JWKS
  e aceito só na área do cliente (`/api/v1/cliente`), que filtra pelo próprio
  cliente;
- funcionários: login da API com JWT HS256 (chave no Secret); todas as rotas
  da oficina exigem perfil, inclusive as de leitura;
- perfis `Administrador`, `Vendedor` e `Mecanico`; autenticação obrigatória por
  padrão;
- webhook externo protegido por `X-Integration-Key` e idempotente;
- hash de senha com PBKDF2;
- container não-root, filesystem somente leitura e capabilities removidas;
- segredos gerados pelo Terraform, fora do Git;
- `x-correlation-id` em todas as respostas e erros padronizados sem detalhes
  internos;
- relatório de vulnerabilidades em [docs/relatorio-vulnerabilidades.md](docs/relatorio-vulnerabilidades.md).

## Documentação

| Documento | Conteúdo |
| --- | --- |
| [Pendências da Fase 3](docs/ToDo.md) | O que falta, por repositório |
| [Monitoramento](docs/monitoramento.md) | Stack, dashboards, métricas, correlação e alertas |
| [Demo da Fase 3](docs/demo/fase-3/README.md) | Credenciais, subida do ambiente e roteiro do vídeo |
| [Autenticação da Fase 3](docs/autenticacao-fase-3.md) | Clientes (JWT da Lambda) e funcionários, com diagrama de sequência |
| [DDD](docs/ddd.md) e [Event Storming](docs/event-storming.md) | Domínio, eventos e fluxos do negócio |
| [Auditoria do Estoque](docs/auditoria-estoque.md) | Contratos, regras, persistência e testes |
| [Índice completo](docs/README.md) | Inclui os documentos da Fase 2 |

## Equipe

| Participante | RM | GitHub |
| --- | --- | --- |
| Gabriel Teixeira | RM374752 | [@louistx](https://github.com/louistx) |
| Brunno de Oliveira | RM374818 | [@DevDoubleN](https://github.com/DevDoubleN) |
| Luís Henrique | RM374786 | [@Ace0777](https://github.com/Ace0777) |
| Caio Montilha | RM375494 | [@cmontilha](https://github.com/cmontilha) |
| Gustavo Keiji | RM374965 | [@GuKeiji](https://github.com/GuKeiji) |

Projeto acadêmico desenvolvido para o **Tech Challenge da Pós-Tech FIAP — Fase 3**.
Organização: [github.com/tech-challenge-SOAT16](https://github.com/tech-challenge-SOAT16)

Projeto acadêmico desenvolvido para o **Tech Challenge da Pós-Tech FIAP**.
