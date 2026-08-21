using BibliotecaApi.Models;

namespace BibliotecaApi.Repositories;

public class LivroRepository
{
    private readonly List<Livro> _livros = new()
    {
        new Livro { Id = 1, Titulo = "Clean Architecture",        Autor = "Robert C. Martin", Ano = 2017, Disponivel = true },
        new Livro { Id = 2, Titulo = "Domain-Driven Design",      Autor = "Eric Evans",       Ano = 2003, Disponivel = false },
        new Livro { Id = 3, Titulo = "Criando Microsserviços",    Autor = "Sam Newman",       Ano = 2022, Disponivel = true },
    };

    private int _nextId = 4;

    public List<Livro> GetAll() => _livros;

    public Livro? GetById(int id) => _livros.FirstOrDefault(l => l.Id == id);

    public Livro Create(Livro livro)
    {
        livro.Id = _nextId++;
        _livros.Add(livro);
        return livro;
    }

    public Livro? Update(int id, Livro atualizado)
    {
        var existente = GetById(id);
        if (existente is null) return null;

        existente.Titulo = atualizado.Titulo;
        existente.Autor = atualizado.Autor;
        existente.Ano = atualizado.Ano;
        existente.Disponivel = atualizado.Disponivel;

        return existente;
    }

    public bool Delete(int id)
    {
        var livro = GetById(id);
        if (livro is null) return false;

        _livros.Remove(livro);
        return true;
    }
}
