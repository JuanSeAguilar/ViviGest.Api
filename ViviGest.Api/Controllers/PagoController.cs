using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViviGest.Api.Models;
using ViviGest.Data;

namespace ViviGestBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PagoController(AppDbContext db)
        {
            _db = db;
        }

        // Obtener cargos pendientes (solo los no pagados)
        [Authorize]
        [HttpGet("cargos-pendientes")]
        public async Task<IActionResult> GetCargosPendientes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Obtener unidades del residente (usando Residencia)
            var unidades = await _db.Residencias
                .Where(r => r.IdUsuario == Guid.Parse(userId) && (r.FechaFin == null || r.FechaFin > DateTime.Today))
                .Select(r => r.IdUnidad)
                .ToListAsync();

            // Obtener cargos pendientes (solo aquellos sin pago registrado)
            var cargosPendientes = await _db.CargoCuentas
                .Where(c => unidades.Contains(c.IdUnidad) &&
                            !_db.Pago.Any(p => p.IdCargoCuenta == c.IdCargoCuenta))
                .Include(c => c.Unidad)
                .Include(c => c.PeriodoPago)
                .Select(c => new {
                    idCargoCuenta = c.IdCargoCuenta,
                    unidad = c.Unidad.Codigo,
                    periodo = c.PeriodoPago.Periodo,
                    concepto = c.Concepto,
                    valor = c.Valor
                })
                .ToListAsync();

            return Ok(cargosPendientes);
        }

        // Registrar un pago
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Verificar que el cargo existe y pertenece al usuario
            var cargo = await _db.CargoCuentas
                .Include(c => c.Unidad)
                .FirstOrDefaultAsync(c => c.IdCargoCuenta == dto.IdCargoCuenta);

            if (cargo == null) return NotFound("Cargo no encontrado");

            // Verificar que el usuario es residente de la unidad (usando Residencia)
            var esResidente = await _db.Residencias
                .AnyAsync(r => r.IdUnidad == cargo.IdUnidad && r.IdUsuario == Guid.Parse(userId) &&
                               (r.FechaFin == null || r.FechaFin > DateTime.Today));

            if (!esResidente) return Forbid("No tienes permiso para pagar este cargo");

            // Crear el pago (ligado al cargo)
            var pago = new Pago
            {
                IdPago = Guid.NewGuid(),
                IdUnidad = cargo.IdUnidad,
                IdPeriodoPago = cargo.IdPeriodoPago,
                Valor = dto.Valor,
                IdMetodoPago = 3,
                FechaPago = DateTime.UtcNow,
                IdUsuarioRegistro = Guid.Parse(userId),
                FechaRegistro = DateTime.UtcNow,
                IdCargoCuenta = dto.IdCargoCuenta
            };

            _db.Pago.Add(pago);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Pago registrado", idPago = pago.IdPago });
        }

        // Obtener todos los pagos (para admin)
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _db.Pago
                .Include(p => p.Unidad)
                    .ThenInclude(u => u.Residencias)
                        .ThenInclude(r => r.Usuario)
                            .ThenInclude(u => u.Persona)
                .Include(p => p.PeriodoPago)
                .Include(p => p.MetodoPago)
                .AsNoTracking()
                .Select(p => new
                {
                    idPago = p.IdPago,
                    idCargoCuenta = p.IdCargoCuenta,
                    residente = p.Unidad.Residencias
                        .Where(r => r.FechaFin == null)
                        .Select(r => r.Usuario.Persona.Nombres + " " + r.Usuario.Persona.Apellidos)
                        .FirstOrDefault() ?? "Sin residente",
                    unidad = p.Unidad.Codigo,
                    periodo = p.PeriodoPago.Periodo,
                    metodoPago = p.MetodoPago.Nombre,
                    valor = p.Valor,
                    fechaPago = p.FechaPago,
                    fechaRegistro = p.FechaRegistro
                })
                .ToListAsync(); // ✅ Aquí va el await y ToListAsync()

            return Ok(pagos);
        }
    }
        public class RegistrarPagoDto
    {
        public Guid IdCargoCuenta { get; set; }
        public decimal Valor { get; set; }
    }
}