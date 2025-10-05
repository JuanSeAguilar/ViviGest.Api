namespace ViviGest.Api.Models
{
    public class Persona
    {
        public Guid IdPersona { get; set; }
        public int IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }

        public Usuario? Usuario { get; set; } // 1-1
    }
}
