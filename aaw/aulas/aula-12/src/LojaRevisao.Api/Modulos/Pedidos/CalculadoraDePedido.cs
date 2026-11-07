namespace LojaRevisao.Api.Modulos.Pedidos;

/// <summary>
/// Regras de negócio (elas são a verdade):
///   R1. Frete fixo de R$ 25,00.
///   R2. Frete GRÁTIS A PARTIR DE R$ 300,00 (R$ 300,00 inclusive).
///   R3. Total = subtotal + frete.
/// </summary>
public static class CalculadoraDePedido
{
    public const decimal FretePadrao = 25m;

    // FALHA 11 corrigida (parte A): "a partir de" inclui os R$ 300,00, por isso >=.
    public static decimal CalcularFrete(decimal subtotal) =>
        subtotal >= 300m ? 0m : FretePadrao;

    public static decimal CalcularTotal(decimal subtotal) =>
        subtotal + CalcularFrete(subtotal);
}
