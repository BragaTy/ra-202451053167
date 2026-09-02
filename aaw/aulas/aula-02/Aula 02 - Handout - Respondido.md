# HANDOUT — AULA 02

## Dissecando o HTTP

*6 requisições sob o microscópio — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês interceptaram 6 conversas entre um app e a API de uma biblioteca. Para CADA card:

- Descrevam o que o cliente pediu (verbo + recurso na URI)
- Expliquem o que o status code da resposta informa
- Respondam: repetindo a MESMA requisição 3 vezes seguidas, o estado do servidor muda?

Ao final, preencham juntos a TABELA-SÍNTESE dos verbos na última página.

*⏱️ Tempo: 30 minutos  |  👥 Formato: em duplas  |  Dica: o card 6 esconde uma pegadinha de quem é a culpa.*

> **Nomes:** Matheus Braga de Sousa   **Turma:**    **Data:** 02/09/2026

## REQUISIÇÃO 01 — A prateleira inteira

```text
→ REQUISIÇÃO
GET /api/livros HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

[ { "id": 1, "titulo": "Clean Code", "autor": "Robert C. Martin" },
  { "id": 7, "titulo": "O Programador Pragmático", "autor": "Hunt & Thomas" } ]
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou GET para pedir a lista de todos os livros no recurso /api/livros.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 200 OK informa que a requisição deu certo e a lista de livros foi retornada. Não teve erro.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
O estado do servidor não muda porque GET serve apenas para consultar. A resposta será a mesma se nenhum livro for alterado entre as requisições.

## REQUISIÇÃO 02 — O livro fantasma

```text
→ REQUISIÇÃO
GET /api/livros/99 HTTP/1.1
Host: biblioteca.newton.br
Accept: application/json
```

```text
← RESPOSTA
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{ "title": "Not Found", "status": 404 }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou GET para buscar o livro com ID 99 no recurso /api/livros/99.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 404 Not Found informa que o livro 99 não foi encontrado. Não deu certo e é um erro do lado do cliente, porque ele pediu um recurso que não existe.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
O estado do servidor não muda. Enquanto o livro 99 não existir, todas as respostas serão 404 Not Found.

## REQUISIÇÃO 03 — Livro novo na estante

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

```text
← RESPOSTA
HTTP/1.1 201 Created
Location: /api/livros/8
Content-Type: application/json

{ "id": 8, "titulo": "Domain-Driven Design", "autor": "Eric Evans" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou POST no recurso /api/livros para cadastrar um novo livro.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 201 Created informa que o livro foi criado com sucesso. Não teve erro.

3. Enviando este POST 3 vezes seguidas, o que acontece na estante? Para que serve o header Location?
Serão criados 3 livros, normalmente com IDs diferentes, porque POST não é idempotente. O header Location informa a rota do novo recurso criado, neste caso /api/livros/8.

## REQUISIÇÃO 04 — Corrigindo a ficha completa

```text
→ REQUISIÇÃO
PUT /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

```text
← RESPOSTA
HTTP/1.1 200 OK
Content-Type: application/json

{ "id": 7, "titulo": "O Programador Pragmático", "autor": "D. Hunt; D. Thomas" }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou PUT para substituir os dados completos do livro 7 no recurso /api/livros/7.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 200 OK informa que os dados do livro foram atualizados com sucesso. Não teve erro.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
A primeira requisição atualiza o livro. As outras deixam o livro no mesmo estado, porque PUT é idempotente. A resposta deve continuar mostrando os mesmos dados atualizados.

## REQUISIÇÃO 05 — Fora do catálogo

```text
→ REQUISIÇÃO
DELETE /api/livros/7 HTTP/1.1
Host: biblioteca.newton.br
```

```text
← RESPOSTA
HTTP/1.1 204 No Content
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou DELETE para remover o livro 7 no recurso /api/livros/7.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 204 No Content informa que a exclusão deu certo e não existe conteúdo para retornar. Não teve erro.

3. Repetindo o DELETE, o estado do servidor muda? Que resposta você ESPERA na segunda vez?
A primeira requisição remove o livro. Repetir a requisição não muda mais o estado, porque o livro já foi removido e DELETE é idempotente. Na segunda vez espero 404 Not Found, porque o livro 7 não existe mais.

## REQUISIÇÃO 06 — O cadastro capenga

```text
→ REQUISIÇÃO
POST /api/livros HTTP/1.1
Host: biblioteca.newton.br
Content-Type: application/json

{ "autor": "Anônimo" }
```

```text
← RESPOSTA
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{ "title": "Bad Request", "status": 400,
  "errors": { "Titulo": [ "O campo Titulo é obrigatório" ] } }
```

**Sua análise:**

1. O que o cliente pediu (verbo + recurso)?
O cliente usou POST no recurso /api/livros para cadastrar um livro, mas enviou somente o autor e não enviou o título.

2. O que o status code informa? Deu certo? Culpa de quem se não deu?
O status 400 Bad Request informa que a requisição está inválida porque o campo título é obrigatório. Não deu certo e a culpa é do cliente, que enviou os dados incompletos.

3. Repetindo esta requisição 3 vezes seguidas, o estado do servidor muda? E a resposta?
O estado do servidor não muda porque nenhum livro será criado. As três respostas serão 400 Bad Request enquanto o título não for enviado.

## TABELA-SÍNTESE — Os verbos do HTTP

*Preencham com base nos 6 cards. “Seguro” = não altera nada no servidor. “Idempotente” = repetir N vezes deixa o servidor no mesmo estado que 1 vez.*

| **Verbo** | **Para que serve** | **Seguro?** | **Idempotente?** | **Status típicos** |
| --- | --- | --- | --- | --- |
| **`GET`** | Consultar ou buscar recursos | Sim | Sim | 200 OK, 404 Not Found |
| **`POST`** | Criar um novo recurso | Não | Não | 201 Created, 400 Bad Request |
| **`PUT`** | Substituir ou atualizar um recurso completo | Não | Sim | 200 OK, 204 No Content, 404 Not Found |
| **`PATCH`** | Atualizar apenas parte de um recurso | Não | Nem sempre | 200 OK, 204 No Content, 400 Bad Request, 404 Not Found |
| **`DELETE`** | Remover um recurso | Não | Sim | 204 No Content, 404 Not Found |

## DESAFIO

1. O verbo PATCH não apareceu em nenhum card. Qual a diferença entre PATCH e PUT? Um app de banco quer alterar SÓ o apelido do usuário, entre dezenas de campos do perfil — qual dos dois você usaria e por quê?

PUT substitui ou atualiza o recurso inteiro, então normalmente precisa receber todos os campos. PATCH altera somente os campos informados. Para alterar apenas o apelido do usuário eu usaria PATCH, porque não é necessário enviar nem substituir os outros campos do perfil.