# Aula 03.1 — REST na prática: duas APIs para consumir

Duas APIs .NET 6, mesmo tamanho, domínios diferentes. Uma é a **referência do certo**;
a outra é o **campo de caça**.

| Projeto | Tema | Porta | Papel |
|---|---|---|---|
| `CafeteriaApi/` | Café Newton (cafés, pedidos, itens) | **5301** | REST bem projetada — **sem pegadinhas** |
| `PetShopApi/` | PetHouse (pets, tutores, consultas) | **5302** | **12 endpoints, 12 erros** de design REST |
| `GABARITO/` | — | — | correções comentadas (professor) |
| `testes/` | — | — | coleções Postman, arquivos `.http` e scripts curl |

O guia impresso — **"Aula 03.1 - Guia de Consumo de APIs REST.pdf"**, na pasta da aula —
ensina o passo a passo de `git clone` → `dotnet run` → Postman / cURL / Swagger.

## Como rodar

Pré-requisito: **.NET 6 SDK** (ou superior — os projetos usam `RollForward`, então rodam
também em quem só tem .NET 8/9/10 instalado).

```bash
# terminal 1 — a API correta
cd "aulas/aula 03.1 - rest revisao/src/CafeteriaApi"
dotnet run
# http://localhost:5301/swagger

# terminal 2 — a API com os erros
cd "aulas/aula 03.1 - rest revisao/src/PetShopApi"
dotnet run
# http://localhost:5302/swagger
```

As duas guardam os dados em memória (`List<T>`): ao parar o processo, tudo volta ao estado
inicial — o que é ótimo para a prática (e é o gancho da próxima aula).

## Roteiro sugerido (60 min)

1. **(15 min) Aquecimento na API correta.** Rode os 7 casos do guia na `CafeteriaApi`:
   200, 201 + `Location`, 400, 404, 204, 404 na segunda exclusão e 304 com `If-None-Match`.
   O objetivo é fixar como uma resposta REST *deveria* ser.
2. **(30 min) Caça aos erros na PetHouse.** Em duplas, chame os 12 endpoints da `PetShopApi`
   e preencha a tabela do guia: endpoint → o que observei → regra violada → como eu redesenharia.
3. **(15 min) Fechamento.** Cada dupla apresenta 2 erros; o professor abre o `GABARITO/`.

## Entregável

Um documento (ou a própria coleção Postman exportada) com os **12 erros** identificados:
para cada um, a URI/método chamado, o que a resposta mostrou, o princípio REST violado
(com o nome dado em aula) e a versão corrigida do endpoint.

## Estrutura

```
src/
├── CafeteriaApi/          # versão correta (porta 5301)
│   ├── Controllers/       #   CafesController, PedidosController, PedidoItensController
│   ├── Data/              #   CafeteriaStore — 240 cafés e 60 pedidos em memória
│   └── Models/
├── PetShopApi/            # versão com os 12 erros (porta 5302)
│   ├── Controllers/       #   PetShopController — endpoints 01 a 12
│   ├── Data/              #   PetShopStore — 2.000 pets, 6.000 consultas
│   └── Models/
├── GABARITO/              # correções comentadas — não distribuir antes da prática
└── testes/                # Postman, .http e curl
```

## Resposta do entregável

| # | O que eu chamei | O que a resposta mostrou | Regra REST violada | Como eu redesenharia |
|---|---|---|---|---|
| 01 | `POST /api/v1/getPets` | Usou POST só para buscar os pets. | Método errado e verbo na URI. | `GET /api/v1/pets`. |
| 02 | `GET /api/v1/deletarPet?id=7` | O GET apagou um pet. | GET deveria somente consultar. | `DELETE /api/v1/pets/7`, retornando 204 ou 404. |
| 03 | `GET /api/v1/pet/{id}` | A rota usou `pet` no singular, diferente das outras. | Falta de padrão na URI. | `GET /api/v1/pets/{id}`. |
| 04 | `GET /api/v1/banhosTosa` e `/tutores_vip` | As rotas misturaram camelCase e underline. | Falta de padrão e filtro tratado como recurso. | `GET /api/v1/banhos-e-tosas` e `GET /api/v1/tutores?vip=true`. |
| 05 | `POST /api/v1/pets` | Criou o pet, mas retornou 200 e sem `Location`. | Status errado na criação. | Manter o POST e retornar 201 com `Location`. |
| 06 | `GET /api/v1/pets/999999` | Retornou 200 com uma mensagem de erro. | Erro não pode ser resposta de sucesso. | Retornar 404 com erro em JSON. |
| 07 | `GET /api/pets` | Não tinha versão e mudou `nome` para `nomeDoPet`. | Quebra de contrato sem versionamento. | Manter `/api/v1/pets` com `nome` e criar `/api/v2/pets` com `nomeDoPet`. |
| 08 | Rota completa de petshop até exame | Precisava informar vários ids para buscar um exame. | Aninhamento muito grande. | `GET /api/v1/exames/{id}`. |
| 09 | `GET /api/v1/consultas` | Retornou todas as consultas de uma vez. | Coleção grande sem paginação. | `GET /api/v1/consultas?page=1&size=20`. |
| 10 | `PUT /api/v1/pets/12/vacinas` | Cada chamada adicionou outra vacina igual. | PUT deveria ser idempotente. | Usar `POST /api/v1/pets/12/vacinas` para criar uma aplicação. |
| 11 | `POST /api/v1/sessao` e `GET /api/v1/meus-pets` | O servidor guardou qual foi o último usuário. | A API deixou de ser stateless. | O login retorna um token e cada chamada envia esse token. |
| 12 | `GET /api/v1/tabela-de-precos` | A resposta proibiu cache mesmo mudando pouco. | Cache foi desperdiçado. | Usar `Cache-Control`, `ETag` e retornar 304 quando não mudar. |

Os principais erros foram métodos errados, rotas sem padrão, status incorretos, falta de paginação, falta de versionamento e estado salvo no servidor.
