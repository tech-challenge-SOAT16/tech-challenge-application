# Roteiro do vídeo demonstrativo - Fase 2

## Objetivo e recorte da demonstração

O vídeo deve ter no máximo 15 minutos. O enunciado exige demonstrar:

- deploy da aplicação;
- execução do CI/CD;
- consumo das APIs;
- escalabilidade automática.

Neste projeto, o GitHub Actions executa build, testes e publicação da imagem no
GHCR. O provisionamento das dependências é feito localmente com Terraform e o
deploy da aplicação é feito com um único `kubectl apply -k`. Durante o vídeo,
apresente essas duas etapas como partes complementares do fluxo, sem afirmar que
o GitHub Actions acessa o cluster local.

Tempo planejado: aproximadamente **13 minutos e 40 segundos**, deixando margem
para respostas lentas e troca de telas.

## Preparação antes de gravar

### 1. Promover uma imagem imutável

O workflow publica a imagem com `latest` e com os 12 primeiros caracteres do SHA
do commit. O SHA completo continua registrado no rótulo OCI
`org.opencontainers.image.revision`. Depois da publicação, o próprio workflow
atualiza `images[].newTag` no overlay e cria o commit de promoção:

1. Commit A: contém a versão final do código, Terraform, Kubernetes e documentação.
2. Faça push/merge do Commit A na `main` e aguarde o workflow **Publish Docker image** ficar verde.
3. Confirme o passo **Promote image in Kubernetes overlay**.
4. Confirme o commit automático `chore(k8s): promove imagem <tag> [skip ci]`.

O commit automático altera somente o `newTag` e não inicia outra execução da
pipeline. O manifesto versionado aponta para a imagem imutável do Commit A.

### 2. Validar o ambiente

Execute antes de gravar:

```bash
git status --short --branch
dotnet test TechChallenge.slnx --configuration Release
terraform -chdir=infra/environments/local fmt -check -recursive
terraform -chdir=infra/environments/local validate
terraform -chdir=infra/environments/local test
kubectl kustomize k8s/overlays/docker-local > /dev/null
kubectl kustomize k8s/tests > /dev/null
kubectl --context=docker-desktop get nodes
```

O `terraform init` pode ser executado antes da gravação, pois ele apenas prepara
providers. Não mostre state, Secrets, senhas, tokens ou connection strings.

### 3. Preparar as telas

- GitHub aberto na página do commit de promoção e na execução dos workflows.
- Diagrama de arquitetura aberto no README/documentação.
- Terminal na raiz do repositório com fonte legível.
- Lens ou um segundo terminal no namespace `techchallenge`.
- Cliente HTTP com os arquivos de `docs/demo/fase-2/` preparados.
- Mailpit aberto em `http://localhost:8025`.
- API de demonstração já iniciada pelo Docker Compose com dados fictícios.
- Job de carga antigo removido antes de iniciar a gravação.

Para subir a API, o PostgreSQL e o Mailpit de demonstração, sem o SonarQube:

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  up -d --build db mailpit techchallenge.api
```

## Roteiro cronometrado

### 0:00-0:35 - Abertura

**Tela:** título do projeto ou README.

**Fala sugerida:**

> Olá. Este é o Tech Challenge da Fase 2, uma API para gestão de uma oficina
> mecânica. Nesta evolução trabalhamos qualidade de código, testes,
> conteinerização, infraestrutura como código, Kubernetes, CI/CD e
> escalabilidade. Vamos demonstrar a arquitetura, a publicação da imagem, o
> deploy, o fluxo principal de uma ordem de serviço e o HPA em funcionamento.

### 0:35-1:35 - Arquitetura

**Tela:** diagrama de arquitetura e estrutura de pastas.

**Mostrar:**

- API ASP.NET Core, aplicação, domínio, abstrações e infraestrutura;
- PostgreSQL em StatefulSet com PVC;
- Terraform responsável por namespace, banco, Secrets e Metrics Server;
- Kustomize responsável por ConfigMap, Deployment, Service e HPA;
- GitHub Actions responsável por build, testes, publicação no GHCR e promoção da tag no overlay.

**Fala sugerida:**

> A solução é um monólito organizado em camadas. A API chama os casos de uso,
> o domínio concentra as regras e a infraestrutura implementa persistência e
> autenticação. No ambiente local, o cluster já é fornecido pelo Docker Desktop.
> O Terraform provisiona as dependências, e o Kustomize implanta a aplicação.
> Cada recurso tem um único responsável, evitando duplicação entre Terraform e
> manifests.

### 1:35-3:10 - CI/CD e imagem Docker

**Tela:** GitHub Actions do Commit A.

**Mostrar:**

1. SHA do Commit A.
2. Job de validação com restore, build, 123 testes unitários e 31 testes de integração.
3. Job `Build and push image` concluído para `linux/amd64`.
4. Tag do GHCR igual aos 12 primeiros caracteres do SHA do Commit A.
5. Commit automático alterando `newTag` para essa tag imutável.

**Fala sugerida:**

> A cada push na main, a pipeline restaura e compila a solução, executa os
> testes e somente depois libera a publicação. A imagem recebe `latest` e uma
> tag imutável formada pelos 12 primeiros caracteres do SHA. O SHA completo fica
> preservado nos metadados OCI. Para reduzir o tempo e a complexidade de uma
> entrega acadêmica local, o build gera apenas a imagem AMD64; o Docker Desktop
> no Apple Silicon a executa por emulação. Depois do push ao GHCR, a própria
> pipeline promove a tag no overlay Kubernetes com um commit automático que não
> dispara outra execução. O cluster local não é acessado pelo runner do GitHub;
> a aplicação da infraestrutura é demonstrada a seguir.

### 3:10-5:15 - Fundação Terraform e deploy Kustomize

**Tela:** terminal e, depois, Lens ou outro terminal.

**Comandos:**

```bash
kubectl --context=docker-desktop get namespace techchallenge

kubectl --context=docker-desktop apply -k k8s/overlays/docker-local

terraform -chdir=infra/environments/local init
terraform -chdir=infra/environments/local fmt -check
terraform -chdir=infra/environments/local validate
terraform -chdir=infra/environments/local test
terraform -chdir=infra/environments/local plan -out=local.tfplan
terraform -chdir=infra/environments/local apply local.tfplan

kubectl --context=docker-desktop -n techchallenge get pods,svc,pvc

kubectl --context=docker-desktop apply -k k8s/overlays/docker-local

kubectl --context=docker-desktop -n techchallenge rollout status \
  deployment/fiap-tech-challenge-api --timeout=300s

kubectl --context=docker-desktop -n techchallenge get pods,svc,pvc,hpa
```

**Fala sugerida:**

> O namespace da aplicação ainda não existe. Por isso, a primeira tentativa de
> aplicar o overlay falha de forma esperada e demonstra a separação de
> responsabilidades. Depois de validar os arquivos e testes, o plano mostra as
> dependências que o Terraform criará: namespace, Secrets, PostgreSQL persistente
> e Metrics Server. Em seguida, um único apply do overlay cria ConfigMap,
> Deployment, Service e HPA, usando a imagem imutável promovida no Git. O rollout
> termina com o pod pronto; o PostgreSQL mantém uma réplica, enquanto a API pode
> variar entre uma e três.

Se houver tempo, abra um port-forward e mostre os endpoints de saúde:

```bash
kubectl --context=docker-desktop -n techchallenge port-forward \
  svc/fiap-tech-challenge-api-service 18080:8080
```

- `http://localhost:18080/health/live`
- `http://localhost:18080/health/ready`
- `http://localhost:18080/swagger`

### 5:15-10:35 - Consumo das APIs

**Tela:** cliente HTTP e, ao lado, logs da API de demonstração.

Use a instância Docker Compose em `http://localhost:8080`, que possui somente
dados fictícios. Execute primeiro `docs/demo/fase-2/00-auth.http` para obter
tokens de administrador, vendedor e mecânico.

#### Abertura completa e identificação única

Execute o passo 1 de `01-aprovacao-por-email.http`.

**Fala sugerida:**

> A abertura vincula o cliente, o veículo e o funcionário responsável. A
> resposta é `201 Created` e contém o identificador único da nova ordem de
> serviço. Os serviços e produtos necessários são registrados depois, durante
> o diagnóstico.

Mostre rapidamente `GET /api/v1/ordens-servico/{id}/status` com o estado
`Recebida`.

#### Fluxo operacional

Execute, sem ler todo o JSON:

1. atribuição ao mecânico;
2. registro do diagnóstico;
3. envio do orçamento;
4. abertura do e-mail no Mailpit;
5. aprovação pelo link assinado e confirmação no navegador;
6. consulta do novo status;
7. finalização e entrega.

**Fala sugerida:**

> A máquina de estados protege a sequência da OS. O diagnóstico registra os
> itens e o orçamento aguarda decisão. O cliente recebe um e-mail com links
> assinados, válidos por 48 horas e sem exposição da chave do integrador. Antes
> de decidir, ele visualiza os serviços, produtos e o valor total. O primeiro
> clique abre uma confirmação e somente o envio do formulário altera a OS. A
> finalização baixa o estoque e, depois da entrega, o registro permanece no
> histórico.

Em seguida, execute o fluxo de recusa em `02-recusa-externa.http` para mostrar o
webhook protegido por API key. Reutilize o identificador com outro conteúdo e
mostre o `409 Conflict` da idempotência.

**Fala sugerida:**

> Cada transição também cria uma mensagem na outbox dentro da mesma transação.
> Um worker envia essas mensagens por SMTP e registra a entrega. Para a
> demonstração usamos o Mailpit local, portanto não dependemos de infraestrutura
> externa. Se o SMTP falhar, a mensagem permanece no banco e recebe novas
> tentativas com espera progressiva.

Mostre `GET /api/v1/ordens-servico/oficina` e destaque a priorização:
`EmExecucao`, `AguardandoAprovacao`, `EmDiagnostico` e `Recebida`, com as mais
antigas primeiro e sem finalizadas ou entregues.

#### Regra de estoque e acompanhamento

Se houver tempo, use a coleção legada em `src/TechChallenge.Api/demo/` para
demonstrar também a regra de falta de estoque.

**Fala sugerida:**

> Ao solicitar quantidade maior que o saldo, a API responde 400, impede saldo
> negativo e registra a notificação simulada nos logs. Para acompanhamento sem
> autenticação, o cliente utiliza o código único da OS. Também disponibilizamos
> a métrica de tempo médio de execução.

Mostre rapidamente as chamadas de acompanhamento, fila e tempo médio em
`03-metricas-e-acompanhamento.http`.

### 10:35-13:10 - Escalabilidade automática

**Tela:** dois terminais ou Lens.

No primeiro terminal:

```bash
kubectl --context=docker-desktop -n techchallenge get hpa,pods -w
```

No segundo:

```bash
kubectl --context=docker-desktop apply -f k8s/tests/hpa-load.yaml
```

**Fala sugerida enquanto as métricas atualizam:**

> O HPA observa a utilização de CPU da API. O alvo é 70% do request de 100
> millicores, com mínimo de uma e máximo de três réplicas. Este Job executa carga
> por 120 segundos contra o readiness endpoint. Quando a utilização ultrapassa o
> alvo, o Kubernetes aumenta automaticamente o número de pods. O PostgreSQL não
> participa dessa escala e permanece com uma réplica.

Mostre o HPA mudando o número desejado e os novos pods ficando `Running` e
`Ready`. Não é necessário esperar toda a redução durante a fala; se houver corte
do tempo ocioso, deixe isso claro e mostre depois o retorno para uma réplica.

### 13:10-13:40 - Encerramento

**Tela:** README, repositório e recursos saudáveis.

**Fala sugerida:**

> Demonstramos a pipeline com build, testes e publicação da imagem, o
> provisionamento declarativo, o deploy versionado no Kubernetes, os principais
> fluxos da API e a escala automática de uma para até três réplicas. O
> repositório contém os fontes, manifests, Terraform, documentação e instruções
> para reprodução. Obrigado.

## Evidências que precisam aparecer

- SHA do código e tag imutável da imagem;
- workflows verdes e quantidade de testes;
- criação da fundação pelo `terraform apply` no cluster vazio;
- resultado do único `kubectl apply -k` e rollout saudável;
- pod, Service, PVC e HPA no namespace `techchallenge`;
- abertura da OS com cliente, veículo, serviços e produtos;
- e-mail de aprovação com resumo do orçamento e botões de decisão;
- recusa pelo webhook e proteção idempotente retornando `409 Conflict`;
- consulta de status, fluxo da OS, priorização e baixa de estoque;
- aumento real do número de réplicas durante a carga;
- ausência de segredos e dados pessoais reais na tela.

## Plano de contingência

- Se o GitHub Actions estiver lento, use uma execução já concluída do mesmo SHA.
- Se o GHCR estiver indisponível, não troque para `latest`; mostre a execução
  verde e grave o deploy quando a tag imutável estiver acessível.
- Se o HPA mostrar `<unknown>`, verifique o Metrics Server e `kubectl top pods`
  antes de iniciar a gravação.
- Se o Job de carga já existir, remova apenas esse Job antes da gravação e aplique
  novamente o arquivo de teste.
- Se alguma chamada da API falhar, pare a gravação e reinicie a base de
  demonstração; não edite respostas nem esconda erros.
