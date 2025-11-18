using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;

namespace ViviGest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Quita esto temporalmente para probar si es auth
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // 📊 1. Ocupación de Unidades
        [HttpGet("ocupacion-unidades")]
        public async Task<IActionResult> ReporteOcupacionUnidades()
        {
            try
            {
                Console.WriteLine("Iniciando ReporteOcupacionUnidades");

                // Optimización: Cargar todo con Include anidados para evitar N+1 queries
                var unidades = await _context.Unidades
                    .Include(u => u.Residencias)
                        .ThenInclude(r => r.Usuario)
                            .ThenInclude(u => u.Persona)
                    .ToListAsync();

                Console.WriteLine($"Unidades cargadas: {unidades.Count}");

                var datos = new List<dynamic>();

                foreach (var unidad in unidades)
                {
                    Console.WriteLine($"Procesando unidad: {unidad.Codigo ?? "N/A"}");
                    var residente = "N/A";
                    var estado = "Desocupado";

                    // Buscar residencia activa
                    var residenciaActiva = unidad.Residencias?.FirstOrDefault(r => r.FechaFin == null);
                    if (residenciaActiva != null)
                    {
                        Console.WriteLine($"Residencia activa encontrada para {unidad.Codigo}");
                        estado = "Ocupado";
                        if (residenciaActiva.Usuario?.Persona != null)
                        {
                            residente = $"{residenciaActiva.Usuario.Persona.Nombres ?? "N/A"} {residenciaActiva.Usuario.Persona.Apellidos ?? "N/A"}";
                            Console.WriteLine($"Residente: {residente}");
                        }
                        else
                        {
                            Console.WriteLine($"Usuario o Persona null para residencia en {unidad.Codigo}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No hay residencia activa para {unidad.Codigo}");
                    }

                    datos.Add(new
                    {
                        Codigo = unidad.Codigo ?? "N/A",
                        Piso = unidad.Piso,
                        AreaM2 = unidad.AreaM2,
                        Estado = estado,
                        Residente = residente
                    });
                }

                Console.WriteLine($"Datos preparados: {datos.Count}");

                var ms = GenerarPdfOcupacion(datos);
                return File(ms.ToArray(), "application/pdf", $"Reporte-Ocupacion-{DateTime.Now:yyyy-MM-dd}.pdf");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ocupacion-unidades: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 👮 2. Control de Visitantes
        [HttpGet("control-visitantes")]
        public async Task<IActionResult> ReporteControlVisitantes([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            try
            {
                desde ??= DateTime.UtcNow.AddMonths(-1);
                hasta ??= DateTime.UtcNow;

                var visitas = await _context.Visitas
                    .Include(v => v.Visitante)
                    .Include(v => v.Unidad)
                    .Include(v => v.UsuarioRegistro)
                    .Where(v => v.FechaRegistro >= desde && v.FechaRegistro <= hasta)
                    .ToListAsync();

                var datos = new List<dynamic>();

                foreach (var visita in visitas)
                {
                    var visitante = "Desconocido";
                    var documento = "N/A";

                    if (visita.Visitante != null)
                    {
                        var persona = await _context.Personas
                            .FirstOrDefaultAsync(p => p.IdPersona == visita.Visitante.IdPersona);

                        if (persona != null)
                        {
                            visitante = $"{persona.Nombres ?? "N/A"} {persona.Apellidos ?? "N/A"}";
                            documento = persona.NumeroDocumento ?? "N/A";
                        }
                    }

                    var guarda = "N/A";
                    if (visita.UsuarioRegistro != null)
                    {
                        var personaGuarda = await _context.Personas
                            .FirstOrDefaultAsync(p => p.IdPersona == visita.UsuarioRegistro.IdPersona);

                        if (personaGuarda != null)
                        {
                            guarda = $"{personaGuarda.Nombres ?? "N/A"} {personaGuarda.Apellidos ?? "N/A"}";
                        }
                    }

                    datos.Add(new
                    {
                        Visitante = visitante,
                        Documento = documento,
                        Unidad = visita.Unidad?.Codigo ?? "N/A",
                        Guarda = guarda,
                        FechaEntrada = visita.FechaRegistro,
                        Placa = visita.PlacaVehiculo ?? "N/A"
                    });
                }

                using var ms = GenerarPdfVisitantes(datos, desde.Value, hasta.Value);
                return File(ms.ToArray(), "application/pdf", $"Reporte-Visitantes-{DateTime.Now:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en control-visitantes: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 📦 3. Correspondencias
        [HttpGet("correspondencias")]
        public async Task<IActionResult> ReporteCorrespondencias([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            try
            {
                desde ??= DateTime.UtcNow.AddMonths(-1);
                hasta ??= DateTime.UtcNow;

                var correspondencias = await _context.Correspondencias
                    .Include(c => c.Unidad)
                    .Include(c => c.TipoCorrespondencia)
                    .Include(c => c.EstadoCorrespondencia)
                    .Where(c => c.FechaRecepcion >= desde && c.FechaRecepcion <= hasta)
                    .ToListAsync();

                List<dynamic> datos = correspondencias.Select(c => new
                {
                    Unidad = c.Unidad?.Codigo ?? "N/A",
                    Tipo = c.TipoCorrespondencia?.Nombre ?? "N/A",
                    Remitente = c.Remitente ?? "Desconocido",
                    Estado = c.EstadoCorrespondencia?.Nombre ?? "N/A",
                    FechaRecepcion = c.FechaRecepcion,
                    EntregadoA = c.EntregadoA ?? "Pendiente",
                    FechaEntrega = c.FechaEntregado
                }).Cast<dynamic>().ToList();

                using var ms = GenerarPdfCorrespondencias(datos, desde.Value, hasta.Value);
                return File(ms.ToArray(), "application/pdf", $"Reporte-Correspondencias-{DateTime.Now:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en correspondencias: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // 👤 4. Usuarios del Sistema
        [HttpGet("usuarios-sistema")]
        public async Task<IActionResult> ReporteUsuariosSistema()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Persona)
                    .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                    .Where(u => u.Activo)
                    .ToListAsync();

                List<dynamic> datos = usuarios.Select(u => new
                {
                    Nombre = u.Persona != null
                        ? $"{u.Persona.Nombres ?? "N/A"} {u.Persona.Apellidos ?? "N/A"}"
                        : "N/A",
                    Email = u.Persona?.CorreoElectronico ?? "N/A",
                    Documento = u.Persona?.NumeroDocumento ?? "N/A",
                    Rol = string.Join(", ", u.UsuarioRoles.Select(ur => ur.Rol.Nombre ?? "N/A")),
                    
                }).Cast<dynamic>().ToList();

                using var ms = GenerarPdfUsuarios(datos);
                return File(ms.ToArray(), "application/pdf", $"Reporte-Usuarios-{DateTime.Now:yyyy-MM-dd}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en usuarios-sistema: {ex}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private MemoryStream GenerarPdfOcupacion(List<dynamic> datos)
        {
            // Stream temporal que iText usará (puede ser cerrado por iText)
            var ms = new MemoryStream();

            // Usamos using normales con PdfWriter/PdfDocument/Document
            // (sin depender de SetCloseStream que tu versión no tiene)
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                doc.SetMargins(40, 40, 40, 40);

                doc.Add(new Paragraph("📊 REPORTE DE OCUPACIÓN DE UNIDADES")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                var table = new Table(5);
                table.AddHeaderCell("Unidad");
                table.AddHeaderCell("Piso");
                table.AddHeaderCell("Área (m²)");
                table.AddHeaderCell("Estado");
                table.AddHeaderCell("Residente");

                foreach (var unidad in datos)
                {
                    table.AddCell(unidad.Codigo?.ToString() ?? "N/A");
                    // unidad.Piso puede no existir en algunos objetos; usa null-safe
                    table.AddCell((unidad.GetType().GetProperty("Piso")?.GetValue(unidad)?.ToString()) ?? "N/A");
                    table.AddCell(unidad.AreaM2?.ToString() ?? "N/A");
                    table.AddCell(unidad.Estado?.ToString() ?? "N/A");
                    table.AddCell(unidad.Residente?.ToString() ?? "N/A");
                }

                doc.Add(table);

                // IMPORTANTE: aquí extraemos los bytes **antes** de que using cierre ms
                doc.Close();    // cierra pdf internamente, writer también se cerrará al salir del using
                var pdfBytes = ms.ToArray();

                // Devolvemos un MemoryStream nuevo e independiente (abierto)
                return new MemoryStream(pdfBytes);
            }
            // NOTA: el MemoryStream original 'ms' será cerrado por disposing del writer,
            // pero eso no afecta al MemoryStream que devolvimos (fuera del using).
        }



        private MemoryStream GenerarPdfVisitantes(List<dynamic> datos, DateTime desde, DateTime hasta)
        {
            var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                doc.SetMargins(40, 40, 40, 40);

                doc.Add(new Paragraph("👮 REPORTE DE CONTROL DE VISITANTES")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));

                doc.Add(new Paragraph($"Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20)
                    .SetFontSize(10));

                var table = new Table(6);
                table.AddHeaderCell("Visitante").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Documento").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Unidad").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Guarda").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Fecha").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Placa").SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                foreach (var visita in datos)
                {
                    table.AddCell(visita.Visitante.ToString());
                    table.AddCell(visita.Documento.ToString());
                    table.AddCell(visita.Unidad.ToString());
                    table.AddCell(visita.Guarda.ToString());
                    table.AddCell(((DateTime)visita.FechaEntrada).ToString("dd/MM/yyyy HH:mm"));
                    table.AddCell(visita.Placa.ToString());
                }

                doc.Add(table);
                doc.Add(new Paragraph($"\nGenerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10));
            }
            ms.Position = 0;
            return ms;
        }

        private MemoryStream GenerarPdfCorrespondencias(List<dynamic> datos, DateTime desde, DateTime hasta)
        {
            var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                doc.SetMargins(40, 40, 40, 40);

                doc.Add(new Paragraph("📦 REPORTE DE CORRESPONDENCIAS")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));

                doc.Add(new Paragraph($"Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20)
                    .SetFontSize(10));

                var table = new Table(6);
                table.AddHeaderCell("Unidad").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Tipo").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Remitente").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Estado").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Entregado A").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Fecha Entrega").SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                foreach (var corr in datos)
                {
                    table.AddCell(corr.Unidad.ToString());
                    table.AddCell(corr.Tipo.ToString());
                    table.AddCell(corr.Remitente.ToString());
                    table.AddCell(corr.Estado.ToString());
                    table.AddCell(corr.EntregadoA.ToString());
                    table.AddCell(corr.FechaEntrega != null ? ((DateTime)corr.FechaEntrega).ToString("dd/MM/yyyy") : "N/A");
                }

                doc.Add(table);
                doc.Add(new Paragraph($"\nGenerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10));
            }
            ms.Position = 0;
            return ms;
        }

        private MemoryStream GenerarPdfUsuarios(List<dynamic> datos)
        {
            var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                doc.SetMargins(40, 40, 40, 40);

                doc.Add(new Paragraph("👤 REPORTE DE USUARIOS DEL SISTEMA")
                    .SetFont(boldFont)
                    .SetFontSize(18)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                var table = new Table(5);
                table.AddHeaderCell("Nombre").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Email").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Documento").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Rol").SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell("Fecha Creación").SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                foreach (var usuario in datos)
                {
                    table.AddCell(usuario.Nombre.ToString());
                    table.AddCell(usuario.Email.ToString());
                    table.AddCell(usuario.Documento.ToString());
                    table.AddCell(usuario.Rol.ToString());
                    table.AddCell(((DateTime)usuario.FechaCreacion).ToString("dd/MM/yyyy"));
                }

                doc.Add(table);
                doc.Add(new Paragraph($"\nGenerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10));
            }
            ms.Position = 0;
            return ms;
        }
    }
}