# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Matheus Braga de Sousa · **Curso/Turma:** Arquitetura de Aplicações Web (2026.2)
- **Repositório:** https://github.com/BragaTy/ra-202451053167.git

---

## 1. Domínio e problema

A aplicação é um sistema de controle de atendimentos para uma empresa de suporte técnico (Help Desk). O problema resolvido é a desorganização no registro dos clientes atendidos, no cálculo de prazos de atendimento (SLA) e no acompanhamento do status de cada chamado aberto. O sistema atende a equipe de suporte e o gestor da empresa, centralizando o cadastro de clientes e a fila de chamados.

---

## 2. Entidades

### Entidade A: `Clientes`
Representa o cliente atendido pela empresa de suporte.
- `Id`: identificador único no banco (`ObjectId`);
- `Nome`: nome do cliente ou empresa atendida;
- `Email`: e-mail de contato;
- `Telefone`: telefone de contato;
- `Plano`: plano contratado (`"Basico"` ou `"Completo"`);
- `Ativo`: booleano que indica se o cadastro está ativo.

### Entidade B: `Chamados`
Representa uma solicitação de suporte aberta para um cliente.
- `Id`: identificador único no banco (`ObjectId`);
- `ClienteId`: referência ao `Id` do cliente atendido;
- `Assunto`: título resumido do problema;
- `Descricao`: explicação detalhada da solicitação;
- `Prioridade`: urgência do atendimento (`"Baixa"`, `"Media"` ou `"Alta"`);
- `PrazoHoras`: prazo máximo em horas calculado automaticamente pelo serviço conforme a prioridade;
- `Status`: situação do chamado (`"Aberto"`, `"EmAtendimento"` ou `"Concluido"`);
- `CriadoPorId`: identificador do usuário que registrou o chamado (para controle de *ownership* / IDOR);
- `DataAbertura`: data e hora de abertura do chamado.

### Relacionamento e Modelagem NoSQL:
O relacionamento entre `Clientes` e `Chamados` é de **1 para N** (um cliente pode ter vários chamados abertos ao longo do tempo).

- **Modelagem por Referência (`ClienteId`):** Os `Chamados` são armazenados em uma coleção própria (`chamados`), referenciando o cliente pelo campo `ClienteId`. Justificativa: um cliente acumula vários chamados ao longo do tempo e a equipe de suporte consulta, filtra e atualiza o status dos chamados de forma independente; separar em duas coleções evita que o documento do cliente cresça sem limite e permite paginar e filtrar a fila de chamados diretamente.

---

## 3. Endpoints previstos

Seguindo o padrão visto em aula (substantivos no plural, versionamento `/api/v1`, paginação/filtros via query string e rota aninhada de até 2 níveis):

### Autenticação (`Auth`)
- `POST /api/v1/auth/registrar`: cadastra um novo usuário com perfil padrão `usuario`.
- `POST /api/v1/auth/login`: autentica o usuário e devolve o token JWT.

### Entidade `Clientes`
- `GET /api/v1/clientes?page=1&size=20`: lista os clientes cadastrados de forma paginada.
- `GET /api/v1/clientes/{id}`: busca os detalhes de um cliente pelo `id`.
- `GET /api/v1/clientes/{id}/chamados`: rota aninhada que lista os chamados de um cliente específico.
- `POST /api/v1/clientes`: cadastra um novo cliente (restrito ao perfil `admin`).
- `PUT /api/v1/clientes/{id}`: atualiza os dados de um cliente (restrito ao perfil `admin`).
- `DELETE /api/v1/clientes/{id}`: remove um cliente (restrito ao perfil `admin`).

### Entidade `Chamados`
- `GET /api/v1/chamados?page=1&size=20&status=Aberto`: lista os chamados com paginação e filtro por status ou prioridade.
- `GET /api/v1/chamados/{id}`: busca os detalhes de um chamado específico.
- `POST /api/v1/chamados`: abre um novo chamado calculando automaticamente o `PrazoHoras`.
- `PUT /api/v1/chamados/{id}`: atualiza os dados e o status de um chamado (apenas o criador do chamado ou `admin`).
- `DELETE /api/v1/chamados/{id}`: remove um chamado (restrito ao perfil `admin`).

---

## 4. Regra de negócio

A camada de serviço aplicará as seguintes regras nas operações de escrita:

1. **Cálculo automático de SLA (`PrazoHoras`):** ao abrir um chamado, o serviço define o prazo automaticamente conforme a `Prioridade` (`Alta` = 8h, `Media` = 24h, `Baixa` = 48h), aplicando desconto de metade do tempo caso o cliente possua o plano `"Completo"` (4h, 12h e 24h).
2. **Validação de cliente ativo:** o serviço impede a abertura de chamados para clientes com `Ativo = false`.
3. **Transição de estado:** um chamado com status `"Concluido"` não pode ser reaberto nem voltar para `"Aberto"`.

---

## 5. Casos de erro

As respostas de erro seguirão o padrão **Problem Details (RFC 9457)** visto em aula:

1. **Violação de regra de negócio / dado inválido (`400 Bad Request`):** tentar abrir chamado para cliente inativo ou tentar reabrir chamado já `"Concluido"` retorna `400 Bad Request`.
2. **Recurso inexistente (`404 Not Found`) e Conflito (`409 Conflict`):** buscar ou remover `id` inexistente retorna `404 Not Found`; cadastrar cliente com `Email` já existente retorna `409 Conflict`.
3. **Autenticação e Autorização (`401 Unauthorized` e `403 Forbidden`):** requisição sem token válido retorna `401`; tentativa de deletar registro sem perfil `admin` ou editar chamado de outro usuário (prevenção de IDOR) retorna `403`.

---

## 6. Stack escolhida

- **Backend:** .NET 10 (C#) com ASP.NET Core Web API estruturada em camadas (Controllers, Services e Repositories com interfaces).
- **Banco de Dados:** MongoDB (NoSQL), com string de conexão lida por variável de ambiente.
- **Frontend:** HTML, CSS e JavaScript puro consumindo a API de forma assíncrona (`fetch`), sem recarregar o navegador.

---

## 7. Segurança e testes

- **Autenticação JWT e Boas Práticas OWASP:**
  - Token JWT assinado com `HS256` (chave de 32+ caracteres via variável de ambiente), claims `sub`, `email` e `role`, e expiração configurada (`exp`).
  - Uso de **DTOs de entrada restritos** para evitar *Mass Assignment* (o cliente não consegue enviar `Perfil = "admin"` nem forçar o `PrazoHoras` manualmente).
  - Prevenção de **Broken Access Control (IDOR)**: validação de *ownership* para garantir que um usuário comum só altere os próprios chamados.
- **Autorização (RBAC):**
  - `usuario` (atendente): consulta clientes, abre chamados e atualiza os próprios chamados;
  - `admin` (gestor): gerencia o cadastro de clientes e pode editar ou remover qualquer registro.
- **Testes Unitários (xUnit):**
  - Testes na camada de serviço usando o padrão **AAA (Arrange, Act, Assert)**, nomenclatura `Metodo_Cenario_ResultadoEsperado`, `[Fact]` e `[Theory]`/`[InlineData]` com repositórios isolados por interface, cobrindo pelo menos 2 cenários de sucesso e 2 de erro e executáveis com `dotnet test`.
