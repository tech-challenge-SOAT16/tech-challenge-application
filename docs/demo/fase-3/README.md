# Demo da Fase 3

Roteiro para demonstrar a solução no AWS EKS: infraestrutura por Terraform,
entrega por GitOps (Argo CD), Kong como gateway e monitoramento com
OpenTelemetry e Grafana. A infraestrutura fica no repositório
[tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s).

> Pendências para o roteiro completo: overlay `k8s/overlays/aws-eks-homolog`
> (namespace `techchallenge-homolog`, login local e dados de demo ligados),
> `deploy.yml` gravando a tag nos overlays em vez de `kubectl apply` e o
> repositório `tech-challenge-terraform` (banco e Secret da API).

## 1. Credenciais da AWS no GitHub (uma vez)

As pipelines usam as chaves temporárias do AWS Academy guardadas como secrets
do GitHub. Cadastre na organização (vale para todos os repositórios) ou em
cada repositório, em *Settings → Secrets and variables → Actions*:

| Tipo | Nome | Valor |
| --- | --- | --- |
| Secret | `AWS_ACCESS_KEY_ID` | sessão atual do Learner Lab |
| Secret | `AWS_SECRET_ACCESS_KEY` | sessão atual do Learner Lab |
| Secret | `AWS_SESSION_TOKEN` | sessão atual do Learner Lab |
| Variable | `AWS_DEPLOY_ENABLED` | `true` (sem ela as pipelines só rodam o CI) |
| Variable (opcional) | `AWS_REGION` | padrão `us-east-1` |

O bucket do state do Terraform (`techchallenge-tfstate-<account-id>`) é criado
pela pipeline do `tech-challenge-k8s` na primeira execução.

A variable pode ser criada pelo terminal:

```bash
gh variable set AWS_DEPLOY_ENABLED --org tech-challenge-SOAT16 --visibility all --body true
```

## 2. A cada sessão do Learner Lab

As chaves do Academy expiram em 4 horas. Ao iniciar o lab:

1. Em *AWS Details → AWS CLI*, copie o bloco para `~/.aws/credentials`.
2. Atualize os três secrets com os valores da sessão:

   ```bash
   for key in aws_access_key_id aws_secret_access_key aws_session_token; do aws configure get "$key" | gh secret set "$(echo "$key" | tr a-z A-Z)" --org tech-challenge-SOAT16 --visibility all; done
   ```

   Sem permissão de admin na organização, troque `--org tech-challenge-SOAT16
   --visibility all` por `--repo tech-challenge-SOAT16/<repositório>` e repita
   para cada repositório.

3. Confira a sessão: `aws sts get-caller-identity`.

Com a sessão vencida, as pipelines falham no passo "Conferir sessão e bucket
do state" pedindo a renovação dos secrets.

## 3. Subir o ambiente (antes de gravar, cerca de 25 minutos)

1. No `tech-challenge-k8s`, *Actions → Terraform e GitOps → Run workflow* com
   `apply` (ou merge de um PR em `main`). Cria VPC, EKS, ECR e Argo CD; o Argo CD
   sincroniza Kong, observabilidade, Mailpit e a API.
2. Banco e Secret da API: pipeline do `tech-challenge-terraform`.
3. Imagem da API: pipeline desta aplicação publica no ECR e grava a tag nos
   overlays `k8s/overlays/aws-eks-homolog` e `k8s/overlays/aws-eks` da `main`.
4. Conectar o kubectl e conferir:

   ```bash
   aws eks update-kubeconfig --region us-east-1 --name techchallenge-eks
   kubectl -n argocd get applications
   ```

   Todas as Applications devem estar `Synced` e `Healthy`.

## 4. Endereço da demo

O hostname do NLB muda sempre que o cluster é recriado. Para a demo, publique o
Kong numa porta fixa da máquina (deixe o terminal aberto):

```bash
kubectl -n kong port-forward svc/kong-gateway-proxy 8000:80
```

| Serviço | Endereço | Login |
| --- | --- | --- |
| API de homologação | `http://localhost:8000/homolog` | usuários de demo (`Demo@123`) |
| API de produção | `http://localhost:8000` | JWT da Lambda de autenticação |
| Argo CD | `http://localhost:8000/argocd` | `admin` / comando abaixo |
| Grafana | `http://localhost:8000/grafana` | `admin` / comando abaixo |
| Mailpit | `http://localhost:8025` | port-forward abaixo |

```bash
# Senha do Argo CD
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}' | base64 -d

# Senha do Grafana
kubectl -n monitoring get secret grafana-admin -o jsonpath='{.data.admin-password}' | base64 -d

# Mailpit de homologação (use -n techchallenge para o de produção)
kubectl -n techchallenge-homolog port-forward svc/mailpit 8025
```

O endereço público continua disponível para mostrar no vídeo:

```bash
kubectl -n kong get svc kong-gateway-proxy -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
```

## 5. Roteiro do vídeo

1. **Infraestrutura**: pipeline do `tech-challenge-k8s` (fmt, validate, tflint,
   testes, plan e apply) e o cluster no console da AWS.
2. **GitOps**: Argo CD em `/argocd` com as Applications por sync wave
   (observabilidade, Kong, Mailpit e API em homologação e produção).
3. **Fluxo de negócio em homologação**: os arquivos de
   [docs/demo/fase-2](../fase-2/README.md) com `@baseUrl =
   http://localhost:8000/homolog`; aprovação do orçamento pelo e-mail no
   Mailpit.
4. **Monitoramento**: execute [monitoramento.http](monitoramento.http) com o
   mesmo `@baseUrl` algumas vezes e mostre no Grafana:
   - dashboard *TechChallenge - Ordens de Serviço* (volume diário, tempo médio
     por status, erros de integração, latência, uptime, CPU e memória);
   - dashboards *ASP.NET Core* e *ASP.NET Core Endpoint*;
   - *Explore* → Loki `{service_name="techchallenge-api"} | CorrelationId="demo-monitoramento"`
     e o trace correspondente no Tempo;
   - dashboards padrão do Kubernetes (CPU e memória dos pods).
5. **Alerta**: gere falhas (passos 9 a 11 do `monitoramento.http`) e mostre o
   alerta disparado e o e-mail do Alertmanager no Mailpit de produção.
6. **Deploy contínuo**: merge de um PR desta aplicação → pipeline publica a
   imagem e commita a tag no overlay → Argo CD passa de `OutOfSync` para
   `Synced` e faz o rollout.

O Kong limita a 60 requisições por minuto por IP; repetir os arquivos muito
rápido retorna `429`.

## 6. Encerrar

Para poupar o crédito do Learner Lab, no `tech-challenge-k8s` rode *Run
workflow* com `destroy`. O Argo CD remove os recursos (inclusive o NLB) antes
do cluster ser apagado.

## Alternativas sem AWS

- **Docker compose**: [docs/monitoramento.md](../../monitoramento.md) sobe a API
  com `grafana/otel-lgtm`; mesmos dashboards e alertas.
- **kind**: `terraform/local` do `tech-challenge-k8s` sobe Argo CD, Kong e a
  stack de observabilidade num cluster local (`http://localhost:9080`).
