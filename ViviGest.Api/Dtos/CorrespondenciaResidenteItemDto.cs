namespace ViviGest.Api.Dtos
{
    public class CorrespondenciaResidenteItemDto
    {
        public Guid Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string? Remitente { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaRecepcion { get; set; }
        public DateTime? FechaEntregado { get; set; }
        public string Unidad { get; set; } = null!;       // Código unidad, ej. 101
    }
}
