using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuporteApi.Models;

// Documento da coleção "clientes".
public class Cliente
{
    // Na API o Id é texto; no banco é gravado como ObjectId.
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Plano { get; set; } = "Basico"; // "Basico" ou "Completo"
    public bool Ativo { get; set; } = true;
}
