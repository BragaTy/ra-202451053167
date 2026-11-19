using SuporteApi.Dtos;
using SuporteApi.Erros;
using SuporteApi.Models;
using SuporteApi.Repositories;

namespace SuporteApi.Services;

// Regras de negócio dos chamados (proposta, seção 4).
public class ChamadoService(IChamadoRepository chamados, IClienteRepository clientes)
{
    public static readonly string[] Prioridades = ["Baixa", "Media", "Alta"];
    public static readonly string[] StatusValidos = ["Aberto", "EmAtendimento", "Concluido"];

    // Regra 1 — SLA: Alta = 8h, Media = 24h, Baixa = 48h; plano "Completo" tem metade do prazo.
    public static int CalcularPrazoHoras(string prioridade, string plano)
    {
        var horas = prioridade switch
        {
            "Alta" => 8,
            "Media" => 24,
            "Baixa" => 48,
            _ => throw new RegraDeNegocioException($"Prioridade '{prioridade}' inválida. Use Baixa, Media ou Alta.")
        };
        return plano == "Completo" ? horas / 2 : horas;
    }

    public Task<PaginaResultado<Chamado>> ListarAsync(int page, int size, string? status, string? prioridade)
    {
        Validacao.Paginacao(page, size);
        if (!string.IsNullOrWhiteSpace(status) && !StatusValidos.Contains(status))
            throw new RegraDeNegocioException("Filtro status inválido. Use Aberto, EmAtendimento ou Concluido.");
        if (!string.IsNullOrWhiteSpace(prioridade) && !Prioridades.Contains(prioridade))
            throw new RegraDeNegocioException("Filtro prioridade inválido. Use Baixa, Media ou Alta.");
        return chamados.ListarAsync(page, size, status, prioridade);
    }

    public async Task<Chamado> ObterAsync(string id)
    {
        Validacao.Id(id);
        return await chamados.ObterPorIdAsync(id)
            ?? throw new NaoEncontradoException($"Chamado {id} não encontrado.");
    }

    public async Task<Chamado> AbrirAsync(CriarChamadoRequest dados, string usuarioId)
    {
        Validacao.Id(dados.ClienteId, "ClienteId");
        var cliente = await clientes.ObterPorIdAsync(dados.ClienteId)
            ?? throw new NaoEncontradoException($"Cliente {dados.ClienteId} não encontrado.");

        // Regra 2: não abre chamado para cliente inativo.
        if (!cliente.Ativo)
            throw new RegraDeNegocioException("Não é possível abrir chamado para um cliente inativo.");

        // PrazoHoras, Status, dono e data são definidos aqui, nunca pelo cliente da API.
        var chamado = new Chamado
        {
            ClienteId = cliente.Id,
            Assunto = dados.Assunto.Trim(),
            Descricao = dados.Descricao.Trim(),
            Prioridade = dados.Prioridade,
            PrazoHoras = CalcularPrazoHoras(dados.Prioridade, cliente.Plano),
            Status = "Aberto",
            CriadoPorId = usuarioId,
            DataAbertura = DateTime.UtcNow
        };
        await chamados.CriarAsync(chamado);
        return chamado;
    }

    public async Task<Chamado> AtualizarAsync(string id, AtualizarChamadoRequest dados, string usuarioId, bool ehAdmin)
    {
        var chamado = await ObterAsync(id);

        // Ownership (contra IDOR): usuário comum só altera o chamado que ele abriu.
        if (!ehAdmin && chamado.CriadoPorId != usuarioId)
            throw new AcessoNegadoException("Você só pode alterar chamados abertos por você.");

        // Regra 3: chamado concluído não volta para Aberto nem EmAtendimento.
        if (chamado.Status == "Concluido" && dados.Status != "Concluido")
            throw new RegraDeNegocioException("Chamado concluído não pode ser reaberto.");

        chamado.Assunto = dados.Assunto.Trim();
        chamado.Descricao = dados.Descricao.Trim();
        chamado.Status = dados.Status;

        if (!await chamados.AtualizarAsync(chamado))
            throw new NaoEncontradoException($"Chamado {id} não encontrado.");
        return chamado;
    }

    public async Task RemoverAsync(string id)
    {
        Validacao.Id(id);
        if (!await chamados.RemoverAsync(id))
            throw new NaoEncontradoException($"Chamado {id} não encontrado.");
    }
}
