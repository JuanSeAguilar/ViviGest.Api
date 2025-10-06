using System;

namespace ViviGest.Api.Models
{
    public class Torre
    {
        public Guid IdTorre { get; set; }
        public Guid IdConjunto { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navigation properties
        public Conjunto Conjunto { get; set; }
    }
}