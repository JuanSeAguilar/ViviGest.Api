namespace ViviGest.Api.Models
{
    public class Correspondencia
    {
        public Guid IdCorrespondencia { get; set; }
        public Guid IdUnidad { get; set; }
        public int IdTipoCorrespondencia { get; set; }
        public string Remitente { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public int IdEstadoCorrespondencia { get; set; }
        public DateTime? FechaNotificado { get; set; }
        public DateTime? FechaEntregado { get; set; }
        public string? EntregadoA { get; set; }
        public string Observacion { get; set; }
        public Guid IdUsuarioRegistro { get; set; }

        // Navigation properties
        public Unidad Unidad { get; set; }
        public TipoCorrespondencia TipoCorrespondencia { get; set; }
        public EstadoCorrespondencia EstadoCorrespondencia { get; set; }
        public Usuario UsuarioRegistro { get; set; }
    }
}