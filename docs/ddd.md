# Domain-Driven Design (DDD)

## Visão geral

O domínio deste projeto representa o funcionamento de uma oficina mecânica. O fluxo principal começa com a solicitação de um orçamento pelo cliente e acompanha todo o ciclo de vida de uma Ordem de Serviço (OS), desde a sua criação até a finalização.

Na Fase 2, o código passou por uma refatoração para encapsular acessores das entidades e separar catálogo, categorias e saldo de estoque. As falhas bloqueantes encontradas em 25/08/2026 foram corrigidas e a validação de 26/08/2026 terminou com build sem avisos e todas as suítes verdes.

![Visualização DDD do projeto](assets/DDD_fluxo.png)

## Linguagem do domínio

Os principais termos utilizados no projeto são:

- **Cliente:** proprietário ou responsável pelo veículo e pela aprovação do orçamento.
- **Veículo:** automóvel vinculado ao atendimento realizado pela oficina.
- **Orçamento:** estimativa dos serviços e produtos necessários para o atendimento.
- **Ordem de Serviço (OS):** registro central que acompanha o atendimento do veículo.
- **Administrador:** responsável pelos cadastros, abertura e encerramento administrativo da OS.
- **Vendedor:** também pode abrir uma OS, conforme definido nos requisitos funcionais.
- **Mecânico:** responsável pelo diagnóstico, inclusão dos serviços e produtos, execução e conclusão técnica do trabalho.
- **Serviço:** atividade executada pelo mecânico, como revisão, troca ou reparo.
- **Produto:** peça ou item de inventário utilizado na execução do serviço.
- **Estoque:** saldo disponível de um produto, separado de sua descrição, preço e categoria.
- **Categoria de produto:** classificação do catálogo de peças e insumos.
- **Categoria de serviço:** classificação dos serviços oferecidos.
- **Categoria de veículo:** classificação dos tipos de veículo cadastrados.
- **Diagnóstico:** identificação dos serviços e produtos necessários para resolver o problema relatado.
- **Código de acompanhamento:** identificador público usado pelo cliente para consultar o andamento da OS.
- **Notificação interna:** comunicação simulada por log para funcionários e perfis envolvidos no fluxo.

## Agregado Ordem de Serviço

A `OrdemServico` é o principal candidato a **raiz de agregado** do domínio. As operações do fluxo devem acontecer por meio dela, garantindo que as regras de negócio sejam respeitadas em cada mudança de estado.

Atualmente, a entidade relaciona:

- um cliente responsável;
- um veículo;
- um funcionário responsável;
- uma coleção de serviços;
- uma coleção de produtos;
- uma descrição ou relato inicial;
- um código único de acompanhamento;
- o estado atual e as transições permitidas;
- valor, desconto e acréscimo;
- data de criação;
- data de atualização;
- data de finalização.

As entidades `Cliente`, `Veiculo`, `Funcionario`, `Servico`, `Produto` e `Estoque` possuem identidade própria. Por isso, são referenciadas por identificadores. O saldo usa `ProdutoId` como vínculo de negócio e deve possuir uma única linha por produto.

## Evolução do domínio na Fase 2

A refatoração substituiu setters públicos por `private set` e adicionou construtores/métodos de alteração. O Estoque agora protege suas invariantes; permanecem oportunidades de aprofundar o modelo:

- `Estoque` aceita somente entradas/baixas positivas e nunca permite saldo negativo;
- `Produto` deve validar descrição, valor e categoria;
- `OrdemServico` deve expor operações de negócio em vez de alterações genéricas de valor/data;
- quantidades de peças usam `int` entre request, domínio e persistência;
- atualização de saldo e transição da OS devem ocorrer na mesma unidade transacional.

A construção de `Estoque(Guid id, Guid produtoId, int quantidade)` está alinhada à propriedade `ProdutoId`, possui construtor privado para o EF Core e inicializa o token de concorrência `Versao`.

## Fluxo do orçamento

Antes da abertura da Ordem de Serviço, o diagrama apresenta a solicitação inicial:

1. O **cliente solicita um orçamento** para a oficina.
2. A solicitação é encaminhada ao **administrador**.
3. A partir das informações do cliente e do veículo, o administrador inicia o atendimento e cria a Ordem de Serviço.

No código atual, o orçamento é representado pela composição de serviços e produtos da própria OS. Quantidades e preços são copiados para os itens da OS, e o valor total é calculado no envio para aprovação. Ainda não existe uma entidade `Orcamento` independente.

## Fluxo de execução da Ordem de Serviço

### 1. Criação da OS

O administrador ou vendedor cria uma OS informando o cliente responsável, o veículo, o funcionário responsável, o relato inicial e, opcionalmente, serviços e produtos. A OS recebe um código único de acompanhamento, é persistida diretamente no estado **Recebida** e gera uma notificação interna aos mecânicos.

**Comando de domínio:** `CriarOrdemServico`  
**Estado resultante:** `Recebida`

### 2. Diagnóstico e inclusão de itens

O mecânico assume a OS e registra o diagnóstico, adicionando os serviços e produtos necessários. Pelos requisitos, o mecânico somente pode assumir uma OS que esteja com estado **Recebida** e não pode possuir outra OS em andamento.

Ao ser atribuída ao mecânico, a OS deve passar pelo estado **Em diagnóstico**.

**Comandos de domínio:**

- `AtribuirOrdemServico`;
- `RegistrarDiagnostico`;
- `AdicionarServico`;
- `AdicionarProduto`.

**Eventos esperados:**

- `OrdemServicoAtribuida`;
- `DiagnosticoRegistrado`;
- `ItemAdicionadoAoOrcamento`.

As quantidades e os preços atuais de serviços e produtos são copiados para os itens associados à OS. A disponibilidade é consultada na entidade separada `Estoque`; quando falta saldo, o sistema registra notificações por logger para administradores e para o mecânico responsável. Persistência e testes desses fluxos estão verdes.

### 3. Envio para aprovação

Depois do diagnóstico, o sistema soma serviços e produtos multiplicando valores pelas quantidades e considerando descontos e acréscimos dos itens e da OS. Havendo ao menos um item e estoque suficiente, a OS passa para **Aguardando aprovação**. A mudança gera a notificação interna e uma mensagem de outbox para envio por e-mail.

**Comando de domínio:** `EnviarOrcamentoParaAprovacao`  
**Eventos esperados:**

- `OrcamentoEnviado`;
- `OrdemServicoAguardandoAprovacao`;
- `ClienteNotificado`.

### 4. Decisão do orçamento

O orçamento pode ser aprovado ou reprovado. A aprovação move a OS diretamente de **Aguardando aprovação** para **Em execução**; não há um estado intermediário `Aprovada`. A reprovação move a OS para **Reprovada**, de onde ela pode retornar para **Em diagnóstico** para revisão.

A decisão também pode chegar pelo webhook externo. O agregado registra o
identificador do evento e seu conteúdo; uma repetição idêntica não provoca nova
transição, enquanto a reutilização divergente retorna conflito.

**Comandos de aplicação:**

- `AprovarOrcamento`;
- `ReprovarOrcamento`;
- `RetornarParaDiagnostico`.

O fluxo de aprovação parcial e negociação ainda precisa ser definido.

### 5. Execução

Com a aprovação, a OS já entra em **Em execução**. O mecânico executa o trabalho e, ao concluí-lo, utiliza a operação de finalização.

### 6. Finalização técnica

Ao terminar os serviços, o mecânico finaliza a OS. O sistema valida o estoque novamente, baixa as quantidades dos produtos consumidos e realiza a transição de **Em execução** para **Finalizada**, registrando a data de finalização.

**Comando de aplicação:** `FinalizarOS`

### 7. Entrega e cancelamento

Depois da finalização técnica, o administrador ou vendedor registra a entrega do veículo, movendo a OS para **Entregue**. Um administrador também pode cancelar uma OS nos estados `Recebida`, `EmDiagnostico`, `AguardandoAprovacao`, `Reprovada` ou `EmExecucao`.

**Comandos de aplicação:** `EntregarOS` e `CancelarOS`

## Ciclo de estados

O fluxo descrito pelo diagrama pode ser representado pela seguinte sequência:

```text
Recebida
  ↓
Em diagnóstico
  ↓
Aguardando aprovação
  ↓
Em execução
  ↓
Finalizada
  ↓
Entregue
```

O caminho alternativo de revisão do orçamento é `Aguardando aprovação → Reprovada → Em diagnóstico`. O cancelamento é terminal e pode ocorrer antes da finalização. As transições são controladas pela própria Ordem de Serviço. Por exemplo:

- somente uma OS `Recebida` pode ser atribuída a um mecânico;
- somente uma OS `Em diagnóstico` pode receber o diagnóstico;
- somente uma OS `Em diagnóstico` e com ao menos um item pode aguardar aprovação;
- somente uma OS `Aguardando aprovação` pode entrar em execução ou ser reprovada;
- somente uma OS `Em execução` pode ser finalizada;
- somente uma OS `Finalizada` pode ser entregue.

Cada transição executada pelos casos de uso gera uma notificação interna por log e uma mensagem persistida na outbox. O worker envia a mensagem por SMTP e aplica retentativa em caso de falha. O código de acompanhamento permite consultar publicamente o estado atual e os itens da OS, enquanto a métrica administrativa calcula o tempo médio entre criação e finalização.

## Contextos do domínio

Com base no desenho e nos requisitos atuais, o domínio pode ser dividido nos seguintes contextos:

- **Atendimento:** clientes, veículos, solicitação de orçamento e abertura da OS.
- **Execução da oficina:** atribuição do mecânico, diagnóstico, execução e conclusão técnica.
- **Catálogo e inventário:** serviços oferecidos, produtos e disponibilidade em estoque.
- **Aprovação:** apresentação do orçamento e decisão do cliente.
- **Notificações:** comunicação com cliente, mecânico e administrador.

Essa separação é uma proposta inicial. Os limites devem ser refinados conforme as regras do negócio forem implementadas.

## Relação entre o desenho e o código atual

| Parte do domínio | Situação atual |
| --- | --- |
| Entidades `Cliente`, `Veiculo`, `Funcionario`, `Servico`, `Produto`, `Estoque` e `OrdemServico` | Modeladas e materializadas pelo EF Core |
| Categorias de produto, serviço e veículo | Modeladas com endpoints, casos de uso, migration e exercícios de integração |
| Relacionamentos da OS | Configurados com Entity Framework |
| Criação, consulta, listagem e exclusão da OS | Integradas aos casos de uso e à persistência |
| Atribuição da OS ao mecânico | Implementada, incluindo limite de uma OS ativa por mecânico |
| Registro do diagnóstico | Implementado com associação de serviços e produtos |
| Listagem de OS para a oficina | Implementada para OS em diagnóstico |
| Persistência da OS | Implementada com Entity Framework e PostgreSQL |
| Estados da OS | Máquina de estados presente e suíte verde |
| Orçamento e decisão do cliente | Cálculo, envio, aprovação, reprovação e revisão implementados |
| Verificação de estoque | Entidade/repositório próprios, rotas consistentes, saldo protegido e concorrência otimista |
| Quantidades dos itens | Modeladas e consideradas no cálculo, validação e baixa de estoque |
| Notificações | Logs internos e e-mail de mudança de status com outbox, SMTP e retentativa |
| Acompanhamento público | Implementado por código único da OS |
| Tempo médio de execução | Implementado para OS finalizadas ou entregues |
| Finalização, entrega e cancelamento da OS | Implementados |

Portanto, o diagrama documenta o **fluxo de negócio desejado** e o ciclo principal está operacionalmente validado por 123 testes unitários e 31 de integração. Permanecem como evoluções a reserva antecipada de estoque, pagamento, recibo, aprovação parcial e retrabalho.

Consulte também a [Auditoria do Estoque](auditoria-estoque.md) e o [Checklist de Entregáveis da Fase 2](fase-2-entregaveis.md).
