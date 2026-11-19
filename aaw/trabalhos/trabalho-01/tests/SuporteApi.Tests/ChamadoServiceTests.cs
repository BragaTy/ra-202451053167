using MongoDB.Bson;
using SuporteApi.Dtos;
using SuporteApi.Erros;
using SuporteApi.Models;
using SuporteApi.Services;
using SuporteApi.Tests.Fakes;

namespace SuporteApi.Tests;

// Padrão AAA (Arrange, Act, Assert) e nomes Metodo_Cenario_ResultadoEsperado.
public class ChamadoServiceTests
{
    private readonly ClienteRepositoryFalso _clientes = new();
    private readonly ChamadoRepositoryFalso _chamados = new();
    private readonly ChamadoService _service;
    private readonly string _usuarioId = ObjectId.GenerateNewId().ToString();

    public ChamadoServiceTests() => _service = new ChamadoService(_chamados, _clientes);

    private Cliente NovoCliente(string plano = "Basico", bool ativo = true)
    {
        var cliente = new Cliente
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Nome = "Padaria Central",
            Email = "contato@padaria.com",
            Telefone = "11999998888",
            Plano = plano,
            Ativo = ativo
        };
        _clientes.Itens.Add(cliente);
        return cliente;
    }

    private Chamado NovoChamado(string status, string criadoPorId)
    {
        var chamado = new Chamado
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ClienteId = ObjectId.GenerateNewId().ToString(),
            Assunto = "Sistema fora do ar",
            Descricao = "Painel não abre",
            Prioridade = "Alta",
            PrazoHoras = 8,
            Status = status,
            CriadoPorId = criadoPorId
        };
        _chamados.Itens.Add(chamado);
        return chamado;
    }

    // ---- Sucesso ----

    [Theory]
    [InlineData("Alta", "Basico", 8)]
    [InlineData("Media", "Basico", 24)]
    [InlineData("Baixa", "Basico", 48)]
    [InlineData("Alta", "Completo", 4)]
    [InlineData("Media", "Completo", 12)]
    [InlineData("Baixa", "Completo", 24)]
    public void CalcularPrazoHoras_PrioridadeEPlano_RetornaSlaCorreto(string prioridade, string plano, int esperado)
    {
        // Act
        var prazo = ChamadoService.CalcularPrazoHoras(prioridade, plano);

        // Assert
        Assert.Equal(esperado, prazo);
    }

    [Fact]
    public async Task AbrirAsync_ClienteAtivoPlanoCompleto_CriaChamadoAbertoComPrazoEDono()
    {
        // Arrange
        var cliente = NovoCliente(plano: "Completo");
        var dados = new CriarChamadoRequest(cliente.Id, "Sistema fora do ar", "Painel não abre", "Alta");

        // Act
        var chamado = await _service.AbrirAsync(dados, _usuarioId);

        // Assert
        Assert.Equal(4, chamado.PrazoHoras);
        Assert.Equal("Aberto", chamado.Status);
        Assert.Equal(_usuarioId, chamado.CriadoPorId);
        Assert.Single(_chamados.Itens);
    }

    [Fact]
    public async Task AtualizarAsync_AdminEmChamadoDeOutroUsuario_AtualizaStatus()
    {
        // Arrange
        var chamado = NovoChamado("Aberto", criadoPorId: ObjectId.GenerateNewId().ToString());
        var dados = new AtualizarChamadoRequest("Sistema fora do ar", "Em análise", "EmAtendimento");

        // Act
        var atualizado = await _service.AtualizarAsync(chamado.Id, dados, _usuarioId, ehAdmin: true);

        // Assert
        Assert.Equal("EmAtendimento", atualizado.Status);
    }

    // ---- Erro ----

    [Fact]
    public async Task AbrirAsync_ClienteInativo_LancaRegraDeNegocio()
    {
        // Arrange
        var cliente = NovoCliente(ativo: false);
        var dados = new CriarChamadoRequest(cliente.Id, "Erro no login", "Não entra", "Media");

        // Act + Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(() => _service.AbrirAsync(dados, _usuarioId));
        Assert.Empty(_chamados.Itens);
    }

    [Fact]
    public async Task AbrirAsync_ClienteInexistente_LancaNaoEncontrado()
    {
        // Arrange
        var dados = new CriarChamadoRequest(ObjectId.GenerateNewId().ToString(), "Erro", "Detalhe", "Baixa");

        // Act + Assert
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _service.AbrirAsync(dados, _usuarioId));
    }

    [Theory]
    [InlineData("Aberto")]
    [InlineData("EmAtendimento")]
    public async Task AtualizarAsync_ChamadoConcluido_NaoPodeSerReaberto(string novoStatus)
    {
        // Arrange
        var chamado = NovoChamado("Concluido", _usuarioId);
        var dados = new AtualizarChamadoRequest("Sistema fora do ar", "Voltou o erro", novoStatus);

        // Act + Assert
        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.AtualizarAsync(chamado.Id, dados, _usuarioId, ehAdmin: false));
        Assert.Equal("Concluido", chamado.Status);
    }

    [Fact]
    public async Task AtualizarAsync_UsuarioQueNaoEDono_LancaAcessoNegado()
    {
        // Arrange: chamado aberto por OUTRO usuário (tentativa de IDOR).
        var chamado = NovoChamado("Aberto", criadoPorId: ObjectId.GenerateNewId().ToString());
        var dados = new AtualizarChamadoRequest("Alterado", "Alterado", "Concluido");

        // Act + Assert
        await Assert.ThrowsAsync<AcessoNegadoException>(
            () => _service.AtualizarAsync(chamado.Id, dados, _usuarioId, ehAdmin: false));
        Assert.Equal("Aberto", chamado.Status);
    }
}
