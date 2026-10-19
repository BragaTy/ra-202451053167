using System.Text;
using BibliotecaApi.Autorizacao;
using BibliotecaApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole apenas o token JWT gerado no login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Acervo em memória (compartilhado por toda a aplicação).
builder.Services.AddSingleton<BibliotecaStore>();

// ── Autenticação JWT — exatamente como ficou na Aula 08 ─────────────
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
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

// ── Autorização: o lugar CENTRAL das regras de acesso ───────────────
builder.Services.AddAuthorization(options =>
{
    // RBAC (Falha 1 e 2): policies baseadas na matriz de acesso.
    options.AddPolicy("GerenciarAcervo", p => p.RequireRole("Bibliotecario", "Admin"));
    options.AddPolicy("RemoverLivro", p => p.RequireRole("Admin"));
    options.AddPolicy("VerRelatorios", p => p.RequireRole("Admin"));

    // ABAC (Falha 3): a policy que decide olhando o RECURSO.
    options.AddPolicy("PodeRenovarEmprestimo",
        p => p.Requirements.Add(new DonoDoEmprestimoRequirement()));

    // FALHA 2 CORRIGIDA: Negar por padrão (endpoints sem atributo exigem autenticação).
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddSingleton<IAuthorizationHandler, DonoDoEmprestimoHandler>();

// ── PBAC (Falha 4): políticas fora do código, recarregadas a quente ─
builder.Configuration.AddJsonFile("politicas.json", optional: false, reloadOnChange: true);
builder.Services.Configure<PoliticasOptions>(builder.Configuration.GetSection("Politicas"));
builder.Services.AddSingleton<MotorDePoliticas>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
