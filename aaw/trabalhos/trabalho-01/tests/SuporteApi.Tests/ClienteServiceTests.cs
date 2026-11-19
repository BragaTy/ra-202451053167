using MongoDB.Bson;
using SuporteApi.Dtos;
using SuporteApi.Erros;
using SuporteApi.Models;
using SuporteApi.Services;
using SuporteApi.Tests.Fakes;

namespace SuporteApi.Tests;

public class ClienteServiceTests
{
    private readonly ClienteRepositoryFalso _clientes = new();
    private readonly ChamadoRepositoryFalso _chamados = new();
    private readonly ClienteService _service;

    public ClienteServiceTests() => _service = new ClienteService(_clientes, _chamados);

    [Fact]
    public async Task CriarAsync_EmailNovo_NormalizaEmailECriaAtivo()
    {
        // Arrange
        var dados = new CriarClienteRequest("Padaria Central", "  Contato@Padaria.COM ", "11999998888", "Basico");

        // Act
        var cliente = await _service.CriarAsync(dados);

        // Assert
        Assert.Equal("contato@padaria.com", cliente.Email);
        Assert.True(cliente.Ativo);
        Assert.Single(_clientes.Itens);
    }

    [Fact]
    public async Task CriarAsync_EmailJaCadastrado_LancaConflito()
    {
        // Arrange
        _clientes.Itens.Add(new Cliente { Id = ObjectId.GenerateNewId().ToString(), Email = "contato@padaria.com" });
        var dados = new CriarClienteRequest("Outra", "CONTATO@padaria.com", "11999998888", "Basico");

        // Act + Assert
        await Assert.ThrowsAsync<ConflitoException>(() => _service.CriarAsync(dados));
        Assert.Single(_clientes.Itens);
    }

    [Fact]
    public async Task RemoverAsync_ClienteComChamados_LancaConflito()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        _clientes.Itens.Add(new Cliente { Id = id, Email = "a@b.com" });
        _chamados.Itens.Add(new Chamado { Id = ObjectId.GenerateNewId().ToString(), ClienteId = id });

        // Act + Assert
        await Assert.ThrowsAsync<ConflitoException>(() => _service.RemoverAsync(id));
        Assert.Single(_clientes.Itens);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    public async Task ObterAsync_IdForaDoFormato_LancaRegraDeNegocio(string id)
    {
        // Act + Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _service.ObterAsync(id));
    }
}
