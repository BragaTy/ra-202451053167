using System.ComponentModel.DataAnnotations;

namespace SuporteApi.Dtos;

// Sem campo de perfil: todo registro nasce como "usuario" (evita Mass Assignment).
public record RegistrarRequest(
    [Required, StringLength(100)] string Nome,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Senha);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha);

// Respostas: nunca devolvem o hash da senha.
public record UsuarioResponse(string Id, string Nome, string Email, string Perfil);

public record LoginResponse(string Token, DateTime ExpiraEm, string Nome, string Perfil);
