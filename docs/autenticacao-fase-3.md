# Autenticação — contrato da Fase 3

## Responsabilidades

A **Function Serverless** é o único emissor de access tokens em produção. Ela
valida credenciais, gera o JWT e o assina com uma chave privada RSA. A aplicação
principal no EKS funciona como **OAuth 2.0 Resource Server**: recebe o bearer
token, obtém chaves públicas por OIDC discovery/JWKS e autoriza as operações.

A chave privada RSA nunca é entregue à aplicação, incluída em imagem, ConfigMap,
Secret Kubernetes ou variável do GitHub Actions. Ela pertence ao repositório da
Function e deve ser protegida por AWS KMS ou AWS Secrets Manager.

## Discovery e JWKS

O emissor deve publicar os endpoints HTTPS padrão:

```text
GET {authority}/.well-known/openid-configuration
GET {jwks_uri}
```

O documento de discovery deve conter, no mínimo:

```json
{
  "issuer": "https://<api-id>.execute-api.<region>.amazonaws.com/auth",
  "jwks_uri": "https://<api-id>.execute-api.<region>.amazonaws.com/auth/.well-known/jwks.json",
  "id_token_signing_alg_values_supported": ["RS256"]
}
```

O JWKS contém apenas chaves públicas. Cada JWT deve possuir `kid`; esse valor
seleciona a chave correspondente e permite rotação sem indisponibilidade. Durante
a rotação, a Function deve publicar simultaneamente a chave atual e a anterior
até todos os tokens antigos expirarem.

## Contrato do access token

Algoritmo permitido em produção: **RS256**.

Header mínimo:

```json
{
  "alg": "RS256",
  "typ": "JWT",
  "kid": "<id-da-chave>"
}
```

Claims esperadas:

| Claim | Obrigatória | Contrato |
| --- | --- | --- |
| `iss` | Sim | Exatamente igual a `Jwt__Issuer` e ao `issuer` do discovery |
| `aud` | Sim | `techchallenge-api` (confirmar antes da integração final) |
| `sub` | Sim | GUID do usuário existente na aplicação |
| `name` | Sim | Login/nome exibível |
| `role` | Sim | `Administrador`, `Vendedor` ou `Mecanico` |
| `funcionarioId` | Não | GUID do funcionário vinculado |
| `jti` | Sim | Identificador único do token |
| `iat` | Sim | Instante de emissão |
| `nbf` | Recomendado | Início da validade |
| `exp` | Sim | Expiração curta do access token |

A API mantém `MapInboundClaims=false`; portanto os nomes devem ser enviados
exatamente como definidos. As policies existentes utilizam a claim `role`.

## Configuração da aplicação

Produção/EKS:

```text
Jwt__Authority=https://<api-id>.execute-api.<region>.amazonaws.com/auth
Jwt__Issuer=https://<api-id>.execute-api.<region>.amazonaws.com/auth
Jwt__Audience=techchallenge-api
Jwt__RequireHttpsMetadata=true
Jwt__EnableLocalIssuance=false
```

Se o discovery não estiver em `{Authority}/.well-known/openid-configuration`,
configure também `Jwt__MetadataAddress` com a URL HTTPS completa. A aplicação
aceita somente RS256 no modo externo e atualiza o metadata/JWKS quando recebe um
`kid` desconhecido.

Discovery, JWKS e chaves públicas não são segredos. Os pods precisam de saída
HTTPS e resolução DNS para acessar esses endpoints.

## Compatibilidade temporária local

Desenvolvimento e testes podem habilitar explicitamente o emissor legado:

```text
Jwt__EnableLocalIssuance=true
Jwt__LocalSigningKey=<chave local com pelo menos 32 caracteres>
```

Esse modo aceita somente HS256 e registra as rotas locais de login, refresh,
logout, troca de senha e gestão de refresh tokens. A inicialização bloqueia
`EnableLocalIssuance=true` em `Production`. No modo externo, somente `/api/v1/auth/me`
permanece registrado neste grupo; login e ciclo de credenciais pertencem à
Function Serverless.

## Assinatura dos links de orçamento

Os links de aprovação/recusa não são JWT bearer. Eles continuam usando HMAC com
uma chave independente:

```text
ApprovalLinks__SigningKey=<chave com pelo menos 32 caracteres>
```

Essa chave fica no Secret `techchallenge-api-secrets`. Ela não deve ser derivada,
reutilizada ou rotacionada junto com a chave privada RSA da Function.

## Pendências de integração

Antes de habilitar o ambiente de produção, confirmar com o repositório da
Function Serverless:

- URL definitiva do emissor (`Jwt__Authority` / `Jwt__Issuer`) publicada pelo
  API Gateway;
- URL do documento de discovery, caso não siga o padrão
  `{Authority}/.well-known/openid-configuration` (seria aplicada em
  `Jwt__MetadataAddress`);
- `aud` definitivo exigido pela Function (hoje usamos `techchallenge-api`);
- quem é dono dos fluxos de refresh, logout e troca de senha — Function ou API;
- base de persistência dos usuários (compartilhada com a API ou própria da
  Function) e estratégia de revogação de tokens (lista negra, `jti` curto,
  rotação de `kid`).
