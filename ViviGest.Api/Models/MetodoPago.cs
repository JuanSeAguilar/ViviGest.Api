using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("MetodoPago")]
    public class MetodoPago
    {
        [Key]
        public int IdMetodoPago { get; set; }
        public string Nombre { get; set; } = null!;
    }
}
