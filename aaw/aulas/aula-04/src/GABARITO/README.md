# GABARITO — os 12 erros da API PetHouse

> **Professor:** não distribuir antes da prática. Esta pasta responde tudo.

Cada arquivo `NN-*.cs.txt` traz o mesmo roteiro: o erro, por que ele importa,
o **código errado**, o **código corrigido comentado** e o **comando que prova**
o problema ao vivo. Extensão `.txt` para não compilar junto com o projeto.

## Tabela de fechamento (projetar nos últimos 15 min)

| # | Endpoint auditado | Sintoma que o aluno observa | Regra REST violada | Slide | Correção |
|---|---|---|---|---|---|
| 01 | `POST /api/v1/getPets` | a ação está na URI; leitura feita com POST | Interface uniforme: URI = substantivo, verbo = método HTTP | 8, 9 | `GET /api/v1/pets` |
| 02 | `GET /api/v1/deletarPet?id=7` | abrir a URL no navegador APAGA o pet | métodos seguros: GET não altera estado | 2, 9 | `DELETE /api/v1/pets/7` → 204 |
| 03 | `GET /api/v1/pet/{id}` | singular aqui, plural no resto da API | nomenclatura consistente, recurso no plural | 5, 8 | `GET /api/v1/pets/{id}` |
| 04 | `GET /api/v1/banhosTosa` e `/tutores_vip` | camelCase e snake_case na mesma API | um único padrão de nomenclatura (kebab-case) | 7, 8 | `/banhos-e-tosas` e `/tutores?vip=true` |
| 05 | `POST /api/v1/pets` | criou o recurso e devolveu 200, sem `Location` | status code de criação é 201 + `Location` | 2 | `CreatedAtAction(...)` |
| 06 | `GET /api/v1/pets/999999` | "não encontrado" com status **200** | status code é o contrato: 4xx = erro do cliente | 2 | 404 + `problem+json` |
| 07 | `GET /api/pets` | sem `/v1` e com `nome` renomeado para `nomeDoPet` | versionamento: a API é contrato público | 13 | v1 intacta + `/api/v2/pets` |
| 08 | `GET /api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/3` | 5 ids para ler 1 exame (3 deles ignorados) | máximo 2 níveis; recurso com identidade própria vira top-level | 14 | `GET /api/v1/exames/{id}` |
| 09 | `GET /api/v1/consultas` | ~680 KB, 6.000 registros, sempre | paginação + filtro + ordenação; teto de `size` no servidor | 15 | envelope `page/size/total/items` |
| 10 | `PUT /api/v1/pets/{id}/vacinas` | 3 chamadas idênticas = 3 vacinas | PUT é idempotente | 2 | `POST` (evento) ou `PUT` que substitui |
| 11 | `POST /api/v1/sessao` + `GET /api/v1/meus-pets` | o 2º login troca a identidade do 1º usuário | stateless: contexto viaja na requisição | 10 | token no `Authorization` |
| 12 | `GET /api/v1/tabela-de-precos` | `no-store` num dado que muda 1x/ano; sem `ETag` | cacheability | 11 | `Cache-Control: public, max-age` + `ETag`/304 |

## Como conduzir o fechamento

1. Peça a cada dupla **dois** erros — quem achou, mostra a chamada no Postman e
   diz o nome da regra. O nome importa: é o vocabulário que eles vão usar na prova.
2. Rode ao vivo os dois experimentos que costumam gerar mais reação:
   - **erro 02**: abrir `http://localhost:5302/api/v1/deletarPet?id=7` no navegador e
     depois mostrar o 404 do pet;
   - **erro 11**: dois alunos logando em sequência e um vendo os pets do outro.
3. Feche com a frase do slide 17: *"uma boa API é aquela que o próximo dev usa
   sem precisar te perguntar nada"* — e volte à API do Café Newton, onde os 12
   pontos estão resolvidos.

## Arquivos

```
01-verbo-na-uri.cs.txt              07-sem-versionamento.cs.txt
02-get-com-efeito-colateral.cs.txt  08-aninhamento-profundo.cs.txt
03-recurso-no-singular.cs.txt       09-colecao-sem-paginacao.cs.txt
04-nomenclatura-camel-e-snake.cs.txt 10-put-nao-idempotente.cs.txt
05-status-code-na-criacao.cs.txt    11-stateless-quebrado.cs.txt
06-erro-com-status-200.cs.txt       12-cache-desperdicado.cs.txt

PetShopApi.Corrigido.cs.txt   ← o controller inteiro já refatorado
```
