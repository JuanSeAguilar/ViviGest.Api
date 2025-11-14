using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViviGest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("publico")]
    public IActionResult Publico() => Ok(new { mensaje = "Acceso libre" });

    [Authorize]
    [HttpGet("privado")] 
    public IActionResult Privado() => Ok(new { mensaje = "Acceso con token válido" });
}
