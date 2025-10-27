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
﻿// Models/Torre.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Torre")]
    public class Torre
    {
        [Key]
        public Guid IdTorre { get; set; }

        public Guid IdConjunto { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }

        public Conjunto Conjunto { get; set; } = null!;
        public ICollection<Unidad>? Unidades { get; set; }
    }
}
