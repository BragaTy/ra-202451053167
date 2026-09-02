using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.Models;

/// <summary>Um pedido feito no balcão.</summary>
public class Pedido
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(80, MinimumLength = 2)]
    public string Cliente { get; set; } = string.Empty;

    /// <summary>recebido | preparando | entregue</summary>
    [RegularExpression("recebido|preparando|entregue", ErrorMessage = "Status deve ser: recebido, preparando ou entregue.")]
    public string Status { get; set; } = "recebido";

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Itens NÃO viajam na representação do pedido: eles são um sub-recurso,
    /// acessível em GET /api/v1/pedidos/{id}/itens.
    /// </summary>
    public int TotalDeItens { get; set; }

    public decimal ValorTotal { get; set; }
}

/// <summary>Item de um pedido — só existe DENTRO de um pedido (1 nível de aninhamento).</summary>
public class ItemPedido
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe o id do café.")]
    public int CafeId { get; set; }

    [Range(1, 50, ErrorMessage = "A quantidade deve estar entre 1 e 50.")]
    public int Quantidade { get; set; } = 1;

    public decimal PrecoUnitario { get; set; }
}
