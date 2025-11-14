// Models/Residencia.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Residencia")]
    public class Residencia
    {
        [Key]
        public Guid IdResidencia { get; set; }

        public Guid IdUsuario { get; set; } 
        public Guid IdUnidad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public Unidad Unidad { get; set; } = null!;
    }
}
