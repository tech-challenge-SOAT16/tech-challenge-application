# Requisitos Funcionais - Fases 1 e 2

Este documento reúne as regras funcionais da Fase 1 e as evoluções obrigatórias da Fase 2. Para o estado confirmado de cada item, consulte o [Checklist de Entregáveis](fase-2-entregaveis.md).

## RF01 - Cadastro do cliente
- O sistema deve permitir que administradores e vendedores cadastrem, consultem, atualizem e excluam clientes.
- O cadastro deve conter nome, e-mail, tipo de documento, documento e endereço.
- O tipo de documento deve aceitar CPF, CNPJ ou RG.
- O documento deve ser validado, normalizado e único conforme o tipo informado.

## RF02 - Cadastro do veículo
- O sistema deve permitir que administradores e vendedores cadastrem, consultem, atualizem e excluam veículos.
- O cadastro deve conter tipo, placa, modelo, marca, cor, ano, quilometragem, valor e cliente responsável.
- Os tipos aceitos são carro, moto e caminhão.
- A placa deve seguir o padrão antigo ou Mercosul, ser normalizada e única.

## RF03 - Cadastro de funcionários
- O sistema deve permitir que o administrador cadastre, consulte, atualize e exclua funcionários.
- O cadastro deve conter nome, CPF, RG, endereço e cargo.
- Os cargos aceitos são vendedor, mecânico e administrador.

## RF04 - Cadastro de OS
- O sistema deve permitir que administradores e vendedores criem uma OS.
- A OS deve conter veículo, cliente responsável, funcionário responsável e relato inicial.
- A OS deve ser criada diretamente com o estado `Recebida`.

## RF05 - Cadastro de inventário
- O sistema deve permitir que administradores e vendedores cadastrem, consultem, atualizem e excluam produtos do inventário.
- O produto deve conter descrição, valor e quantidade disponível.

## RF06 - Cadastro de serviços
- O sistema deve permitir que administradores e vendedores cadastrem, consultem, atualizem e excluam os serviços oferecidos pela oficina.

## RF07 - Listagem de inventário
- O sistema deve permitir que usuários autenticados listem e consultem produtos do inventário.

## RF08 - Listagem de OS
- O sistema deve permitir que administradores e vendedores listem todas as OS.
- A listagem deve aceitar filtro opcional por estado.
- O sistema deve permitir que administradores, vendedores e mecânicos consultem uma OS pelo identificador.

## RF09 - Listagem de OS para a oficina
- O sistema deve fornecer a listagem das OS em diagnóstico para exibição na oficina.
- A resposta deve conter o identificador da OS, a placa do veículo, o mecânico atribuído e o relato inicial.

## RF10 - Atribuição de OS
- O sistema deve permitir que um mecânico assuma uma OS no estado `Recebida`.
- O mecânico pode ter apenas uma OS ativa por vez.
- Ao ser atribuída, a OS deve passar para `EmDiagnostico`.

## RF11 - Registro de diagnóstico
- O mecânico deve poder associar serviços e produtos a uma OS em diagnóstico.
- Cada item deve registrar quantidade e o preço vigente do serviço ou produto.
- O sistema deve impedir a inclusão de produtos sem quantidade suficiente em estoque.

## RF12 - Envio do orçamento
- Administradores, vendedores e mecânicos devem poder enviar o orçamento de uma OS em diagnóstico.
- A OS deve possuir ao menos um serviço ou produto.
- O sistema deve calcular o total considerando valores, descontos e acréscimos.
- Após o envio, a OS deve passar para `AguardandoAprovacao`.

## RF13 - Decisão do orçamento
- Administradores e vendedores devem poder aprovar ou reprovar o orçamento.
- A aprovação deve mover a OS de `AguardandoAprovacao` para `EmExecucao`.
- A reprovação deve mover a OS para `Reprovada`.
- Uma OS reprovada deve poder retornar para `EmDiagnostico` para revisão.

## RF14 - Finalização e entrega
- O mecânico deve poder mover uma OS de `EmExecucao` para `Finalizada`.
- A finalização deve registrar a data correspondente.
- Administradores e vendedores devem poder registrar a entrega, movendo a OS para `Entregue`.

## RF15 - Cancelamento
- O administrador deve poder cancelar uma OS ainda não finalizada ou entregue.
- O estado `Cancelada` deve ser terminal.

## RF16 - Verificação de estoque
- Antes do envio para aprovação, o sistema deve verificar se os produtos necessários estão disponíveis.
- Quando não houver estoque, o sistema deve notificar o administrador e o mecânico.
- A finalização deve validar novamente a disponibilidade e realizar a baixa das quantidades consumidas.
- A reserva antecipada de estoque ainda não está implementada.

## RF17 - Autenticação e autorização
- O sistema deve autenticar usuários com JWT e permitir renovação por refresh token rotativo.
- O sistema deve permitir troca de senha, logout e revogação de refresh tokens.
- O acesso às operações deve respeitar os perfis administrador, vendedor e mecânico.
- Por padrão, toda rota deve exigir autenticação, exceto as explicitamente públicas.

## RF18 - Acompanhamento público da OS
- O sistema deve gerar um código de acompanhamento único ao criar a OS.
- O cliente deve poder consultar o andamento pelo código sem autenticação.

## RF19 - Tempo médio de execução
- Administradores e vendedores devem poder consultar a quantidade de OS finalizadas e o tempo médio de execução em minutos e horas.
- O cálculo deve considerar o intervalo entre criação e finalização das OS finalizadas ou entregues.

## RF20 - Notificações internas
- O sistema deve notificar os mecânicos quando uma nova OS entrar na fila.
- Mudanças de estado devem gerar notificação para o funcionário responsável ou para administradores quando não houver responsável.
- A falta de estoque deve notificar administradores e o mecânico responsável.
- As notificações internas continuam registradas em log e as mudanças de status
  do cliente são enviadas por SMTP através da outbox.

## Evoluções obrigatórias da Fase 2

### RF21 - Abertura completa da OS

- A abertura deve receber os dados do cliente, veículo, serviços e peças necessários.
- A resposta deve conter a identificação única da OS.
- O contrato deve definir se os dados serão criados em cascata ou referenciados por IDs existentes.
- **Situação atual:** implementada; cliente e veículo são referenciados por IDs e serviços/produtos podem ser enviados na abertura.

### RF22 - Consulta exclusiva do status

- Deve existir uma rota não ambígua que retorne a situação atual da OS.
- Os estados exigidos são `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `EmExecucao`, `Finalizada` e `Entregue`.
- **Situação atual:** implementada em `GET /ordens-servico/{id}/status` e exercitada no fluxo HTTP principal.

### RF23 - Decisão externa do orçamento

- A API deve receber de um sistema externo a aprovação ou recusa do cliente.
- O contrato deve possuir autenticação do integrador, idempotência, correlação e tratamento de repetição.
- **Situação atual:** implementada em `POST /api/v1/integracoes/orcamentos/respostas`, com API key, identificador global do evento, auditoria, repetição idempotente e conflito para conteúdo divergente.

### RF24 - Priorização da listagem de OS

- A listagem operacional deve priorizar `EmExecucao`, depois `AguardandoAprovacao`, `EmDiagnostico` e `Recebida`.
- Dentro de cada prioridade, as OS mais antigas devem aparecer primeiro.
- Finalizadas e entregues não devem aparecer nessa listagem, sem exclusão física dos registros.
- O tratamento de `Reprovada` e `Cancelada` deve ser definido explicitamente.
- **Situação atual:** implementada e validada; reprovadas, canceladas, finalizadas e entregues não entram na fila operacional.

### RF25 - Notificação de mudança de status

- Mudanças de estado devem ser enviadas por e-mail ou ferramenta equivalente.
- Falhas de envio não podem corromper a transação da OS; recomenda-se fila/outbox e retentativa.
- **Situação atual:** implementada por SMTP com outbox PostgreSQL, reserva concorrente e retentativa progressiva. O ambiente local utiliza Mailpit. O e-mail apresenta os itens e o total do orçamento e inclui links assinados e temporários para aprovação ou recusa, com confirmação antes da mudança de estado.

### RF26 - Movimentação de estoque

- O saldo deve ser separado do catálogo do produto.
- Entradas e baixas devem validar produto, quantidade e saldo.
- A quantidade nunca pode ficar negativa.
- Deve existir uma única posição de estoque por produto e controle de concorrência.
- A finalização da OS deve baixar os produtos exatamente uma vez.
- **Situação atual:** implementada com migration, índice único, saldo não negativo, concorrência otimista e testes verdes.

## Requisitos não funcionais da Fase 2

- arquitetura em camadas com dependências orientadas ao domínio;
- build e execução reproduzíveis por Docker;
- Deployment, Service, ConfigMap e HPA em `/k8s`, com migrations executadas pela API e Secrets fornecidos pelo Terraform;
- recursos e banco provisionados por Terraform em `/infra`, usando o cluster existente do Docker Desktop no escopo local;
- fluxo de CI/CD com build, testes e imagem no GitHub Actions, seguido de infraestrutura e deploy no Kubernetes local;
- documentação da arquitetura, execução local, Kubernetes, Terraform, APIs e vídeo.

A implementação detalhada e as lacunas estão no [Checklist de Entregáveis da Fase 2](fase-2-entregaveis.md).
