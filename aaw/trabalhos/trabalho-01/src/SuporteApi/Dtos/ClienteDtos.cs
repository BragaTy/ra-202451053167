using System.ComponentModel.DataAnnotations;

namespace SuporteApi.Dtos;

// Todo cliente nasce ativo; Ativo só muda pelo PUT.
public record CriarClienteRequest(
    [Required, StringLength(100)] string Nome,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Telefone,
    [Required, RegularExpression("^(Basico|Completo)$")] string Plano);

public record AtualizarClienteRequest(
    [Required, StringLength(100)] string Nome,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Telefone,
    [Required, RegularExpression("^(Basico|Completo)$")] string Plano,
    bool Ativo);
