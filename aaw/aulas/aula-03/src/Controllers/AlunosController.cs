using EscolaApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Controllers;

[ApiController]
[Route("api/v1/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext db;

    public AlunosController(AppDbContext db)
    {
        this.db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1 || size > 50)
        {
            return Problem(
                title: "Parâmetros de paginação inválidos",
                detail: "page deve ser maior ou igual a 1 e size deve ficar entre 1 e 50.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var total = await db.Alunos.CountAsync();

        var alunos = await db.Alunos
            .OrderBy(a => a.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new
            {
                a.Id,
                a.Nome,
                a.Curso
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            size,
            totalItens = total,
            totalPaginas = (int)Math.Ceiling(total / (double)size),
            itens = alunos
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var aluno = await db.Alunos
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Nome,
                a.Curso,
                Matriculas = a.Matriculas.Select(m => new
                {
                    m.Id,
                    m.Disciplina,
                    m.Semestre,
                    m.Ativa,
                    m.AlunoId
                })
            })
            .FirstOrDefaultAsync();

        if (aluno is null)
        {
            return Problem(
                title: "Aluno não encontrado",
                detail: $"Não existe aluno com o id {id}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(aluno);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var aluno = await db.Alunos.FindAsync(id);

        if (aluno is null)
        {
            return Problem(
                title: "Aluno não encontrado",
                detail: $"Não existe aluno com o id {id}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/matriculas")]
    public async Task<IActionResult> GetMatriculas(int id)
    {
        var alunoExiste = await db.Alunos.AnyAsync(a => a.Id == id);

        if (!alunoExiste)
        {
            return Problem(
                title: "Aluno não encontrado",
                detail: $"Não existe aluno com o id {id}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        var matriculas = await db.Matriculas
            .Where(m => m.AlunoId == id)
            .Select(m => new
            {
                m.Id,
                m.Disciplina,
                m.Semestre,
                m.Ativa,
                m.AlunoId
            })
            .ToListAsync();

        return Ok(matriculas);
    }

    [HttpGet("{id:int}/matriculas/{matriculaId:int}")]
    public async Task<IActionResult> GetMatricula(int id, int matriculaId)
    {
        var matricula = await db.Matriculas
            .Where(m => m.Id == matriculaId && m.AlunoId == id)
            .Select(m => new
            {
                m.Id,
                m.Disciplina,
                m.Semestre,
                m.Ativa,
                m.AlunoId
            })
            .FirstOrDefaultAsync();

        if (matricula is null)
        {
            return Problem(
                title: "Matrícula não encontrada",
                detail: $"A matrícula {matriculaId} não pertence ao aluno {id}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(matricula);
    }
}