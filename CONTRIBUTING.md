# Contribuindo

Obrigado por considerar contribuir com este projeto.

## Como propor uma mudança
1. Abra uma issue descrevendo o problema ou a melhoria antes de implementar mudanças maiores.
2. Crie uma branch a partir de main com um nome descritivo.
3. Inclua testes para o comportamento novo ou alterado (dotnet test tests/Payments.Tests).
4. Abra um Pull Request explicando o que mudou e por quê.
## Decisões de arquitetura
Mudanças que alterem contratos, fluxos de estado do pagamento ou trade-offs relevantes devem ser registradas como ADR em docs/adr/.

## Padrão de código
Siga o estilo já usado no projeto (C# 12, Minimal API) e mantenha a separação entre Payments.Domain e Payments.Api.
