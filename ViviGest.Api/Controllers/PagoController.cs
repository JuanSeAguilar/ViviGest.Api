using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ViviGest.Api.Dtos;
using ViviGest.Api.Models;
using ViviGest.Data;

namespace ViviGest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagoController : ControllerBase
{
    private readonly AppDbContext _db;

    public PagoController(AppDbContext db) => _db = db;

    private Guid UsuarioId => Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "uid").Value);

    // GET: /api/pago/cargos-pendientes (Residente: ver cargos pendientes de sus unidades)
    [Authorize(Roles = "Residente")]
    [HttpGet("cargos-pendientes")]
    public async Task<IActionResult> GetCargosPendientes()
    {
        var unidadesIds = await _db.Residencias
            .Where(r => r.IdUsuario == UsuarioId && r.FechaFin == null)
            .Select(r => r.IdUnidad)
            .ToListAsync();

        var cargos = await _db.CargoCuentas
            .Include(c => c.Unidad).ThenInclude(u => u.Torre)
            .Include(c => c.PeriodoPago)
            .Where(c => unidadesIds.Contains(c.IdUnidad))
            .Select(c => new
            {
                idCargoCuenta = c.IdCargoCuenta,
                unidad = $"{c.Unidad.Torre.Nombre} - {c.Unidad.Codigo}",
                periodo = c.PeriodoPago.Periodo,
                concepto = c.Concepto,
                valor = c.Valor
            })
            .ToListAsync();

        return Ok(cargos);
    }

    // POST: /api/pago (Residente: registrar un pago)
    [Authorize(Roles = "Residente")]
    [HttpPost]
    public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoDto dto)
    {
        // Validar que el cargo existe y pertenece al residente
        var cargo = await _db.CargoCuentas
            .Include(c => c.Unidad).ThenInclude(u => u.Residencias)
            .FirstOrDefaultAsync(c => c.IdCargoCuenta == dto.IdCargoCuenta);

        if (cargo == null) return NotFound("Cargo no encontrado");

        var esDeResidente = cargo.Unidad.Residencias.Any(r => r.IdUsuario == UsuarioId && r.FechaFin == null);
        if (!esDeResidente) return Forbid("No tienes permiso para pagar este cargo");

        // Registrar pago
        var metodoTarjeta = await _db.MetodoPagos.FirstAsync(m => m.Nombre == "Tarjeta");
        var pago = new Pago
        {
            IdPago = Guid.NewGuid(),
            IdUnidad = cargo.IdUnidad,
            IdPeriodoPago = cargo.IdPeriodoPago,
            Valor = dto.Valor,
            IdMetodoPago = metodoTarjeta.IdMetodoPago,
            FechaPago = DateTime.UtcNow,
            IdUsuarioRegistro = UsuarioId
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Pago registrado exitosamente", idPago = pago.IdPago });
    }

    // GET: /api/pago (Admin: ver todos los pagos)
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetPagos()
    {
        var pagos = await _db.Pagos
            .Include(p => p.Unidad).ThenInclude(u => u.Torre)
            .Include(p => p.PeriodoPago)
            .Include(p => p.MetodoPago)
            .Include(p => p.UsuarioRegistro).ThenInclude(u => u.Persona)
            .OrderByDescending(p => p.FechaPago)
            .Select(p => new
            {
                idPago = p.IdPago,
                residente = p.UsuarioRegistro.Persona.Nombres + " " + p.UsuarioRegistro.Persona.Apellidos,
                unidad = $"{p.Unidad.Torre.Nombre} - {p.Unidad.Codigo}",
                periodo = p.PeriodoPago.Periodo,
                valor = p.Valor,
                metodo = p.MetodoPago.Nombre,
                fechaPago = p.FechaPago
            })
            .ToListAsync();

        return Ok(pagos);
    }
}
