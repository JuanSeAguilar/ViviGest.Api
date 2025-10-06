using System;

namespace ViviGest.Api.Models
{
    public class Unidad
    {
        public Guid IdUnidad { get; set; }
        public Guid IdTorre { get; set; }
        public string Codigo { get; set; }
        public int? Piso { get; set; }
        public decimal? AreaM2 { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navigation properties
        public Torre Torre { get; set; }
    }
}