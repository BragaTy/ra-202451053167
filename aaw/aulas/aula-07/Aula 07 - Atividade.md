# HANDOUT — AULA 07

## Caça às Vulnerabilidades

*Revisão de segurança de uma API .NET — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês são a dupla de revisores de segurança da empresa. Os 4 trechos abaixo são da MESMA API, prestes a ir para produção. Para CADA card:

- Descrevam a falha com as próprias palavras (não precisa do nome técnico ainda)
- Estimem o dano possível se isso chegar à produção
- Proponham a correção

*⏱️ Tempo: 30 minutos  |  👥 Formato: em duplas  |  Todo o código é fictício e roda apenas no laboratório.*

> **Nomes:** Matheus Braga de Sousa   **Turma:** ____________________   **Data:** 17/09/2026

## VULNERABILIDADE 01 — A busca de clientes

> `GET /api/clientes/buscar?nome=...`

Endpoint de busca usado pela tela de atendimento. O parâmetro nome vem direto da caixa de busca do site.

```text
 1  [HttpGet("buscar")]
 2  public IActionResult Buscar(string nome)
 3  {
 4      var sql = "SELECT * FROM Clientes WHERE Nome = '"
 5                + nome + "'";
 6      var clientes = _db.Clientes.FromSqlRaw(sql).ToList();
 7      return Ok(clientes);
 8  }
```

**Sua análise:**

1. Qual é a falha?
 var sql = "SELECT * FROM Clientes WHERE Nome = '"
                 + nome + "'";

2. Qual o dano possível em produção?
Injection, coletar todas as tabelas.

3. Como corrigir?
_db.Clientes.FromSqlInterpolated(
  $"SELECT * FROM Clientes" +
  $" WHERE Nome = {nome}");

## VULNERABILIDADE 02 — A consulta de faturas

> `GET /api/faturas/{id}`

Endpoint usado pelo app para exibir a fatura do cartão. O usuário está autenticado quando chama esta rota.

```text
 1  [HttpGet("{id}")]
 2  public IActionResult GetFatura(int id)
 3  {
 4      var fatura = _db.Faturas.Find(id);
 5      if (fatura == null) return NotFound();
 6      return Ok(fatura);
 7  }
```

**Sua análise:**

1. Qual é a falha?
var fatura = _db.Faturas.Find(id);
       if (fatura == null) return NotFound();

2. Qual o dano possível em produção?
Broken Acess Control, pegar os dados de outro cliente.

3. Como corrigir?
var fatura = _db.Faturas.Find(id);
if (fatura == null) return NotFound();
if (fatura.ClienteId != usuarioLogadoId)
    return Forbid();  // 403


## VULNERABILIDADE 03 — A configuração do servidor

> `Program.cs (roda igual em dev e em produção)`

Trecho de inicialização da API, idêntico em todos os ambientes. Este arquivo está versionado no Git da empresa.

```text
 1  public const string Conn =
 2      "Server=prod-db;Database=Banco;User=sa;" +
 3      "Password=Newton@2026!";
 4
 5  var app = WebApplication.CreateBuilder(args).Build();
 6  app.UseDeveloperExceptionPage();
 7  app.Run();
```

**Sua análise:**

1. Qual é a falha?
"Server=prod-db;Database=Banco;User=sa;" +
      "Password=Newton@2026!";

2. Qual o dano possível em produção?
Security Misconfiguration, acesso ao "admin" do programa, desenvolvedor.

3. Como corrigir?
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

## VULNERABILIDADE 04 — A atualização de perfil

> `PUT /api/usuarios/{id}`

Endpoint que o app chama quando o usuário edita o próprio perfil. O corpo da requisição é o JSON enviado pelo cliente.

```text
 1  public class UsuarioUpdate
 2  {
 3      public string Nome  { get; set; }
 4      public string Email { get; set; }
 5      public string Role  { get; set; }   // "user" | "admin"
 6  }
 7
 8  [HttpPut("{id}")]
 9  public IActionResult Atualizar(int id, UsuarioUpdate dto)
10  {
11      _repo.AtualizarTudo(id, dto);
12      return NoContent();
13  }
```

**Sua análise:**

1. Qual é a falha?
public string Role  { get; set; }   // "user" | "admin"

2. Qual o dano possível em produção?
Confiar no cliente para decidir o nível de acesso. É um risco de controle de acesso.

3. Como corrigir?
public string Nome  { get; set; }
  public string Email { get; set; }
  // Role NÃO existe no contrato

## DESAFIO

1. Qual das 4 falhas um scanner automático de código teria MAIS dificuldade de encontrar? Por quê?
Broken Access Control, seria uma falha de autorização, não um padrão de código que seria mais fácil de achar.
