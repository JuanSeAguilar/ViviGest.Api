using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("TipoRelacionAutorizado")]
    public class TipoRelacionAutorizado
    {
        [Key]
        public int IdTipoRelacionAutorizado { get; set; }

        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Descripcion { get; set; }
    }
}
