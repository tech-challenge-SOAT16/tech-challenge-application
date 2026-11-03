# Estado da entrega — Fase 3

Resumo do que foi entregue por repositório e das decisões tomadas durante a
fase. Atualizado em 03/11/2026.

## Repositórios

| Repositório | Entrega |
| --- | --- |
| [tech-challenge-lambda](https://github.com/tech-challenge-SOAT16/tech-challenge-lambda) | Function .NET 10 que valida o CPF, consulta o cliente, confirma código por e-mail e emite JWT RS256 com JWKS; Terraform da Lambda e do API Gateway; CI com 76 testes |
| [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s) | Terraform da VPC, EKS 1.35, ECR e security groups; Argo CD app-of-apps com Kong, observabilidade e Mailpit; pipeline com fmt, validate, tflint, test, plan e apply |
| [tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database) | Terraform do RDS PostgreSQL 17, Secrets Manager, bootstrap por Jobs no cluster e Secret `techchallenge-api-secrets`; justificativa do banco, diagrama ER e ADRs |
| tech-challenge-application (este) | API .NET 10, 126 testes unitários e 46 de integração, imagem no ECR, overlays Kustomize de homologação e produção, instrumentação OpenTelemetry, dashboards e alertas |

Em 24/10/2026 as pipelines aplicaram a infraestrutura na conta do AWS Academy
(cluster e banco) e publicaram a API no EKS por GitOps, com homologação e
produção sincronizadas pelo Argo CD. O ambiente do Academy é temporário:
cluster, NLB e API Gateway são recriados a cada sessão, então os endereços não
são fixos. O roteiro em [demo/fase-3](demo/fase-3/README.md) descreve como
recriar o ambiente e obter os endereços ativos.

## Decisões que mudaram em relação ao planejamento inicial

| Planejado | Adotado | Motivo |
| --- | --- | --- |
| GitHub Actions com IAM Role via OIDC | Chaves temporárias da sessão do AWS Academy em secrets | O Learner Lab não permite criar IAM roles nem OIDC provider; tudo usa a `LabRole` |
| AWS API Gateway + VPC Link até o EKS | API Gateway HTTP só para a Lambda; Kong (NLB) na frente da API | Kong já era o gateway do grupo; evita VPC Link e IAM |
| `kubectl apply` pela pipeline | GitOps com Argo CD lendo os overlays deste repositório | Sem credencial do cluster na pipeline; rollout visível no Argo CD |
| Commit da tag direto na `main` pelo bot | Pipeline abre branch `deploy/<overlay>-<run>` e a tag entra por PR revisado | Mantém a `main` sem exceções de proteção; o PR do deploy fica no histórico |
| Branch por ambiente (`develop`/`main`) | Um overlay por ambiente na `main` (`aws-eks-homolog` e `aws-eks`) | Evita divergência entre branches; promover é copiar a tag ([ADR 0005](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s/blob/main/docs/adr/0005-gitops-tags-no-repo-da-aplicacao.md)) |
| Amazon SES | Mailpit no cluster, um por namespace | SES exige identidade verificada e credenciais IAM |
| Secret sincronizado do Secrets Manager por operador | Secret criado direto pelo Terraform do `tech-challenge-database` | Menos peças; valores gerados pelo Terraform, fora do Git |

## Entregue neste repositório

- Manifests Kustomize em `k8s/base` e nos overlays `aws-eks-homolog` e `aws-eks`: probes, HPA de 2 a 10 réplicas, PDB, distribuição entre zonas, container não-root e filesystem somente leitura.
- Migrations e seed na inicialização protegidos por advisory lock do PostgreSQL, para que só uma réplica migre por vez.
- API como resource server RS256 via OIDC/JWKS para clientes; emissão HS256 para funcionários.
- Área do cliente (`/api/v1/cliente`) filtrada pelo `sub` do token da Lambda; rotas da oficina exigem perfil de funcionário.
- Chave dos links de aprovação separada do JWT (`ApprovalLinks__SigningKey`).
- Monitoramento com OpenTelemetry: traces, métricas, logs JSON e `x-correlation-id` ([monitoramento.md](monitoramento.md)).
- Métricas de negócio: volume de OS, tempo por status, falhas, notificações e decisões externas; dashboards Grafana e alertas.
- CI: build, testes unitários e de integração em push e PR; validação dos manifests com kustomize e kubeconform.
- CD: `deploy.yml` publica a imagem imutável no ECR e propõe a tag no overlay; após o merge o Argo CD faz o rollout.

## Integração com a Lambda de autenticação

- Esquema `Customer` para o JWT da Lambda, audiência `tech-challenge-api` e política `Cliente` (`scope=customer`) ([autenticacao-fase-3.md](autenticacao-fase-3.md)).
- Validado com a Lambda apontando para o banco da aplicação: login por CPF, OS do próprio cliente e bloqueio das rotas da oficina. A demo local em [demo/fase-3](demo/fase-3/README.md) sobe a Lambda junto com a API.
- `CustomerAuth__Authority` e `CustomerAuth__Issuer` dos overlays recebem a URL do API Gateway da Lambda (output `issuer`), que muda a cada sessão do Academy; o valor é atualizado por PR junto com o deploy da Lambda.

## Organização e GitHub

- `main` protegida nos quatro repositórios: PR com aprovação, sem force push nem exclusão e checks obrigatórios (aplicação: testes unitários e de integração; k8s: validação Terraform e render GitOps; banco: validação Terraform).
- Descrição e exclusão automática da branch após o merge nos quatro repositórios.
- Usuário `soat-architecture` com permissão de escrita nos quatro repositórios.
