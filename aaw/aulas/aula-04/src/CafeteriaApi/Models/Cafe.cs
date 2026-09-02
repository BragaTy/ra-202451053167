using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.Models;

/// <summary>Um café do cardápio do "Café Newton".</summary>
public class Cafe
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A origem é obrigatória.")]
    public string Origem { get; set; } = string.Empty;

    [Required(ErrorMessage = "A torra é obrigatória.")]
    [RegularExpression("clara|media|escura", ErrorMessage = "Torra deve ser: clara, media ou escura.")]
    public string Torra { get; set; } = "media";

    [Range(1.0, 200.0, ErrorMessage = "O preço deve estar entre 1 e 200 reais.")]
    public decimal Preco { get; set; }

    public bool Disponivel { get; set; } = true;

    /// <summary>
    /// Versão do recurso: muda a cada alteração. É a base do ETag —
    /// é assim que o cliente sabe se o que ele tem em cache ainda vale.
    /// </summary>
    public int Versao { get; set; } = 1;
}

/// <summary>Corpo do PATCH: tudo opcional, só o que vier é alterado.</summary>
public class CafeParcial
{
    [StringLength(80, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 80 caracteres.")]
    public string? Nome { get; set; }

    public string? Origem { get; set; }

    [RegularExpression("clara|media|escura", ErrorMessage = "Torra deve ser: clara, media ou escura.")]
    public string? Torra { get; set; }

    [Range(1.0, 200.0, ErrorMessage = "O preço deve estar entre 1 e 200 reais.")]
    public decimal? Preco { get; set; }

    public bool? Disponivel { get; set; }
}
