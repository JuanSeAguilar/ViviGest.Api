using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("TipoDocumento")]
    public class TipoDocumento
    {
        [Key]
        public int IdTipoDocumento { get; set; }

        [Required, MaxLength(50)]
        public string Nombre { get; set; } = string.Empty; 

        [MaxLength(10)]
        public string? Abreviatura { get; set; }
    }
}
