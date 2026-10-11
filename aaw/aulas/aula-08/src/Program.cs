using System.Text;
using BibliotecaApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Acervo em memória (compartilhado por toda a aplicação).
builder.Services.AddSingleton<BibliotecaStore>();

// ╔══════════════════════════════════════════════════════════════════╗
// ║  ETAPA 1 — Configurar a autenticação JWT                         ║
// ╚══════════════════════════════════════════════════════════════════╝
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
        ValidateLifetime = true,          // token expirado => 401
        ValidateIssuerSigningKey = true,  // assinatura inválida => 401
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!)),
        ClockSkew = TimeSpan.Zero         // sem os 5 min de tolerância padrão
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();   // quem é você?  (lê e valida o token)
app.UseAuthorization();    // o que pode fazer? (checa [Authorize])

app.MapControllers();

app.Run();
