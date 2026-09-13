# Atividade — AULA 06

## Síncrono ou Assíncrono?

*Análise de fluxos de comunicação entre serviços — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são os arquitetos dos 4 fluxos abaixo. Para CADA cenário:

- Decidam o estilo de comunicação: síncrono (request/response), assíncrono (fila/evento) ou API Gateway/BFF
- Desenhem o fluxo com caixas (serviços) e setas (chamadas/mensagens) no espaço indicado
- Justifiquem com pelo menos 2 fatores (urgência da resposta, tolerância a atraso, picos, falhas...)
- Apontem o principal risco da escolha de vocês

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Não existe resposta única — o que vale é a justificativa.*

> **Nomes:** Matheus Braga de Sousa   **Turma:** ____________________   **Data:** 12 / 09 / 2026

## CENÁRIO 01 — PagFácil — aprovar ou negar AGORA

No checkout do PagFácil, ao clicar em “Pagar”, o serviço de Pagamentos precisa consultar o saldo/limite do cliente no serviço de Contas — e a resposta define se a venda acontece neste exato momento.

- O cliente está na tela, esperando o resultado da compra
- Sem a resposta de Contas, não há decisão possível: aprovar às cegas é proibido
- Tempo de resposta do serviço de Contas: ~80 ms em condições normais

**Sua análise:**

1. Estilo recomendado: **Síncrono.**

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
[Cliente] --> [Pagamentos] --> [Contas]
[Cliente] <-- [Pagamentos] <-- [Contas]
```

3. Justificativa (mínimo 2 fatores): A pessoa está esperando saber se conseguiu pagar. Também precisa ver o saldo antes de aprovar o pagamento. Normalmente essa consulta é rápida.

4. Principal risco da escolha: Se o serviço de Contas ficar lento ou parar, o pagamento também pode demorar ou dar erro.

## CENÁRIO 02 — CadastraJá — o e-mail de boas-vindas

Após criar a conta no CadastraJá, o sistema envia um e-mail de boas-vindas. O provedor de e-mail às vezes demora 8 segundos para responder e falha em 2% das tentativas.

- O usuário quer começar a usar o app imediatamente após o cadastro
- O e-mail chegar 1 minuto depois não incomoda ninguém
- Se o provedor falhar, o envio deve ser tentado de novo — sem o usuário perceber

**Sua análise:**

1. Estilo recomendado: **Assíncrono (fila/evento).**

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
[Usuario] --> [Cadastro] --> [Fila] --> [E-mail]
[Usuario] <-- [Cadastro]
```

3. Justificativa (mínimo 2 fatores): A pessoa pode usar o aplicativo sem esperar o e-mail. Se der erro no envio, o sistema pode tentar mandar depois.

4. Principal risco da escolha: O e-mail pode demorar para chegar ou, em algum erro, ser enviado duas vezes.

## CENÁRIO 03 — MegaMarket — baixa de estoque nos picos

No marketplace MegaMarket, cada venda gera uma baixa no serviço de Estoque. Nas grandes promoções o tráfego sobe 10x e o Estoque não dá conta de responder na velocidade das vendas.

- Atraso de alguns segundos na baixa é aceitável
- PERDER uma baixa de estoque não é aceitável (gera venda sem produto)
- O checkout não pode ficar lento nem cair porque o Estoque está sobrecarregado

**Sua análise:**

1. Estilo recomendado: **Assíncrono (fila/evento).**

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
[Vendas] --> [Fila] --> [Estoque]
[Vendas] <-- [Pedido confirmado]
```

3. Justificativa (mínimo 2 fatores): Em promoção chegam muitas vendas ao mesmo tempo, então a fila ajuda a organizar. O estoque pode atualizar alguns segundos depois e a venda não fica esperando.

4. Principal risco da escolha: Por alguns segundos o número do estoque pode não estar atualizado. Também não pode perder nenhuma mensagem de venda.

## CENÁRIO 04 — AppBanco — uma tela, cinco serviços

A tela inicial do AppBanco mostra saldo, fatura do cartão, investimentos, empréstimos e cashback — dados de 5 serviços diferentes. O time mobile reclama: são 5 chamadas, 5 formatos de resposta e 5 pontos de falha em cada abertura do app.

- A tela precisa abrir rápido, inclusive em redes móveis ruins
- Cada serviço tem equipe, formato e autenticação próprios
- Amanhã nasce a versão web, que precisa de MAIS dados que a mobile

**Sua análise:**

1. Estilo recomendado: **API Gateway/BFF.**

2. Desenhe o fluxo (caixas = serviços, setas = chamadas/mensagens):

```text
[App mobile] --> [BFF] --> [Servicos do banco]
[App mobile] <-- [BFF] <-- [Servicos do banco]
```

3. Justificativa (mínimo 2 fatores): O celular faz só uma chamada e recebe as informações juntas. A versão web pode ter outra resposta, com os dados a mais que ela precisar.

4. Principal risco da escolha: Se o BFF der problema, a tela pode ficar sem carregar. Se um dos serviços falhar, ele também precisa lidar com isso.

## DESAFIO

1. Escolha um cenário em que vocês indicaram ASSÍNCRONO. Os brokers de mensagens costumam garantir entrega “pelo menos uma vez” — ou seja, a MESMA mensagem pode chegar duas vezes. O que aconteceria no seu fluxo? Como o consumidor deveria se proteger?

Escolhi o estoque. Se a mesma mensagem chegar duas vezes, o estoque pode diminuir duas vezes.

O serviço pode guardar o código da venda. Antes de diminuir o estoque, ele vê se esse código já foi usado. Se já foi, ele ignora a mensagem.
