namespace SuporteApi.Data;

// Lido da seção "MongoDb" da configuração. Na prática vem das variáveis de
// ambiente MongoDb__ConnectionString e MongoDb__Database ("__" vira ":").
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "";
    public string Database { get; set; } = "";
}
