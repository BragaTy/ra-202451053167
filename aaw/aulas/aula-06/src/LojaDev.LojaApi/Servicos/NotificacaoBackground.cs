using System.Text;
using System.Text.Json;
using LojaDev.Compartilhado.Fila;
using LojaDev.Compartilhado.Modelos;

namespace LojaDev.LojaApi.Servicos;

public class NotificacaoBackground : BackgroundService
{
    private readonly FilaDePedidos _fila;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    public NotificacaoBackground(
        FilaDePedidos fila,
        IHttpClientFactory httpFactory,
        IConfiguration config)
    {
        _fila = fila;
        _httpFactory = httpFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("[BACKGROUND] Aguardando eventos na fila.");

        try
        {
            await foreach (var evento in _fila.ConsumirAsync(stoppingToken))
            {
                Console.WriteLine(
                    $"[BACKGROUND] Evento recebido: pedido {evento.PedidoId}.");

                await ProcessarEvento(evento, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("[BACKGROUND] Encerrado.");
        }
    }

    /// <summary>
    /// Processa um evento da fila: chama NotificacaoApi via HTTP.
    /// Se falhar, loga o erro (em produção, recolocaria na fila).
    /// </summary>
    private async Task ProcessarEvento(
        EventoPedidoAprovado evento,
        CancellationToken stoppingToken)
    {
        try
        {
            using var client = _httpFactory.CreateClient();

            var urlNotificacao = _config["ServicoNotificacao"];

            var notificacao = new
            {
                Destinatario = evento.Cliente,
                Assunto = $"Pedido {evento.PedidoId} confirmado!",
                Corpo = $"Seu pedido de {evento.Produto} foi aprovado."
            };

            var json = JsonSerializer.Serialize(notificacao);

            using var conteudo = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            using var resposta = await client.PostAsync(
                $"{urlNotificacao}/api/notificacoes",
                conteudo,
                stoppingToken);

            if (resposta.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"[BACKGROUND] Notificacao enviada: pedido {evento.PedidoId}.");
            }
            else
            {
                Console.WriteLine(
                    $"[BACKGROUND] Notificacao falhou: pedido {evento.PedidoId}, status {(int)resposta.StatusCode}.");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[BACKGROUND] Erro no pedido {evento.PedidoId}: {ex.Message}");
        }
    }
}
