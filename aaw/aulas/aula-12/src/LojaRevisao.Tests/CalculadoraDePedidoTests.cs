using LojaRevisao.Api.Modulos.Pedidos;
using Xunit;

namespace LojaRevisao.Tests;

// FALHA 11 corrigida (parte B): o esperado agora vem das regras R1 a R3, não do código.
public class CalculadoraDePedidoTests
{
    [Theory]
    [InlineData(100.00, 25.00)]
    [InlineData(299.99, 25.00)]
    [InlineData(300.00, 0.00)]
    [InlineData(300.01, 0.00)]
    public void CalcularFrete_ConformeSubtotal_RetornaFreteDaRegra(double subtotal, double freteEsperado)
    {
        var frete = CalculadoraDePedido.CalcularFrete((decimal)subtotal);

        Assert.Equal((decimal)freteEsperado, frete);
    }

    [Fact]
    public void CalcularTotal_CompraPequena_SomaOFrete()
    {
        var total = CalculadoraDePedido.CalcularTotal(100m);

        Assert.Equal(125m, total);
    }

    [Fact]
    public void CalcularTotal_CompraDeExatamente300_NaoSomaFrete()
    {
        var total = CalculadoraDePedido.CalcularTotal(300m);

        Assert.Equal(300m, total);
    }
}
