# ADR 0001: Idempotência via chave fornecida pelo cliente

## Contexto
Chamadas de autorização de pagamento podem ser reenviadas pelo cliente (timeout, retry de rede, dupla submissão). Sem controle de idempotência, isso pode gerar cobranças duplicadas.

## Decisão
Adotar uma IdempotencyKey fornecida pelo cliente na criação do pagamento. Uma mesma chave, dentro da janela de validade, sempre retorna a transação já criada, em vez de processar uma nova autorização.

## Consequências
- Retries de rede tornam-se seguros por padrão.
- O cliente é responsável por gerar uma chave única por tentativa de cobrança (ex.: UUID por checkout).
- A chave e o resultado associado precisam ser persistidos com tempo de vida definido para não crescer indefinidamente.
