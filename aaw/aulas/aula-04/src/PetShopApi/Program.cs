using Microsoft.OpenApi.Models;
using PetShopApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Banco em memória compartilhado por todas as requisições.
builder.Services.AddSingleton<PetShopStore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PetHouse API",
        Version = "v1",
        Description =
            "API REST do PetHouse - VERSAO PARA AUDITORIA. " +
            "Tudo aqui funciona: nenhuma requisicao quebra. " +
            "Mas CADA endpoint viola UMA regra ou diretriz REST vista em aula. " +
            "Sao 12 erros. Encontre, nomeie e proponha o redesenho - " +
            "compare com a API do Cafe Newton (porta 5301), que faz tudo certo."
    });

    // Sem isso o Swagger reclama de acoes diferentes com a mesma rota/metodo.
    opcoes.ResolveConflictingActions(apis => apis.First());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "PetHouse API v1");
    opcoes.DocumentTitle = "PetHouse API - cace os 12 erros";
});

// Abrir a raiz leva direto para a documentacao.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
