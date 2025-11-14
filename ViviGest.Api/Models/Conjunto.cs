// Models/Conjunto.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Conjunto")]
    public class Conjunto
    {
        [Key] 
        public Guid IdConjunto { get; set; }

        public string Nombre { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }

        public ICollection<Torre>? Torres { get; set; }
    }
}
