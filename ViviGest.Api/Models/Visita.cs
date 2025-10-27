using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Visita")]
    public class Visita
    {
        [Key]
        public Guid IdVisita { get; set; }

        public Guid IdVisitante { get; set; }
        public Guid IdUnidad { get; set; }

        public string? NotaDestino { get; set; }
        public string? PlacaVehiculo { get; set; }

        public Guid IdUsuarioRegistro { get; set; }

        // Única marca temporal que persistimos (columna real en BD)
        public DateTime FechaRegistro { get; set; }

        // Alias opcional si tu controlador usa "FechaEntrada"
        [NotMapped]
        public DateTime FechaEntrada
        {
            get => FechaRegistro;
            set => FechaRegistro = value;
        }

        // Navs
        public Visitante Visitante { get; set; } = null!;
        public Unidad Unidad { get; set; } = null!;
        public Usuario UsuarioRegistro { get; set; } = null!;
    }
}
