using MongoDB.Bson;
using SuporteApi.Dtos;
using SuporteApi.Erros;
using SuporteApi.Models;
using SuporteApi.Repositories;

namespace SuporteApi.Services;

// Regras de negócio de clientes. Depende só das interfaces dos repositórios,
// por isso pode ser testado com repositórios falsos (sem banco).
public class ClienteService(IClienteRepository clientes, IChamadoRepository chamados)
{
    public const int TamanhoMaximoPagina = 100;

    public Task<PaginaResultado<Cliente>> ListarAsync(int page, int size)
    {
        ValidarPaginacao(page, size);
        return clientes.ListarAsync(page, size);
    }

    public async Task<Cliente> ObterAsync(string id)
    {
        ValidarId(id);
        return await clientes.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException($"Cliente {id} não encontrado.");
    }

    // Rota aninhada /clientes/{id}/chamados: o cliente precisa existir.
    public async Task<PaginaResultado<Chamado>> ListarChamadosAsync(string id, int page, int size)
    {
        ValidarPaginacao(page, size);
        await ObterAsync(id);
        return await chamados.ListarPorClienteAsync(id, page, size);
    }

    public async Task<Cliente> CriarAsync(CriarClienteRequest dados)
    {
        var email = NormalizarEmail(dados.Email);
        if (await clientes.ObterPorEmailAsync(email) is not null)
            throw new ConflitoException($"Já existe um cliente com o e-mail {email}.");

        // Só os campos do DTO são copiados; Id e Ativo são definidos pelo servidor.
        var cliente = new Cliente
        {
            Nome = dados.Nome.Trim(),
            Email = email,
            Telefone = dados.Telefone.Trim(),
            Plano = dados.Plano,
            Ativo = true
        };
        await clientes.CriarAsync(cliente);
        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(string id, AtualizarClienteRequest dados)
    {
        var cliente = await ObterAsync(id);

        // Pode manter o próprio e-mail, mas não usar o de outro cliente.
        var email = NormalizarEmail(dados.Email);
        var donoDoEmail = await clientes.ObterPorEmailAsync(email);
        if (donoDoEmail is not null && donoDoEmail.Id != id)
            throw new ConflitoException($"Já existe um cliente com o e-mail {email}.");

        cliente.Nome = dados.Nome.Trim();
        cliente.Email = email;
        cliente.Telefone = dados.Telefone.Trim();
        cliente.Plano = dados.Plano;
        cliente.Ativo = dados.Ativo;

        if (!await clientes.AtualizarAsync(cliente))
            throw new NaoEncontradoException($"Cliente {id} não encontrado.");
        return cliente;
    }

    public async Task RemoverAsync(string id)
    {
        ValidarId(id);

        // Não apaga cliente com histórico: os chamados ficariam órfãos.
        if (await chamados.ExisteParaClienteAsync(id))
            throw new ConflitoException("O cliente possui chamados e não pode ser removido. Desative-o pelo PUT.");

        if (!await clientes.RemoverAsync(id))
            throw new NaoEncontradoException($"Cliente {id} não encontrado.");
    }

    private static void ValidarId(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            throw new RegraDeNegocioException($"Id '{id}' inválido: deve ter 24 caracteres hexadecimais.");
    }

    private static void ValidarPaginacao(int page, int size)
    {
        if (page < 1)
            throw new RegraDeNegocioException("O parâmetro page deve ser maior ou igual a 1.");
        if (size < 1 || size > TamanhoMaximoPagina)
            throw new RegraDeNegocioException($"O parâmetro size deve estar entre 1 e {TamanhoMaximoPagina}.");
    }

    // E-mail comparado sem diferença de maiúsculas e espaços.
    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
