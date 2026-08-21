using EscolaApi.Data;
using EscolaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Controllers;

// ============================================================================
//  API DA ESCOLATECH — ponto de partida da prática da Aula 03.
//  Esta API FUNCIONA, mas cada endpoint carrega um anti-padrão de design
//  (os mesmos 6 do handout). Sua missão: refatorá-la para o design correto.
//  Procure os comentários "ANTI-PADRÃO N" e consulte o roteiro no README.md.
// ============================================================================
[ApiController]
public class EscolaController : ControllerBase
{
    private readonly AppDbContext db;
    public EscolaController(AppDbContext db) { this.db = db; }

    // ANTI-PADRÃO 1 — verbo na URI e ausência de versionamento (ANTI-PADRÃO 4):
    // a rota deveria ser um substantivo versionado: GET /api/v1/alunos
    // ANTI-PADRÃO 5 — sem paginação: devolve os 120 alunos (com matrículas!)
    // de uma vez. Imagine 120 mil.
    [HttpGet("getAlunos")]
    public async Task<IActionResult> GetAlunos()
    {
        var alunos = await db.Alunos.Include(a => a.Matriculas).ToListAsync();
        return Ok(alunos);
    }

    // ANTI-PADRÃO 2 — status code errado: id inexistente devolve 200 com corpo
    // nulo em vez de 404. O cliente só descobre o problema quando explode.
    [HttpGet("getAlunoPorId/{id}")]
    public async Task<IActionResult> GetAlunoPorId(int id)
    {
        var aluno = await db.Alunos.Include(a => a.Matriculas)
                                   .FirstOrDefaultAsync(a => a.Id == id);
        return Ok(aluno); // aluno pode ser null — e mesmo assim volta 200!
    }

    // ANTI-PADRÃO 1 (de novo) — POST /deletarAluno: verbo errado NA URI e
    // método HTTP errado PARA A AÇÃO (deveria ser DELETE /api/v1/alunos/{id}).
    // ANTI-PADRÃO 2 — devolve 200 com mensagem de texto em vez de 204/404.
    [HttpPost("deletarAluno")]
    public async Task<IActionResult> DeletarAluno([FromQuery] int id)
    {
        var aluno = await db.Alunos.FindAsync(id);
        if (aluno is null) return Ok("Aluno nao encontrado, mas tudo bem!");

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();
        return Ok("Aluno deletado com sucesso!");
    }

    // ANTI-PADRÃO 3 — aninhamento profundo demais: 4 níveis para chegar numa
    // matrícula. A regra prática é no MÁXIMO 2 níveis de recurso:
    // /alunos/{id}/matriculas/{matriculaId}
    [HttpGet("escola/cursos/{curso}/alunos/{alunoId}/semestres/{semestre}/matriculas/{matriculaId}")]
    public async Task<IActionResult> GetMatriculaProfunda(string curso, int alunoId, string semestre, int matriculaId)
    {
        var matricula = await db.Matriculas
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.AlunoId == alunoId);
        return Ok(matricula);
    }

    // ANTI-PADRÃO 6 — erro como HTML/texto em vez de ProblemDetails (RFC 9457):
    // o cliente recebe uma "página" de erro impossível de tratar por código.
    [HttpGet("getMatriculas/{alunoId}")]
    public async Task<IActionResult> GetMatriculas(int alunoId)
    {
        var existe = await db.Alunos.AnyAsync(a => a.Id == alunoId);
        if (!existe)
        {
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 500, // e ainda com o status errado: era caso de 404
                Content = "<html><body><h1>ERRO!!!</h1><p>Aluno nao existe. Contate o suporte.</p></body></html>",
            };
        }

        var matriculas = await db.Matriculas.Where(m => m.AlunoId == alunoId).ToListAsync();
        return Ok(matriculas);
    }
}
