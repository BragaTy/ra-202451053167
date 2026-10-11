# API Biblioteca — Operação Crachá

Projeto prático da **Aula 08 — AuthN & AuthZ (JWT, OAuth 2.0 e OpenID Connect)**.

Você recebe a **API de Biblioteca da Aula 02 funcionando** — CRUD completo de livros —
mas com uma porta escancarada: **qualquer pessoa pode criar, editar e apagar livros**.
Sua missão é colocar um crachá digital na porta.

## Como rodar

```bash
cd src
dotnet run
```

- API: `http://localhost:5290`
- Swagger: `http://localhost:5290/swagger`
- Acervo em memória (sem banco): reiniciar a API restaura os dados iniciais.

## Usuários fictícios

| E-mail | Senha | Role |
|---|---|---|
| `leitor@biblioteca.com` | `leitor123` | `Leitor` |
| `biblio@biblioteca.com` | `biblio123` | `Bibliotecario` |

## Endpoints

| Método | Rota | Hoje | Depois da prática |
|---|---|---|---|
| GET | `/api/livros` | aberto | continua aberto |
| GET | `/api/livros/{id}` | aberto | continua aberto |
| POST | `/api/livros` | **aberto** | exige token |
| PUT | `/api/livros/{id}` | **aberto** | exige token |
| DELETE | `/api/livros/{id}` | **aberto** | exige token **e** role `Bibliotecario` |
| POST | `/api/auth/login` | a implementar | emite o JWT |
| POST | `/api/auth/login-expirado` | pronto | emite token já vencido (para testar 401) |
| GET | `/api/quem-sou-eu` | pronto | mostra as claims do seu token |

## Roteiro da prática (75 min, em duplas)

Procure os comentários `TODO` numerados no código — eles guiam cada etapa.

**Etapa 1 — Configurar (Program.cs, ~15 min).**
Registrar `AddAuthentication` + `AddJwtBearer` com os parâmetros de validação, e ligar
`UseAuthentication()` / `UseAuthorization()` **nessa ordem**. Os pacotes já estão no
`.csproj` (num projeto novo você rodaria `dotnet add package
Microsoft.AspNetCore.Authentication.JwtBearer`).

**Etapa 2 — Emitir o token (AuthController.cs, ~15 min).**
Implementar o `POST /api/auth/login`: validar as credenciais e devolver o JWT. O método
`GerarToken` já vem pronto — **leia-o com atenção**, é ali que as claims viram o crachá.

**Etapa 3 — Proteger a escrita (LivrosController.cs, ~10 min).**
`[Authorize]` no POST e no PUT. Sem token: **401**.

**Etapa 4 — Exigir a role (LivrosController.cs, ~10 min).**
`[Authorize(Roles = "Bibliotecario")]` no DELETE. Com token de Leitor: **403**.

**Etapa 5 — Testar o ciclo no Postman (~25 min).**
Importe `BibliotecaApi.postman_collection.json` e execute na ordem. O resultado esperado:

| # | Requisição | Status |
|---|---|---|
| 1 | POST `/api/livros` sem token | **401** |
| 2 | Login como Leitor | 200 + token |
| 3 | POST `/api/livros` com token de Leitor | **201** |
| 4 | DELETE `/api/livros/1` com token de Leitor | **403** |
| 5 | Login como Bibliotecário | 200 + token |
| 6 | DELETE `/api/livros/1` com token de Bibliotecário | **204** |
| 7 | POST com token expirado | **401** |

**Entregável:** os 7 casos acima demonstrados no Postman.

## O ponto que a aula quer fixar

**401 ≠ 403.**
`401 Unauthorized` significa "não sei quem você é" — falta token, ou ele é inválido/expirado.
`403 Forbidden` significa "sei exatamente quem você é, e mesmo assim você não pode".
O primeiro é sobre **autenticação**; o segundo, sobre **autorização**.

E lembre-se: o payload do JWT **não é criptografado**. Cole seu token em
[jwt.io](https://jwt.io) e leia o conteúdo — qualquer um consegue. A assinatura não esconde,
ela **impede a adulteração**. Por isso nunca coloque senha ou dado sensível no payload.

O gabarito completo está em `GABARITO.txt` — consulte só depois de tentar.

---

## Resposta do entregável

### 1. Tabela dos testes no Postman

| # | Requisição | Status | O que aconteceu |
|---|---|---|---|
| 1 | POST `/api/livros` sem token | **401 Unauthorized** | Não enviou o token, a API barrou porque o endpoint exige autenticação. |
| 2 | POST `/api/auth/login` (Leitor) | **200 OK** | Credenciais válidas (`leitor@biblioteca.com`), gerou e devolveu o token JWT com role `Leitor`. |
| 3 | POST `/api/livros` com token de Leitor | **201 Created** | Usuário autenticado com token válido, livro cadastrado com sucesso. |
| 4 | DELETE `/api/livros/1` com token de Leitor | **403 Forbidden** | Usuário está autenticado, mas a role `Leitor` não tem permissão de apagar (exige `Bibliotecario`). |
| 5 | POST `/api/auth/login` (Bibliotecário) | **200 OK** | Credenciais válidas (`biblio@biblioteca.com`), gerou token com role `Bibliotecario`. |
| 6 | DELETE `/api/livros/1` com token de Bibliotecário | **204 No Content** | Usuário tem a role exigida, livro removido com sucesso. |
| 7 | POST `/api/livros` com token expirado | **401 Unauthorized** | Token expirou (passou do `exp`), a validação rejeitou o token. |

### 2. Soluções das Etapas (Código em texto para cada TODO)

#### Etapa 1 — Configurar autenticação JWT (Program.cs)
```csharp
using System.Text;
using BibliotecaApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Configuração do JWT:
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,          // token vencido vira 401
        ValidateIssuerSigningKey = true,  // assinatura errada vira 401
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ClockSkew = TimeSpan.Zero         // sem tolerância de 5 min
    };
});

builder.Services.AddAuthorization();

// Middlewares na ordem correta:
app.UseAuthentication();   // 1º: quem é você?
app.UseAuthorization();    // 2º: o que você pode fazer?
```

#### Etapa 2 — Emitir o token (Controllers/AuthController.cs)
```csharp
var usuario = BibliotecaStore.Autenticar(request.Email, request.Senha);
if (usuario is null)
    return Unauthorized(new { message = "Credenciais inválidas." });

var token = GerarToken(usuario);
return Ok(new { token, nome = usuario.Nome, role = usuario.Role });
```

#### Etapa 3 — Proteger a escrita (Controllers/LivrosController.cs)
Adicionar using e proteger POST e PUT:
```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]
[HttpPost]
public ActionResult<Livro> Criar([FromBody] LivroRequest dados)

[Authorize]
[HttpPut("{id}")]
public IActionResult Atualizar(int id, [FromBody] LivroRequest dados)
```

#### Etapa 4 — Exigir a role (Controllers/LivrosController.cs)
Adicionar `[Authorize(Roles = "Bibliotecario")]` no DELETE:
```csharp
[Authorize(Roles = "Bibliotecario")]
[HttpDelete("{id}")]
public IActionResult Remover(int id) =>
    _store.Remover(id) ? NoContent() : NotFound();
```

