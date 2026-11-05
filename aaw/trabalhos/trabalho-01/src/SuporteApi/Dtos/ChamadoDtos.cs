using System.ComponentModel.DataAnnotations;

namespace SuporteApi.Dtos;

// PrazoHoras, Status, CriadoPorId e DataAbertura ficam de fora de propósito:
// quem define esses campos é o servidor, nunca o cliente da API.
public record CriarChamadoRequest(
    [Required] string ClienteId,
    [Required, StringLength(120)] string Assunto,
    [Required] string Descricao,
    [Required, RegularExpression("^(Baixa|Media|Alta)$")] string Prioridade);

public record AtualizarChamadoRequest(
    [Required, StringLength(120)] string Assunto,
    [Required] string Descricao,
    [Required, RegularExpression("^(Aberto|EmAtendimento|Concluido)$")] string Status);
