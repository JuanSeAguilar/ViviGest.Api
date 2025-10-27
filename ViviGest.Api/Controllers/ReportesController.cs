using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using Microsoft.AspNetCore.Mvc;

namespace ViviGest.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ReportesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("reporte-visitas")]
        public IActionResult GenerarReporte([FromQuery] string usuario = "guarda1")
        {
            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))
            {
                // 🧩 Configuración de fuentes
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // 🧩 Márgenes
                doc.SetMargins(40, 40, 40, 40);

                // 🧩 Logo (opcional)
                string logoPath = @"C:\Users\juans\Downloads\Vivigest.png";
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(70, 70);
                    logo.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    doc.Add(logo);
                }

                // 🏠 Encabezado tipo banner
                var encabezado = new Paragraph("🏠 ViviGest - Reporte de Visitas")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetBackgroundColor(ColorConstants.BLUE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(10)
                    .SetBorderRadius(new BorderRadius(5));
                doc.Add(encabezado);

                // 📆 Datos dinámicos
                doc.Add(new Paragraph($"Generado por: {usuario}")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetMarginTop(10));
                doc.Add(new Paragraph($"Fecha y hora: {DateTime.Now}")
                    .SetFont(normalFont)
                    .SetFontSize(12));

                doc.Add(new Paragraph("\nListado de visitas registradas:")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginTop(10));

                // 🔹 Datos simulados (reemplázalos con tus datos de BD)
                var visitas = new List<(string Visitante, string Torre, string Unidad)>
                {
                    ("Juan Pérez", "Torre A", "101"),
                    ("María López", "Torre B", "204"),
                    ("Carlos Ruiz", "Torre C", "305")
                };

                // 🧩 Tabla elegante
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 4, 2, 2 }))
                    .UseAllAvailableWidth()
                    .SetMarginTop(10);

                // Cabeceras
                table.AddHeaderCell(new Cell().Add(new Paragraph("Visitante"))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Torre"))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Unidad"))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetFont(boldFont)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5));

                // Filas de datos
                foreach (var v in visitas)
                {
                    table.AddCell(new Cell().Add(new Paragraph(v.Visitante)).SetFont(normalFont).SetPadding(5));
                    table.AddCell(new Cell().Add(new Paragraph(v.Torre)).SetFont(normalFont).SetPadding(5));
                    table.AddCell(new Cell().Add(new Paragraph(v.Unidad)).SetFont(normalFont).SetPadding(5));
                }

                doc.Add(table);

                // 🕒 Línea separadora
                doc.Add(new LineSeparator(new SolidLine()).SetMarginTop(20));

                // 🧾 Pie de página
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
            return File(bytes, "application/pdf", "reporte_visitas.pdf");
        }
    }
}
