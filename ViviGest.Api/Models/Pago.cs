using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Pago")]
    public class Pago
    {
        [Key]
        public Guid IdPago { get; set; }
        public Guid IdUnidad { get; set; }
        public int IdPeriodoPago { get; set; }
        public decimal Valor { get; set; }
        public int IdMetodoPago { get; set; }
        public DateTime FechaPago { get; set; }
        public Guid IdUsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }

        [ForeignKey(nameof(IdUnidad))]
        public Unidad Unidad { get; set; } = null!;
        [ForeignKey(nameof(IdPeriodoPago))]
        public PeriodoPago PeriodoPago { get; set; } = null!;
        [ForeignKey(nameof(IdMetodoPago))]
        public MetodoPago MetodoPago { get; set; } = null!;
        [ForeignKey(nameof(IdUsuarioRegistro))]
        public Usuario UsuarioRegistro { get; set; } = null!;
    }
}
