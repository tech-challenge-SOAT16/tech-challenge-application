# Relatório de Análise de Vulnerabilidades

## Visão geral

A análise estática do projeto foi executada localmente com o **SonarQube Community Build** e o **SonarScanner for .NET**. O processo considera o código da aplicação, métricas de segurança, confiabilidade, manutenibilidade, duplicação e a cobertura produzida pelos testes automatizados.

> **Escopo temporal:** os resultados abaixo correspondem à análise local executada em 01/09/2026, depois da implementação da aprovação/recusa externa, da outbox de notificações, dos links assinados no e-mail e da coleção de demonstração da Fase 2.

## Resultado da análise

O painel registrou aprovação no Quality Gate e apresentou os seguintes resultados para o código geral:

| Métrica | Resultado |
| --- | --- |
| Quality Gate | Aprovado (`Passed`) |
| Linhas de código analisadas | 11.053 |
| Problemas de segurança abertos | 0 |
| Security Hotspots | 0 — classificação A |
| Problemas de confiabilidade | 0 |
| Code Smells | 77 |
| Problemas aceitos | 1, restrito ao SMTP sem TLS do Mailpit local |
| Cobertura de testes | 80,2% |
| Duplicação | 2,9% |

![Evidência histórica do painel do SonarQube](assets/sonarQube.png)

O resultado atual atende à meta mínima de 80% de cobertura definida para os domínios críticos. Os 77 apontamentos de manutenibilidade não impediram a aprovação do Quality Gate e são, em sua maioria, recomendações incrementais sobre código preexistente.

O único apontamento de segurança foi classificado como **Accepted** no SonarQube. A regra `S5332` recomenda TLS para SMTP, mas o Mailpit da demonstração inicia deliberadamente sem criptografia dentro da rede local do Docker. A configuração padrão da aplicação mantém `UseSsl=true`; conexões sem TLS são bloqueadas, salvo quando `AllowInsecureConnection=true` é habilitado explicitamente no Docker Compose local. Ativar `EnableSsl` nessa porta faria o envio falhar por ausência de `STARTTLS`, mantendo a mensagem no outbox para nova tentativa.

> A ausência de problemas de segurança e hotspots no SonarQube representa o resultado da análise estática registrada nessa execução. Ela não garante, isoladamente, que a aplicação esteja livre de vulnerabilidades em dependências, containers, configuração ou ambiente de execução.

## Pré-requisitos

- Docker e Docker Compose;
- .NET SDK 10;
- ferramenta local `dotnet-sonarscanner`, declarada em `.config/dotnet-tools.json`;
- token de acesso criado no SonarQube local.

## Como executar a análise

### 1. Iniciar o SonarQube

Na raiz do repositório, execute:

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  up -d sonarqube
```

O painel ficará disponível em:

```text
http://localhost:9000
```

No primeiro acesso, utilize `admin`/`admin` e altere a senha quando solicitado.

### 2. Criar o token

No SonarQube, acesse:

```text
My Account > Security > Generate Tokens
```

Armazene o token somente na sessão do terminal. Não o adicione ao repositório:

```bash
export SONAR_TOKEN="seu-token"
```

### 3. Restaurar o scanner

```bash
dotnet tool restore
```

### 4. Iniciar a coleta

```bash
dotnet tool run dotnet-sonarscanner -- begin \
  /k:"fiap-tech-challenge" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="$SONAR_TOKEN" \
  /d:sonar.cs.opencover.reportsPaths=".sonar/coverage/**/coverage.opencover.xml" \
  /d:sonar.coverage.exclusions="**/Migrations/**,**/Program.cs,**/obj/**,**/Seeding/**,**/Context/ApplicationDbContextFactory.cs" \
  /d:sonar.cpd.exclusions="**/Migrations/**,tests/**/*.cs"
```

As exclusões removem código gerado, migrations, bootstrap e dados de seeding das métricas de cobertura. Os testes e migrations também são desconsiderados na análise de duplicação.

### 5. Compilar a solução

```bash
dotnet build TechChallenge.slnx --no-incremental
```

### 6. Executar os testes com cobertura

Testes unitários:

```bash
dotnet test tests/TechChallenge.Tests/TechChallenge.Tests.csproj \
  --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../../.sonar/coverage/unit/
```

Testes de integração:

```bash
dotnet test tests/TechChallenge.IntegrationTests/TechChallenge.IntegrationTests.csproj \
  --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../../.sonar/coverage/integration/
```

### 7. Publicar o resultado

```bash
dotnet tool run dotnet-sonarscanner -- end \
  /d:sonar.token="$SONAR_TOKEN"
```

Após o processamento, consulte:

```text
http://localhost:9000/dashboard?id=fiap-tech-challenge
```

## Verificações complementares

Para verificar pacotes NuGet com vulnerabilidades conhecidas:

```bash
while IFS= read -r project_file; do
  dotnet list "$project_file" package --vulnerable --include-transitive
done < <(rg --files -g '*.csproj')
```

O comando é executado por projeto porque a solução também contém o projeto
especial `.dcproj` do Docker Compose, que não usa `PackageReference`. Na
validação de 01/09/2026, nenhum pacote vulnerável foi encontrado nas fontes
NuGet configuradas.

Para analisar a imagem da API com Trivy, primeiro construa os containers e depois execute:

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  build techchallenge.api

trivy image techchallengeapi
```

Essas verificações complementam o SonarQube ao analisar dependências e a imagem do container, superfícies que não são integralmente cobertas pela análise estática do código.

## Revalidação realizada para a Fase 2

Nesta execução foram validados:

1. build da solução sem erros ou avisos;
2. 123 testes unitários e 31 testes de integração aprovados;
3. cobertura de 80,2% e Quality Gate aprovado;
4. ausência de bugs, vulnerabilidades abertas e Security Hotspots;
5. ausência de vulnerabilidades conhecidas nos pacotes NuGet;
6. decisão documentada para o SMTP sem TLS usado somente pelo Mailpit local.

## Conclusão

A execução atual foi aprovada no Quality Gate, não deixou bugs, vulnerabilidades abertas nem Security Hotspots e atingiu 80,2% de cobertura. O único risco aceito está limitado ao servidor SMTP local de demonstração e protegido por configuração explícita. A captura existente permanece identificada como evidência histórica; os números atuais podem ser consultados no painel local enquanto o container do SonarQube estiver em execução.
