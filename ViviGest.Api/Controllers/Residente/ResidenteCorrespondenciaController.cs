using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ViviGest.Data;

[ApiController]
[Route("api/residente/correspondencia")]
[Authorize(Roles = "Residente")]
public class ResidenteCorrespondenciaController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResidenteCorrespondenciaController(AppDbContext db)
    {
        _db = db;
    }

    private Guid UsuarioId =>
        Guid.Parse(User.Claims.First(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "uid" ||
            c.Type == "userId"
        ).Value);

    // GET: /api/residente/correspondencia?soloPendiente=true|false
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool soloPendiente = false)
    {
        // Unidades donde el usuario es residente activo
        var unidadesIds = await _db.Residencias
            .Where(r => r.IdUsuario == UsuarioId && r.FechaFin == null)
            .Select(r => r.IdUnidad)
            .ToListAsync();

        if (!unidadesIds.Any())
            return Ok(Array.Empty<object>());

        var query = _db.Correspondencias
    .Include(c => c.TipoCorrespondencia)
    .Include(c => c.EstadoCorrespondencia)
    .Include(c => c.UsuarioRegistro)  // ← Este Include es clave
        .ThenInclude(u => u.Persona)  // ← Y este
    .Where(c => unidadesIds.Contains(c.IdUnidad));

        if (soloPendiente)
        {
            var idEstadoPendiente = await _db.EstadosCorrespondencia
                .Where(e => e.Nombre == "Pendiente")
                .Select(e => e.IdEstadoCorrespondencia)
                .SingleAsync();

            query = query.Where(c => c.IdEstadoCorrespondencia == idEstadoPendiente);
        }

        var lista = await query
            .OrderByDescending(c => c.FechaRecepcion)
            .Select(c => new
            {
                idCorrespondencia = c.IdCorrespondencia,
                tipoCorrespondencia = c.TipoCorrespondencia.Nombre,
                fechaRecepcion = c.FechaRecepcion,
                estado = c.EstadoCorrespondencia.Nombre,
                observacion = c.Observacion,
                remitente = c.Remitente,
                registradoPor = c.UsuarioRegistro.Persona.Nombres + " " + c.UsuarioRegistro.Persona.Apellidos  // ← Este campo
            })
            .ToListAsync();

        return Ok(lista);
    }
}
