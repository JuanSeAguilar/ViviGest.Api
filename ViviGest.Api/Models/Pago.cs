using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ViviGestBackend.Models;

namespace ViviGest.Api.Models
{
    public class Pago
    {
        [Key]
        public Guid IdPago { get; set; }

        [Required]
        public Guid IdUnidad { get; set; }

        [Required]
        public int IdPeriodoPago { get; set; }

        [Required]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Valor { get; set; }

        [Required]
        public int IdMetodoPago { get; set; }

        [Required]
        public DateTime FechaPago { get; set; }

        [Required]
        public Guid IdUsuarioRegistro { get; set; }

        public DateTime FechaRegistro { get; set; }

        public Guid? IdCargoCuenta { get; set; }

        [ForeignKey("IdUnidad")]
        public virtual Unidad Unidad { get; set; } = null!;

        [ForeignKey("IdPeriodoPago")]
        public virtual PeriodoPago PeriodoPago { get; set; } = null!;

        [ForeignKey("IdMetodoPago")]
        public virtual MetodoPago MetodoPago { get; set; } = null!;

        [ForeignKey("IdUsuarioRegistro")]
        public virtual Usuario UsuarioRegistro { get; set; } = null!;

        [ForeignKey("IdCargoCuenta")]
        public virtual CargoCuenta? CargoCuenta { get; set; }
    }
}