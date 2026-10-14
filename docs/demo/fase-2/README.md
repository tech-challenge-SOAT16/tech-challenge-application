# Demo executável da Fase 2

Esta coleção exercita o fluxo principal da oficina e as duas integrações
externas exigidas na Fase 2: decisão de orçamento por webhook e notificação do
cliente por e-mail.

## Preparação

Na raiz do repositório, execute:

```bash
docker compose \
  -f docker-compose/docker-compose.yml \
  -f docker-compose/docker-compose.override.yml \
  up -d --build db mailpit techchallenge.api
```

Serviços da demonstração:

| Serviço | Endereço |
| --- | --- |
| API e Swagger | `http://localhost:8080` |
| Mailpit | `http://localhost:8025` |
| PostgreSQL | `localhost:5432` |

Os dados fictícios são criados pelo seed de desenvolvimento. Todos os usuários
abaixo usam a senha `Demo@123`:

| Perfil | Login |
| --- | --- |
| Administrador | `admin.demo` |
| Vendedor | `vendedor.demo` |
| Mecânico | `mecanico.demo` |

A chave local padrão do webhook é `demo-integration-key-change-me`. Ela existe
somente para a demonstração e deve ser sobrescrita pela variável
`EXTERNAL_INTEGRATION_API_KEY` fora deste ambiente.

## Ordem sugerida

1. Execute `00-auth.http` para salvar os tokens.
2. Execute `01-aprovacao-por-email.http` até o envio do orçamento.
3. Abra o Mailpit, pressione **Aprovar orçamento** e confirme na página aberta.
4. Termine `01-aprovacao-por-email.http` para finalizar e entregar a OS.
5. Use `01-aprovacao-externa.http` se quiser mostrar também a aprovação pelo webhook.
6. Execute `02-recusa-externa.http` para demonstrar a recusa externa e o conflito idempotente.
7. Use `03-metricas-e-acompanhamento.http` para fechar a demonstração.

Os arquivos usam scripts compatíveis com o cliente HTTP da JetBrains para
guardar tokens, IDs e identificadores de evento. Em outro cliente, copie os
valores retornados para as variáveis no topo de cada arquivo.

## O que destacar no vídeo

- o webhook não usa o JWT interno; ele exige `X-Integration-Key`;
- o e-mail de orçamento possui links assinados, válidos por 48 horas, sem expor
  a API key do integrador;
- o e-mail apresenta os serviços, produtos e valor total antes da decisão;
- o primeiro clique abre uma confirmação e somente o `POST` altera a OS;
- repetir o mesmo evento retorna sucesso com `duplicado: true` e não muda a OS;
- reutilizar o identificador com outro conteúdo retorna `409 Conflict`;
- a decisão, a mudança da OS e a mensagem de outbox são persistidas juntas;
- o worker envia o e-mail pelo SMTP local e registra a entrega na outbox;
- se o SMTP estiver indisponível, a mensagem permanece pendente e recebe nova
  tentativa com espera progressiva.
