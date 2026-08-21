using EscolaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
}
