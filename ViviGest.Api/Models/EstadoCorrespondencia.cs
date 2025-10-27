using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("EstadoCorrespondencia")]
    public class EstadoCorrespondencia
    {
        [Key]
        public int IdEstadoCorrespondencia { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;
    }
}
