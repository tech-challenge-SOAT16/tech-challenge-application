# Validação da infraestrutura local — 01/09/2026

## Ambiente

- Cluster pré-existente do Docker Desktop, contexto `docker-desktop`.
- Kubernetes 1.36.1, nó ARM64.
- Namespace da aplicação: `techchallenge`.
- Terraform 1.15.8; providers Kubernetes 3.2.1, Helm 3.2.0 e Random 3.9.0.
- PostgreSQL `17.11-bookworm`, PVC de 2 GiB na StorageClass `standard`.
- Metrics Server 0.9.0, chart 3.14.0.
- Imagem publicada testada: `ghcr.io/louistx/fiap-tech-challenge:eee8e1a69833`
  (`linux/amd64`, executada por emulação no nó ARM64).

O cluster não foi recriado. Terraform administra as dependências; Kustomize
administra a API, que agora aplica migrations na inicialização. Por definição do
ambiente acadêmico, o GitHub Actions publica a imagem e o deploy é concluído na
máquina que hospeda o Kubernetes do Docker Desktop.

## Resultados observados

Os testes de imagem, rollout, persistência e HPA abaixo foram feitos na implantação
local inicial. A revisão simplificada do startup está validada separadamente adiante.

| Verificação | Resultado |
| --- | --- |
| Build Release da solução | Sem erros ou avisos |
| Testes unitários | 111 aprovados |
| Testes de integração | 29 aprovados |
| `terraform fmt` / `validate` | Aprovados |
| Testes Terraform com providers mockados | 4 aprovados |
| Renderização Kustomize | Base, overlay local único e carga válidos |
| Build da imagem Docker | Publicação `linux/amd64` concluída no GHCR |
| Terraform aplicado | 10 recursos no state, incluindo release Helm e geradores de senha |
| Plano após provisionamento | `No changes`, exit code 0 |
| Migrations no fluxo anterior | Job concluiu com exit code 0; substituído posteriormente por migrations no startup |
| Rollout da API | Uma réplica pronta, sem reinícios no estado final |
| `/health/live` e `/health/ready` | HTTP 200 |
| Swagger e OpenAPI | HTTP 200 |
| Login do administrador e `/api/v1/auth/me` com token | HTTP 200 |
| `/api/v1/auth/me` sem token | HTTP 401 |
| Persistência | Pod PostgreSQL recriado; mesmo registro de administrador preservado |
| Backup/restauração | `pg_dump -Fc` restaurado em banco separado; 1 usuário e 8 migrations recuperados |
| Proteção contra destruição | `plan -destroy` bloqueado por `prevent_destroy`; nenhum destroy aplicado |
| HPA | Escalou de 1 para 3 réplicas e retornou a 1 |

O banco temporário de restauração foi removido após o teste. O backup ficou em
`.local/backups/`, fora do Git e do contexto Docker. Nenhuma senha ou token é
incluída neste relatório.

## Escala observada

O Job de carga fez requisições ao readiness durante 120 segundos. A utilização
abaixo é percentual do request de CPU de 100m, não do limite de 500m.

| Segundos após iniciar a observação | Réplicas atuais | Réplicas desejadas | CPU / request |
| ---: | ---: | ---: | ---: |
| 0 | 1 | 1 | 17% |
| 30 | 1 | 2 | 134% |
| 45 | 2 | 3 | 308% |
| 60 | 3 | 3 | 255% |
| 136 | 3 | 3 | 129% |
| 166 | 3 | 3 | 12% |
| 211 | 3 | 1 | 2% |
| 226 | 1 | 1 | 2% |

O tempo inclui coleta de métricas, avaliação do HPA e janela de estabilização.
O banco permaneceu com uma réplica durante todo o teste.

## Problemas encontrados e tratados

1. **Certificado do kubelet local:** o primeiro apply não concluiu a instalação do
   Metrics Server porque o certificado não contém IP SAN. A release foi removida
   automaticamente pelo Helm (`atomic`). O argumento `--kubelet-insecure-tls`,
   fixo neste ambiente exclusivo do Docker Desktop, permitiu concluir o segundo
   apply. A verificação TLS dos providers permaneceu ativa.
2. **Pod antigo no Lens:** `default/fiap-tech-challenge-api-5d8c7b49c-q6j6h`, criado
   em 20/08, estava em CrashLoopBackOff com 350 reinícios. Seu Deployment usava a
   imagem GHCR `latest` e não fornecia a connection string. O Deployment antigo foi
   reduzido a zero réplicas, com configuração preservada. A aplicação nova está no
   namespace `techchallenge` e recebe o Secret criado pelo Terraform.

## Revisão atual: migrations no startup e um único apply

O Job de migrations e os scripts auxiliares foram removidos. A API voltou a
executar `MigrateAsync` e o seed antes de iniciar o servidor HTTP, exceto em
`Testing`. Se essa inicialização falhar, o processo termina; o Deployment usa
o reinício automático do Kubernetes. A startup probe permite até cinco minutos.

O overlay `docker-local` contém apenas ConfigMap, Deployment, Service e HPA.
O ConfigMap comum recebe os valores por patch; os Secrets continuam no Terraform.
Um único `kubectl apply -k k8s/overlays/docker-local` aplica a configuração da API.
Não são necessários filtros por componente ou espera de um Job externo.

O Terraform também foi reduzido ao único ambiente apresentado: contexto
`docker-desktop`, PostgreSQL 17.11, PVC de 2 GiB e Metrics Server obrigatório.
As variáveis e outputs que não participavam da demonstração foram removidos; não
é necessário criar `terraform.tfvars`.

Validação desta revisão:

- Build Release: sem erros ou avisos.
- 123 testes unitários e 31 testes de integração aprovados.
- `terraform fmt`, `validate` e quatro testes com providers mockados aprovados.
- Quatro recursos aceitos juntos pelo API server em dry-run, sem aplicar a nova imagem.
- API compilada executada contra um banco PostgreSQL temporário vazio: HTTP 200
  após aplicar 8 migrations e criar 1 administrador.
- Segunda inicialização no mesmo banco: HTTP 200, ainda 8 migrations e 1 administrador.
- Banco inacessível: processo terminou com erro antes de iniciar o servidor HTTP.

O banco de validação e seu port-forward temporário foram removidos. O Job antigo,
já concluído, também foi removido; nenhum banco da aplicação foi apagado.

O overlay fixa a tag imutável
`ghcr.io/louistx/fiap-tech-challenge:eee8e1a69833`, com
`imagePullPolicy: Always` na API. Na validação de 01/09, o registry e o cluster
informaram:

- Manifesto OCI: `application/vnd.oci.image.manifest.v1+json`.
- Digest: `sha256:926f9bd2ea981e39717c49e2742d9fbe12d1c22d24eb63eedc902dd234e79c2f`.
- Plataforma publicada: `linux/amd64`.
- Nó do Docker Desktop: `arm64`.
- Rollout: uma réplica pronta, com zero reinícios.
- `/health/live`, `/health/ready`, OpenAPI e Swagger: HTTP 200.

O workflow publica `latest` e os 12 primeiros caracteres do SHA do commit; o
SHA completo fica no rótulo OCI. A publicação foi simplificada para uma única
arquitetura e o job de build e push caiu de 11min32s para 56s. A atestação do
Buildx foi desativada porque ela transformava a tag em um índice OCI sem entrada
ARM64; o containerd do cluster não resolvia esse índice. A tag agora aponta
diretamente para o manifesto AMD64, que o Docker Desktop executou por emulação.

## Limites desta validação

- Não comprova alta disponibilidade: há um único nó e uma réplica do banco.
- Não cria cluster ou recursos de nuvem.
- Não publica API na internet: o acesso é por port-forward local.
- A imagem publicada é `linux/amd64`; a validação em Apple Silicon comprova a
  execução por emulação, não uma imagem ARM64 nativa.
- Não possui backend remoto, rotação automática de segredos ou backup agendado; esses recursos só seriam necessários em um ambiente compartilhado ou produtivo.
- O teste de carga demonstra o HPA, sem pretender medir capacidade de produção.

Para reproduzir, siga o [guia de infraestrutura](../infra/README.md).
