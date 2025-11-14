using Microsoft.AspNetCore.Mvc;
using ViviGest.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }
     
    [HttpGet("correo")]
    public IActionResult EnviarCorreoPrueba()
    {
        _emailService.EnviarCorreo(
            "juanseaguilar10@gmail.com",
            "📧 Prueba de ViviGest",
            "<h1>Correo de prueba</h1><p>Si ves esto, el correo funciona ✅</p>"
        );

        return Ok("Correo enviado correctamente");
    }
}
