namespace ServicoProdutos.Models;

/// <summary>
/// O agregado da nossa aula. Repare que ele é usado IGUAL pelas duas
/// persistências — quem muda é o repositório, não o domínio.
/// </summary>
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }

    // EXERCÍCIO DE EVOLUÇÃO DE ESQUEMA (parte final da prática):
    // descomente a linha abaixo, rode nas DUAS persistências e observe:
    // - no documento (JSON): os arquivos antigos continuam legíveis?
    // - no SQLite: o que acontece sem migração?
    // public List<string> Tags { get; set; } = new();
}
