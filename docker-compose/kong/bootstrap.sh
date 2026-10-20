#!/bin/sh
set -eu

admin_url="${KONG_ADMIN_URL:-http://kong:8001}"
api_url="http://techchallenge.api:8080"

until curl --silent --fail "${admin_url}/status" >/dev/null; do
  sleep 2
done

curl --silent --show-error --fail --request PUT "${admin_url}/services/techchallenge-api" \
  --data "name=techchallenge-api" \
  --data "url=${api_url}" >/dev/null

create_route() {
  name="$1"
  path="$2"

  curl --silent --show-error --fail --request PUT "${admin_url}/routes/${name}" \
    --data "name=${name}" \
    --data "service.name=techchallenge-api" \
    --data "paths[]=${path}" \
    --data "protocols[]=http" \
    --data "protocols[]=https" \
    --data "strip_path=false" >/dev/null
}

# As rotas específicas precisam ser avaliadas antes da rota geral protegida.
create_route "techchallenge-public-acompanhamento" "/api/v1/ordens-servico/acompanhamento"
create_route "techchallenge-public-integracao" "/api/v1/integracoes/orcamentos/respostas"
create_route "techchallenge-public-decisao-orcamento" "/orcamentos/decisao"
create_route "techchallenge-api-v1" "/api/v1"
create_route "techchallenge-openapi" "/openapi"
create_route "techchallenge-swagger" "/swagger"

if [ -n "${CPF_AUTH_UPSTREAM_URL:-}" ]; then
  curl --silent --show-error --fail --request PUT "${admin_url}/services/cpf-auth-lambda" \
    --data "name=cpf-auth-lambda" \
    --data "url=${CPF_AUTH_UPSTREAM_URL}" >/dev/null

  curl --silent --show-error --fail --request PUT "${admin_url}/routes/cpf-auth" \
    --data "name=cpf-auth" \
    --data "service.name=cpf-auth-lambda" \
    --data "paths[]=/auth/cpf" \
    --data "methods[]=POST" \
    --data "protocols[]=http" \
    --data "protocols[]=https" \
    --data "strip_path=false" >/dev/null

  plugins=$(curl --silent "${admin_url}/plugins?instance_name=cpf-auth-rate-limit")
  case "$plugins" in
    *'"instance_name":"cpf-auth-rate-limit"'*) ;;
    *)
      curl --silent --show-error --fail --request POST "${admin_url}/services/cpf-auth-lambda/plugins" \
        --data "name=rate-limiting" \
        --data "instance_name=cpf-auth-rate-limit" \
        --data "config.policy=local" \
        --data "config.limit_by=ip" \
        --data "config.minute=10" \
        --data "config.error_code=429" \
        --data "config.error_message=Limite de tentativas de autenticacao excedido." >/dev/null
      ;;
  esac
fi

plugins=$(curl --silent "${admin_url}/plugins?instance_name=techchallenge-rate-limit")
case "$plugins" in
  *'"instance_name":"techchallenge-rate-limit"'*) ;;
  *)
  curl --silent --show-error --fail --request POST "${admin_url}/services/techchallenge-api/plugins" \
    --data "name=rate-limiting" \
    --data "instance_name=techchallenge-rate-limit" \
    --data "config.policy=local" \
    --data "config.limit_by=ip" \
    --data "config.minute=60" \
    --data "config.error_code=429" \
    --data "config.error_message=Limite de requisicoes por minuto excedido." >/dev/null
    ;;
esac

if [ -n "${JWT_ISSUER:-}" ] && [ -n "${JWT_PUBLIC_KEY_BASE64:-}" ]; then
  public_key=$(printf '%s' "$JWT_PUBLIC_KEY_BASE64" | base64 -d)

  curl --silent --show-error --fail --request PUT "${admin_url}/consumers/cpf-auth-lambda" \
    --data "username=cpf-auth-lambda" >/dev/null

  credentials=$(curl --silent "${admin_url}/consumers/cpf-auth-lambda/jwt")
  case "$credentials" in
    *"${JWT_ISSUER}"*) ;;
    *)
    curl --silent --show-error --fail --request POST "${admin_url}/consumers/cpf-auth-lambda/jwt" \
      --data "key=${JWT_ISSUER}" \
      --data "algorithm=RS256" \
      --data-urlencode "rsa_public_key=${public_key}" >/dev/null
      ;;
  esac

  plugins=$(curl --silent "${admin_url}/plugins?instance_name=techchallenge-jwt")
  case "$plugins" in
    *'"instance_name":"techchallenge-jwt"'*) ;;
    *)
    curl --silent --show-error --fail --request POST "${admin_url}/routes/techchallenge-api-v1/plugins" \
      --data "name=jwt" \
      --data "instance_name=techchallenge-jwt" \
      --data "config.claims_to_verify[]=exp" >/dev/null
      ;;
  esac
fi

echo "Kong configurado. Proxy: http://localhost:8000 | Manager: http://localhost:8002"
