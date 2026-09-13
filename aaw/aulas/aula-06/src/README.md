# Aula 06 — Comunicação entre Serviços — Prática

**Síncrono vs. Assíncrono na prática: sinta a diferença no Postman.**

## Cenário: LojaDev

Uma loja online simplificada com 3 serviços:

| Serviço | Porta | O que faz |
|---------|-------|-----------|
| **LojaApi** | 5090 | Recebe pedidos do cliente (Postman) |
| **PagamentoApi** | 5091 | Aprova/rejeita pagamentos (~1s de latência) |
| **NotificacaoApi** | 5092 | Envia e-mail (~3s de latência, ~20% de falha) |

O serviço de notificação é **propositalmente lento e instável** — é isso que justifica usar comunicação assíncrona.

## Conteúdo desta pasta

| Item | O que é |
|------|---------|
| `LojaDev.slnx` | Solution com os 3 projetos |
| `LojaDev.Compartilhado/` | Modelos compartilhados + fila em memória (`Channel<T>`) |
| `LojaDev.PagamentoApi/` | Serviço de pagamento — **PRONTO** (não alterar) |
| `LojaDev.NotificacaoApi/` | Serviço de notificação — **PRONTO** (não alterar) |
| `LojaDev.LojaApi/` | Serviço principal — **AQUI estão os TODOs** |
| `GABARITO/` | Gabarito dos TODOs (uso do professor) |

## Pré-requisitos

- .NET 8 SDK (ou superior)
- Postman (ou similar)
- 3 terminais abertos (um por serviço)

---

## Roteiro da prática (em duplas — 75 min)

### FASE 1 — Explorar os serviços auxiliares (10 min)

Abra **3 terminais** e suba os serviços auxiliares:

**Terminal 1** — PagamentoApi:
```bash
cd LojaDev.PagamentoApi
dotnet run
```

**Terminal 2** — NotificacaoApi:
```bash
cd LojaDev.NotificacaoApi
dotnet run
```

No **Postman**, teste cada serviço individualmente:

#### Teste 1: PagamentoApi
```
POST http://localhost:5091/api/pagamentos
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```
→ Deve retornar **aprovado = true** em ~1 segundo.

#### Teste 2: PagamentoApi (rejeição)
```
POST http://localhost:5091/api/pagamentos
Content-Type: application/json

{
  "cliente": "João Santos",
  "produto": "Carro Elétrico",
  "valor": 15000.00
}
```
→ Deve retornar **aprovado = false** (valor > R$ 10.000).

#### Teste 3: NotificacaoApi
```
POST http://localhost:5092/api/notificacoes
Content-Type: application/json

{
  "destinatario": "maria@email.com",
  "assunto": "Teste",
  "corpo": "Olá mundo"
}
```
→ Leva **~3 segundos**. Pode **falhar** (~20% das vezes). Repita se falhar.

📝 **Anote**: quanto tempo cada serviço leva para responder?

---

### FASE 2 — Implementar o fluxo SÍNCRONO (20 min)

**Terminal 3** — LojaApi:
```bash
cd LojaDev.LojaApi
dotnet run
```

Abra `LojaDev.LojaApi/Controllers/PedidosController.cs` e complete:

- **TODO 1** — Chamar PagamentoApi via HTTP
- **TODO 2** — Chamar NotificacaoApi via HTTP

Após completar, reinicie a LojaApi (`Ctrl+C` e `dotnet run` de novo).

No Postman, teste o fluxo síncrono:

```
POST http://localhost:5090/api/pedidos/sincrono
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```

📝 **Anote o `tempoTotalMs`** — deve ser **~4-5 segundos** (1s pagamento + 3s notificação).

🔁 Repita 3 vezes. Alguma falhou? O que aconteceu com a resposta quando a notificação falha?

---

### FASE 3 — Implementar o fluxo ASSÍNCRONO (25 min)

Agora complete os TODOs restantes:

Em `Controllers/PedidosController.cs`:
- **TODO 3** — Chamar PagamentoApi (copie do TODO 1)
- **TODO 4** — Publicar evento na fila
- **TODO 5** — Retornar `Accepted()` (HTTP 202)

Em `Servicos/NotificacaoBackground.cs`:
- **TODO 6** — Consumir eventos da fila com `await foreach`
- **TODO 7** — Chamar NotificacaoApi via HTTP (com try/catch)

Reinicie a LojaApi e teste o fluxo assíncrono no Postman:

```
POST http://localhost:5090/api/pedidos/assincrono
Content-Type: application/json

{
  "cliente": "Maria Silva",
  "produto": "Notebook Dell",
  "valor": 4500.00
}
```

📝 **Anote o `tempoTotalMs`** — deve ser **~1 segundo** (só pagamento!).

👀 **Olhe o terminal da LojaApi**: a notificação é processada em background, DEPOIS que a resposta já foi pro Postman.

---

### FASE 4 — Experimentação e análise (15 min)

#### Experimento 1: Rajada de pedidos
No Postman, envie **5 pedidos assíncronos** em sequência rápida. Observe:
- Os 5 retornam rápido (~1s cada)?
- O background processa os 5 na sequência? (olhe os logs)
- Alguma notificação falhou? O que aconteceu? (a resposta do pedido mudou?)

#### Experimento 2: Notificação fora do ar
Pare o NotificacaoApi (`Ctrl+C` no Terminal 2). Envie um pedido:
- **Síncrono** (`/api/pedidos/sincrono`) → O que acontece?
- **Assíncrono** (`/api/pedidos/assincrono`) → O que acontece?

Suba o NotificacaoApi de novo e observe os logs.

#### Experimento 3: Pagamento rejeitado
Envie um pedido com valor > R$ 10.000 em ambos os fluxos:
```json
{
  "cliente": "João Santos",
  "produto": "Carro Elétrico",
  "valor": 15000.00
}
```
- O pedido rejeitado gera notificação? Por quê?

---

### FASE 5 — Reflexão (5 min)

Responda no caderno ou em um comentário no código:

1. **Por que o pagamento é síncrono nos dois fluxos?** Poderia ser assíncrono?

2. **O que acontece se a fila perder um evento?** (Lembre: o `Channel<T>` está em memória — se a aplicação reiniciar, o que acontece com os eventos na fila?)

3. **Em produção, o que substituiria o `Channel<T>`?** Cite pelo menos uma tecnologia.

4. **Se a NotificacaoApi falhar, como o consumidor da fila deveria reagir?** (Dica: o gabarito tem a resposta parcial, mas pesquise sobre "dead-letter queue" e "retry with backoff").

5. **Compare os tempos de resposta** que você anotou:

| Fluxo | Tempo de resposta | Notificação chega? |
|-------|-------------------|--------------------|
| Síncrono | ~_____ ms | ☐ Imediata / ☐ Pode falhar |
| Assíncrono | ~_____ ms | ☐ Em background / ☐ Garantida? |

---

## Entregável

- API funcionando nos dois fluxos (prints do Postman: síncrono e assíncrono, com os tempos)
- Tabela comparativa preenchida (Fase 5, pergunta 5)
- Respostas das perguntas 1-4

## Dica importante

Olhe os **logs nos terminais** — eles contam a história completa do que está acontecendo em cada serviço. No fluxo assíncrono, preste atenção na ORDEM dos logs: a resposta do Postman aparece ANTES da notificação ser processada!

## Gabarito

`GABARITO/PedidosController.Gabarito.cs.txt` e `GABARITO/NotificacaoBackground.Gabarito.cs.txt` — versões completas com todos os TODOs resolvidos (professor: não distribuir antes).

---

## Resposta do entregável

Os TODOs 1 a 7 foram implementados. Os testes foram executados em 12/09/2026 com cURL, pois o Postman não estava funcionando. Os três serviços foram executados localmente, nas portas 5090, 5091 e 5092. Os registros usam horário UTC, que aparece como 13/09/2026, já que o teste ocorreu à noite no horário de Brasília.

### 1. Por que o pagamento é síncrono nos dois fluxos?

Porque a loja precisa saber se o pagamento foi aprovado antes de confirmar o pedido. Poderia ser assíncrono, mas o pedido teria que ficar pendente até chegar a resposta do pagamento.

### 2. O que acontece se a fila perder um evento?

A notificação desse pedido pode não ser enviada. Como a fila está na memória, os eventos que ainda estiverem nela são perdidos quando a LojaApi reinicia.

### 3. O que substituiria o Channel em produção?

Poderia usar RabbitMQ com fila durável, mensagens persistentes e confirmações de publicação e processamento. Assim, os eventos não dependeriam somente da memória da LojaApi.

### 4. Como reagir quando a NotificacaoApi falha?

O consumidor deveria tentar novamente depois de um intervalo. Se continuar falhando, pode aumentar o tempo entre as tentativas, usando retry com backoff.

Depois de um limite de tentativas, a mensagem deve ir para uma fila de erros, chamada dead-letter queue, para análise ou reprocessamento. O consumidor também precisa evitar processar o mesmo evento duas vezes.

Nesta prática, a falha só é registrada no terminal. Não existe tentativa automática de reenvio.

### 5. Comparação dos tempos medidos

| Fluxo | tempoTotalMs retornado pela API | Tempo HTTP medido pelo cURL | Notificação |
|---|---|---|---|
| Síncrono | 4052 ms | 4132 ms | Esperou a tentativa de envio terminar. Nesse teste, foi enviada. |
| Assíncrono | 1002 ms | 1009 ms | Retornou pendente. A tentativa em background falhou com 503, sem alterar o 202 já recebido. |

O síncrono esperou o pagamento e a notificação. O assíncrono esperou somente o pagamento e a publicação do evento na fila. A resposta mais rápida não significa que a notificação foi entregue.

### Resultados dos experimentos

| Teste | Resultado observado |
|---|---|
| Pagamento isolado aprovado | HTTP 200, aprovado = true, 1086 ms de tempo HTTP. |
| Pagamento isolado rejeitado | HTTP 200, aprovado = false, 1004 ms de tempo HTTP. |
| Notificação isolada | HTTP 200, sucesso = true, 3083 ms de tempo HTTP. |
| Três pedidos síncronos | 4052, 4003 e 4003 ms na API. Todos retornaram 200; duas notificações foram enviadas e uma falhou. |
| Cinco pedidos assíncronos seguidos | Todos retornaram 202. Tempos na API: 1001, 1001, 1001, 1001 e 1001 ms. |
| Processamento da rajada | Os cinco eventos foram processados em sequência. Quatro notificações foram enviadas e uma falhou. |
| Notificação desligada, fluxo síncrono | Retornou 500 por erro de conexão com a NotificacaoApi. O pagamento já tinha sido aprovado. |
| Notificação desligada, fluxo assíncrono | Retornou 202 em 1001 ms na API. O erro apareceu no background. |
| Notificação ligada novamente | As tentativas que já falharam não foram reenviadas automaticamente. |
| Pagamento rejeitado nos dois fluxos | Ambos retornaram 400 com status rejeitado. Não houve publicação na fila nem envio de notificação. |

### Evidências

As imagens abaixo são capturas de uma página local exibindo os registros reais do cURL. Não são prints do Postman. Os corpos, headers, horários e tempos originais estão nos arquivos de texto e JSON, para conferência.

![Fluxo síncrono: HTTP 200 e 4004 ms](EVIDENCIAS/01-sincrono.png)

![Fluxo assíncrono: HTTP 202 e 1002 ms](EVIDENCIAS/02-assincrono.png)

- [Registro original do síncrono](EVIDENCIAS/sincrono-2.txt)
- [Registro original do assíncrono](EVIDENCIAS/assincrono-1.txt)
- [Todos os resultados em JSON](EVIDENCIAS/resultados.json)
- [Logs da LojaApi na execução validada](EVIDENCIAS/loja-validada.log)
- [Respostas dos quatro cenários e do desafio](../Aula%2006%20-%20Atividade.md)

### Como rodar nesta máquina

Os projetos fornecidos estão em net6.0. Nesta máquina, execute `set DOTNET_ROLL_FORWARD=Major` em cada CMD antes de `dotnet run`, para usar um runtime instalado.

Abra um CMD em cada pasta: `LojaDev.PagamentoApi`, `LojaDev.NotificacaoApi` e `LojaDev.LojaApi`. Em cada um, execute:

```bat
set DOTNET_ROLL_FORWARD=Major
dotnet run
```

Com os três serviços rodando, abra `testes/testar.cmd` para repetir as consultas via cURL. Cada execução cria novos pedidos de demonstração. A NotificacaoApi só simula o envio de e-mail.

### Fontes da reflexão

- [RabbitMQ: persistência e confirmações](https://www.rabbitmq.com/docs/reliability)
- [Microsoft: retry com backoff e dead-letter queue](https://learn.microsoft.com/en-us/azure/architecture/best-practices/transient-faults)
