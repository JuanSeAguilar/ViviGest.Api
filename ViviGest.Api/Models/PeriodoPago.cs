using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("PeriodoPago")]
    public class PeriodoPago
    {
        [Key]
        public int IdPeriodoPago { get; set; }
        public string Periodo { get; set; } = null!; // 'YYYY-MM'
    }
}
