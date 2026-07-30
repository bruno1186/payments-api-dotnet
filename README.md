# payments-api-dotnet

![CI](https://github.com/bruno1186/payments-api-dotnet/actions/workflows/ci.yml/badge.svg)

API de **pagamentos** em **.NET 8** (ASP.NET Core **Minimal API** + C#) com ciclo de
vida completo de transacao: **autorizacao -> captura -> estorno**, validacao de
entrada e **idempotencia** por chave.

> Casos de uso de referencia: **fintech**, **bancos** e **varejo** (checkout).

## Dominio

Uma `Payment` transita entre estados com regras explicitas:

```
Authorized --capture--> Captured --refund--> Refunded
```

- **Autorizacao**: valida valor positivo, moeda em `{BRL, USD, EUR}` e cartao com
  13-19 digitos; armazena apenas os **ultimos 4 digitos** (nunca o PAN completo).
- **Idempotencia**: a mesma `IdempotencyKey` retorna a transacao ja criada, evitando
  cobranca duplicada em retries.
- **Captura** so a partir de `Authorized`; **estorno** so a partir de `Captured`
  (transicoes invalidas retornam `409 Conflict`).

## Endpoints

| Metodo | Rota | Descricao |
|--------|------|-----------|
| `GET`  | `/health` | Health check |
| `POST` | `/payments` | Autoriza um pagamento |
| `GET`  | `/payments` | Lista pagamentos |
| `GET`  | `/payments/{id}` | Consulta por id |
| `POST` | `/payments/{id}/capture` | Captura |
| `POST` | `/payments/{id}/refund` | Estorna |

### Exemplo

```bash
curl -X POST http://localhost:5000/payments \
  -H "Content-Type: application/json" \
  -d '{"idempotencyKey":"abc-1","amount":250.00,"currency":"BRL","cardNumber":"4111111111111111"}'
```

## Estrutura

```
src/
  Payments.Domain/   # Payment, PaymentService (regras + idempotencia)
  Payments.Api/      # Minimal API (Program.cs)
tests/
  Payments.Tests/    # xUnit: unitarios + integracao (WebApplicationFactory)
```

## Como rodar

```bash
# API
dotnet run --project src/Payments.Api

# Testes
dotnet test tests/Payments.Tests
```

## Stack

.NET 8 | ASP.NET Core Minimal API | C# 12 | xUnit | WebApplicationFactory | GitHub Actions

## Sobre a publicação

Este projeto foi construído e publicado como referência de arquitetura para a comunidade, refletindo padrões e decisões técnicas aplicados na prática profissional (sem reproduzir código ou dados de projetos proprietários).
