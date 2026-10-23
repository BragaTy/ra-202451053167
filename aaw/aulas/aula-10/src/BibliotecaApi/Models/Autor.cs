using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Models;

/// <summary>Representa um autor de livros no sistema da biblioteca.</summary>
public class Autor
{
    /// <summary>Identificador único do autor.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Nome completo do autor.</summary>
    /// <example>Machado de Assis</example>
    [Required(ErrorMessage = "O nome do autor é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>País de origem do autor.</summary>
    /// <example>Brasil</example>
    [StringLength(100)]
    public string? Pais { get; set; }

    /// <summary>Biografia resumida do autor.</summary>
    [StringLength(1000)]
    public string? Biografia { get; set; }

    /// <summary>Livros cadastrados para este autor.</summary>
    public List<Livro> Livros { get; set; } = new();
}
