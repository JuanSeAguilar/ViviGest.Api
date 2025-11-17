using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ViviGest.Api.Models;
using ViviGestBackend.Models;


namespace ViviGestBackend.Models
{
    public class PeriodoPago
    {
        [Key]
        public int IdPeriodoPago { get; set; }

        [Required]
        [StringLength(7)]
        public string Periodo { get; set; } = null!; // 'YYYY-MM'

        // Navegaciones inversas
        public virtual ICollection<CargoCuenta> CargosCuenta { get; set; } = new List<CargoCuenta>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}