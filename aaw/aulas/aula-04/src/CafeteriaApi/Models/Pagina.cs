namespace CafeteriaApi.Models;

/// <summary>
/// Envelope de coleção paginada — o formato do slide 15:
/// { "page": 2, "size": 20, "total": 12482, "items": [...] }.
/// "total" é o que permite ao cliente montar a navegação.
/// </summary>
public class Pagina<T>
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public int TotalDePaginas => Size > 0 ? (int)Math.Ceiling(Total / (double)Size) : 0;
    public List<T> Items { get; set; } = new();
}
