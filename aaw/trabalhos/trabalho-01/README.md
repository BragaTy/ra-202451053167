# Sistema de Suporte Técnico — HelpDesk API

- **Aluno:** Matheus Braga de Sousa
- **RA:** 202451053167
- **Disciplina / Turma:** Arquitetura de Aplicações Web — 2026.2 · Prof. Thalles Noce

---

## 1. Descrição do projeto

Aplicação web para controle de atendimentos de uma empresa de suporte técnico (Help Desk). Organiza o cadastro de **Clientes** e a fila de **Chamados**, calculando automaticamente o prazo de atendimento (SLA) conforme a prioridade do chamado e o plano do cliente.

Conceitos da disciplina aplicados:

- **API REST (nível 2 de Richardson):** substantivos no plural, versionamento `/api/v1`, paginação (`page`/`size`), filtros por query string, rota aninhada e erros padronizados em **Problem Details (RFC 9457)**.
- **Persistência NoSQL:** **MongoDB** (Atlas ou local), com repositórios desacoplados por interface (`IClienteRepository`, `IChamadoRepository`, `IUsuarioRepository`).
- **Segurança:** autenticação **JWT** (HS256, com expiração), autorização por perfil (**RBAC**: `admin` e `usuario`), verificação de dono do chamado (contra **IDOR**), DTOs de entrada contra **Mass Assignment** e senhas guardadas só como hash.
- **Documentação viva:** OpenAPI gerado pelo .NET e **Swagger UI** com botão *Authorize*.
- **Página web:** HTML + JavaScript puro, com `fetch` e navegação lista → detalhe → formulário sem recarregar.
- **Testes automatizados:** xUnit na camada de serviço (padrão AAA, `[Fact]` e `[Theory]`).

### Arquitetura

```
src/
├── Controllers/    HTTP: rotas, status codes e [Authorize]
├── Services/       regras de negócio (SLA, cliente ativo, transição de status, ownership)
├── Repositories/   acesso ao MongoDB, atrás de interfaces
├── Models/         documentos do banco (Cliente, Chamado, Usuario)
├── Dtos/           o que a API aceita receber e devolve
├── Auth/           JWT, usuário logado e esquema Bearer do Swagger
├── Erros/          exceções de negócio e tratador central (Problem Details)
├── Data/           conexão com o MongoDB e criação de índices
└── wwwroot/        página web (index.html, app.js, app.css)
tests/
└── SuporteApi.Tests/   testes unitários xUnit com repositórios falsos
```

### Modelagem NoSQL

`Clientes` e `Chamados` têm relacionamento **1-N por referência**: cada chamado guarda o `ClienteId` e fica na sua própria coleção. Um cliente acumula chamados sem limite, e a fila é consultada, filtrada e paginada de forma independente, então embutir os chamados no documento do cliente faria esse documento crescer sem controle.

Índices criados na subida da API: e-mail único em `clientes` e em `usuarios`, e `ClienteId` em `chamados`.

### Regras de negócio (camada de serviço)

1. **SLA automático (`PrazoHoras`):** `Alta` = 8 h, `Media` = 24 h, `Baixa` = 48 h; clientes do plano `Completo` têm metade do prazo (4 h, 12 h e 24 h).
2. **Cliente ativo:** não é possível abrir chamado para cliente com `Ativo = false` (400).
3. **Transição de estado:** chamado `Concluido` não volta para `Aberto` nem `EmAtendimento` (400).
4. **Ownership:** um `usuario` só altera os chamados que ele abriu (403); o `admin` altera qualquer um.
5. **Integridade:** e-mail de cliente duplicado retorna 409, e cliente com chamados não pode ser removido (409; desative-o pelo PUT).

---

## 2. Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Um MongoDB acessível, de uma destas formas:
  - **MongoDB Atlas** (gratuito, M0), com o IP da sua máquina liberado em *Network Access*; ou
  - **Docker Desktop**, para subir um MongoDB local com o `docker-compose.yml` deste projeto.

---

## 3. Configuração (variáveis de ambiente)

Nenhuma string de conexão, chave ou senha fica no código. Defina os valores abaixo **antes de executar**.

| Variável de ambiente | User Secrets (alternativa) | Descrição | Exemplo |
| :--- | :--- | :--- | :--- |
| `MongoDb__ConnectionString` | `MongoDb:ConnectionString` | URI de conexão com o MongoDB | `mongodb://localhost:27017` ou `mongodb+srv://<usuario>:<senha>@<cluster>.mongodb.net/` |
| `MongoDb__Database` | `MongoDb:Database` | Nome do banco | `SuporteDb` |
| `Jwt__Key` | `Jwt:Key` | Chave de assinatura HS256 (**mínimo 32 caracteres**) | `<gere-uma-chave-aleatoria-com-32-ou-mais-caracteres>` |
| `Admin__Email` | `Admin:Email` | E-mail do admin criado na primeira execução | `admin@suporte.local` |
| `Admin__Senha` | `Admin:Senha` | Senha desse admin | `<defina-uma-senha-forte>` |

Emissor (`SuporteApi`), público (`SuporteWeb`) e expiração do token (60 min) já estão no `appsettings.json` e podem ser sobrescritos por `Jwt__Issuer`, `Jwt__Audience` e `Jwt__ExpiresInMinutes`.

**Opção A — User Secrets (recomendado em desenvolvimento; fica fora do repositório):**

```bash
cd src
dotnet user-secrets set "MongoDb:ConnectionString" "<sua-string-de-conexao>"
dotnet user-secrets set "MongoDb:Database" "SuporteDb"
dotnet user-secrets set "Jwt:Key" "<chave-aleatoria-com-32-ou-mais-caracteres>"
dotnet user-secrets set "Admin:Email" "admin@suporte.local"
dotnet user-secrets set "Admin:Senha" "<senha-forte>"
```

**Opção B — variáveis de ambiente no PowerShell (valem só para aquele terminal):**

```powershell
$env:MongoDb__ConnectionString = "<sua-string-de-conexao>"
$env:MongoDb__Database = "SuporteDb"
$env:Jwt__Key = "<chave-aleatoria-com-32-ou-mais-caracteres>"
$env:Admin__Email = "admin@suporte.local"
$env:Admin__Senha = "<senha-forte>"
```

Se alguma variável obrigatória faltar, a API não sobe e mostra qual definir.

---

## 4. Como executar localmente

1. **(Só se for usar MongoDB local)** suba o banco com Docker, na pasta `trabalho-01`:
   ```bash
   docker compose up -d
   ```
2. **Configure** as variáveis da seção 3.
3. **Execute a API:**
   ```bash
   cd src
   dotnet run
   ```
4. **Acesse no navegador:**
   - **Página web:** `http://localhost:5050`
   - **Swagger UI:** `http://localhost:5050/swagger`

Na primeira execução são criados os índices e o usuário **admin** (com o e-mail e a senha da configuração).

### Usando o Swagger

1. Em `POST /api/v1/auth/login`, clique em **Try it out** e envie o e-mail e a senha.
2. Copie o valor de `token` da resposta.
3. Clique em **Authorize** (cadeado no topo), cole o token e confirme.
4. A partir daí, as chamadas levam o cabeçalho `Authorization: Bearer <token>`.

---

## 5. Endpoints

| Método | Rota | Descrição | Acesso | Status codes |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/registrar` | Registra usuário com perfil `usuario` | público | `201`, `400`, `409` |
| `POST` | `/api/v1/auth/login` | Autentica e devolve o JWT | público | `200`, `400`, `401` |
| `GET` | `/api/v1/clientes?page=1&size=20` | Lista clientes (paginado) | logado | `200`, `400`, `401` |
| `GET` | `/api/v1/clientes/{id}` | Detalhe de um cliente | logado | `200`, `400`, `401`, `404` |
| `GET` | `/api/v1/clientes/{id}/chamados` | Chamados de um cliente | logado | `200`, `400`, `401`, `404` |
| `POST` | `/api/v1/clientes` | Cadastra cliente | `admin` | `201`, `400`, `401`, `403`, `409` |
| `PUT` | `/api/v1/clientes/{id}` | Atualiza cliente (inclusive ativo/inativo) | `admin` | `200`, `400`, `401`, `403`, `404`, `409` |
| `DELETE` | `/api/v1/clientes/{id}` | Remove cliente sem chamados | `admin` | `204`, `400`, `401`, `403`, `404`, `409` |
| `GET` | `/api/v1/chamados?page=1&size=20&status=Aberto&prioridade=Alta` | Lista chamados com filtros | logado | `200`, `400`, `401` |
| `GET` | `/api/v1/chamados/{id}` | Detalhe de um chamado | logado | `200`, `400`, `401`, `404` |
| `POST` | `/api/v1/chamados` | Abre chamado e calcula o SLA | logado | `201`, `400`, `401`, `404` |
| `PUT` | `/api/v1/chamados/{id}` | Atualiza assunto, descrição e status | dono ou `admin` | `200`, `400`, `401`, `403`, `404` |
| `DELETE` | `/api/v1/chamados/{id}` | Remove chamado | `admin` | `204`, `400`, `401`, `403`, `404` |

Falha de conexão com o banco devolve `503`; erro inesperado devolve `500`, sem expor detalhes internos.

---

## 6. Exemplo de requisição e resposta

### Abrir chamado (`POST /api/v1/chamados`)

**Request body:**
```json
{
  "clienteId": "6717f1c2a3b4c5d6e7f80911",
  "assunto": "Sistema fora do ar",
  "descricao": "Não é possível acessar o painel principal.",
  "prioridade": "Alta"
}
```

**Response (`201 Created`)** — cliente do plano `Completo`, então o prazo de 8 h cai para 4 h:
```json
{
  "id": "6717f2a8b1c2d3e4f5a60123",
  "clienteId": "6717f1c2a3b4c5d6e7f80911",
  "assunto": "Sistema fora do ar",
  "descricao": "Não é possível acessar o painel principal.",
  "prioridade": "Alta",
  "prazoHoras": 4,
  "status": "Aberto",
  "criadoPorId": "6717f001a1b2c3d4e5f60001",
  "dataAbertura": "2026-11-18T15:00:00Z"
}
```

### Erro (`409 Conflict`, Problem Details)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflito.",
  "status": 409,
  "detail": "Já existe um cliente com o e-mail contato@padaria.com."
}
```

---

## 7. Testes unitários

Na pasta `trabalho-01`:

```bash
dotnet test
```

Os testes (`tests/SuporteApi.Tests`) cobrem a camada de serviço com repositórios falsos em memória, sem precisar de banco:

- **Sucesso:** cálculo do SLA para cada prioridade e plano (`[Theory]`), abertura de chamado com prazo, status e dono corretos, admin alterando chamado de outro usuário, cadastro de cliente com e-mail normalizado.
- **Erro:** cliente inativo, cliente inexistente, chamado concluído reaberto, usuário alterando chamado de outro (IDOR), e-mail duplicado, remoção de cliente com chamados e id fora do formato.

---

## Uso de IA

- **Ferramentas usadas:** Google Antigravity (Gemini) e Claude Code (Anthropic).
- **Onde ajudou:** o Gemini revisou a proposta e os endpoints; o Claude Code gerou, etapa por etapa e a partir das minhas instruções, o código em C#, os testes, e a página web, explicando cada decisão.
- **O que eu revisei/decidi:** domínio, entidades, regras de SLA e status codes da proposta; configuração do MongoDB Atlas e dos segredos fora do repositório; testes de cada endpoint pelo Swagger; Definir os endpoints.
- **Observação:** 
