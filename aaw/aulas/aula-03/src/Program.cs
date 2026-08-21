using EscolaApi.Data;
using EscolaApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("EscolaDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed: 120 alunos com 2 matrículas cada — o suficiente para a ausência
// de paginação doer de verdade quando a lista inteira volta de uma vez.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Alunos.Any())
    {
        string[] nomes = { "Ana", "Bruno", "Carla", "Diego", "Elisa", "Fabio", "Gabriela", "Hugo", "Iara", "Joao" };
        string[] sobrenomes = { "Silva", "Costa", "Souza", "Lima", "Alves", "Ferreira", "Rocha", "Nunes", "Melo", "Pinto" };
        string[] cursos = { "Sistemas de Informação", "Ciência da Computação", "Engenharia de Software" };
        string[] disciplinas = { "Arquitetura de Aplicações Web", "Banco de Dados", "Estruturas de Dados", "Programação Web" };

        int matriculaId = 1;
        for (int i = 1; i <= 120; i++)
        {
            var aluno = new Aluno
            {
                Id = i,
                Nome = $"{nomes[i % 10]} {sobrenomes[(i / 10) % 10]}",
                Curso = cursos[i % 3],
            };
            aluno.Matriculas.Add(new Matricula { Id = matriculaId++, Disciplina = disciplinas[i % 4], Semestre = "2026.2", Ativa = true, AlunoId = i });
            aluno.Matriculas.Add(new Matricula { Id = matriculaId++, Disciplina = disciplinas[(i + 1) % 4], Semestre = "2026.1", Ativa = false, AlunoId = i });
            db.Alunos.Add(aluno);
        }
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
