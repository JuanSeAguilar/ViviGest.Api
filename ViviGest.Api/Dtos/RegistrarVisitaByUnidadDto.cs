// Dtos/RegistrarVisitaByUnidadDto.cs
using System.ComponentModel.DataAnnotations;

namespace ViviGest.Api.Dtos
{
    public class RegistrarVisitaByUnidadDto
    {
        [Required] public Guid IdUnidad { get; set; }
        [Required] public string NombreVisitante { get; set; } = null!;
        [Required] public string TipoDocumento { get; set; } = null!; // CC|CE|NIT|PAS
        [Required] public string NumeroDocumento { get; set; } = null!; 
        [Required] public string Motivo { get; set; } = null!;
        public string? PlacaVehiculo { get; set; }
    }
}
