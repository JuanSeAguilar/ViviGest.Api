using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViviGest.Data;
using ViviGest.Api.Models;

namespace ViviGest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("reporte-visitas")]    
        public IActionResult GenerarReporte([FromQuery] string usuario = "guarda1")
        {
            // 🔹 Cargar datos con navegación completa (en memoria)
            var visitas = _context.Visitas
                .Include(v => v.Visitante)
                    .ThenInclude(vt => vt.Persona)
                .Include(v => v.Unidad)
                    .ThenInclude(u => u.Residencias)
                        .ThenInclude(r => r.Usuario)
                            .ThenInclude(u => u.Persona)
                .AsNoTracking()
                .ToList() // <- materializamos aquí para evitar el error
                .Select(v => new
                {
                    Visitante = v.Visitante?.Persona != null
                        ? $"{v.Visitante.Persona.Nombres} {v.Visitante.Persona.Apellidos}"
                        : "Desconocido",
                    Unidad = v.Unidad?.Codigo ?? "N/A",
                    Residente = v.Unidad?.Residencias?.FirstOrDefault()?.Usuario?.Persona != null
                        ? $"{v.Unidad.Residencias.First().Usuario.Persona.Nombres} {v.Unidad.Residencias.First().Usuario.Persona.Apellidos}"
                        : "Sin residente asignado",
                    FechaEntrada = v.FechaEntrada
                })
                .OrderByDescending(v => v.FechaEntrada)
                .ToList();

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                doc.SetMargins(40, 40, 40, 40);

                // 🧩 Logo (opcional)
                string logoPath = Path.Combine(_env.ContentRootPath, "wwwroot", "images", "Vivigest.png");
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(70, 70);
                    logo.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    doc.Add(logo);
                }

                // 🏠 Encabezado
                var encabezado = new Paragraph("🏠 ViviGest - Reporte de Visitas")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetBackgroundColor(ColorConstants.BLUE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(10);
                doc.Add(encabezado);

                // Datos del reporte
                doc.Add(new Paragraph($"Generado por: {usuario}")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetMarginTop(10));
                doc.Add(new Paragraph($"Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFont(normalFont)
                    .SetFontSize(12));
                doc.Add(new Paragraph("\nListado de visitas registradas:")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginTop(10));

                // Tabla
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 3, 2, 3 }))
                    .UseAllAvailableWidth()
                    .SetMarginTop(10);

                string[] headers = { "Visitante", "Residente", "Unidad", "Entrada" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(header))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetFont(boldFont)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(5));
                }

                if (!visitas.Any())
                {
                    table.AddCell(new Cell(1, 4)
                        .Add(new Paragraph("No hay visitas registradas"))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(8));
                }
                else
                {
                    foreach (var v in visitas)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(v.Visitante)).SetFont(normalFont).SetPadding(5));
                        table.AddCell(new Cell().Add(new Paragraph(v.Residente)).SetFont(normalFont).SetPadding(5));
                        table.AddCell(new Cell().Add(new Paragraph(v.Unidad)).SetFont(normalFont).SetPadding(5));
                        table.AddCell(new Cell().Add(new Paragraph(v.FechaEntrada.ToString("dd/MM/yyyy HH:mm"))).SetFont(normalFont).SetPadding(5));
                    }
                }

                doc.Add(table);

                // Pie
                doc.Add(new LineSeparator(new SolidLine()).SetMarginTop(20));
                doc.Add(new Paragraph($"Generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFontSize(10)
                    .SetFont(normalFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));
                doc.Add(new Paragraph("ViviGest © 2025 - Sistema de Gestión Residencial")
                    .SetFontSize(10)
                    .SetFont(normalFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));
            }

            var bytes = ms.ToArray();
            return File(bytes, "application/pdf", $"reporte_visitas_{DateTime.Now:yyyyMMddHHmm}.pdf");
        }
    }
}
