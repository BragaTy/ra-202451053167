using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

/// <summary>
/// O CONTRATO de persistência. A API só conhece esta interface —
/// é ela que torna as persistências trocáveis (padrão Repository).
/// </summary>
public interface IProdutoRepositorio
{
    List<Produto> ObterTodos();
    Produto? ObterPorId(int id);

    /// <summary>Produtos com preço menor que <paramref name="precoMaximo"/>.</summary>
    List<Produto> ObterAbaixoDe(decimal precoMaximo);

    Produto Criar(Produto produto);
}
