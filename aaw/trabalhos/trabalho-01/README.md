# Sistema de Suporte Técnico — HelpDesk API

- **Aluno:** Matheus Braga de Sousa
- **RA:** 202451053167
- **Curso / Disciplina:** Arquitetura de Aplicações Web — 2026.2

---

## 1. Descrição do Projeto

Aplicação web para controle de atendimentos de uma empresa de suporte técnico (Help Desk). Resolve o problema de organização do cadastro de **Clientes** e acompanhamento da fila de **Chamados**, calculando automaticamente os prazos de resposta (SLA) conforme a prioridade e o plano do cliente.

O projeto aplica os conceitos trabalhados na disciplina:
- **Design de API REST (Nível 2 de Richardson):** substantivos no plural, versionamento `/api/v1`, paginação (`page`/`size`), rota aninhada e erros padronizados em **Problem Details (RFC 9457)**;
- **Persistência NoSQL:** banco de dados **MongoDB** com repositórios desacoplados por interface (`IClienteRepository`, `IChamadoRepository`);
- **Segurança (OWASP Top 10 + JWT + RBAC/ABAC):** autenticação via Bearer Token JWT, controle de acesso por papéis (`admin` e `usuario`), verificação de *ownership* (contra IDOR) e DTOs de entrada contra *Mass Assignment*;
- **Documentação viva:** especificação OpenAPI e interface **Swagger UI** com suporte ao botão *Authorize*;
- **Testes automatizados:** testes unitários da camada de serviço com **xUnit** (padrão AAA).

---

## 2. Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (ou instância local do **MongoDB** na porta `27017`)

---

## 3. Variáveis de Ambiente

Nenhuma string de conexão ou chave secreta fica fixa no código-fonte. Defina as seguintes variáveis de ambiente antes de executar:

| Variável | Descrição | Exemplo |
| :--- | :--- | :--- |
| `MongoDb__ConnectionString` | URI de conexão com o MongoDB | `mongodb://localhost:27017` |
| `MongoDb__Database` | Nome do banco de dados NoSQL | `SuporteDb` |
| `Jwt__Key` | Chave secreta para assinatura HS256 (mín. 32 caracteres) | `<sua-chave-com-32-ou-mais-caracteres>` |
| `Jwt__Issuer` | Emissor do token JWT | `SuporteApi` |
| `Jwt__Audience` | Público-alvo do token JWT | `SuporteWeb` |
| `Jwt__ExpiresInMinutes` | Tempo de expiração do token em minutos | `60` |
| `Admin__Email` | E-mail do usuário admin criado no seed inicial | `admin@suporte.local` |
| `Admin__Senha` | Senha do usuário admin criado no seed inicial | `<defina-sua-senha-local>` |

---

## 4. Como Executar Localmente

1. **Suba o banco de dados MongoDB com Docker:**
   ```bash
   docker compose up -d
   ```

2. **Configure as variáveis de ambiente no terminal (PowerShell):**
   ```powershell
   $env:MongoDb__ConnectionString = "mongodb://localhost:27017"
   $env:MongoDb__Database = "SuporteDb"
   $env:Jwt__Key = "ChaveDeDesenvolvimentoLocalComMaisDe32Caracteres!"
   $env:Admin__Email = "admin@suporte.local"
   $env:Admin__Senha = "Admin@123"
   ```

3. **Execute a aplicação:**
   ```bash
   cd src
   dotnet run
   ```

4. **Acesse no navegador:**
   - **Página Web (Frontend Assíncrono):** `http://localhost:5050`
   - **Swagger UI (OpenAPI):** `http://localhost:5050/swagger`

---

## 5. Endpoints Principais

| Método | Rota | Descrição | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/registrar` | Registra um novo usuário (`usuario`) | `201`, `400`, `409` |
| `POST` | `/api/v1/auth/login` | Autentica e retorna o token JWT | `200`, `401` |
| `GET` | `/api/v1/clientes` | Lista clientes (paginado: `?page=1&size=20`) | `200` |
| `GET` | `/api/v1/clientes/{id}` | Detalhes de um cliente pelo ID | `200`, `400`, `404` |
| `GET` | `/api/v1/clientes/{id}/chamados` | Lista os chamados de um cliente específico | `200`, `404` |
| `POST` | `/api/v1/clientes` | Cadastra novo cliente *(apenas `admin`)* | `201`, `400`, `401`, `403`, `409` |
| `PUT` | `/api/v1/clientes/{id}` | Atualiza cliente *(apenas `admin`)* | `200`, `400`, `401`, `403`, `404` |
| `DELETE` | `/api/v1/clientes/{id}` | Remove cliente *(apenas `admin`)* | `204`, `401`, `403`, `404`, `409` |
| `GET` | `/api/v1/chamados` | Lista chamados (`?page=1&size=20&status=Aberto`) | `200`, `401` |
| `GET` | `/api/v1/chamados/{id}` | Busca chamado por ID | `200`, `401`, `404` |
| `POST` | `/api/v1/chamados` | Abre chamado e calcula `PrazoHoras` (SLA) | `201`, `400`, `401`, `404` |
| `PUT` | `/api/v1/chamados/{id}` | Atualiza chamado *(dono ou `admin`)* | `200`, `400`, `401`, `403`, `404` |
| `DELETE` | `/api/v1/chamados/{id}` | Remove chamado *(apenas `admin`)* | `204`, `401`, `403`, `404` |

---

## 6. Exemplo de Requisição e Resposta

### Abrir Chamado (`POST /api/v1/chamados`)
**Request Body:**
```json
{
  "clienteId": "6717f1c2a3b4c5d6e7f80911",
  "assunto": "Sistema fora do ar",
  "descricao": "Não é possível acessar o painel principal.",
  "prioridade": "Alta"
}
```

**Response (`201 Created`):**
```json
{
  "id": "6717f2a8b1c2d3e4f5a60123",
  "clienteId": "6717f1c2a3b4c5d6e7f80911",
  "assunto": "Sistema fora do ar",
  "descricao": "Não é possível acessar o painel principal.",
  "prioridade": "Alta",
  "prazoHoras": 8,
  "status": "Aberto",
  "criadoPorId": "6717f001a1b2c3d4e5f60001",
  "dataAbertura": "2026-10-30T15:00:00Z"
}
```

---

## 7. Como Executar os Testes Unitários

```bash
dotnet test
```

---

## Uso de IA

- **Ferramentas usadas:** Google Antigravity (Gemini).
- **Onde ajudou:** revisão da proposta e alinhamento dos endpoints, DTOs e exemplos do README com o padrão REST e de segurança visto nas aulas.
- **O que eu revisei/reescrevi:** defini o domínio de suporte técnico com as entidades simplificadas (`Clientes` e `Chamados`), as regras de cálculo de SLA por prioridade e plano, e a tabela de status codes (`200`, `201`, `204`, `400`, `401`, `403`, `404`, `409`).
