using MongoDB.Bson;
using SuporteApi.Erros;

namespace SuporteApi.Services;

// Validações usadas por mais de um serviço.
public static class Validacao
{
    public const int TamanhoMaximoPagina = 100;

    public static void Id(string id, string campo = "Id")
    {
        if (!ObjectId.TryParse(id, out _))
            throw new RegraDeNegocioException($"{campo} '{id}' inválido: deve ter 24 caracteres hexadecimais.");
    }

    public static void Paginacao(int page, int size)
    {
        if (page < 1)
            throw new RegraDeNegocioException("O parâmetro page deve ser maior ou igual a 1.");
        if (size < 1 || size > TamanhoMaximoPagina)
            throw new RegraDeNegocioException($"O parâmetro size deve estar entre 1 e {TamanhoMaximoPagina}.");
    }
}
