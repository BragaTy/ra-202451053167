# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Matheus Braga de Sousa · **Curso/Turma:** Arquitetura de Aplicações Web (2026.2)
- **Repositório:** https://github.com/BragaTy/ra-202451053167.git

---

## 1. Domínio e problema

A aplicação é um sistema simples de controle de atendimentos para uma empresa de suporte técnico. O problema que ela resolve é a falta de organização para registrar os clientes atendidos, acompanhar os chamados abertos e controlar o prazo e o status de cada suporte. O sistema é voltado para os atendentes e para o administrador da empresa de suporte.

---

## 2. Entidades

### Entidade A: `Clientes`
Representa quem contrata o serviço de suporte.
- `Id`: identificador único no banco (ObjectId);
- `Nome`: nome do cliente;
- `Email`: e-mail para contato;
- `Telefone`: telefone do cliente;
- `Plano`: tipo de suporte contratado ("Basico" ou "Completo");
- `Ativo`: indica se o cliente está ativo para receber suporte.

### Entidade B: `Chamados`
Representa um atendimento de suporte aberto para um cliente.
- `Id`: identificador único no banco (ObjectId);
- `ClienteId`: referência ao `Id` do cliente atendido;
- `Assunto`: título curto do problema;
- `Descricao`: explicação do que aconteceu;
- `Prioridade`: urgência do chamado ("Baixa", "Media" ou "Alta");
- `PrazoHoras`: prazo de atendimento em horas (calculado automaticamente pela regra de negócio);
- `Status`: situação do chamado ("Aberto", "EmAtendimento" ou "Concluido").

### Relacionamento e Modelagem NoSQL:
O relacionamento entre `Clientes` e `Chamados` é de **1 para N** (um cliente pode ter vários chamados de suporte).

- **Modelagem por Referência (`ClienteId`):** Os `Chamados` são salvos em uma coleção separada da coleção de `Clientes`, guardando apenas o `ClienteId`. Justificativa: um cliente pode abrir muitos chamados ao longo do tempo e os chamados precisam ser listados e atualizados individualmente na tela principal, sem precisar carregar ou regravar o cadastro inteiro do cliente a cada atendimento.

---

## 3. Endpoints previstos

### Entidade `Clientes`
- `GET /api/clientes`: lista todos os clientes.
- `GET /api/clientes/{id}`: busca os dados de um cliente pelo id.
- `POST /api/clientes`: cadastra um novo cliente.
- `PUT /api/clientes/{id}`: atualiza os dados de um cliente.
- `DELETE /api/clientes/{id}`: remove um cliente (restrito ao perfil `admin`).

### Entidade `Chamados`
- `GET /api/chamados`: lista todos os chamados de suporte.
- `GET /api/chamados/{id}`: mostra os detalhes de um chamado específico.
- `POST /api/chamados`: abre um novo chamado de suporte.
- `PUT /api/chamados/{id}`: atualiza o status ou dados de um chamado.
- `DELETE /api/chamados/{id}`: exclui um chamado (restrito ao perfil `admin`).

---

## 4. Regra de negócio

A camada de serviço aplicará as seguintes regras nas operações de escrita:

1. **Cálculo automático do prazo (`PrazoHoras`):** ao criar um chamado, o serviço define o prazo automaticamente conforme a `Prioridade` escolhida (`Alta` = 8 horas, `Media` = 24 horas, `Baixa` = 48 horas).
2. **Validação de cliente ativo:** não é permitido abrir chamado para um cliente que esteja com `Ativo = false`.
3. **Transição de estado:** um chamado que já está com status `"Concluido"` não pode voltar para `"Aberto"`.

---

## 5. Casos de erro

1. **Regra de negócio / Dado inválido (`400 Bad Request`):** tentar abrir um chamado para um cliente inativo ou tentar reabrir um chamado já `"Concluido"` devolve `400 Bad Request`.
2. **Registro não encontrado (`404 Not Found`) e Duplicidade (`409 Conflict`):** buscar ou deletar um `id` que não existe devolve `404 Not Found`; tentar cadastrar dois clientes com o mesmo `Email` devolve `409 Conflict`.

---

## 6. Stack escolhida

- **Backend:** .NET 10 (C#) com ASP.NET Core Web API.
- **Banco de Dados:** MongoDB (NoSQL).
- **Frontend:** HTML, CSS e JavaScript puro com `fetch`.

---

## 7. Segurança e testes

- **Autenticação:** login e registro com JWT (`Authorization: Bearer <token>`) e tempo de expiração configurado.
- **Autorização (RBAC):** dois perfis no token (`admin` e `usuario`), onde a exclusão de registros (`DELETE`) é restrita ao perfil `admin`.
- **Testes Unitários:** testes com xUnit na camada de serviço cobrindo 2 cenários de sucesso (cálculo automático do prazo e abertura de chamado para cliente ativo) e 2 cenários de erro (bloqueio de chamado para cliente inativo e bloqueio de alteração de chamado concluído), rodados com `dotnet test`.
