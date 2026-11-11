using SuporteApi.Data;
using SuporteApi.Erros;
using SuporteApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// MongoDB: conexão vinda das variáveis de ambiente + repositórios.
builder.Services.AddMongoDb(builder.Configuration);

// Camada de serviço (regras de negócio).
builder.Services.AddScoped<ClienteService>();

// Erros no padrão Problem Details (RFC 9457), tratados em um lugar só.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorDeErros>();

var app = builder.Build();

await app.Services.CriarIndicesMongoAsync();

app.UseExceptionHandler();

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
