# Documentação

## Fase 3

- [Pendências e responsáveis por repositório](ToDo.md)
- [Monitoramento e observabilidade](monitoramento.md)
- [Contrato de autenticação (JWT da Lambda)](autenticacao-fase-3.md)
- [Demo e roteiro do vídeo](demo/fase-3/README.md)
- [Requisições para gerar dados no monitoramento](demo/fase-3/monitoramento.http)
- [Manifests Kubernetes da API](../k8s/README.md)
- Infraestrutura Kubernetes, GitOps e ADRs: [tech-challenge-k8s](https://github.com/tech-challenge-SOAT16/tech-challenge-k8s)
- Banco gerenciado, modelo de dados (ER) e justificativa: [tech-challenge-database](https://github.com/tech-challenge-SOAT16/tech-challenge-database)
- Function de autenticação por CPF: [tech-challenge-lambda](https://github.com/tech-challenge-SOAT16/tech-challenge-lambda)

## Fase 2 (histórico)

> A infraestrutura local da Fase 2 (Terraform em `infra/` e overlay
> `docker-local`) ficou no repositório legado
> [fiap-tech-challenge](https://github.com/louistx/fiap-tech-challenge). Os
> documentos abaixo descrevem aquela entrega.

- [Checklist de entregáveis e progresso](fase-2-entregaveis.md)
- [Arquitetura proposta, infraestrutura e fluxo de deploy](arquitetura-fase-2.md)
- [Terraform e execução no Kubernetes local](https://github.com/louistx/fiap-tech-challenge/blob/main/infra/README.md)
- [Evidências de validação da infraestrutura local](validacao-infra-local.md)
- [Auditoria do endpoint de Estoque, entidades e validações](auditoria-estoque.md)
- [Fonte do documento final de entrega](entrega-fase-2.md)
- [Roteiro do vídeo demonstrativo](roteiro-video-fase-2.md)

## Domínio e requisitos

- [Requisitos funcionais das fases 1 e 2](requisitos.md)
- [Event Storming](event-storming.md)
- [Domain-Driven Design (DDD)](ddd.md)

## Qualidade, segurança e demonstração

- [Testes automatizados e resultado da auditoria](testes.md)
- [Relatório de análise de vulnerabilidades](relatorio-vulnerabilidades.md)
- [Demo executável da Fase 2](demo/fase-2/README.md)

## Estado da revisão

Documentação atualizada em 24/10/2026 para a Fase 3. Monitoramento validado
localmente (Docker Compose e kind). Sem acesso à AWS até o momento: nada foi
aplicado no EKS/RDS reais; as pipelines de infraestrutura rodam a validação com
credenciais provisórias.
