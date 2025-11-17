using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ViviGest.Api.Dtos;
using ViviGest.Data;
using ViviGest.Api.Models;

[ApiController]
[Route("api/guarda/correspondencia")]
[Authorize(Roles = "Guarda")]
public class GuardaCorrespondenciaController : ControllerBase
{
    private readonly AppDbContext _db;

    public GuardaCorrespondenciaController(AppDbContext db)
    {
        _db = db;
    }

    private Guid UsuarioId =>
        Guid.Parse(User.Claims.First(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "uid" ||
            c.Type == "userId"
        ).Value);

    // GET: /api/guarda/correspondencia/unidades
    // Unidades con residencia activa + nombre del residente
    [HttpGet("unidades")]
    public async Task<IActionResult> ObtenerUnidades()
    {
        var unidades = await _db.Residencias
            .Include(r => r.Unidad)
                .ThenInclude(u => u.Torre)
            .Include(r => r.Usuario)
                .ThenInclude(u => u.Persona)
            .Where(r => r.FechaFin == null)
            .OrderBy(r => r.Unidad.Torre.Nombre)
            .ThenBy(r => r.Unidad.Codigo)
            .Select(r => new
            {
                idUnidad = r.Unidad.IdUnidad,
                nombreCompleto = r.Unidad.Torre.Nombre + " - " +
                                 r.Unidad.Codigo + " · " +
                                 r.Usuario.Persona.Nombres + " " +
                                 r.Usuario.Persona.Apellidos
            })
            .ToListAsync();

        return Ok(unidades);
    }

    // GET: /api/guarda/correspondencia/tiposCorrespondencia
    [HttpGet("tiposCorrespondencia")]
    public async Task<IActionResult> ObtenerTipos()
    {
        var tipos = await _db.TiposCorrespondencia
            .Select(t => new
            {
                idTipoCorrespondencia = t.IdTipoCorrespondencia,
                nombre = t.Nombre
            })
            .ToListAsync();

        return Ok(tipos);
    }

    // POST: /api/guarda/correspondencia
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCorrespondenciaDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "DTO vacío" });

        // 1. Validar Unidad
        var unidad = await _db.Unidades
            .FirstOrDefaultAsync(u => u.IdUnidad == dto.IdUnidad);

        if (unidad == null)
            return BadRequest(new { message = "No se encontró la unidad especificada." });

        // 2. Validar TipoCorrespondencia
        var tipo = await _db.TiposCorrespondencia
            .FirstOrDefaultAsync(t => t.IdTipoCorrespondencia == dto.IdTipoCorrespondencia);

        if (tipo == null)
            return BadRequest(new { message = $"Tipo de correspondencia inválido (Id = {dto.IdTipoCorrespondencia})." });

        // 3. Obtener estado "Pendiente" de forma segura
        var estadoPendiente = await _db.EstadosCorrespondencia
            .FirstOrDefaultAsync(e => e.Nombre == "Pendiente");
        // OJO: revisa que en tu tabla el nombre sea EXACTAMENTE "Pendiente"

        if (estadoPendiente == null)
            return StatusCode(500, new { message = "No existe el estado 'Pendiente' en la tabla EstadoCorrespondencia." });

        try
        {
            var corr = new Correspondencia
            {
                IdCorrespondencia = Guid.NewGuid(),
                IdUnidad = dto.IdUnidad,
                IdTipoCorrespondencia = dto.IdTipoCorrespondencia,
                Remitente = dto.Remitente,
                FechaRecepcion = DateTime.UtcNow,
                IdEstadoCorrespondencia = estadoPendiente.IdEstadoCorrespondencia,
                Observacion = dto.Observacion,
                IdUsuarioRegistro = UsuarioId
            };

            _db.Correspondencias.Add(corr);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Correspondencia registrada correctamente." });
        }
        catch (DbUpdateException ex)
        {
            // Aquí normalmente tienes innerException con info del FK
            return StatusCode(500, new
            {
                message = "Error al guardar la correspondencia en base de datos.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error inesperado al registrar la correspondencia.",
                detail = ex.Message
            });
        }
    }


    // GET: /api/guarda/correspondencia/pendientes
    // Para el dashboard "CorrespondenciaPendiente"
    [HttpGet("pendientes")]
    public async Task<IActionResult> Pendientes()
    {
        var idEstadoPendiente = await _db.EstadosCorrespondencia
            .Where(e => e.Nombre == "Pendiente")
            .Select(e => e.IdEstadoCorrespondencia)
            .SingleAsync();

        var data = await _db.Correspondencias
            .Include(c => c.Unidad)
                .ThenInclude(u => u.Torre)
            .Include(c => c.TipoCorrespondencia)
            .Where(c => c.IdEstadoCorrespondencia == idEstadoPendiente)
            .OrderBy(c => c.FechaRecepcion)
            .Select(c => new
            {
                idCorrespondencia = c.IdCorrespondencia,
                unidad = c.Unidad.Codigo,
                torre = c.Unidad.Torre.Nombre,
                tipoCorrespondencia = c.TipoCorrespondencia.Nombre,
                remitente = c.Remitente,
                fechaRecepcion = c.FechaRecepcion,
                observacion = c.Observacion
            })
            .ToListAsync();

        return Ok(data);
    }

    // PUT: /api/guarda/correspondencia/{id}/notificar
    [HttpPut("{id:guid}/notificar")]
    public async Task<IActionResult> Notificar(Guid id)
    {
        var corr = await _db.Correspondencias
            .FirstOrDefaultAsync(c => c.IdCorrespondencia == id);

        if (corr == null)
            return NotFound(new { message = "No se encontró la correspondencia." });

        var idEstadoNotificado = await _db.EstadosCorrespondencia
            .Where(e => e.Nombre == "Notificado")
            .Select(e => e.IdEstadoCorrespondencia)
            .SingleAsync();

        if (corr.IdEstadoCorrespondencia == idEstadoNotificado)
            return BadRequest(new { message = "La correspondencia ya está notificada." });

        corr.IdEstadoCorrespondencia = idEstadoNotificado;
        corr.FechaNotificado = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Residente notificado correctamente." });
    }
}
