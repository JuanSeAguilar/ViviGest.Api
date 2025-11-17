using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("CargoCuenta")]
    public class CargoCuenta
    {
        [Key]
        public Guid IdCargoCuenta { get; set; }
        public Guid IdUnidad { get; set; }
        public int IdPeriodoPago { get; set; }
        public decimal Valor { get; set; }
        public string Concepto { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }

        [ForeignKey(nameof(IdUnidad))]
        public Unidad Unidad { get; set; } = null!;
        [ForeignKey(nameof(IdPeriodoPago))]
        public PeriodoPago PeriodoPago { get; set; } = null!;
    }
}
