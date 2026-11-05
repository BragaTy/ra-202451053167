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
