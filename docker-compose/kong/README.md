# Kong Gateway local

O Kong é o único ponto de entrada local da API. O Kong Manager fica em
`http://localhost:8002`; a Admin API em `http://localhost:8001` é acessível
somente pela própria máquina; e o proxy público local usa
`http://localhost:8000`.

## Subir

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  --profile gateway up --build
```

O serviço `kong-config` configura o serviço upstream `techchallenge-api`, as
rotas e o rate limiting de 60 requisições por minuto e IP. O estado pode ser
visualizado e administrado pelo Kong Manager.

| URL no proxy | Upstream | JWT no Kong |
| --- | --- | --- |
| `POST /auth/cpf` | Lambda de autenticação | Não; retorna o JWT |
| `/api/v1/**` | API da oficina | Sim, quando configurado abaixo |
| `/api/v1/ordens-servico/acompanhamento/**` | API da oficina | Não |
| `/api/v1/integracoes/orcamentos/respostas` | API da oficina | Não; exige `X-Integration-Key` na API |
| `/orcamentos/decisao` | API da oficina | Não; usa o token assinado do link |
| `/swagger/**`, `/openapi/**` | API da oficina | Não, apenas desenvolvimento |

## JWT emitido pela Lambda

A Lambda precisa assinar tokens com `RS256`, definir `iss` igual a
`JWT_ISSUER` e fornecer sua chave pública. Antes de subir os containers, defina:

```bash
export CPF_AUTH_UPSTREAM_URL='https://id-da-api.execute-api.us-east-1.amazonaws.com'
export JWT_ISSUER='https://auth.exemplo.com'
export JWT_PUBLIC_KEY_BASE64="$(base64 < lambda-public.pem | tr -d '\n')"
```

O endpoint configurado em `CPF_AUTH_UPSTREAM_URL` deve receber
`POST /auth/cpf`. O Kong encaminha essa rota sem JWT e limita-a a dez tentativas
por minuto e IP.

O `kong-config` cria o Consumer `cpf-auth-lambda`, associa sua chave pública e
habilita o plugin JWT exclusivamente na rota geral `/api/v1`. A API continua
validando issuer, audience, assinatura e expiração por OIDC/JWKS.

O plugin JWT do Kong identifica o emissor pela claim `iss`; portanto o valor de
`JWT_ISSUER` e a claim emitida pela Lambda precisam ser idênticos. O Kong
valida assinatura e expiração; a API mantém a validação de `aud`.

## Verificação

```bash
curl -i http://localhost:8000/swagger/index.html
curl -i http://localhost:8000/api/v1/clientes
curl -i http://localhost:8001/services/techchallenge-api
```

Sem uma configuração JWT, a segunda chamada chega à API e recebe `401` dela.
Com a chave pública configurada, o Kong rejeita token ausente ou inválido antes
de encaminhar a requisição.

## Produção no EKS

Este compose existe para desenvolvimento e demonstração. No repositório de
infraestrutura Kubernetes, provisionar Kong via Terraform/Helm, manter a Admin
API privada, publicar somente o proxy com TLS e repetir esta configuração como
código. O banco usado por Kong deve ser separado do PostgreSQL de negócio.
