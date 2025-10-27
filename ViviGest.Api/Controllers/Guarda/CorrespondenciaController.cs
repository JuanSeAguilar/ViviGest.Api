using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;
using ViviGest.Api.Models;

namespace ViviGest.Api.Controllers.Guarda
{
    [ApiController]
    [Route("api/guarda/correspondencia")]
    public class CorrespondenciaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CorrespondenciaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCorrespondencia()
        {
            var data = await _context.Correspondencias
                .Include(c => c.Unidad)
                .Include(c => c.TipoCorrespondencia)
                .Include(c => c.EstadoCorrespondencia)
                .Include(c => c.UsuarioRegistro)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCorrespondencia([FromBody] Correspondencia model)
        {
            model.IdCorrespondencia = Guid.NewGuid();
            model.FechaRecepcion = DateTime.Now;

            _context.Correspondencias.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Correspondencia registrada correctamente" });
        }

        [HttpPut("{id}/entregar")]
        public async Task<IActionResult> MarcarEntregada(Guid id)
        {
            var correspondencia = await _context.Correspondencias.FindAsync(id);
            if (correspondencia == null)
                return NotFound();

            correspondencia.IdEstadoCorrespondencia = 2; // 2 = Entregado
            correspondencia.FechaEntrega = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Correspondencia entregada" });
        }
    }
}
