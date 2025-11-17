using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ViviGest.Api.Models;

namespace ViviGestBackend.Models // Ajusta el namespace
{
    public class CargoCuenta
    {
        [Key]
        public Guid IdCargoCuenta { get; set; }

        [Required]
        public Guid IdUnidad { get; set; }

        [Required]
        public int IdPeriodoPago { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Valor { get; set; }

        [Required]
        [StringLength(120)]
        public string Concepto { get; set; } = null!;

        public DateTime FechaRegistro { get; set; }

        // Navegaciones (agregadas)
        [ForeignKey("IdUnidad")]
        public virtual Unidad Unidad { get; set; } = null!; // ← Navegación a Unidad

        [ForeignKey("IdPeriodoPago")]
        public virtual PeriodoPago PeriodoPago { get; set; } = null!; // ← Navegación a PeriodoPago

        // Navegación inversa (para pagos ligados a este cargo)
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}