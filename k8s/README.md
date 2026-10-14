# Kubernetes — Aplicação principal (Fase 3)

Este repositório contém apenas a aplicação `TechChallenge.Api` executada em
Kubernetes no AWS EKS. Cluster, banco gerenciado, Function de autenticação e API
Gateway pertencem a outros repositórios Terraform. Aqui ficam os manifests
Kustomize e o CI/CD de publicação no ECR e deploy no EKS.

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
Não há Ingress ou LoadBalancer neste repositório. O API Gateway e a integração
privada por VPC Link/target interno serão criados pelo repositório de
infraestrutura. TLS termina no API Gateway.

## Autenticação

Em produção a aplicação apenas valida JWT **RS256**. O overlay configura
`Jwt__Authority`, `Jwt__Issuer`, `Jwt__Audience`, HTTPS metadata e desabilita a
emissão local. A API busca o documento OIDC e as chaves públicas JWKS do emissor.
Nenhuma chave privada ou segredo de assinatura JWT entra no pod.

Os valores versionados ainda são placeholders `replace-me` e devem ser
substituídos quando o API Gateway/Function fornecerem as URLs. O contrato
completo está em [Autenticação da Fase 3](../docs/autenticacao-fase-3.md).

## Secret criado pela infraestrutura

O Deployment referencia `techchallenge-api-secrets` no namespace
`techchallenge`. Este repositório não cria o Secret. Chaves esperadas:

| Chave | Finalidade |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Conexão com PostgreSQL/RDS |
| `ApprovalLinks__SigningKey` | HMAC exclusivo dos links de aprovação/recusa |
| `IntegracaoExterna__ApiKey` | Autenticação do callback externo |
| `Seed__AdminPassword` | Senha inicial do administrador |
| `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password` | SMTP gerenciado |

OIDC discovery, JWKS, issuer, audience e chaves públicas são configurações não
sensíveis e ficam no ConfigMap, não no Secret.

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

Todas as configurações e integrações ainda pendentes estão em
[`docs/ToDo.md`](../docs/ToDo.md).
