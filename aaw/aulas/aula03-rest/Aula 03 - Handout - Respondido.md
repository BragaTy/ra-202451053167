# HANDOUT — AULA 03

## Consultoria de Design: a API da EscolaTech

*Identifique os anti-padrões e proponha o redesenho — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

A EscolaTech contratou a consultoria de vocês para auditar a API do sistema escolar. Todos os endpoints abaixo FUNCIONAM e estão em produção — mas o time novo se recusa a mexer neles. Para CADA endpoint:

- Identifiquem o(s) problema(s) de design (pode haver mais de um!)
- Proponham o redesenho: método HTTP + rota + status codes corretos

*⏱️ Tempo: 25 minutos  |  👥 Formato: em duplas  |  Dica: se a rota conta o que faz em português, algo está errado.*

> **Nomes:** Matheus Braga de Sousa   **Turma:**    **Data:** 20/08/2026

## ENDPOINT 01 — POST /api/getAlunos

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/getAlunos
Retorna TODOS os alunos cadastrados (hoje: 12.482 registros).
Resposta: 200 OK + array JSON completo (~9 MB).
Obs. da wiki: "usar POST porque GET não estava funcionando".
```

1. Qual(is) problema(s) de design vocês identificam?
Não usar verbos na api, deveria ser GET para obter a lista de alunos não POST e faltou usar de paginação.

2. Seu redesenho (método + rota + status codes):
GET/api/Alunos

## ENDPOINT 02 — GET /deletarAluno?id=7

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /deletarAluno?id=7
Remove o aluno do banco de dados.
Resposta: 200 OK + "OK" (mesmo se o aluno não existir).
Obs. da wiki: "dá pra deletar pelo navegador, bem prático".
```

1. Qual(is) problema(s) de design vocês identificam?
Não usar verbos na rota, deletarAluno descreve ação. Usar DELETE ao invés de GET. Status code errado, 200 OK mesmo quando recurso não existe deveria ser 400.

2. Seu redesenho (método + rota + status codes):
DELETE /api/alunos/7 - 200 se sucesso, ou 400 se aluno não existe

## ENDPOINT 03 — POST /api/alunos (criação)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
POST /api/alunos
Body: { "nome": "...", "curso": "..." }
Cria o aluno e responde: 200 OK + body "OK".
O app precisa buscar a lista inteira de novo para descobrir o ID gerado.
```

1. Qual(is) problema(s) de design vocês identificam?
Status code errado, 200 OK. ID não é retornado então o app precisa fazer nova GET para descobrir o ID gerado.

2. Seu redesenho (método + rota + status codes):
POST /api/alunos - 200

## ENDPOINT 04 — GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /escolas/1/turmas/3/alunos/25/matriculas/88/disciplinas/12
Retorna os dados da disciplina 12 da matrícula 88.
Para montar a URL o app precisa conhecer 5 IDs diferentes.
Resposta: 200 OK + JSON da disciplina.
```

1. Qual(is) problema(s) de design vocês identificam?
Rota frágil, com vários IDS, mudança na estrutura quebra todas as URLs. Se disciplina tem ID único, não precisa percorrer toda hierarquia.

2. Seu redesenho (método + rota + status codes):
GET /api/disciplinas/12 - 200 OK + retorna dados da disciplina com ID da matrícula

## ENDPOINT 05 — GET /api/alunos/7/matriculas (erro)

**Documentação atual (extraída da wiki da EscolaTech):**

```text
GET /api/alunos/7/matriculas
Se o aluno 7 não existe, responde:
200 OK + "<html><b>Erro: aluno nao existe!</b></html>"
O app mobile quebra tentando fazer parse do JSON.
```

1. Qual(is) problema(s) de design vocês identificam?
Content-Type errado, retorna HTML em vez de JSON. Status code errado, 200 OK para erro não faz sentido deveria ser 400. Incompatível com cliente mobile que espera JSON e recebe HTML.

2. Seu redesenho (método + rota + status codes):
GET /api/alunos/7/matriculas - 200 OK + array JSON se existe, ou 400 + erro JSON se aluno não existe

## ENDPOINT 06 — PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5

**Documentação atual (extraída da wiki da EscolaTech):**

```text
PUT /api/atualizarNotaParcial?aluno=7&disc=12&nota=8.5
Atualiza SÓ a nota parcial da disciplina, sem body.
Todos os dados vão na query string.
Resposta: 200 OK + "OK".
```

1. Qual(is) problema(s) de design vocês identificam?
Usa PUT mas não substitui recurso inteiro, deveria ser PATCH. Rota "atualizarNotaParcial descreve ação. 

2. Seu redesenho (método + rota + status codes):
PATCH /api/alunos/7/disciplinas/12, 200 OK + retorna disciplina atualizada, 400 se inválido

## DESAFIO

1. A EscolaTech quer lançar mudanças na API sem quebrar o app mobile antigo, que não recebe atualização há 2 anos. Que decisão de design — que falta na API INTEIRA — resolve esse problema? Como ficariam as rotas?

