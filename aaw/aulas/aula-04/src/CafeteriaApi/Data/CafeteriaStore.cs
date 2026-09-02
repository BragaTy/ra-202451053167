using CafeteriaApi.Models;

namespace CafeteriaApi.Data;

/// <summary>
/// Banco em memória (List&lt;T&gt;) registrado como singleton.
/// Some quando o processo morre — e é exatamente esse o gancho da próxima aula.
/// Observação importante: guardar DADOS no servidor não fere o stateless;
/// o que feriria seria guardar SESSÃO do cliente (ver slide 10).
/// </summary>
public class CafeteriaStore
{
    private readonly object _trava = new();
    private readonly List<Cafe> _cafes = new();
    private readonly List<Pedido> _pedidos = new();
    private readonly List<ItemPedido> _itens = new();
    private int _proximoCafeId = 1;
    private int _proximoPedidoId = 1;
    private int _proximoItemId = 1;

    private static readonly string[] Origens =
        { "Cerrado Mineiro", "Sul de Minas", "Mogiana Paulista", "Chapada Diamantina", "Matas de Rondonia", "Norte Pioneiro do Parana" };
    private static readonly string[] Torras = { "clara", "media", "escura" };
    private static readonly string[] Prefixos =
        { "Espresso", "Coado", "Cold Brew", "Cappuccino", "Latte", "Mocha", "Ristretto", "Macchiato" };
    private static readonly string[] Sufixos =
        { "Classico", "da Casa", "Reserva", "do Chef", "Especial", "Tradicional", "Premium", "de Sitio" };
    private static readonly string[] Clientes =
        { "Ana", "Bruno", "Carla", "Diego", "Elisa", "Fabio", "Gabi", "Heitor", "Isis", "Joao", "Kelly", "Lucas" };

    public CafeteriaStore()
    {
        // 240 cafés: volume suficiente para a paginação deixar de ser teoria.
        for (var i = 0; i < 240; i++)
        {
            _cafes.Add(new Cafe
            {
                Id = _proximoCafeId++,
                Nome = $"{Prefixos[i % Prefixos.Length]} {Sufixos[(i / Prefixos.Length) % Sufixos.Length]} {i + 1:D3}",
                Origem = Origens[i % Origens.Length],
                Torra = Torras[i % Torras.Length],
                Preco = Math.Round(6.5m + (i % 23) * 0.85m, 2),
                Disponivel = i % 11 != 0
            });
        }

        // 60 pedidos com 1 a 3 itens cada.
        var statusPossiveis = new[] { "recebido", "preparando", "entregue" };
        for (var p = 0; p < 60; p++)
        {
            var pedido = new Pedido
            {
                Id = _proximoPedidoId++,
                Cliente = Clientes[p % Clientes.Length],
                Status = statusPossiveis[p % statusPossiveis.Length],
                CriadoEm = new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc).AddMinutes(p * 37)
            };
            _pedidos.Add(pedido);

            var quantosItens = (p % 3) + 1;
            for (var k = 0; k < quantosItens; k++)
            {
                var cafe = _cafes[(p * 7 + k * 13) % _cafes.Count];
                _itens.Add(new ItemPedido
                {
                    Id = _proximoItemId++,
                    PedidoId = pedido.Id,
                    CafeId = cafe.Id,
                    Quantidade = (k % 3) + 1,
                    PrecoUnitario = cafe.Preco
                });
            }

            RecalcularTotais(pedido.Id);
        }
    }

    // ---------- Cafés ----------

    public (List<Cafe> Itens, int Total) ListarCafes(int page, int size, string? origem, string? torra, string? sort)
    {
        lock (_trava)
        {
            IEnumerable<Cafe> consulta = _cafes;

            if (!string.IsNullOrWhiteSpace(origem))
                consulta = consulta.Where(c => c.Origem.Equals(origem, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(torra))
                consulta = consulta.Where(c => c.Torra.Equals(torra, StringComparison.OrdinalIgnoreCase));

            consulta = sort?.ToLowerInvariant() switch
            {
                "nome" => consulta.OrderBy(c => c.Nome),
                "-nome" => consulta.OrderByDescending(c => c.Nome),
                "preco" => consulta.OrderBy(c => c.Preco),
                "-preco" => consulta.OrderByDescending(c => c.Preco),
                _ => consulta.OrderBy(c => c.Id)
            };

            var todos = consulta.ToList();
            var pagina = todos.Skip((page - 1) * size).Take(size).ToList();
            return (pagina, todos.Count);
        }
    }

    public Cafe? BuscarCafe(int id)
    {
        lock (_trava) return _cafes.FirstOrDefault(c => c.Id == id);
    }

    public Cafe CriarCafe(Cafe cafe)
    {
        lock (_trava)
        {
            cafe.Id = _proximoCafeId++;
            cafe.Versao = 1;
            _cafes.Add(cafe);
            return cafe;
        }
    }

    /// <summary>Substituição total (PUT). Idempotente: repetir a chamada leva ao mesmo estado.</summary>
    public Cafe? SubstituirCafe(int id, Cafe novo)
    {
        lock (_trava)
        {
            var atual = _cafes.FirstOrDefault(c => c.Id == id);
            if (atual is null) return null;

            // A versão só sobe se o conteúdo REALMENTE mudou. Duas requisições PUT
            // idênticas deixam o recurso — e o ETag — exatamente iguais: é assim
            // que a idempotência se comprova de fora, olhando a representação.
            var mudou = atual.Nome != novo.Nome
                        || atual.Origem != novo.Origem
                        || atual.Torra != novo.Torra
                        || atual.Preco != novo.Preco
                        || atual.Disponivel != novo.Disponivel;

            atual.Nome = novo.Nome;
            atual.Origem = novo.Origem;
            atual.Torra = novo.Torra;
            atual.Preco = novo.Preco;
            atual.Disponivel = novo.Disponivel;
            if (mudou) atual.Versao++;

            return atual;
        }
    }

    /// <summary>Atualização parcial (PATCH): só os campos enviados mudam.</summary>
    public Cafe? AlterarCafe(int id, CafeParcial mudancas)
    {
        lock (_trava)
        {
            var atual = _cafes.FirstOrDefault(c => c.Id == id);
            if (atual is null) return null;

            var mudou = false;

            if (mudancas.Nome is not null && mudancas.Nome != atual.Nome)
            {
                atual.Nome = mudancas.Nome;
                mudou = true;
            }

            if (mudancas.Origem is not null && mudancas.Origem != atual.Origem)
            {
                atual.Origem = mudancas.Origem;
                mudou = true;
            }

            if (mudancas.Torra is not null && mudancas.Torra != atual.Torra)
            {
                atual.Torra = mudancas.Torra;
                mudou = true;
            }

            if (mudancas.Preco is not null && mudancas.Preco.Value != atual.Preco)
            {
                atual.Preco = mudancas.Preco.Value;
                mudou = true;
            }

            if (mudancas.Disponivel is not null && mudancas.Disponivel.Value != atual.Disponivel)
            {
                atual.Disponivel = mudancas.Disponivel.Value;
                mudou = true;
            }

            if (mudou) atual.Versao++;

            return atual;
        }
    }

    public bool RemoverCafe(int id)
    {
        lock (_trava)
        {
            var atual = _cafes.FirstOrDefault(c => c.Id == id);
            if (atual is null) return false;

            _cafes.Remove(atual);
            return true;
        }
    }

    // ---------- Pedidos ----------

    public (List<Pedido> Itens, int Total) ListarPedidos(int page, int size, string? status, string? cliente)
    {
        lock (_trava)
        {
            IEnumerable<Pedido> consulta = _pedidos;

            if (!string.IsNullOrWhiteSpace(status))
                consulta = consulta.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(cliente))
                consulta = consulta.Where(p => p.Cliente.Contains(cliente, StringComparison.OrdinalIgnoreCase));

            var todos = consulta.OrderBy(p => p.Id).ToList();
            var pagina = todos.Skip((page - 1) * size).Take(size).ToList();
            return (pagina, todos.Count);
        }
    }

    public Pedido? BuscarPedido(int id)
    {
        lock (_trava) return _pedidos.FirstOrDefault(p => p.Id == id);
    }

    public Pedido CriarPedido(Pedido pedido)
    {
        lock (_trava)
        {
            pedido.Id = _proximoPedidoId++;
            pedido.CriadoEm = DateTime.UtcNow;
            pedido.TotalDeItens = 0;
            pedido.ValorTotal = 0m;
            _pedidos.Add(pedido);
            return pedido;
        }
    }

    public Pedido? AlterarStatusDoPedido(int id, string status)
    {
        lock (_trava)
        {
            var pedido = _pedidos.FirstOrDefault(p => p.Id == id);
            if (pedido is null) return null;

            pedido.Status = status;
            return pedido;
        }
    }

    // ---------- Itens do pedido ----------

    public List<ItemPedido> ListarItens(int pedidoId)
    {
        lock (_trava) return _itens.Where(i => i.PedidoId == pedidoId).OrderBy(i => i.Id).ToList();
    }

    public ItemPedido? BuscarItem(int pedidoId, int itemId)
    {
        lock (_trava) return _itens.FirstOrDefault(i => i.PedidoId == pedidoId && i.Id == itemId);
    }

    public ItemPedido AdicionarItem(int pedidoId, ItemPedido item, decimal precoUnitario)
    {
        lock (_trava)
        {
            item.Id = _proximoItemId++;
            item.PedidoId = pedidoId;
            item.PrecoUnitario = precoUnitario;
            _itens.Add(item);
        }

        RecalcularTotais(pedidoId);
        return item;
    }

    public bool RemoverItem(int pedidoId, int itemId)
    {
        bool removeu;
        lock (_trava)
        {
            var item = _itens.FirstOrDefault(i => i.PedidoId == pedidoId && i.Id == itemId);
            removeu = item is not null && _itens.Remove(item);
        }

        if (removeu) RecalcularTotais(pedidoId);
        return removeu;
    }

    private void RecalcularTotais(int pedidoId)
    {
        lock (_trava)
        {
            var pedido = _pedidos.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido is null) return;

            var itens = _itens.Where(i => i.PedidoId == pedidoId).ToList();
            pedido.TotalDeItens = itens.Sum(i => i.Quantidade);
            pedido.ValorTotal = Math.Round(itens.Sum(i => i.Quantidade * i.PrecoUnitario), 2);
        }
    }
}
