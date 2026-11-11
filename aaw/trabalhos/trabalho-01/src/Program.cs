using SuporteApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// MongoDB: conexão vinda das variáveis de ambiente + repositórios.
builder.Services.AddMongoDb(builder.Configuration);

var app = builder.Build();

await app.Services.CriarIndicesMongoAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Tela do Swagger em /swagger, lendo o documento gerado pelo .NET acima.
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "SuporteApi v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
