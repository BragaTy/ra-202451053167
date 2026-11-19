using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SuporteApi.Models;

// Documento da coleção "usuarios": quem acessa o sistema (atendentes e gestor).
public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = "";

    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";

    // Nunca guardamos a senha, só o hash (PBKDF2 com salt, via PasswordHasher).
    public string SenhaHash { get; set; } = "";

    public string Perfil { get; set; } = Perfis.Usuario;
}

// Perfis do RBAC. Vão na claim "role" do JWT.
public static class Perfis
{
    public const string Admin = "admin";
    public const string Usuario = "usuario";
}
