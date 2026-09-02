using CafeteriaApi.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Banco em memória compartilhado por todas as requisições.
builder.Services.AddSingleton<CafeteriaStore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cafe Newton API",
        Version = "v1",
        Description =
            "API REST do Cafe Newton - VERSAO CORRETA. " +
            "Serve de referencia do que e um bom design: URIs de recursos no plural, " +
            "verbo no metodo HTTP, status codes corretos, versionamento no path, " +
            "paginacao, ETag/Cache-Control e nenhuma sessao no servidor."
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Cafe Newton API v1");
    opcoes.DocumentTitle = "Cafe Newton API - versao correta";
});

// Abrir a raiz leva direto para a documentacao.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
