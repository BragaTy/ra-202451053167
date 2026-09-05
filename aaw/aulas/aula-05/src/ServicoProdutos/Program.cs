using Microsoft.EntityFrameworkCore;
using ServicoProdutos.Models;
using ServicoProdutos.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// A ESCOLHA da persistência mora na CONFIGURAÇÃO, não no código.
// Troque em appsettings.json:  "Persistencia": "Sql"  |  "Documento"
var persistencia = builder.Configuration["Persistencia"] ?? "Sql";

if (persistencia.Equals("Documento", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IProdutoRepositorio, ProdutoRepositorioDocumento>();
}
else
{
    builder.Services.AddDbContext<ProdutosDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=produtos.db"));
    builder.Services.AddScoped<IProdutoRepositorio, ProdutoRepositorioSql>();
}

var app = builder.Build();

// Cria o banco e as sementes na primeira execução (apenas no modo Sql)
if (!persistencia.Equals("Documento", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProdutosDbContext>();
    db.Database.EnsureCreated();
    if (!db.Produtos.Any())
    {
        db.Produtos.AddRange(
            new Produto { Nome = "Clean Architecture", Preco = 89.90m, Estoque = 12 },
            new Produto { Nome = "Domain-Driven Design", Preco = 120.00m, Estoque = 5 },
            new Produto { Nome = "Criando Microsservicos", Preco = 149.90m, Estoque = 8 });
        db.SaveChanges();
    }
}

app.MapGet("/", () => Results.Ok(new
{
    servico = "ServicoProdutos — Aula 05 (Persistência Distribuída)",
    persistenciaAtiva = persistencia,
    endpoints = new[] { "GET /produtos", "GET /produtos/{id}", "GET /produtos/barato?max=100", "POST /produtos" }
}));

app.MapGet("/produtos", (IProdutoRepositorio repo) =>
    Results.Ok(repo.ObterTodos()));

app.MapGet("/produtos/{id:int}", (int id, IProdutoRepositorio repo) =>
    repo.ObterPorId(id) is Produto p ? Results.Ok(p) : Results.NotFound());

app.MapGet("/produtos/barato", (decimal max, IProdutoRepositorio repo) =>
    Results.Ok(repo.ObterAbaixoDe(max)));

app.MapPost("/produtos", (Produto produto, IProdutoRepositorio repo) =>
{
    if (string.IsNullOrWhiteSpace(produto.Nome) || produto.Preco <= 0)
        return Results.BadRequest(new { erro = "Nome é obrigatório e Preco deve ser maior que zero." });
    var criado = repo.Criar(produto);
    return Results.Created($"/produtos/{criado.Id}", criado);
});

app.Run();
