using System;

namespace ViviGest.Api.Models
{
    public class Conjunto
    {
        public Guid IdConjunto { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}