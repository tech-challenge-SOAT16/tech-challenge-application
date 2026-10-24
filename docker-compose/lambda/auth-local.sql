-- Estruturas da autenticação de clientes no banco local da aplicação, para a
-- demo com a Lambda. Na AWS, o tech-challenge-database cria as mesmas estruturas
-- (scripts 002-auth-state.sql e 004-auth-clients.sql). Idempotente.

-- Estado da Lambda (tech-challenge-lambda/infra/002-auth-state.sql).
CREATE TABLE IF NOT EXISTS auth_challenges (
 id varchar(36) PRIMARY KEY,
 client_id varchar(36) NOT NULL,
 verifier varchar(64) NOT NULL,
 expires_at bigint NOT NULL,
 attempts integer NOT NULL DEFAULT 0,
 consumed boolean NOT NULL DEFAULT false
);
CREATE INDEX IF NOT EXISTS ix_auth_challenges_expires_at ON auth_challenges(expires_at);
CREATE TABLE IF NOT EXISTS auth_rate_limits (
 key varchar(64) NOT NULL,
 "window" bigint NOT NULL,
 count integer NOT NULL,
 PRIMARY KEY (key, "window")
);

-- Clientes com CPF que podem fazer login (id = Cliente.Id = sub do token).
CREATE OR REPLACE VIEW auth_clients AS
SELECT
  c."Id"::varchar(36)                          AS id,
  regexp_replace(c."Documento", '\D', '', 'g') AS cpf,
  true                                         AS active,
  c."Email"                                    AS email
FROM "Cliente" c
WHERE c."TipoDocumento" = 0
  AND length(regexp_replace(c."Documento", '\D', '', 'g')) = 11;
