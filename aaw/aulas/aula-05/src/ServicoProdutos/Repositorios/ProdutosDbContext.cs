using Microsoft.EntityFrameworkCore;
using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

public class ProdutosDbContext : DbContext
{
    public ProdutosDbContext(DbContextOptions<ProdutosDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
}
