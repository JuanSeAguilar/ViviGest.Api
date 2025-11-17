using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Correspondencia")]
    public class Correspondencia
    {
        [Key]
        public Guid IdCorrespondencia { get; set; }

        // 👇 FK REAL que existe en la tabla
        public Guid IdUnidad { get; set; }

        public int IdTipoCorrespondencia { get; set; }
        public string? Remitente { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public int IdEstadoCorrespondencia { get; set; }
        public DateTime? FechaNotificado { get; set; }
        public DateTime? FechaEntregado { get; set; }
        public string? EntregadoA { get; set; }
        public string? Observacion { get; set; }
        public Guid IdUsuarioRegistro { get; set; }

        // NAVS — SIN propiedades raras
        [ForeignKey(nameof(IdUnidad))]
        public Unidad Unidad { get; set; } = null!;

        [ForeignKey(nameof(IdTipoCorrespondencia))]
        public TipoCorrespondencia TipoCorrespondencia { get; set; } = null!;

        [ForeignKey(nameof(IdEstadoCorrespondencia))]
        public EstadoCorrespondencia EstadoCorrespondencia { get; set; } = null!;

        [ForeignKey(nameof(IdUsuarioRegistro))]
        public Usuario UsuarioRegistro { get; set; } = null!;
    }
}
