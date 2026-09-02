# CafeteriaApi — "Café Newton" (versão CORRETA)

A API de referência da Aula 03.1: um cardápio de cafés e os pedidos do balcão.
Ela não tem pegadinha nenhuma — cada decisão de design aqui existe para ilustrar
uma regra do PPTX.

## Rodar

```bash
dotnet run
```

`http://localhost:5301` — Swagger UI em `/swagger` (a raiz redireciona para lá).
Dados em memória: 240 cafés e 60 pedidos, recriados a cada `dotnet run`.

## Endpoints

### Cafés — `/api/v1/cafes`

| Método | Rota | Respostas |
|---|---|---|
| GET | `/api/v1/cafes?page=1&size=20&origem=&torra=&sort=` | 200 (envelope paginado), 400 |
| GET | `/api/v1/cafes/{id}` | 200 + `ETag`/`Cache-Control`, 304, 404 |
| POST | `/api/v1/cafes` | 201 + `Location`, 400 |
| PUT | `/api/v1/cafes/{id}` | 200, 400, 404 |
| PATCH | `/api/v1/cafes/{id}` | 200, 400, 404 |
| DELETE | `/api/v1/cafes/{id}` | 204, 404 |

`sort` aceita `nome`, `-nome`, `preco`, `-preco`. `size` é limitado a **100** pelo servidor.

### Pedidos — `/api/v1/pedidos`

| Método | Rota | Respostas |
|---|---|---|
| GET | `/api/v1/pedidos?page=&size=&status=&cliente=` | 200, 400 |
| GET | `/api/v1/pedidos/{id}` | 200, 404 |
| POST | `/api/v1/pedidos` | 201 + `Location`, 400 |
| PATCH | `/api/v1/pedidos/{id}` | 200, 400, 404 |

Não existe `/pedidos/{id}/entregar`: mudar o status é `PATCH` no próprio pedido —
**ação vira campo, recurso continua substantivo**.

### Itens do pedido — `/api/v1/pedidos/{pedidoId}/itens`

| Método | Rota | Respostas |
|---|---|---|
| GET | `.../itens` | 200, 404 |
| GET | `.../itens/{itemId}` | 200, 404 |
| POST | `.../itens` | 201 + `Location`, 400, 404 |
| DELETE | `.../itens/{itemId}` | 204, 404 |

**1 nível** de aninhamento: um item só existe dentro de um pedido.

## O que cada decisão ilustra

| Regra (slide) | Onde ver na API |
|---|---|
| Interface uniforme: URI = substantivo plural, verbo no método (5, 8, 9) | todas as rotas |
| Status codes honestos (2) | 200 / 201 / 204 / 304 / 400 / 404 |
| Métodos seguros: GET não altera nada (2) | nenhum GET escreve no store |
| Idempotência de PUT e DELETE (2) | `PUT /cafes/{id}` substitui; `DELETE` 2x → 204 depois 404 |
| Versionamento no path (13) | prefixo `/api/v1` |
| Aninhamento ≤ 2 níveis (14) | `/pedidos/{id}/itens/{itemId}` |
| Paginação, filtro e ordenação (15) | `?page=&size=&origem=&sort=` + envelope `page/size/total/items` |
| Cacheability (11) | `ETag` + `Cache-Control` em `GET /cafes/{id}`; 304 com `If-None-Match` |
| Stateless (10) | nenhuma sessão no servidor: toda requisição se basta |

## Teste rápido (7 casos)

```bash
curl -i "http://localhost:5301/api/v1/cafes?page=1&size=3"                       # 200
curl -i http://localhost:5301/api/v1/cafes/1                                     # 200 + ETag
curl -i -H 'If-None-Match: "cafe-1-v1"' http://localhost:5301/api/v1/cafes/1     # 304
curl -i http://localhost:5301/api/v1/cafes/99999                                 # 404
curl -i -X POST http://localhost:5301/api/v1/cafes -H "Content-Type: application/json" \
     -d '{"nome":"Espresso da Aula","origem":"Sul de Minas","torra":"media","preco":9.5}'   # 201 + Location
curl -i -X POST http://localhost:5301/api/v1/cafes -H "Content-Type: application/json" \
     -d '{"origem":"Sul de Minas"}'                                              # 400
curl -i -X DELETE http://localhost:5301/api/v1/cafes/3                           # 204 (repita: 404)
```

Os mesmos comandos estão em `../testes/` (Postman, `.http` e scripts `.sh`/`.ps1`).
