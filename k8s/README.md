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
    └── aws-eks/
        ├── configmap-patch.yaml
        ├── hpa.yaml
        └── kustomization.yaml
```

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
ainda são placeholders `replace-me`, a substituir pelos outputs da Lambda e do
Kong. Detalhes em [Autenticação da Fase 3](../docs/autenticacao-fase-3.md).

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

## GitHub Actions e OIDC AWS

O workflow `.github/workflows/deploy.yml` espera:

| Nome | Tipo GitHub | Exemplo |
| --- | --- | --- |
| `AWS_ROLE_ARN` | Secret | `arn:aws:iam::123456789012:role/gh-actions-techchallenge-deploy` |
| `AWS_REGION` | Variable | `us-east-1` |
| `EKS_CLUSTER_NAME` | Variable | `techchallenge-eks` |

O repositório de infraestrutura cria a IAM Role, trust policy OIDC, permissões,
EKS Access Entry/RBAC, namespace, ECR e Metrics Server.

Fluxo automático em push para `main`:

1. restore, build e testes;
2. assume IAM Role via OIDC;
3. build e push no ECR com tag imutável baseada no SHA;
4. atualiza a imagem no overlay em memória;
5. `kubectl apply -k k8s/overlays/aws-eks`;
6. aguarda o rollout do Deployment.

> **Fluxo alvo (GitOps).** O Argo CD do
> [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s)
> lê os overlays da `main` deste repositório. Quando o `deploy.yml` for ajustado,
> os passos 4 a 6 viram um commit da nova tag no overlay, feito pelo
> `github-actions[bot]` direto na `main`, e o Argo CD aplica. No AWS Academy
> não é possível criar a role OIDC; a pipeline usará as chaves temporárias da
> sessão (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN`)
> apenas para publicar no ECR.

## Validar e aplicar manualmente

```bash
kubectl kustomize k8s/base
kubectl kustomize k8s/overlays/aws-eks

aws eks update-kubeconfig --name <cluster> --region <region>
kubectl apply -k k8s/overlays/aws-eks
kubectl -n techchallenge rollout status deployment/techchallenge-api --timeout=300s
kubectl -n techchallenge get deploy,pod,svc,hpa,pdb
```

## Escalabilidade e disponibilidade

O HPA mantém de 2 a 10 réplicas, com alvo de 70% de CPU e 80% de memória. O
PodDisruptionBudget preserva ao menos um pod em interrupções voluntárias. O
Metrics Server deve existir no cluster. A API usa startup, readiness e liveness
probes; migrations e seed são executados antes de começar a servir HTTP.
