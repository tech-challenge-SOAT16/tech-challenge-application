# Kubernetes — Aplicação principal (Fase 3)

Este repositório contém apenas a aplicação `TechChallenge.Api` executada em
Kubernetes no AWS EKS. Cluster, Kong, banco gerenciado e Function de
autenticação pertencem a outros repositórios. Aqui ficam os manifests Kustomize
e o CI/CD de publicação da imagem.

```text
k8s/
├── base/
│   └── api/
│       ├── serviceaccount.yaml
│       ├── configmap.yaml
│       ├── deployment.yaml
│       ├── service.yaml
│       ├── pdb.yaml
│       └── kustomization.yaml
└── overlays/
    ├── aws-eks-homolog/        # namespace techchallenge-homolog
    │   ├── configmap-patch.yaml
    │   ├── hpa.yaml
    │   └── kustomization.yaml
    └── aws-eks/                # namespace techchallenge (produção)
        ├── configmap-patch.yaml
        ├── hpa.yaml
        └── kustomization.yaml
```

Homologação e produção são overlays na branch `main`, lidos pelo Argo CD do
`tech-challenge-k8s` (ADR 0005).

## Entrada da aplicação

A API é exposta internamente pelo Service ClusterIP `techchallenge-api:8080`.
Não há Ingress ou LoadBalancer neste repositório: o Kong do
[tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s)
publica as rotas por um NLB.

## Autenticação

Dois esquemas, configurados no overlay:

- funcionários: login da própria API (`Jwt__EnableLocalIssuance=true`), com a
  chave `Jwt__LocalSigningKey` vinda do Secret;
- clientes: JWT RS256 da Function Serverless (`CustomerAuth__*`), validado pelo
  JWKS do emissor e aceito só em `/api/v1/cliente`.

`CustomerAuth__Authority`, `CustomerAuth__Issuer` e `ApprovalLinks__BaseUrl`
recebem os endereços do API Gateway da Lambda e do NLB do Kong, que mudam a
cada sessão do AWS Academy; são atualizados por PR junto com o deploy. Detalhes
em [Autenticação da Fase 3](../docs/autenticacao-fase-3.md).

## Secret criado pela infraestrutura

O Deployment referencia `techchallenge-api-secrets`, criado pelo
[tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database)
em cada namespace. Este repositório não cria o Secret. Chaves esperadas:

| Chave | Finalidade |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Conexão com PostgreSQL/RDS |
| `ApprovalLinks__SigningKey` | HMAC exclusivo dos links de aprovação/recusa |
| `IntegracaoExterna__ApiKey` | Autenticação do callback externo |
| `Seed__AdminPassword` | Senha inicial do administrador |
| `Jwt__LocalSigningKey` | Assinatura dos tokens de funcionários |
| `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password` | SMTP (Mailpit do namespace) |

Issuer, audience e a URL do emissor dos clientes são configurações não
sensíveis e ficam no ConfigMap.

## GitHub Actions e GitOps

O workflow `.github/workflows/deploy.yml` usa (secrets e variables do
repositório):

| Nome | Tipo GitHub | Valor |
| --- | --- | --- |
| `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN` | Secret | sessão atual do AWS Academy (renovada a cada 4 horas) |
| `AWS_DEPLOY_ENABLED` | Variable | `true` (sem ela só rodam build e testes) |
| `AWS_REGION` | Variable | `us-east-1` |

No AWS Academy não é possível criar role OIDC; as chaves da sessão servem
apenas para publicar no ECR. A pipeline não tem credencial do cluster.

Fluxo em push para `main`:

1. restore, build, testes unitários e de integração;
2. build e push no ECR com a tag imutável do SHA (`linux/amd64`);
3. branch `deploy/<overlay>-<run>` com a nova tag no `kustomization.yaml` do
   overlay (`aws-eks` para `main`, `aws-eks-homolog` para `develop`);
4. Pull Request revisado leva a tag para a `main`;
5. o Argo CD do
   [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s)
   detecta o commit e faz o rollout.

O workflow `manifest-validation.yml` renderiza `k8s/base` e os dois overlays
com kustomize e valida os manifests com kubeconform em todo PR que toca `k8s/`.

## Validar localmente

```bash
kubectl kustomize k8s/base
kubectl kustomize k8s/overlays/aws-eks-homolog
kubectl kustomize k8s/overlays/aws-eks

# Com acesso ao cluster
aws eks update-kubeconfig --name techchallenge-eks --region us-east-1
kubectl -n argocd get applications
kubectl -n techchallenge get deploy,pod,svc,hpa,pdb
```

## Escalabilidade e disponibilidade

O HPA mantém de 2 a 10 réplicas, com alvo de 70% de CPU e 80% de memória. O
PodDisruptionBudget preserva ao menos um pod em interrupções voluntárias e as
réplicas são distribuídas entre zonas e nós (`topologySpreadConstraints`). O
Metrics Server deve existir no cluster. A API usa startup, readiness e liveness
probes; migrations e seed são executados antes de começar a servir HTTP, sob
advisory lock do PostgreSQL para que só uma réplica migre por vez.

Estado da entrega e decisões: [`docs/estado-fase-3.md`](../docs/estado-fase-3.md).
