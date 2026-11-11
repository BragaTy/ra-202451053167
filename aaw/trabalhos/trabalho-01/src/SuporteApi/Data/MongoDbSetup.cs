using MongoDB.Driver;
using SuporteApi.Models;
using SuporteApi.Repositories;

namespace SuporteApi.Data;

public static class MongoDbSetup
{
    // Registra o MongoDB e os repositórios na injeção de dependência.
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("MongoDb").Get<MongoDbSettings>() ?? new MongoDbSettings();

        // Sem valor padrão no código: se a variável não existir, a API nem sobe.
        if (string.IsNullOrWhiteSpace(settings.ConnectionString) || string.IsNullOrWhiteSpace(settings.Database))
            throw new InvalidOperationException(
                "Defina as variáveis de ambiente MongoDb__ConnectionString e MongoDb__Database.");

        // MongoClient é thread-safe e deve existir uma única vez (Singleton).
        services.AddSingleton<IMongoClient>(_ => new MongoClient(settings.ConnectionString));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(settings.Database));

        // A aplicação depende da interface; a classe concreta só aparece aqui.
        services.AddSingleton<IClienteRepository, ClienteRepository>();
        services.AddSingleton<IChamadoRepository, ChamadoRepository>();

        return services;
    }

    // Índices criados na subida da API. Criar um índice que já existe não faz nada.
    public static async Task CriarIndicesMongoAsync(this IServiceProvider services)
    {
        var db = services.GetRequiredService<IMongoDatabase>();

        // E-mail único: o próprio banco barra cliente duplicado (vira 409 depois).
        await db.GetCollection<Cliente>(ClienteRepository.Colecao).Indexes.CreateOneAsync(
            new CreateIndexModel<Cliente>(
                Builders<Cliente>.IndexKeys.Ascending(c => c.Email),
                new CreateIndexOptions { Unique = true }));

        // Acelera a rota aninhada /clientes/{id}/chamados.
        await db.GetCollection<Chamado>(ChamadoRepository.Colecao).Indexes.CreateOneAsync(
            new CreateIndexModel<Chamado>(Builders<Chamado>.IndexKeys.Ascending(c => c.ClienteId)));
    }
}
