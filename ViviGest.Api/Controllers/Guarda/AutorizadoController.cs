using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;
using ViviGest.Api.Models;

namespace ViviGest.Api.Controllers.Guarda
{
    [ApiController]
    [Route("api/guarda/autorizados")]
    public class AutorizadoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AutorizadoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAutorizados()
        {
            var lista = await _context.PersonasAutorizadas
                .Include(a => a.Persona)
                .Include(a => a.TipoRelacion)
                .ToListAsync();

            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAutorizado([FromBody] PersonaAutorizada autorizado)
        {
            autorizado.IdPersonaAutorizada = Guid.NewGuid();
            _context.PersonasAutorizadas.Add(autorizado);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Autorizado registrado correctamente" });
        }
    }
}
