using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Unidad")]
    public class Unidad
    {
        [Key]
        public Guid IdUnidad { get; set; }

        public Guid IdTorre { get; set; }
        public string Codigo { get; set; } = null!;
        public decimal AreaM2 { get; set; }
        public int? Piso { get; set; }

        [ForeignKey(nameof(IdTorre))]
        public Torre Torre { get; set; } = null!;

        public ICollection<Residencia> Residencias { get; set; } = new List<Residencia>();
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();

        // 👇 navegación correcta hacia Correspondencia
        public ICollection<Correspondencia> Correspondencias { get; set; } = new List<Correspondencia>();
    }
}
