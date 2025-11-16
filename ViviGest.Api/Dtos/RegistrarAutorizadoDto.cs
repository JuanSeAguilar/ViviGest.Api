public class RegistrarAutorizadoDto
{
    public int IdTipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public int IdTipoRelacionAutorizado { get; set; }
    // opcional

    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
}
