# Aula 05 — Persistência em Arquiteturas Distribuídas — Prática

Template da prática "duas persistências trocáveis" + ferramenta visual de NoSQL.

## Conteúdo desta pasta

| Item | O que é |
|------|---------|
| `nosql-explorer.html` | Ferramenta visual usada nas fases de exploração/explicação (abrir no navegador) |
| `ServicoProdutos/` | Projeto .NET da prática — API com `IProdutoRepositorio` e duas implementações |
| `ServicoProdutos/GABARITO-RepositorioDocumento.txt` | Gabarito dos TODOs (uso do professor) |

## Roteiro da prática (em duplas — 75 min)

1. **Rodar no modo Sql (15 min)** — `cd ServicoProdutos && dotnet run`. Testar no Postman:
   `GET /produtos`, `GET /produtos/1`, `GET /produtos/barato?max=100`, `POST /produtos`.
   Abrir `produtos.db` num visualizador SQLite e ver a TABELA. Ler `ProdutoRepositorioSql.cs`
   (implementação completa — é a referência).
2. **Implementar o modo Documento (35 min)** — completar os TODOs 1–4 de
   `Repositorios/ProdutoRepositorioDocumento.cs` (um arquivo JSON por produto na pasta `dados/`).
3. **Trocar a persistência (10 min)** — em `appsettings.json`, mudar `"Persistencia"` para
   `"Documento"`, rodar de novo e repetir os MESMOS testes no Postman. A API não mudou —
   só o repositório. Abrir a pasta `dados/` e ver os documentos.
4. **Comparar (10 min)** — responder no caderno: onde o filtro do `/produtos/barato` roda
   em cada implementação? O que isso significa para coleções gigantes?
5. **Evolução de esquema (5 min + bônus)** — descomentar a propriedade `Tags` em
   `Models/Produto.cs` e rodar nas duas persistências. Qual sobrevive sem migração? Por quê?

## Entregável

API rodando nas DUAS persistências (mesmos testes no Postman) + anotações das comparações
dos passos 4 e 5.

## Requisitos

.NET SDK 8+ (testado com 10), VSCode, Postman. Sem servidor de banco: SQLite e JSON são arquivos locais.
