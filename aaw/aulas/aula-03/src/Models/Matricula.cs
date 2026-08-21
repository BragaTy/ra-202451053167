namespace EscolaApi.Models;

public class Matricula
{
    public int Id { get; set; }
    public string Disciplina { get; set; } = string.Empty;
    public string Semestre { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }
}
