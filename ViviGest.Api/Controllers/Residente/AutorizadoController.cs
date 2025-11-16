using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViviGest.Api.Dtos;
using ViviGest.Api.Models;
using ViviGest.Data;

namespace ViviGest.Api.Controllers.Residente
{
    [ApiController]
    [Route("api/residente/autorizados")]
    [Authorize(Roles = "Residente")]
    public class ResidenteAutorizadosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ResidenteAutorizadosController(AppDbContext db)
        {
            _db = db;
        }

        private Guid UsuarioId
        {
            get
            {
                var claim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == "uid" ||
                    c.Type == "userId");

                if (claim == null)
                    throw new UnauthorizedAccessException("Usuario sin identificador válido.");

                return Guid.Parse(claim.Value);
            }
        }

        // 🔹 GET: lista de autorizados del residente
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.PersonasAutorizadas
                .Where(a => a.IdUsuarioResidente == UsuarioId && a.Activo)
                .Include(a => a.Persona)
                .Include(a => a.TipoRelacion)
                .Select(a => new
                {
                    id = a.IdPersonaAutorizada,
                    nombre = a.Persona.Nombres + " " + a.Persona.Apellidos,
                    documento = a.Persona.NumeroDocumento,
                    relacion = a.TipoRelacion.Nombre,
                    idTipoRelacionAutorizado = a.IdTipoRelacionAutorizado
                })
                .ToListAsync();

            return Ok(lista);
        }

        // 🔹 POST: registrar autorizado
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarAutorizadoDto dto)
        {
            if (dto is null)
                return BadRequest("DTO vacío");

            // 1️⃣ Buscar persona por tipo + documento
            var persona = await _db.Personas
                .FirstOrDefaultAsync(p =>
                    p.IdTipoDocumento == dto.IdTipoDocumento &&
                    p.NumeroDocumento == dto.NumeroDocumento);

            // 2️⃣ Si no existe por documento, probar por correo
            if (persona == null && !string.IsNullOrWhiteSpace(dto.CorreoElectronico))
            {
                persona = await _db.Personas
                    .FirstOrDefaultAsync(p => p.CorreoElectronico == dto.CorreoElectronico);
            }

            // 3️⃣ Si sigue sin existir, crearla
            if (persona == null)
            {
                persona = new Persona
                {
                    IdPersona = Guid.NewGuid(),
                    IdTipoDocumento = dto.IdTipoDocumento,
                    NumeroDocumento = dto.NumeroDocumento,
                    Nombres = dto.Nombre,
                    Apellidos = dto.Apellidos,
                    Telefono = dto.Telefono,
                    CorreoElectronico = dto.CorreoElectronico
                };

                _db.Personas.Add(persona);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Opcional: refrescar datos básicos
                persona.Nombres = dto.Nombre;
                persona.Apellidos = dto.Apellidos;
                persona.Telefono = dto.Telefono;
                persona.CorreoElectronico = dto.CorreoElectronico;
                _db.Personas.Update(persona);
                await _db.SaveChangesAsync();
            }

            // 4️⃣ Evitar duplicados de autorizado para ese residente
            bool yaAutorizado = await _db.PersonasAutorizadas
                .AnyAsync(a =>
                    a.IdUsuarioResidente == UsuarioId &&
                    a.IdPersona == persona.IdPersona &&
                    a.Activo);

            if (yaAutorizado)
                return BadRequest(new { message = "Esta persona ya está autorizada." });

            // 5️⃣ Crear PersonaAutorizada
            var autorizado = new PersonaAutorizada
            {
                IdPersonaAutorizada = Guid.NewGuid(),
                IdUsuarioResidente = UsuarioId,
                IdPersona = persona.IdPersona,
                IdTipoRelacionAutorizado = dto.IdTipoRelacionAutorizado,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _db.PersonasAutorizadas.Add(autorizado);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Autorizado registrado correctamente" });
        }

        // 🔹 DELETE: eliminar autorizado
        // Borra de PersonaAutorizada y, si queda huérfana, también de Persona
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            // 1️⃣ Traer el autorizado del residente logueado
            var autorizado = await _db.PersonasAutorizadas
                .Include(a => a.Persona)
                .FirstOrDefaultAsync(a =>
                    a.IdPersonaAutorizada == id &&
                    a.IdUsuarioResidente == UsuarioId);

            if (autorizado == null)
                return NotFound(new { message = "Autorizado no encontrado." });

            var personaId = autorizado.IdPersona;

            // 2️⃣ Borrar el registro de PersonaAutorizada (borrado físico)
            _db.PersonasAutorizadas.Remove(autorizado);
            await _db.SaveChangesAsync();

            // 3️⃣ Ver si esa Persona se sigue usando en otro lado
            bool usadaEnUsuario = await _db.Usuarios
                .AnyAsync(u => u.IdPersona == personaId);

            bool usadaEnVisitante = await _db.Visitantes
                .AnyAsync(v => v.IdPersona == personaId);

            bool usadaEnOtroAutorizado = await _db.PersonasAutorizadas
                .AnyAsync(a => a.IdPersona == personaId);

            // 4️⃣ Si NO se usa en ningún sitio → borrar también de Persona
            if (!usadaEnUsuario && !usadaEnVisitante && !usadaEnOtroAutorizado)
            {
                var persona = await _db.Personas
                    .FirstOrDefaultAsync(p => p.IdPersona == personaId);

                if (persona != null)
                {
                    _db.Personas.Remove(persona);
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Autorizado eliminado correctamente" });
        }
    }
}
