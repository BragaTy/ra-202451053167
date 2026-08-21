# EscolaApi ("API da EscolaTech") — Aula 03 (REST Design de APIs)

Ponto de partida da prática: uma API que **funciona**, mas carrega de propósito
os 6 anti-padrões de design analisados no handout. A missão da dupla é
refatorá-la para o design correto.

## Pré-requisitos

- .NET 6 SDK (ou superior)
- Postman

## Como rodar

```bash
dotnet run
```

A API sobe em `http://localhost:5214` (Swagger UI em `/swagger`).
O banco em memória é populado com 120 alunos e 240 matrículas.

## Os 6 anti-padrões plantados (procure os comentários no código)

| # | Anti-padrão | Onde dói |
|---|-------------|----------|
| 1 | Verbo na URI (`getAlunos`, `deletarAluno` via POST) | `EscolaController` |
| 2 | Status codes errados (200 para tudo, até para "não achei") | `getAlunoPorId`, `deletarAluno` |
| 3 | Aninhamento de 4 níveis | `escola/cursos/.../matriculas/{id}` |
| 4 | Sem versionamento | todas as rotas |
| 5 | Lista sem paginação (120 alunos + matrículas de uma vez) | `getAlunos` |
| 6 | Erro como HTML com status 500 | `getMatriculas/{alunoId}` |

## Roteiro da refatoração (em duplas)

1. Explore a API no Postman e reproduza cada anti-padrão (prints do ANTES).
2. Crie `AlunosController` com rota `api/v1/alunos` e migre os endpoints:
   substantivos, métodos HTTP corretos, 200/201/204/404 semânticos.
3. Achate o aninhamento: `GET api/v1/alunos/{id}/matriculas/{matriculaId}`
   (máximo 2 níveis — curso/semestre viram filtros, não rota).
4. Adicione paginação em `GET api/v1/alunos` (`?page=1&size=10` + metadados).
5. Troque o erro HTML por `ProblemDetails` (`NotFound()`/`Problem()`).
6. Delete o `EscolaController` antigo e repita os testes (prints do DEPOIS).

**Entregável:** API refatorada + prints no Postman comparando antes/depois
de cada anti-padrão.

## Gabarito

`GABARITO/AlunosController.Gabarito.cs.txt` — versão refatorada completa,
com o mapa correção-por-anti-padrão (professor: não distribuir antes).
