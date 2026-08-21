namespace EscolaApi.Models;

public class Aluno
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
