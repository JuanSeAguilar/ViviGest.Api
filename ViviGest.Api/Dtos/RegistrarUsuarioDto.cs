namespace ViviGest.Api.Dtos
{
    public class RegistrarUsuarioDto
    {
        public int IdTipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int IdRol { get; set; } // 1=Admin, 3=Guarda
    }
}
