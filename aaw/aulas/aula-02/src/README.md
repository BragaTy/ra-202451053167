# BibliotecaApi — Aula 02 (REST Fundamentos)

Template de partida da prática: uma API REST de biblioteca com CRUD completo e
status codes corretos. O `GET api/livros` já está implementado como exemplo —
os demais endpoints são os passos da atividade.

## Pré-requisitos

- .NET 8 SDK (ou superior)
- Postman

## Como rodar

```bash
dotnet run
```

A API sobe em `http://localhost:5268` (Swagger UI em `/swagger`).

## Roteiro da prática (em duplas)

| Passo | Endpoint | Status esperados |
|-------|----------|------------------|
| 1 (pronto) | `GET api/livros` | 200 |
| 2 | `GET api/livros/{id}` | 200 / 404 |
| 3 | `POST api/livros` | 201 + header `Location` / 400 sem `titulo` |
| 4 | `PUT api/livros/{id}` | 200 / 404 |
| 5 | `DELETE api/livros/{id}` | 204 / 404 |

Siga os comentários `// PASSO N` em `Controllers/LivrosController.cs`.
Teste CADA caso no Postman antes de avançar — o entregável é a coleção
Postman reproduzindo os 6 casos do handout na sua própria API.

## Entregável

Coleção Postman com: GET lista (200), GET id existente (200), GET id
inexistente (404), POST válido (201 + Location), POST sem título (400),
PUT (200/404) e DELETE duas vezes no mesmo id (204 depois 404 — por quê?).

## Gabarito

`GABARITO/LivrosController.Gabarito.cs.txt` — solução completa comentada
(professor: não distribuir antes da prática).
