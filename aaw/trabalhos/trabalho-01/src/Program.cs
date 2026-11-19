using SuporteApi.Auth;
using SuporteApi.Data;
using SuporteApi.Erros;
using SuporteApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Documento OpenAPI gerado pelo .NET, com o esquema Bearer (botão Authorize).
builder.Services.AddOpenApi(o => o.AdicionarBearer());

// MongoDB: conexão vinda das variáveis de ambiente + repositórios.
builder.Services.AddMongoDb(builder.Configuration);

// JWT: chave em Jwt__Key; emissor, público e expiração no appsettings.json.
builder.Services.AddAutenticacaoJwt(builder.Configuration);

// Camada de serviço (regras de negócio).
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<AuthService>();

// Erros no padrão Problem Details (RFC 9457), tratados em um lugar só.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeErros>();

var app = builder.Build();

// Na subida: índices do Mongo e usuário admin inicial (Admin__Email / Admin__Senha).
await app.Services.CriarIndicesMongoAsync();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AuthService>()
        .GarantirAdminAsync(app.Configuration["Admin:Email"], app.Configuration["Admin:Senha"]);
}

app.UseExceptionHandler();

// 401 e 403 sem corpo viram Problem Details também.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Tela do Swagger em /swagger, lendo o documento gerado pelo .NET acima.
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "HelpDesk API v1");
        o.DocumentTitle = "HelpDesk API - Swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
