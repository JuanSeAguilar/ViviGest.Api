namespace ViviGest.Api.Models
{
    public class Usuario
    {
        public Guid IdUsuario { get; set; }
        public Guid IdPersona { get; set; }
        public byte[] ContrasenaHash { get; set; } = Array.Empty<byte>();
        public byte[] ContrasenaSalt { get; set; } = Array.Empty<byte>();
        public bool Activo { get; set; } = true;

        public Persona Persona { get; set; } = null!;
        public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();

    }
}
