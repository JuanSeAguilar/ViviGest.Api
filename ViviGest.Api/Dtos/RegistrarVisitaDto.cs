using System.ComponentModel.DataAnnotations;

namespace ViviGest.Api.Dtos
{
    public class RegistrarVisitaDto
    {
        [Required] public string NombreVisitante { get; set; } = null!;
        [Required] public string TipoDocumento { get; set; } = null!;
        [Required] public string NumeroDocumento { get; set; } = null!;
        [Required] public string Torre { get; set; } = null!;
        [Required] public string Unidad { get; set; } = null!;
        [Required] public string Motivo { get; set; } = null!;
        public string? PlacaVehiculo { get; set; }
    }
}
