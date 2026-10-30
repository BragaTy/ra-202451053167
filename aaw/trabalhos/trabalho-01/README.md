# Sistema de Suporte Técnico

- **Aluno:** Matheus Braga de Sousa
- **RA:** 202451053167
- **Curso / Disciplina:** Arquitetura de Aplicações Web — 2026.2

---

## Sobre o Projeto

Aplicação web para controle de uma empresa de suporte técnico, permitindo gerenciar o cadastro de **Clientes** e o acompanhamento de **Chamados** de atendimento. Desenvolvida com API REST em .NET 10, banco de dados NoSQL MongoDB e página web assíncrona com `fetch`.

---

## Pré-requisitos

- .NET 10 SDK
- MongoDB (local ou via Docker na porta `27017`)

---

## Variáveis de Ambiente

Configure as variáveis abaixo antes de iniciar a aplicação (nenhuma string de conexão fica fixa no código):

| Variável | Descrição | Exemplo |
| :--- | :--- | :--- |
| `MongoDb__ConnectionString` | Conexão com o MongoDB | `mongodb://localhost:27017` |
| `MongoDb__Database` | Nome do banco NoSQL | `SuporteDb` |
| `Jwt__Key` | Chave para assinatura do token JWT (mín. 32 caracteres) | `<sua-chave-secreta-com-32-caracteres>` |
| `Jwt__Issuer` | Emissor do token | `SuporteApi` |
| `Jwt__Audience` | Público do token | `SuporteWeb` |

---

## Como Executar Localmente

1. Suba o MongoDB:
   ```bash
   docker compose up -d
   ```
2. Defina as variáveis de ambiente no terminal (PowerShell):
   ```powershell
   $env:MongoDb__ConnectionString = "mongodb://localhost:27017"
   $env:MongoDb__Database = "SuporteDb"
   $env:Jwt__Key = "ChaveDeDesenvolvimentoLocalComMaisDe32Caracteres!"
   ```
3. Rode a API:
   ```bash
   dotnet run --project src/SuporteApi
   ```

---

## Como Acessar

- **Página Web:** `http://localhost:5050`
- **Swagger:** `http://localhost:5050/swagger`

---

## Testes Unitários

Para rodar os testes unitários da camada de serviço (2 cenários de sucesso e 2 de erro):

```bash
dotnet test
```

---

## Uso de IA

- **Ferramentas usadas:** Google Antigravity (Gemini).
- **Onde ajudou:** auxílio na organização da proposta, estrutura inicial de camadas da API e página HTML/JS.
- **O que eu revisei/reescrevi:** simplifiquei os atributos das entidades `Clientes` e `Chamados`, defini as regras de negócio de cálculo de prazo por prioridade e validei os status codes de erro (`400`, `404` e `409`).
