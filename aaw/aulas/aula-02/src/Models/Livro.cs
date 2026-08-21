using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Models;

public class Livro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O autor é obrigatório.")]
    public string Autor { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "Ano deve estar entre 1450 e 2100.")]
    public int Ano { get; set; }

    public bool Disponivel { get; set; } = true;
}
