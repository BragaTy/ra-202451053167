# Aula 11 — Testes e Qualidade dos Serviços · Caçada ao Bug

Projeto da atividade prática da **Aula 11** da disciplina *Arquitetura de
Aplicações Web* (Newton/Wyden).

> Stack: **.NET 8 · C# · xUnit · Moq · FluentAssertions · WebApplicationFactory**

---

## 🎯 Missão

Você recebe uma API de pedidos funcional (`ResilientOrders.Api`) que **contém um
bug escondido** em uma regra de negócio. Ninguém vai te contar onde ele está: os
seus testes é que vão encontrá-lo.

As regras do `PedidoService` (as mesmas do handout impresso):

| # | Regra |
|---|-------|
| R1 | Item com quantidade ≤ 0 torna o pedido inválido |
| R2 | Desconto: **5%** a partir de R$200 (inclusive) · **10%** a partir de R$500 (inclusive) |
| R3 | Frete fixo de **R$25**; grátis a partir de R$300 (inclusive) |
| R4 | Total = subtotal − desconto + frete |

## 🗺️ Roteiro (55 min, em duplas)

A prática começa logo depois da atividade em papel (handout, 20 min): a tabela
de casos que a dupla preencheu é o ponto de partida da Etapa 1.

| Etapa | Tempo | O que fazer |
|-------|-------|-------------|
| 0 | 5 min | `dotnet test` — os 2 exemplos prontos passam; os TODOs aparecem como *Skipped* (já rodado no início da aula, durante o estudo) |
| 1 | 15 min | `PedidoServiceTests.cs` → TODO 1: transformar a **tabela de casos do handout** em `[Theory]`/`[InlineData]` (não esqueça as bordas exatas: 199,99 · 200 · 299,99 · 300 · 499,99 · **500**) |
| 2 | 10 min | Algum caso de borda falhou? **Você achou o bug.** Corrija o `PedidoService` e deixe tudo verde |
| 3 | 10 min | TODOs 2 e 3: validações com `Mock.Verify` e exceções |
| 4 | 15 min | `PedidosEndpointTests.cs` → teste de integração do `POST /api/pedidos` (201 e 400) com `WebApplicationFactory` |
| ★ | bônus | `CriarPedido` válido salva exatamente 1 vez com valores coerentes |

## ▶️ Como rodar

```bash
dotnet build                                  # compila tudo
dotnet test                                   # roda a suíte completa
dotnet test tests/ResilientOrders.UnitTests   # só os unitários
dotnet run --project src/ResilientOrders.Api  # sobe a API (Swagger em /swagger)
```

## 🗂️ Estrutura

```
src/
├── ResilientOrders.sln
├── src/ResilientOrders.Api/                  ← API com o bug escondido
│   ├── Controllers/PedidosController.cs
│   ├── Models/Pedido.cs
│   └── Services/PedidoService.cs             ← classe sob teste
└── tests/
    ├── ResilientOrders.UnitTests/            ← ETAPA 1-3 (você implementa)
    │   └── PedidoServiceTests.cs
    └── ResilientOrders.IntegrationTests/     ← ETAPA 4 (você implementa)
        ├── ApiFactoryFixture.cs              ← fixture pronta (sobe a API em memória)
        └── PedidosEndpointTests.cs
```

## ✅ Critérios de aceite (entregável)

- [x] `[Theory]` com a tabela completa do handout, incluindo todas as bordas
- [x] Bug localizado, corrigido e **documentado no exit ticket** (qual caso o revelou?)
- [x] Validações com `Mock.Verify(..., Times.Never)` nos TODOs 2 e 3
- [x] Teste de integração cobrindo 201 (com `Location`) e 400
- [x] `dotnet test` 100% verde ao final, sem testes *Skipped*

---

## 🐛 Relatório da Caçada ao Bug (Exit Ticket)

### 1. Tabela de Casos de Teste (`[Theory]` / `[InlineData]`)

| Subtotal (R\$) | Desconto (R\$) | Frete (R\$) | Total Esperado (R\$) | Observação |
|---|---|---|---|---|
| `100,00` | `0,00` | `25,00` | `125,00` | Exemplo pronto (sem desconto, paga frete) |
| `199,99` | `0,00` | `25,00` | `224,99` | Borda: logo abaixo de R\$ 200 |
| `200,00` | `10,00` (5%) | `25,00` | `215,00` | Borda: início da faixa de 5% de desconto |
| `299,99` | `15,00` (5%) | `25,00` | `309,99` | Borda: último valor que ainda paga frete |
| `300,00` | `15,00` (5%) | `0,00` | `285,00` | Borda: início do frete grátis |
| `499,99` | `25,00` (5%) | `0,00` | `474,99` | Borda: último valor na faixa de 5% |
| **`500,00`** | **`50,00` (10%)** | **`0,00`** | **`450,00`** | **Borda que revelou o bug** (início dos 10%) |
| `600,00` | `60,00` (10%) | `0,00` | `540,00` | Acima da última faixa |

### 2. Qual era o bug e qual caso de teste o revelou?

- **Caso que revelou o bug:** `[InlineData(500, 450)]` (subtotal exatamente igual a **R\$ 500,00**).Antes da correção, o método retornava `475,00` em vez de `450,00`.
- **Onde estava o bug:** No arquivo `src/ResilientOrders.Api/Services/PedidoService.cs`, dentro do método `CalcularDesconto(decimal subtotal)`, a condição estava escrita como `if (subtotal > 500m)` em vez de `if (subtotal >= 500m)`.
- **Por que acontecia:** Como a regra R2 diz que o desconto de 10% vale **a partir de R\$ 500 (inclusive)**, um pedido de exatos R\$ 500,00 caía por engano na faixa de 5% (dando R\$ 25 de desconto em vez de R\$ 50).
- **Correção aplicada:** Alterado o operador de `>` para `>=`:
  ```csharp
  if (subtotal >= 500m)
  {
      return Math.Round(subtotal * 0.10m, 2);
  }
  ```

> **Professor:** o gabarito completo (bug, tabela e suíte) está em
> `GABARITO-PROFESSOR.txt` — não distribuir.
