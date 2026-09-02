using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.Models;

/// <summary>Corpo do PATCH de pedido: { "status": "entregue" }.</summary>
public class AlteracaoDeStatus
{
    [Required(ErrorMessage = "Informe o status.")]
    [RegularExpression("recebido|preparando|entregue", ErrorMessage = "Status deve ser: recebido, preparando ou entregue.")]
    public string Status { get; set; } = string.Empty;
}
