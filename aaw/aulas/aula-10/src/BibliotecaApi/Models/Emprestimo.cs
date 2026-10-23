using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Models;

/// <summary>Representa um empréstimo de livro feito por um usuário.</summary>
public class Emprestimo
{
    /// <summary>Identificador único do empréstimo.</summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>Identificador do livro emprestado.</summary>
    /// <example>1</example>
    [Required]
    public int LivroId { get; set; }

    /// <summary>Nome da pessoa que realizou o empréstimo.</summary>
    /// <example>João Silva</example>
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    [StringLength(200)]
    public string NomeUsuario { get; set; } = string.Empty;

    /// <summary>Data e hora em que o empréstimo foi realizado.</summary>
    public DateTime DataEmprestimo { get; set; } = DateTime.UtcNow;

    /// <summary>Data prevista para devolução do livro.</summary>
    public DateTime DataDevolucaoPrevista { get; set; } = DateTime.UtcNow.AddDays(14);

    /// <summary>Data efetiva de devolução, quando o livro já foi devolvido.</summary>
    public DateTime? DataDevolucaoEfetiva { get; set; }

    /// <summary>Situação atual do empréstimo.</summary>
    public StatusEmprestimo Status { get; set; } = StatusEmprestimo.Ativo;
}

/// <summary>Estados possíveis de um empréstimo.</summary>
public enum StatusEmprestimo
{
    /// <summary>Empréstimo em andamento.</summary>
    Ativo,
    /// <summary>Livro devolvido.</summary>
    Devolvido,
    /// <summary>Prazo de devolução vencido.</summary>
    Atrasado
}
