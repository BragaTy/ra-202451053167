# PetShopApi — "PetHouse" (versão para AUDITORIA)

> Esta API **funciona**. Nenhuma requisição quebra, nenhum endpoint dá erro 500.
> Foi por isso que ela passou na revisão de código.
>
> E, ainda assim, **cada um dos 12 endpoints viola uma regra ou diretriz REST**
> vista em aula. Sua missão é encontrar as 12 — sem olhar o `GABARITO/`.

## Rodar

```bash
dotnet run
```

`http://localhost:5302` — Swagger UI em `/swagger`.
Dados em memória: 300 tutores, 2.000 pets, 6.000 consultas — recriados a cada `dotnet run`
(se você "estragar" alguma coisa, é só reiniciar).

## Os 12 endpoints a auditar

| # | Método | Rota |
|---|---|---|
| 01 | POST | `/api/v1/getPets` |
| 02 | GET | `/api/v1/deletarPet?id=7` |
| 03 | GET | `/api/v1/pet/{id}` |
| 04 | GET | `/api/v1/banhosTosa` **e** `/api/v1/tutores_vip` |
| 05 | POST | `/api/v1/pets` |
| 06 | GET | `/api/v1/pets/{id}` (teste com um id que **não existe**) |
| 07 | GET | `/api/pets` |
| 08 | GET | `/api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/6` |
| 09 | GET | `/api/v1/consultas` |
| 10 | PUT | `/api/v1/pets/{id}/vacinas` (chame **três vezes seguidas**) |
| 11 | POST | `/api/v1/sessao` + GET `/api/v1/meus-pets` |
| 12 | GET | `/api/v1/tabela-de-precos` |

## Como caçar (o que observar em cada resposta)

1. **A URI** — é um substantivo? Está no plural? Todas as rotas usam o mesmo padrão
   de nomenclatura? Tem alguma ação escondida no caminho?
2. **O método HTTP** — combina com o que o endpoint faz? Um GET pode mudar dados?
   Repetir a mesma chamada muda o resultado?
3. **O status code** — 200 para tudo? Cadê o 201, o 204, o 404? Um erro pode voltar como sucesso?
4. **Os headers** — a criação devolveu `Location`? A resposta tem `ETag`/`Cache-Control`?
   Faz sentido o que veio?
5. **O corpo** — o tamanho da resposta é razoável? O contrato mudou sem aviso?
   O servidor está "lembrando" de você entre requisições?

> Dica de ouro do PPTX: *a URI identifica **o que** (recurso); o método HTTP diz **o que fazer** (ação).*

## Entregável

Para **cada** endpoint, preencha:

| # | O que eu chamei | O que a resposta mostrou | Regra REST violada | Como eu redesenharia |
|---|---|---|---|---|
| 01 | `POST /api/v1/getPets` | … | … | … |

Use a API do **Café Newton** (porta 5301) como gabarito vivo: ela faz certo tudo
o que esta faz errado.
