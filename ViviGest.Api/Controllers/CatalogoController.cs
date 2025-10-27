using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;

namespace ViviGest.Api.Controllers
{
    [ApiController]
    [Route("api/catalogo")]
    [Authorize] // el guarda debe estar logueado
    public class CatalogoController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CatalogoController(AppDbContext db) => _db = db;

        [HttpGet("torres")]
        public async Task<IActionResult> GetTorres()
        {
            var data = await _db.Torres
                .OrderBy(t => t.Nombre)
                .Select(t => new { t.IdTorre, t.Nombre })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("unidades")]
        public async Task<IActionResult> GetUnidades([FromQuery] Guid torreId)
        {
            if (torreId == Guid.Empty) return BadRequest(new { message = "torreId es requerido." });

            var data = await _db.Unidades
                .Where(u => u.IdTorre == torreId)
                .OrderBy(u => u.Codigo)
                .Select(u => new { u.IdUnidad, u.Codigo })
                .ToListAsync();

            return Ok(data);
        }
    }
}
