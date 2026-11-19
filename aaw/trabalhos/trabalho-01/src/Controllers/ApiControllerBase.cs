using Microsoft.AspNetCore.Mvc;

namespace SuporteApi.Controllers;

// Base comum: [ApiController] valida os DTOs e devolve 400 automaticamente;
// 401 e 403 podem vir de qualquer endpoint protegido.
[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public abstract class ApiControllerBase : ControllerBase;
