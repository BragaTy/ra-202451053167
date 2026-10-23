namespace BibliotecaApi.Models;

/// <summary>Representa uma página de resultados com metadados de paginação.</summary>
/// <typeparam name="T">Tipo dos itens retornados.</typeparam>
public class RespostaPaginada<T>
{
    /// <summary>Itens da página atual.</summary>
    public List<T> Itens { get; set; } = new();

    /// <summary>Número da página atual, começando em 1.</summary>
    public int Pagina { get; set; }

    /// <summary>Quantidade máxima de itens por página.</summary>
    public int TamanhoPagina { get; set; }

    /// <summary>Quantidade total de itens disponíveis.</summary>
    public int TotalItens { get; set; }

    /// <summary>Quantidade total de páginas disponíveis.</summary>
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
}
