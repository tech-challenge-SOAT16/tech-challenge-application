# ToDo — Fase 3

Este documento consolida tudo que ainda precisa ser configurado, decidido,
provisionado ou validado para colocar a aplicação principal em produção no AWS
EKS. Os itens estão separados por contexto e indicam o repositório responsável.

## Estado já implementado neste repositório

- [x] Manifests Kustomize em `k8s/base` e `k8s/overlays/aws-eks`.
- [x] Deployment, Service ClusterIP, ServiceAccount e PodDisruptionBudget.
- [x] Startup, readiness e liveness probes.
- [x] Requests/limits e HPA de 2 a 10 réplicas por CPU/memória.
- [x] Container não-root, filesystem somente leitura, seccomp e capabilities removidas.
- [x] Pipeline GitHub Actions com build, testes, publicação no ECR e deploy no EKS.
- [x] Autenticação AWS do pipeline por OIDC, sem access keys permanentes.
- [x] Aplicação configurada como resource server OIDC/JWKS em produção.
- [x] Validação restrita a RS256 no modo externo.
- [x] Emissão HS256 local restrita a desenvolvimento/testes e proibida em Production.
- [x] Rotas locais de login/refresh/credenciais não são registradas no modo externo.
- [x] Chave dos links de aprovação separada do JWT em `ApprovalLinks__SigningKey`.
- [x] Build Release concluído; 123 testes unitários e 31 testes de integração aprovados.

---

## 1. Configuração do repositório GitHub

**Responsável:** este repositório / administração do GitHub.

### Bloqueadores do deploy automático

- [ ] Criar o GitHub Environment chamado exatamente `production`, usado pelo job
  `deploy` de `.github/workflows/deploy.yml`.
- [ ] Cadastrar o secret `AWS_ROLE_ARN` no Environment `production` ou no repositório.
- [ ] Cadastrar a variable `AWS_REGION` (exemplo: `us-east-1`).
- [ ] Cadastrar a variable `EKS_CLUSTER_NAME` com `techchallenge-eks` ou ajustar o
  nome no repositório/infraestrutura.
- [ ] Confirmar que GitHub Actions está habilitado e pode solicitar `id-token: write`.
- [ ] Criar commit inicial com os arquivos deste checkout e enviar para o remoto.
  No estado local verificado, todos os arquivos aparecem como não rastreados no Git.
- [ ] Confirmar que a branch de produção é `main`, pois somente push nessa branch
  dispara o deploy automático.

### Governança recomendada

- [ ] Proteger `main` e exigir pull request, build, testes unitários e integração.
- [ ] Definir reviewers/approval no Environment `production`, se a equipe desejar
  aprovação antes do deploy. Sem protection rule, o deploy continua automático.
- [ ] Impedir exclusão/force push em `main`.
- [ ] Definir retenção dos logs e artefatos do GitHub Actions.
- [ ] Revisar os oito avisos atuais do build, embora eles não bloqueiem a compilação.
- [ ] Corrigir/configurar os feeds NuGet da máquina local. O restore padrão tentou
  feeds Nexus sem versões 10.0.9; a validação só passou ao usar explicitamente
  `https://api.nuget.org/v3/index.json`. O runner GitHub precisa conseguir acessar
  um feed que contenha todas as versões declaradas.

---

## 2. OIDC e IAM para GitHub Actions

**Responsável:** repositório de infraestrutura Kubernetes/AWS (Terraform).

### Bloqueadores

- [ ] Criar ou reutilizar o IAM OIDC Provider `token.actions.githubusercontent.com`.
- [ ] Criar a IAM Role referenciada por `AWS_ROLE_ARN`.
- [ ] Restringir a trust policy ao audience `sts.amazonaws.com`.
- [ ] Restringir a trust policy ao subject deste repositório e Environment. Como o
  workflow usa `environment: production`, o subject esperado é:

  ```text
  repo:<ORGANIZACAO>/<REPOSITORIO>:environment:production
  ```

- [ ] Conceder à Role apenas as permissões ECR necessárias para login e push:
  `ecr:GetAuthorizationToken`, `ecr:BatchCheckLayerAvailability`,
  `ecr:GetDownloadUrlForLayer`, `ecr:BatchGetImage`, `ecr:InitiateLayerUpload`,
  `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload` e `ecr:PutImage`.
- [ ] Conceder `eks:DescribeCluster` para gerar o kubeconfig.
- [ ] Criar EKS Access Entry (preferível) para a Role do GitHub Actions.
- [ ] Criar RBAC Kubernetes com privilégio mínimo no namespace `techchallenge`
  para aplicar/consultar Deployment, Service, ConfigMap, ServiceAccount, HPA e PDB.
- [ ] Evitar conceder `cluster-admin` ao pipeline.

---

## 3. Amazon ECR

**Responsável:** repositório de infraestrutura Kubernetes/AWS (Terraform).

### Bloqueadores

- [ ] Criar o repositório ECR `techchallenge-application` na região configurada.
- [ ] Garantir que a IAM Role do pipeline possa publicar nesse repositório.
- [ ] Garantir que a IAM Role dos nós/Fargate do EKS possa baixar a imagem.
- [ ] Confirmar se ECR e EKS pertencem à mesma conta; se não, criar repository policy
  para acesso cross-account.

### Recomendações

- [ ] Ativar scan de vulnerabilidades no ECR.
- [ ] Configurar imutabilidade de tags. Se `latest` for mantida, avaliar excluir essa
  tag da regra ou remover sua publicação; o Deployment usa a tag imutável do SHA.
- [ ] Criar lifecycle policy para remover imagens antigas sem apagar versões em uso.
- [ ] Avaliar assinatura de imagem (Cosign/Notation) e geração de SBOM.

---

## 4. Cluster AWS EKS

**Responsável:** repositório de infraestrutura Kubernetes (Terraform).

### Bloqueadores

- [ ] Provisionar o cluster EKS.
- [ ] Criar o namespace `techchallenge`; este repositório apenas o referencia.
- [ ] Provisionar capacidade de execução (managed node groups ou Fargate).
- [ ] Instalar/configurar Metrics Server para o HPA.
- [ ] Garantir DNS e saída HTTPS dos pods para OIDC discovery/JWKS e serviços externos.
- [ ] Garantir comunicação dos pods com o banco gerenciado nas subnets/portas corretas.
- [ ] Definir se o endpoint da API do EKS será público ou privado.

### Runner e endpoint privado

- [ ] Se o endpoint do EKS for privado, disponibilizar runner GitHub Actions
  self-hosted dentro da VPC ou outra conectividade privada. O runner hospedado pelo
  GitHub não conseguirá executar `kubectl apply` em um endpoint exclusivamente privado.
- [ ] Se o endpoint for público, restringir CIDRs e avaliar a compatibilidade com os
  endereços dinâmicos dos runners hospedados. Para produção, runner privado é preferível.

### Compatibilidade e disponibilidade

- [ ] Alinhar a versão de `kubectl` fixada no workflow (`v1.30.4`) com a versão real
  do EKS, respeitando a política de skew do Kubernetes.
- [ ] Distribuir nós e pods entre pelo menos duas Availability Zones.
- [ ] Avaliar `topologySpreadConstraints` ou pod anti-affinity para impedir que as
  duas réplicas mínimas fiquem no mesmo nó/AZ.
- [ ] Instalar Cluster Autoscaler ou Karpenter caso o HPA possa exceder a capacidade
  atual dos nós.
- [ ] Configurar logs, métricas, alertas e retenção (CloudWatch/Prometheus/Grafana).

---

## 5. Secret da aplicação

**Responsável:** repositório de infraestrutura/Secrets (Terraform).

O Secret Kubernetes deve se chamar `techchallenge-api-secrets` e existir no
namespace `techchallenge` antes do deploy da aplicação.

### Valores obrigatórios

- [ ] `ConnectionStrings__DefaultConnection` — conexão TLS com PostgreSQL/RDS.
- [ ] `ApprovalLinks__SigningKey` — chave aleatória de pelo menos 32 caracteres,
  exclusiva dos links de aprovação/recusa.
- [ ] `IntegracaoExterna__ApiKey` — API key do callback externo.
- [ ] `Seed__AdminPassword` — confirmar se o seed administrativo continuará existindo.
- [ ] `Smtp__Host`.
- [ ] `Smtp__Port`.
- [ ] `Smtp__Username` e `Smtp__Password`, quando o provedor exigir.

### Gestão recomendada

- [ ] Definir se o Secret será criado diretamente pelo Terraform ou sincronizado do
  AWS Secrets Manager por External Secrets Operator/Secrets Store CSI Driver.
- [ ] Não versionar nenhum valor real no Git.
- [ ] Definir estratégia de rotação para API key, SMTP e chave dos links.
- [ ] Planejar restart/rollout dos pods quando um segredo consumido por `envFrom`
  mudar, pois variáveis de ambiente não são atualizadas em processos já iniciados.

> Não incluir chave privada RSA da Function nem segredo JWT neste Secret. A API
> obtém apenas chaves públicas por OIDC/JWKS.

---

## 6. Banco de dados gerenciado

**Responsável:** repositório de banco de dados gerenciado (Terraform).

### Bloqueadores

- [ ] Provisionar PostgreSQL gerenciado (por exemplo, Amazon RDS/Aurora PostgreSQL).
- [ ] Definir database, usuário da aplicação e política de senha/rotação.
- [ ] Criar Security Groups permitindo acesso ao banco somente a partir da aplicação.
- [ ] Usar subnets privadas e bloquear exposição pública, salvo justificativa explícita.
- [ ] Exigir conexão TLS e refletir os parâmetros na connection string.
- [ ] Armazenar a connection string/credenciais no AWS Secrets Manager ou mecanismo
  equivalente e sincronizar para `techchallenge-api-secrets`.
- [ ] Configurar backups automáticos, retenção, janela de manutenção e recuperação.
- [ ] Definir Multi-AZ conforme custo e requisito de disponibilidade.

### Migrations — decisão obrigatória antes da produção

- [ ] Revisar a execução de EF Core migrations no startup. O HPA inicia com duas
  réplicas, então dois pods podem tentar migrar/seedar simultaneamente.
- [ ] Escolher uma estratégia segura: Job Kubernetes/etapa exclusiva de migration,
  lock distribuído/advisory lock, ou mecanismo comprovadamente idempotente.
- [ ] Garantir que seed administrativo seja idempotente e compatível com a Function.
- [ ] Testar migrations e rollback contra PostgreSQL da mesma versão do ambiente real.

---

## 7. Function Serverless de autenticação

**Responsável:** futuro repositório da Function Serverless.

### Responsabilidades a implementar

- [ ] Implementar login e emissão de access token RS256.
- [ ] Definir e implementar refresh token, rotação, revogação e detecção de reuso.
- [ ] Definir ownership de logout, troca/reset de senha e listagem/revogação de
  sessões. Em Production, as rotas locais equivalentes não são registradas na API.
- [ ] Definir como a Function acessará usuários e hashes de senha: mesmo RDS,
  serviço interno ou armazenamento próprio.
- [ ] Se reutilizar o RDS, garantir VPC networking, Security Groups e compatibilidade
  com o hash PBKDF2 já usado pela aplicação.
- [ ] Definir limites de tentativa, proteção contra brute force e auditoria de login.

### Chaves assimétricas

- [ ] Gerar/provisionar par RSA adequado para RS256.
- [ ] Manter a chave privada exclusivamente na Function, protegida por AWS KMS ou
  AWS Secrets Manager e nunca retornada por API.
- [ ] Atribuir um `kid` estável à chave publicada.
- [ ] Publicar somente parâmetros públicos RSA (`kty`, `kid`, `use`, `alg`, `n`, `e`).
- [ ] Definir rotação: publicar chave nova e anterior durante pelo menos o tempo de
  vida máximo dos tokens antigos.
- [ ] Definir procedimento de revogação emergencial de uma chave comprometida.

### OIDC discovery/JWKS

- [ ] Publicar `GET {authority}/.well-known/openid-configuration`.
- [ ] Publicar o `jwks_uri` HTTPS indicado pelo discovery.
- [ ] Configurar cache headers adequados no discovery e JWKS.
- [ ] Garantir que `issuer` do discovery seja idêntico à claim `iss` dos tokens.
- [ ] Confirmar a URL final do Authority antes de substituir os placeholders do overlay.

### Contrato do token a confirmar

- [ ] Confirmar `iss` (deve ser URL HTTPS estável do emissor).
- [ ] Confirmar `aud`; o valor proposto atualmente é `techchallenge-api`.
- [ ] Emitir `sub` como GUID do usuário existente na aplicação.
- [ ] Emitir `name` com login/nome.
- [ ] Emitir `role` exatamente como `Administrador`, `Vendedor` ou `Mecanico`.
- [ ] Emitir `funcionarioId` como GUID quando houver vínculo.
- [ ] Emitir `jti`, `iat`, `nbf` e `exp`.
- [ ] Definir duração curta do access token e clock skew compatível (API usa 30 segundos).
- [ ] Não emitir HS256 para produção; a API aceita exclusivamente RS256 nesse modo.

---

## 8. API Gateway e conectividade com EKS

**Responsável:** repositório de infraestrutura/API Gateway (Terraform).

### Bloqueadores

- [ ] Criar API Gateway para controle e roteamento, conforme requisito do PDF.
- [ ] Definir REST API ou HTTP API. Essa escolha afeta authorizer e integração privada.
- [ ] Publicar as rotas da Function: login, refresh e OIDC discovery/JWKS.
- [ ] Roteiar as APIs de negócio para a aplicação no EKS.
- [ ] Criar VPC Link e destino interno compatível (NLB, ALB ou arquitetura de target
  group suportada pelo tipo escolhido de API Gateway).
- [ ] Conectar o destino interno ao Service/pods `techchallenge-api:8080`.
- [ ] Manter a aplicação sem exposição pública direta; o Service atual é ClusterIP.
- [ ] Definir health check do target usando `/health/live` ou `/health/ready` conforme
  comportamento desejado.
- [ ] Confirmar se o API Gateway também validará JWT. Mesmo nesse caso, a API mantém
  validação RS256 como defesa em profundidade.
- [ ] Configurar throttling, limites, logs de acesso, correlation ID e respostas padrão.
- [ ] Configurar CORS somente para origens/métodos necessários.

### URL pública

- [ ] Como não haverá domínio/ACM inicialmente, registrar a URL padrão `execute-api`.
- [ ] Substituir no overlay `k8s/overlays/aws-eks/configmap-patch.yaml`:
  - `Jwt__Authority`;
  - `Jwt__Issuer`;
  - `ApprovalLinks__BaseUrl`.
- [ ] Definir `Jwt__MetadataAddress` somente se o discovery não estiver em
  `{Authority}/.well-known/openid-configuration`.

---

## 9. E-mail e notificações

**Responsável:** repositório de infraestrutura e configuração da aplicação.

- [ ] Escolher provedor SMTP (por exemplo, Amazon SES SMTP).
- [ ] Validar identidade/remetente no provedor.
- [ ] Substituir `Smtp__FromAddress=nao-responda@techchallenge.local` por um remetente
  válido; `.local` não será aceito por provedores públicos.
- [ ] Configurar host, porta, usuário e senha no Secret.
- [ ] Validar TLS, limites de envio, retentativas e saída de sandbox do SES, se aplicável.
- [ ] Testar outbox e envio real no ambiente cloud.

---

## 10. Configuração pendente neste repositório

**Responsável:** este repositório, após outputs dos outros repositórios.

### Bloqueadores

- [ ] Substituir o account ID/região placeholder
  `000000000000.dkr.ecr.us-east-1.amazonaws.com` em
  `k8s/overlays/aws-eks/kustomization.yaml`. O pipeline sobrescreve esse valor em
  runtime, mas o manifesto versionado deve refletir o ambiente real quando conhecido.
- [ ] Substituir todos os valores `replace-me` de
  `k8s/overlays/aws-eks/configmap-patch.yaml`.
- [ ] Confirmar e ajustar `Jwt__Audience`.
- [ ] Ajustar o pin de `kubectl` no workflow para a versão do EKS.
- [ ] Confirmar requests/limits e limites do HPA após observar consumo real.
- [ ] Decidir a estratégia de migrations concorrentes descrita no contexto do banco.

### Documentação

- [ ] Atualizar o título e conteúdo principal do `README.md`, ainda centrado na Fase 2,
  para apresentar arquitetura e responsabilidades da Fase 3.
- [ ] Criar/atualizar o diagrama de arquitetura cloud com os quatro repositórios.
- [ ] Atualizar documentos antigos que descrevem Kubernetes/Terraform local para não
  apresentá-los como arquitetura vigente da Fase 3.
- [ ] Registrar URLs finais dos quatro repositórios e respectivos pipelines.

---

## 11. Validação final de CI/CD e produção

**Responsável:** equipe, após todos os recursos anteriores existirem.

### Pipeline e manifests

- [ ] Executar `kubectl kustomize k8s/base` e
  `kubectl kustomize k8s/overlays/aws-eks` com ferramentas instaladas. Esta máquina
  não possuía `kubectl`/`kustomize` durante a implementação.
- [ ] Adicionar validação automática de manifests ao CI (por exemplo, renderização
  Kustomize e validação de schema/policy).
- [ ] Fazer push controlado em `main` e comprovar que a pipeline publica no ECR.
- [ ] Comprovar que a pipeline assume a Role via OIDC sem access keys estáticas.
- [ ] Comprovar `kubectl apply`, rollout e estado saudável das duas réplicas.
- [ ] Validar HPA sob carga e escalabilidade dos nós.
- [ ] Validar PDB durante drain de nó.
- [ ] Testar rollback de imagem e definir procedimento operacional. O pipeline aguarda
  o rollout, mas não executa rollback automático atualmente.

### Integração ponta a ponta

- [ ] Autenticar pela Function e obter JWT RS256.
- [ ] Confirmar validação por `kid`/JWKS na aplicação.
- [ ] Confirmar rejeição de HS256, `alg=none`, issuer/audience incorretos e token expirado.
- [ ] Confirmar acesso por role às policies da aplicação.
- [ ] Testar rotação JWKS sem reiniciar pods.
- [ ] Testar API Gateway → VPC Link → aplicação EKS.
- [ ] Testar aplicação EKS → RDS e persistência após rollout.
- [ ] Testar links de aprovação usando `ApprovalLinks__SigningKey` independente.
- [ ] Testar SMTP/outbox no ambiente AWS.
- [ ] Executar smoke test após deploy e considerar automatizá-lo no workflow.

### Segurança e operação

- [ ] Executar análise de vulnerabilidades da imagem antes da promoção.
- [ ] Configurar alertas para falha de rollout, pods não prontos, HPA no máximo,
  erros 5xx, latência, autenticação e indisponibilidade do banco.
- [ ] Revisar logs para garantir que tokens, senhas, API keys e connection strings
  nunca sejam registrados.
- [ ] Definir runbook de incidente, rollback e rotação de credenciais/chaves.

---

## Critério de conclusão do item do PDF

O requisito **“CI/CD implementado, com deploy automático para a nuvem”** estará
comprovado quando:

1. push/merge em `main` executar build e todos os testes;
2. o GitHub Actions assumir a IAM Role via OIDC;
3. a imagem imutável for publicada no ECR;
4. os manifests forem aplicados automaticamente no EKS;
5. o rollout terminar saudável com probes e pelo menos duas réplicas;
6. o HPA estiver funcional;
7. o API Gateway alcançar a aplicação pela integração privada;
8. um JWT RS256 emitido pela Function for aceito pela API e um token inválido for rejeitado;
9. a aplicação acessar o banco gerenciado usando segredo não versionado;
10. logs/evidências da pipeline e do cluster forem registrados para a entrega.
