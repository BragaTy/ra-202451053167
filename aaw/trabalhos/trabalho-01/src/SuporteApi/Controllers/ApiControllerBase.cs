using Microsoft.AspNetCore.Mvc;

namespace SuporteApi.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    // Resposta temporária: some quando a camada de serviço for ligada aos controllers.
    protected ObjectResult NaoImplementado() =>
        Problem(statusCode: StatusCodes.Status501NotImplemented, title: "Endpoint ainda não implementado.");
}
