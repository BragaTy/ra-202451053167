using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuporteApi.Models;

// Documento da coleção "chamados". Relacionamento 1-N por referência:
// o chamado guarda o ClienteId em vez de ficar embutido dentro do cliente.
public class Chamado
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    [BsonRepresentation(BsonType.ObjectId)]
    public string ClienteId { get; set; } = "";

    public string Assunto { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Prioridade { get; set; } = "Baixa"; // "Baixa", "Media" ou "Alta"
    public int PrazoHoras { get; set; }               // calculado pelo serviço (SLA)
    public string Status { get; set; } = "Aberto";    // "Aberto", "EmAtendimento" ou "Concluido"

    [BsonRepresentation(BsonType.ObjectId)]
    public string CriadoPorId { get; set; } = "";     // dono do chamado (controle de IDOR)

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
}
