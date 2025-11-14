using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViviGest.Api.Dtos;
using ViviGest.Api.Models;
using ViviGest.Data;

namespace ViviGest.Api.Controllers.Guarda
{
    [ApiController]
    [Route("api/guarda/visitas")]
    public class VisitaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VisitaController(AppDbContext context) 
        {
            _context = context;
        }

        // GET: api/guarda/visitas
        [Authorize] // si quieres listar solo autenticado
        [HttpGet]
        public async Task<IActionResult> ObtenerVisitas()
        {
            var visitas = await _context.Visitas
                .Include(v => v.Visitante).ThenInclude(vi => vi.Persona)
                .Include(v => v.Unidad).ThenInclude(u => u.Torre)
                .Include(v => v.UsuarioRegistro).ThenInclude(u => u.Persona)
                .OrderByDescending(v => v.FechaRegistro)
                .Select(v => new {
                    idVisita = v.IdVisita,
                    fecha = v.FechaRegistro,                 // 👈 camelCase
                    visitante = v.Visitante.Persona.Nombres,
                    documento = v.Visitante.Persona.NumeroDocumento,
                    torre = v.Unidad.Torre.Nombre,
                    unidad = v.Unidad.Codigo,
                    motivo = v.NotaDestino,
                    placaVehiculo = v.PlacaVehiculo
                })
                .ToListAsync();

            return Ok(visitas);
        }

        // En VisitaController
        [Authorize]
        [HttpPost("by-unidad")]
        public async Task<IActionResult> RegistrarVisitaByUnidad([FromBody] RegistrarVisitaByUnidadDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int idTipoDocumento = dto.TipoDocumento.ToUpperInvariant() switch
            {
                "CC" => 1,
                "CE" => 2,
                "NIT" => 3,
                "PAS" or "PA" => 4,
                _ => 0
            };
            if (idTipoDocumento == 0)
                return BadRequest(new { field = "tipoDocumento", message = "Tipo de documento inválido." });

            // Persona
            var persona = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdTipoDocumento == idTipoDocumento && p.NumeroDocumento == dto.NumeroDocumento);
            if (persona == null)
            {
                persona = new Persona
                {
                    IdPersona = Guid.NewGuid(),
                    IdTipoDocumento = idTipoDocumento,
                    NumeroDocumento = dto.NumeroDocumento,
                    Nombres = dto.NombreVisitante,
                    Apellidos = ""
                };
                _context.Personas.Add(persona);
            }

            // Visitante
            var visitante = await _context.Visitantes.FirstOrDefaultAsync(v => v.IdPersona == persona.IdPersona);
            if (visitante == null)
            {
                visitante = new Visitante { IdVisitante = Guid.NewGuid(), IdPersona = persona.IdPersona };
                _context.Visitantes.Add(visitante);
            }

            // Unidad
            var unidad = await _context.Unidades.FindAsync(dto.IdUnidad);
            if (unidad == null) return BadRequest(new { code = "UnidadNoExiste" });

            // Usuario (desde JWT)
            var claim = User?.Claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId" || c.Type == "uid")?.Value;
            if (!Guid.TryParse(claim, out var idUsuarioRegistro))
                return Unauthorized(new { message = "Token sin Id de usuario (GUID)." });

            var visita = new Visita
            {
                IdVisita = Guid.NewGuid(),
                IdVisitante = visitante.IdVisitante,
                IdUnidad = unidad.IdUnidad,
                IdUsuarioRegistro = idUsuarioRegistro,
                NotaDestino = dto.Motivo,
                PlacaVehiculo = dto.PlacaVehiculo,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Visita registrada", id = visita.IdVisita });
        }

        // POST: api/guarda/visitas
        [Authorize] // JWT requerido (extrae el Id de aquí)
        [HttpPost]
        public async Task<IActionResult> RegistrarVisita([FromBody] RegistrarVisitaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // --- Normalización básica del DTO
            dto.TipoDocumento = (dto.TipoDocumento ?? "").Trim().ToUpperInvariant();
            dto.Torre = (dto.Torre ?? "").Trim();
            dto.Unidad = (dto.Unidad ?? "").Trim();
            dto.NombreVisitante = (dto.NombreVisitante ?? "").Trim();

            // --- Tipo de documento según tu seed (CC, CE, NIT, PAS)
            int idTipoDocumento = dto.TipoDocumento switch
            {
                "CC" => 1,
                "CE" => 2,
                "NIT" => 3,
                "PAS" or "PA" => 4, // tolerante con "PA"
                _ => 0
            };
            if (idTipoDocumento == 0)
                return BadRequest(new { field = "tipoDocumento", message = "Tipo de documento inválido (usa CC, CE o PAS)." });

            // --- Persona (buscar por (TipoDoc, NumDoc); crear si no existe)
            var persona = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdTipoDocumento == idTipoDocumento && p.NumeroDocumento == dto.NumeroDocumento);

            if (persona == null)
            {
                persona = new Persona
                {
                    IdPersona = Guid.NewGuid(),
                    IdTipoDocumento = idTipoDocumento,
                    NumeroDocumento = dto.NumeroDocumento,
                    Nombres = dto.NombreVisitante,
                    Apellidos = "" // si luego separas en UI, ajustas esto
                };
                _context.Personas.Add(persona);
            }

            // --- Visitante (1-1 con Persona)
            var visitante = await _context.Visitantes
                .FirstOrDefaultAsync(v => v.IdPersona == persona.IdPersona);

            if (visitante == null)
            {
                visitante = new Visitante
                {
                    IdVisitante = Guid.NewGuid(),
                    IdPersona = persona.IdPersona
                };
                _context.Visitantes.Add(visitante);
            }

            // --- Resolver Unidad (tolerante con nombre de Torre)
            var unidad = await ResolverUnidadAsync(dto.Torre, dto.Unidad);
            if (unidad == null)
            {
                var torresEjemplo = await _context.Torres
                    .OrderBy(t => t.Nombre)
                    .Select(t => t.Nombre)
                    .Take(5)
                    .ToListAsync();

                return BadRequest(new
                {
                    code = "UnidadNoEncontrada",
                    message = "No se pudo resolver la unidad con los datos enviados.",
                    recibimos = new { torre = dto.Torre, unidad = dto.Unidad },
                    ejemplosTorre = torresEjemplo
                });
            }

            // --- Usuario que registra desde JWT (admite varios claim types)
            var claimVal = User?.Claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId" || c.Type == "uid")?.Value;

            if (!Guid.TryParse(claimVal, out Guid idUsuarioRegistro))
                return Unauthorized(new { message = "No se pudo determinar el usuario (JWT debe incluir un GUID en sub/nameid/userId/uid)." });

            // --- Crear Visita
            var visita = new Visita
            {
                IdVisita = Guid.NewGuid(),
                IdVisitante = visitante.IdVisitante,
                IdUnidad = unidad.IdUnidad,
                IdUsuarioRegistro = idUsuarioRegistro,
                NotaDestino = dto.Motivo,
                PlacaVehiculo = dto.PlacaVehiculo,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Visita registrada correctamente", id = visita.IdVisita });
        }

        // ===== Helpers =====
        private async Task<Unidad?> ResolverUnidadAsync(string torreInput, string codigoUnidad)
        {
            // 1) Intento exacto: Torre.Nombre + Unidad.Codigo
            var unidad = await _context.Unidades
                .Include(u => u.Torre)
                .FirstOrDefaultAsync(u => u.Codigo == codigoUnidad && u.Torre.Nombre == torreInput);

            if (unidad != null)
                return unidad;

            // 2) Solo por código (si no hay ambigüedad)
            var candidatos = await _context.Unidades
                .Include(u => u.Torre)
                .Where(u => u.Codigo == codigoUnidad)
                .ToListAsync();

            if (candidatos.Count == 1)
                return candidatos[0];

            if (candidatos.Count > 1)
            {
                // 3) Probar variantes: "10" vs "Torre 10"
                var cand1 = candidatos.FirstOrDefault(u =>
                    string.Equals(u.Torre.Nombre, torreInput, StringComparison.OrdinalIgnoreCase));
                if (cand1 != null) return cand1;

                var prefijada = $"Torre {torreInput}";
                var cand2 = candidatos.FirstOrDefault(u =>
                    string.Equals(u.Torre.Nombre, prefijada, StringComparison.OrdinalIgnoreCase));
                if (cand2 != null) return cand2;
            }

            return null;
        }
    }
}
