using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace LojaDev.LojaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly FilaDePedidos _fila;

    public PedidosController(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        FilaDePedidos fila)
    {
        _httpFactory = httpFactory;
        _config = config;
        _fila = fila;
    }

    /// <summary>
    /// POST /api/pedidos/sincrono
    /// Fluxo síncrono: chama PagamentoApi, depois NotificacaoApi,
    /// e só então retorna a resposta ao cliente.
    /// </summary>
    [HttpPost("sincrono")]
    public async Task<IActionResult> CriarPedidoSincrono([FromBody] Pedido pedido)
    {
        var tempo = Stopwatch.StartNew();

        Console.WriteLine($"Pedido {pedido.Id}: iniciando fluxo sincrono.");

        using var client = _httpFactory.CreateClient();

        var urlPagamento = _config["ServicoPagamento"];
        var json = JsonSerializer.Serialize(pedido);

        using var conteudo = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var resposta = await client.PostAsync(
            $"{urlPagamento}/api/pagamentos",
            conteudo);

        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement>(corpo);
        var aprovado = resultado.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempo.ElapsedMilliseconds
            });
        }

        var urlNotificacao = _config["ServicoNotificacao"];

        var notificacao = new
        {
            Destinatario = pedido.Cliente,
            Assunto = $"Pedido {pedido.Id} confirmado!",
            Corpo = $"Seu pedido de {pedido.Produto} foi aprovado."
        };

        var jsonNotificacao = JsonSerializer.Serialize(notificacao);

        using var conteudoNotificacao = new StringContent(
            jsonNotificacao,
            Encoding.UTF8,
            "application/json");

        using var respostaNotificacao = await client.PostAsync(
            $"{urlNotificacao}/api/notificacoes",
            conteudoNotificacao);

        var statusNotificacao = respostaNotificacao.IsSuccessStatusCode
            ? "enviada"
            : "falhou";

        tempo.Stop();

        Console.WriteLine(
            $"Pedido {pedido.Id}: fluxo sincrono terminou em {tempo.ElapsedMilliseconds} ms.");

        return Ok(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = statusNotificacao,
            fluxo = "sincrono",
            tempoTotalMs = tempo.ElapsedMilliseconds
        });
    }

    /// <summary>
    /// POST /api/pedidos/assincrono
    /// Fluxo misto: pagamento síncrono (precisa da resposta),
    /// mas notificação é publicada na fila e processada em background.
    /// </summary>
    [HttpPost("assincrono")]
    public async Task<IActionResult> CriarPedidoAssincrono([FromBody] Pedido pedido)
    {
        var tempo = Stopwatch.StartNew();

        Console.WriteLine($"Pedido {pedido.Id}: iniciando fluxo assincrono.");

        using var client = _httpFactory.CreateClient();

        var urlPagamento = _config["ServicoPagamento"];
        var json = JsonSerializer.Serialize(pedido);

        using var conteudo = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var resposta = await client.PostAsync(
            $"{urlPagamento}/api/pagamentos",
            conteudo);

        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement>(corpo);
        var aprovado = resultado.GetProperty("aprovado").GetBoolean();

        if (!aprovado)
        {
            return BadRequest(new
            {
                pedidoId = pedido.Id,
                status = "rejeitado",
                mensagem = "Pagamento não aprovado",
                tempoTotalMs = tempo.ElapsedMilliseconds
            });
        }

        var evento = new EventoPedidoAprovado
        {
            PedidoId = pedido.Id,
            Cliente = pedido.Cliente,
            Produto = pedido.Produto,
            Valor = pedido.Valor
        };

        await _fila.PublicarAsync(evento);

        tempo.Stop();

        Console.WriteLine(
            $"Pedido {pedido.Id}: resposta pronta em {tempo.ElapsedMilliseconds} ms.");

        return Accepted(new
        {
            pedidoId = pedido.Id,
            status = "aprovado",
            notificacao = "pendente",
            fluxo = "assincrono",
            tempoTotalMs = tempo.ElapsedMilliseconds
        });
    }
}
