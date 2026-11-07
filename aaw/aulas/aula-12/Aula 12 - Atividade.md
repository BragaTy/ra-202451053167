# HANDOUT — AULA 11.1

## O Prontuário da LojaRevisao

*Revisão das Aulas 01 a 11 — Arquitetura de Aplicações Web*

> **Nomes:** MATHEUS BRAGA DE SOUSA   **Turma:**    **Data:** 05/11/2026

## PARTE 01 — Diário de correção

| Falha | Aula | A regra que estava sendo violada, em uma frase | Verde? |
|---|---|---|---|
| 01 | 01 | Um módulo só usa outro pelo contrato dele, sem mexer direto nos dados, porque a regra mora no dono do dado. | ☑ |
| 02 | 02 | O status code tem que dizer o resultado real: 404 quando não existe e 201 com Location quando cria. | ☑ |
| 03 | 03 | A URI é substantivo e a ação fica no método HTTP, e um GET nunca pode alterar dados. | ☑ |
| 04 | 04 | Lista grande precisa de paginação, com um limite de itens imposto pelo servidor. | ☑ |
| 05 | 05 | O pedido é tudo ou nada: ou grava o estoque e o pedido juntos, ou não grava nada. | ☑ |
| 06 | 06 | O cliente não deve esperar uma tarefa que pode ser feita depois, como o e-mail, então ela vai para a fila. | ☑ |
| 07 | 07 | Dado do usuário não pode virar parte do comando SQL, tem que ir como parâmetro. | ☑ |
| 08 | 08 | A API tem que validar a validade do token e recusar com 401 o que já venceu. | ☑ |
| 09 | 09 | Não basta saber quem é o usuário: é preciso checar o papel dele (RBAC) e se o recurso é dele (ABAC). | ☑ |
| 10 | 10 | O contrato OpenAPI tem que mostrar o que a API faz de verdade, com os status certos e a autenticação. | ☑ |
| 11 | 11 | O esperado do teste vem da regra de negócio, não do código, e um teste sem Assert não verifica nada. | ☑ Postman ☑ dotnet test |

> Verificação: `dotnet test` com 6 de 6 testes aprovados e coleção do Postman com 36 de 36 verificações aprovadas, rodada pelo Newman (a linha de comando do Postman). Saídas em `src/PRINTS/`.

## PARTE 02 — Sintoma, aula e conceito

| # | Sintoma relatado | Aula | Conceito violado |
|---|---|---|---|
| A | Cada compra demora mais de 3 segundos esperando o provedor de e-mail. | 06 | Acoplamento temporal: a comunicação síncrona deveria ser assíncrona, com fila. |
| B | Produto inexistente responde 200 com `{"erro": "..."}` no corpo. | 02 | Semântica do status code: deveria ser 404. |
| C | João abriu o pedido da Maria e um Cliente apagou produtos. | 09 | Controle de acesso: faltou ABAC no pedido e RBAC no produto. |
| D | A tela mostra 10 itens, mas baixa 2.000 registros. | 04 | Falta de paginação e de limite no servidor. |
| E | Quem compra exatamente R$ 300,00 paga frete, com os testes verdes. | 11 | Teste de borda errado: o esperado foi copiado do código, não da regra. |
| F | Pedido de 999.999 unidades deixou o estoque negativo. | 01 | Fronteira entre módulos: Pedidos alterou a tabela do Catálogo e pulou a regra dele. |
| G | Token vencido há 5 minutos continua abrindo a API. | 08 | Autenticação: o tempo de vida do token não era validado. |
| H | Um robô cancelou pedidos em `GET /api/v1/cancelarPedido?id=7`. | 03 | Design de URI: verbo na rota e GET com efeito colateral. O certo é `DELETE /pedidos/7`. |
| I | Item válido e item inexistente: deu erro, mas o estoque baixou. | 05 | Atomicidade (ACID): o pedido foi salvo em partes. |
| J | O Swagger diz 200, o POST devolve 201 e não há onde informar o token. | 10 | Contrato OpenAPI diferente da API real. |
| K | `%' OR '1'='1` na busca listou o catálogo inteiro. | 07 | Injeção de SQL (OWASP): comando e dado misturados. |

## PARTE 03 — Perguntas que cruzam aulas

**1. Sintoma I: se o Catálogo fosse outro serviço, com banco próprio, a correção continuaria simples? Por quê?**

Não. Hoje é simples porque os dois módulos usam o mesmo banco, então um único `SaveChanges` grava tudo numa transação só. Com bancos separados não existe mais essa transação única. Se o segundo passo falhasse, seria preciso desfazer o primeiro com uma compensação, por exemplo devolvendo o estoque (Saga), e o sistema passaria a ter consistência eventual.

**2. O sintoma C esconde duas falhas. Uma se resolve com papel; a outra, não. Qual é qual, e por quê?**

Apagar produto se resolve com papel (RBAC), porque a regra é só "Admin remove", então `[Authorize(Roles = "Admin")]` já basta. Abrir o pedido da Maria não se resolve com papel, porque João e Maria têm o mesmo papel, Cliente. A decisão depende de um atributo do recurso, que é o dono do pedido, então é preciso comparar esse dono com o usuário do token (ABAC).

**3. Sintoma E: como uma suíte pode estar toda verde com um bug no código? Que cuidado foi esquecido?**

O teste só prova que o código faz o que o teste espera. Se o esperado foi copiado do próprio código, o teste confirma o bug: o caso de R$ 300,00 esperava frete de R$ 25,00. Além disso, outro teste executava o método sem nenhum `Assert`, então nunca falharia. O cuidado esquecido foi tirar o esperado das regras de negócio e testar bem os valores de borda.

## 🏆 DESAFIO

**1. Se a sexta-feira chegasse e só desse tempo de corrigir 3 das 11 falhas, quais seriam? Justifiquem pelo dano em produção.**

- **Falha 09 (RBAC e ABAC):** qualquer cliente logado pode apagar o catálogo e ver pedidos de outras pessoas, o que derruba a loja e ainda vaza dados.
- **Falha 07 (Injeção de SQL):** o atacante não precisa nem de login e o texto dele é executado no banco, então o dano pode ir além da busca.
- **Falha 01 (Fronteira e estoque):** a loja venderia o que não tem, e isso gera prejuízo e reclamação em toda compra errada.
