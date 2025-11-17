namespace ViviGest.Api.Dtos
{
    public class RegistrarCorrespondenciaDto
    {
        public Guid IdUnidad { get; set; }                // a qué unidad llega
        public int IdTipoCorrespondencia { get; set; }    // Paquete, Carta, etc.
        public string? Remitente { get; set; }
        public string? Observacion { get; set; }
    }
}