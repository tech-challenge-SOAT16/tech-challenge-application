# ToDo — Fase 3

Estado da Fase 3 por repositório e o que ainda falta para a entrega. Atualizado
em 24/10/2026. O grupo ainda não tem acesso ao AWS Academy: tudo abaixo marcado
como concluído foi validado localmente (Docker Compose, kind e testes com mocks),
não na AWS.

## Repositórios

| Repositório | Estado |
| --- | --- |
| [tech-challenge-lambda](https://github.com/tech-challenge-SOAT16/tech-challenge-lambda) | Na `main`; deploy desligado até haver conta AWS |
| [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s) | [PR #1](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s/pull/1) aguardando revisão |
| [tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database) | Na `main`; plan e apply desligados até haver conta AWS |
| tech-challenge-application (este) | [PR #3](https://github.com/tech-challenge-SOAT16/tech-challenge-application/pull/3) aguardando revisão |

## Decisões que mudaram em relação ao planejamento inicial

| Planejado | Adotado | Motivo |
| --- | --- | --- |
| GitHub Actions com IAM Role via OIDC | Chaves temporárias da sessão do AWS Academy em secrets | O Learner Lab não permite criar IAM roles nem OIDC provider; tudo usa a `LabRole` |
| AWS API Gateway + VPC Link até o EKS | API Gateway HTTP só para a Lambda; Kong (NLB) na frente da API | Kong já era o gateway do grupo; evita VPC Link e IAM |
| `kubectl apply` pela pipeline | GitOps com Argo CD lendo os overlays deste repositório | Sem credencial do cluster na pipeline; rollout visível no Argo CD |
| Amazon SES | Mailpit no cluster, um por namespace | SES exige identidade verificada e credenciais IAM |
| Secret sincronizado do Secrets Manager por operador | Secret criado direto pelo Terraform do `tech-challenge-database` | Menos peças; valores gerados pelo Terraform, fora do Git |

## Concluído

### Este repositório

- [x] Manifests Kustomize em `k8s/base` e `k8s/overlays/aws-eks`: probes, HPA de 2 a 10 réplicas, PDB, container não-root e filesystem somente leitura.
- [x] API como resource server RS256 via OIDC/JWKS em produção; emissão HS256 só em desenvolvimento e testes.
- [x] Chave dos links de aprovação separada do JWT (`ApprovalLinks__SigningKey`).
- [x] Monitoramento com OpenTelemetry: traces, métricas, logs JSON e `x-correlation-id` ([monitoramento.md](monitoramento.md)).
- [x] Métricas de negócio: volume de OS, tempo por status, falhas, notificações e decisões externas.
- [x] Dashboards Grafana (ASP.NET Core 19924, ASP.NET Core Endpoint 19925 e TechChallenge - Ordens de Serviço) e alertas.
- [x] Corpo de requisição inválido devolve 400; finalizar OS valida o status antes de baixar o estoque.
- [x] CI: build, 126 testes unitários e 36 de integração.

### tech-challenge-k8s

- [x] VPC em duas AZs, EKS 1.35 com node group t3.medium (2 a 3), add-ons e Metrics Server.
- [x] ECR `techchallenge-application` com scan e lifecycle policy.
- [x] Namespaces `techchallenge` e `techchallenge-homolog`; security groups do banco e da Lambda.
- [x] Argo CD com app-of-apps: Kong, kube-prometheus-stack, Loki, Tempo, OpenTelemetry Collector, dashboards, alertas, Mailpit e as Applications da API.
- [x] Validação local no kind com todas as Applications `Synced`/`Healthy`.

### tech-challenge-database

- [x] RDS PostgreSQL 17 em subnets privadas, TLS obrigatório, backups e senha master no Secrets Manager.
- [x] Databases `techchallenge` e `techchallenge_homolog` com usuário por ambiente.
- [x] Tabelas da autenticação, view `auth_clients` e usuário `lambda_auth`.
- [x] Secret `techchallenge-api-secrets` nos dois namespaces.
- [x] Justificativa do banco, diagrama ER e ADRs.

## Pendente

### Este repositório

- [ ] Ajustar o `deploy.yml` para GitOps: publicar no ECR com as chaves do Academy e commitar a tag no overlay do ambiente direto na `main` (`github-actions[bot]`), removendo kubeconfig, `kubectl` e espera de rollout. Isso também elimina o pin `kubectl v1.30.4`, incompatível com o EKS 1.35.
- [ ] Liberar a exceção do ruleset da `main` para o commit automático da tag. O GitHub não aceita o bot do Actions como exceção em ruleset de repositório desta organização; a alternativa é uma deploy key de escrita usada só pelo `deploy.yml`, com `DeployKey` na lista de bypass do ruleset.
- [ ] Criar o overlay `k8s/overlays/aws-eks-homolog` (namespace `techchallenge-homolog`, login local e dados de demo ligados).
- [ ] Adicionar `OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector.monitoring.svc.cluster.local:4317`, `OTEL_SERVICE_NAME` e `Smtp__Host=mailpit` aos overlays.
- [ ] Trocar os valores `replace-me` de `Jwt__Authority`, `Jwt__Issuer` e `ApprovalLinks__BaseUrl` pelos outputs reais da Lambda e do Kong.
- [ ] Decidir a estratégia de migrations com duas réplicas iniciais (Job dedicado ou advisory lock).

### Integração com a Lambda de autenticação

- [ ] Alinhar a audiência do token: a API espera `techchallenge-api` e a Lambda emite `tech-challenge-api`.
- [ ] Definir como clientes autenticados por CPF acessam as rotas protegidas: a Lambda emite `scope=customer` sem `role`, e as políticas atuais da API exigem `Administrador`, `Vendedor` ou `Mecanico`.
- [ ] Definir como funcionários se autenticam em produção, já que o login local não é registrado nesse ambiente.

### Organização e GitHub

- [x] Branch padrão do `tech-challenge-lambda` trocada para `main`.
- [x] `main` protegida: PR com 1 aprovação, sem force push nem exclusão e checks obrigatórios (aplicação: testes unitários e de integração; k8s: validação Terraform e render GitOps; banco: validação Terraform). Na Lambda vale a proteção que já existia (PR obrigatório).
- [x] Descrição nos quatro repositórios e exclusão automática da branch após o merge.
- [ ] Adicionar o usuário `soat-architecture` aos quatro repositórios (exige um admin; a ação concede acesso a um usuário externo).
- [ ] Apagar as branches já mescladas `feature/api-gateway-kong` (aplicação) e `feature/lambda-auth` (Lambda); nenhuma tem commits fora da `main`.
- [ ] Avaliar o repositório privado `demo-repository`, criado automaticamente pelo GitHub e sem relação com o projeto.
- [ ] Cadastrar o secret `DISCORD_WEBHOOK_URL` para as notificações.

### Com acesso à AWS

- [ ] Cadastrar as chaves da sessão do Academy e `AWS_DEPLOY_ENABLED=true`.
- [ ] Aplicar `tech-challenge-k8s`, depois `tech-challenge-database`, depois publicar a imagem.
- [ ] Preencher o `TFVARS_JSON` da Lambda com os outputs de rede e do segredo do banco, e fazer o primeiro deploy.
- [ ] Validar ponta a ponta: login por CPF, JWT aceito pela API, abertura de OS, rollout pelo Argo CD, dashboards e alertas com dados reais.
- [ ] Registrar links dos deploys ativos nos READMEs.

### Entrega

- [ ] Vídeo de até 15 minutos seguindo [demo/fase-3](demo/fase-3/README.md).
- [ ] PDF com os links dos quatro repositórios, vídeo e documentação, e a confirmação do `soat-architecture`.

## Critério de conclusão do CI/CD

O requisito "CI/CD com deploy automático para a nuvem" estará comprovado
quando:

1. merge em `main` executar build e testes;
2. a imagem imutável for publicada no ECR;
3. a pipeline gravar a tag no overlay e o Argo CD fizer o rollout sozinho;
4. o rollout terminar saudável com pelo menos duas réplicas e HPA funcional;
5. o Kong alcançar a API e um JWT da Lambda for aceito, e um token inválido recusado;
6. a API acessar o RDS com o Secret criado pelo Terraform;
7. dashboards, logs e traces mostrarem a execução.
