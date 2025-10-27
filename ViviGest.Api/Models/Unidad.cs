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
﻿using System.ComponentModel.DataAnnotations;
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
        public int? Piso { get; set; }
        public decimal? AreaM2 { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }

        // Navs correctas según tu DDL:
        public Torre Torre { get; set; } = null!;
        public ICollection<Residencia>? Residencias { get; set; }   // <— en lugar de "Residente"
        public ICollection<Visita>? Visitas { get; set; }
        public ICollection<Correspondencia>? Correspondencias { get; set; }
    }
}
