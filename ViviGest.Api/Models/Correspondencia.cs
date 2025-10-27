using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ViviGest.Api.Models;

public class Correspondencia
{
    [Key]
    public Guid IdCorrespondencia { get; set; }

    public Guid IdUnidad { get; set; }
    public int IdTipoCorrespondencia { get; set; }
    public int IdEstadoCorrespondencia { get; set; }

    public DateTime FechaRecepcion { get; set; }
    public DateTime? FechaEntrega { get; set; }

    [ForeignKey("IdUnidad")]
    public Unidad Unidad { get; set; }

    [ForeignKey("IdTipoCorrespondencia")]
    public TipoCorrespondencia TipoCorrespondencia { get; set; }

    [ForeignKey("IdEstadoCorrespondencia")]
    public EstadoCorrespondencia EstadoCorrespondencia { get; set; }

    public Guid IdUsuarioRegistro { get; set; }
    [ForeignKey("IdUsuarioRegistro")]
    public Usuario UsuarioRegistro { get; set; }
}
