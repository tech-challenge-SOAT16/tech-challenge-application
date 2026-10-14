# Documentação

## Fase 2

- [Checklist de entregáveis e progresso](fase-2-entregaveis.md)
- [Arquitetura proposta, infraestrutura e fluxo de deploy](arquitetura-fase-2.md)
- [Terraform e execução no Kubernetes local](../infra/README.md)
- [Organização dos manifests e migrations](../k8s/README.md)
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

A documentação funcional foi revisada em 01/09/2026. Terraform e o deploy Kubernetes foram implementados e validados no cluster do Docker Desktop, que é o ambiente de entrega do trabalho. O GitHub Actions publica a imagem no GHCR, promove sua tag no overlay e a etapa local de CD usa Terraform e Kustomize.
