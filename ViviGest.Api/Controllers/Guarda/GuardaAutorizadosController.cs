using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;

[ApiController]
[Route("api/guarda/autorizados")]
public class GuardaAutorizadosController : ControllerBase
{
    private readonly AppDbContext _db;
    public GuardaAutorizadosController(AppDbContext db) => _db = db;

    [HttpGet("validar")]
    public async Task<IActionResult> Validar([FromQuery] string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return BadRequest(new { message = "El documento es requerido." });

        documento = documento.Trim();

        // Buscamos todos los autorizados activos que coincidan con ese documento
        var coincidencias = await
            (from pa in _db.PersonasAutorizadas
                    .Include(a => a.Persona)
                    .Include(a => a.TipoRelacion)
                    .Include(a => a.UsuarioResidente)
                        .ThenInclude(u => u.Persona)
             where pa.Activo && pa.Persona.NumeroDocumento == documento
             join r in _db.Residencias
                    .Include(r => r.Unidad)
                on pa.IdUsuarioResidente equals r.IdUsuario into resGroup
             from r in resGroup
                    .Where(r => r.FechaFin == null)
                    .DefaultIfEmpty() // puede no tener residencia activa
             select new
             {
                 nombreAutorizado = pa.Persona.Nombres + " " + pa.Persona.Apellidos,
                 relacion = pa.TipoRelacion.Nombre,
                 // quién lo autorizó (residente)
                 residente = pa.UsuarioResidente.Persona.Nombres + " " +
                             pa.UsuarioResidente.Persona.Apellidos,
                 // unidad del residente (si tiene residencia activa)
                 unidad = r != null ? r.Unidad.Codigo : null
             })
            .ToListAsync();

        if (!coincidencias.Any())
            return Ok(new { autorizado = false });

        return Ok(new
        {
            autorizado = true,
            coincidencias
        });
    }
}
