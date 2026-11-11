namespace SuporteApi.Erros;

// Exceções de negócio lançadas pelos serviços. Cada uma vira um status HTTP
// no TratadorDeErros, então o serviço não precisa conhecer HTTP.

// 400 Bad Request: dado inválido ou regra de negócio violada.
public class RegraDeNegocioException(string mensagem) : Exception(mensagem);

// 404 Not Found: o registro pedido não existe.
public class NaoEncontradoException(string mensagem) : Exception(mensagem);

// 409 Conflict: a operação conflita com o estado atual (ex.: e-mail duplicado).
public class ConflitoException(string mensagem) : Exception(mensagem);
