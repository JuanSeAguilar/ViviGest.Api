using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using ViviGest.Data;
using ViviGest.Api.Models;


namespace ViviGest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CorrespondenciaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CorrespondenciaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/correspondencia
        [HttpGet]
        public async Task<IActionResult> GetCorrespondencia([FromQuery] string? estado = null)
        {
            var query = _context.Correspondencias
                .Include(c => c.Unidad)
                .ThenInclude(u => u.Torre)
                .Include(c => c.TipoCorrespondencia)
                .Include(c => c.EstadoCorrespondencia)
                .Include(c => c.UsuarioRegistro)
                .ThenInclude(u => u.Persona)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(c => c.EstadoCorrespondencia.Nombre == estado);
            }

            var correspondencias = await query
                .OrderByDescending(c => c.FechaRecepcion)
                .Select(c => new CorrespondenciaResponseDto
                {
                    IdCorrespondencia = c.IdCorrespondencia,
                    UnidadCodigo = c.Unidad.Codigo,
                    TorreNombre = c.Unidad.Torre.Nombre,
                    TipoCorrespondencia = c.TipoCorrespondencia.Nombre,
                    Remitente = c.Remitente,
                    FechaRecepcion = c.FechaRecepcion,
                    Estado = c.EstadoCorrespondencia.Nombre,
                    Observacion = c.Observacion,
                    UsuarioRegistro = $"{c.UsuarioRegistro.Persona.Nombres} {c.UsuarioRegistro.Persona.Apellidos}"
                })
                .ToListAsync();

            return Ok(correspondencias);
        }

        // GET: api/correspondencia/tipos
        [HttpGet("tipos")]
        public async Task<IActionResult> GetTiposCorrespondencia()
        {
            var tipos = await _context.TiposCorrespondencia
                .Select(t => new { t.IdTipoCorrespondencia, t.Nombre })
                .ToListAsync();
            return Ok(tipos);
        }

        // GET: api/correspondencia/unidades
        [HttpGet("unidades")]
        public async Task<IActionResult> GetUnidades()
        {
            var unidades = await _context.Unidades
                .Include(u => u.Torre)
                .Where(u => u.Activo)
                .Select(u => new {
                    u.IdUnidad,
                    CodigoCompleto = $"{u.Torre.Nombre} - {u.Codigo}"
                })
                .ToListAsync();
            return Ok(unidades);
        }

        // POST: api/correspondencia
        [HttpPost]
        public async Task<IActionResult> CreateCorrespondencia([FromBody] CorrespondenciaDto dto)
        {
            try
            {
                // ✅ OBTENER EL USERNAME (EMAIL) DEL TOKEN
                var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(username))
                    return Unauthorized();

                Console.WriteLine($"🔍 BACKEND DEBUG - Username: {username}");

                // ✅ BUSCAR EL USUARIO EN LA BASE DE DATOS POR EMAIL
                var usuario = await _context.Usuarios
                    .Include(u => u.Persona)
                    .FirstOrDefaultAsync(u => u.Persona.CorreoElectronico == username);

                if (usuario == null)
                    return Unauthorized(new { message = "Usuario no encontrado" });

                Console.WriteLine($"🔍 BACKEND DEBUG - UserId encontrado: {usuario.IdUsuario}");

                var correspondencia = new Correspondencia
                {
                    IdCorrespondencia = Guid.NewGuid(),
                    IdUnidad = dto.IdUnidad,
                    IdTipoCorrespondencia = dto.IdTipoCorrespondencia,
                    Remitente = dto.Remitente,
                    Observacion = dto.Observacion,
                    FechaRecepcion = DateTime.Now,
                    IdEstadoCorrespondencia = 1, // Pendiente
                    IdUsuarioRegistro = usuario.IdUsuario // ← USA EL ID DEL USUARIO DE LA BD
                };

                _context.Correspondencias.Add(correspondencia);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Correspondencia registrada exitosamente", id = correspondencia.IdCorrespondencia });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                return BadRequest(new { message = "Error interno: " + ex.Message });
            }
        }

        // PUT: api/correspondencia/{id}/notificar
        [HttpPut("{id}/notificar")]
        public async Task<IActionResult> NotificarCorrespondencia(Guid id)
        {
            var correspondencia = await _context.Correspondencias.FindAsync(id);
            if (correspondencia == null)
                return NotFound(new { message = "Correspondencia no encontrada" });

            correspondencia.IdEstadoCorrespondencia = 2; // Notificado
            correspondencia.FechaNotificado = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Residente notificado exitosamente" });
        }

        // PUT: api/correspondencia/{id}/entregar
        [HttpPut("{id}/entregar")]
        public async Task<IActionResult> EntregarCorrespondencia(Guid id, [FromBody] EntregaDto entregaDto)
        {
            var correspondencia = await _context.Correspondencias.FindAsync(id);
            if (correspondencia == null)
                return NotFound(new { message = "Correspondencia no encontrada" });

            correspondencia.IdEstadoCorrespondencia = 3; // Entregado
            correspondencia.FechaEntregado = DateTime.Now;
            correspondencia.EntregadoA = entregaDto.EntregadoA;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Correspondencia marcada como entregada" });
        }

        // DELETE: api/correspondencia/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> AnularCorrespondencia(Guid id)
        {
            var correspondencia = await _context.Correspondencias.FindAsync(id);
            if (correspondencia == null)
                return NotFound(new { message = "Correspondencia no encontrada" });

            _context.Correspondencias.Remove(correspondencia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Correspondencia anulada exitosamente" });
        }
    }

    // DTOs
    public class CorrespondenciaDto
    {
        public Guid? IdCorrespondencia { get; set; }
        public Guid IdUnidad { get; set; }
        public int IdTipoCorrespondencia { get; set; }
        public string? Remitente { get; set; }
        public string? Observacion { get; set; }
    }

    public class CorrespondenciaResponseDto
    {
        public Guid IdCorrespondencia { get; set; }
        public string? UnidadCodigo { get; set; }
        public string? TorreNombre { get; set; }
        public string? TipoCorrespondencia { get; set; }
        public string? Remitente { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public string? Estado { get; set; }
        public string? Observacion { get; set; }
        public string? UsuarioRegistro { get; set; }
    }

    public class EntregaDto
    {
        public string? EntregadoA { get; set; }
    }
}