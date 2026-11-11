using Microsoft.AspNetCore.Diagnostics;
using MongoDB.Driver;

namespace SuporteApi.Erros;

// Ponto único que transforma exceções em respostas Problem Details (RFC 9457).
// Assim os controllers não precisam de try/catch.
public class TratadorDeErros(IProblemDetailsService problemDetails, ILogger<TratadorDeErros> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception erro, CancellationToken ct)
    {
        var (status, titulo, detalhe) = erro switch
        {
            RegraDeNegocioException => (400, "Requisição inválida.", erro.Message),
            NaoEncontradoException => (404, "Recurso não encontrado.", erro.Message),
            ConflitoException => (409, "Conflito.", erro.Message),

            // Duas requisições iguais ao mesmo tempo: o índice único do Mongo barra a segunda.
            MongoWriteException { WriteError.Category: ServerErrorCategory.DuplicateKey } =>
                (409, "Conflito.", "Registro duplicado."),

            // Banco fora do ar ou inacessível (ex.: IP não liberado no Atlas).
            TimeoutException => (503, "Banco de dados indisponível.", "Tente novamente em instantes."),

            // Qualquer outro erro: não expõe detalhes internos para quem chamou a API.
            _ => (500, "Erro interno no servidor.", null)
        };

        if (status >= 500)
            logger.LogError(erro, "Erro ao processar {Metodo} {Caminho}", http.Request.Method, http.Request.Path);

        http.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = erro,
            ProblemDetails = { Status = status, Title = titulo, Detail = detalhe }
        });
    }
}
