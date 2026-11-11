namespace SuporteApi.Models;

// Resposta das listagens paginadas (?page=1&size=20).
public record PaginaResultado<T>(IReadOnlyList<T> Itens, int Page, int Size, long Total);
