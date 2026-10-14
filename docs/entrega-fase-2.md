# Documento de Entrega - Fase 2

> Fonte do PDF final da Fase 2, concluído e revisado para envio no portal do aluno.

## Grupo

| Participante | RM | GitHub |
| --- | --- | --- |
| Gabriel Teixeira | RM374752 | [@louistx](https://github.com/louistx) |
| Brunno de Oliveira | RM374818 | [@DevDoubleN](https://github.com/DevDoubleN) |
| Luís Henrique | RM374786 | [@Ace0777](https://github.com/Ace0777) |
| Caio Montilha | RM375494 | [@cmontilha](https://github.com/cmontilha) |
| Gustavo Keiji | RM374965 | [@GuKeiji](https://github.com/GuKeiji) |

## Links da entrega

| Item | Link ou estado |
| --- | --- |
| Repositório | [github.com/louistx/fiap-tech-challenge](https://github.com/louistx/fiap-tech-challenge) |
| Documentação | [Índice](README.md) |
| Arquitetura | [Arquitetura Proposta - Fase 2](arquitetura-fase-2.md) |
| APIs | Swagger/OpenAPI em runtime, com URLs e cenários documentados no repositório |
| Vídeo de até 15 minutos | [youtu.be/EOGnetwuzPE](https://youtu.be/EOGnetwuzPE?si=w7ww-NdiRvKyG6Y5) |
| PDF final | Gerado e revisado para envio no portal do aluno |
| Compartilhamento com `soat-architecture` | Confirmado com permissão de escrita |

## Desenho resumido da arquitetura

```mermaid
flowchart LR
    User[Cliente/Sistema externo] --> API[API .NET]
    API --> APP[Casos de uso]
    APP --> DOMAIN[Domínio]
    APP --> PORTS[Portas]
    DBADAPTER[EF Core/Repositórios] --> PORTS
    DBADAPTER --> PG[(PostgreSQL)]
    NOTIFY[Worker de notificações] --> PORTS
    PG --> NOTIFY
    NOTIFY --> SMTP[SMTP / Mailpit local]
    CICD[GitHub Actions] --> REG[GHCR]
    DEV[Terminal local] --> TF[Terraform local]
    DEV --> K8S[Kubernetes Docker Desktop]
    TF --> PG
    REG --> K8S
    K8S --> PG
```

A descrição completa de componentes, recursos e fluxo de deploy está em [Arquitetura Proposta - Fase 2](arquitetura-fase-2.md).

## Resumo da solução

A solução mantém o monólito modular da Fase 1 e separa API, aplicação, domínio, contratos e infraestrutura. PostgreSQL é usado para persistência; Docker e Docker Compose suportam o desenvolvimento local. A Fase 2 propõe execução em Kubernetes, escalabilidade via HPA, infraestrutura Terraform e entrega por GitHub Actions.

Em 01/09/2026, o build está sem avisos, com 123 testes unitários, 31 integrações e quatro testes Terraform aprovados. O webhook externo e a notificação por e-mail, incluindo os links assinados de decisão, foram validados no Docker Compose com PostgreSQL e Mailpit. A infraestrutura e o CD foram validados no cluster do Docker Desktop, que é o ambiente de execução deste trabalho acadêmico. O GitHub Actions publica a imagem validada no GHCR e promove sua tag no overlay; Terraform e Kustomize concluem a entrega localmente. O vídeo foi publicado, os links foram inseridos e o PDF final foi gerado e revisado. O guia local está em [Infraestrutura](../infra/README.md).

## Checklist antes de exportar o PDF

- [x] Todos os bloqueadores técnicos do [Checklist de Entregáveis](fase-2-entregaveis.md) foram resolvidos.
- [x] Build, testes unitários e testes de integração estão verdes na validação local.
- [x] Swagger/OpenAPI e os cenários HTTP das APIs foram documentados e validados localmente.
- [x] Kubernetes e HPA foram validados localmente, com escala de 1 para 3 e retorno a 1 réplica.
- [x] Terraform provisionou recursos e banco no cluster local existente, conforme o escopo documentado.
- [x] CI/CD executou build, testes e imagem, seguido da entrega no Kubernetes local.
- [x] O vídeo foi publicado e o link foi incluído no documento final.
- [x] O repositório foi compartilhado com `soat-architecture`.
- [x] O desenho final corresponde aos recursos utilizados.
- [x] O PDF foi revisado visualmente e todos os links foram testados.

O [Roteiro do Vídeo](roteiro-video-fase-2.md) foi utilizado para orientar a demonstração final.
