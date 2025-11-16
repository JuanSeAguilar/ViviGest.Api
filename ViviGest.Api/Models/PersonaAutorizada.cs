using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("PersonaAutorizada")]
    public class PersonaAutorizada
    { 
        [Key]
        public Guid IdPersonaAutorizada { get; set; }
        public Guid IdUsuarioResidente { get; set; }
        public Guid IdPersona { get; set; }
        public int IdTipoRelacionAutorizado { get; set; }
        public string? PlacaVehiculo { get; set; }
        public bool Activo { get; set; } = true;   // 👈 importante
        public DateTime FechaCreacion { get; set; }

        [ForeignKey("IdUsuarioResidente")]
        public Usuario UsuarioResidente { get; set; }

        [ForeignKey("IdPersona")]
        public Persona Persona { get; set; }

        [ForeignKey("IdTipoRelacionAutorizado")]
        public TipoRelacionAutorizado TipoRelacion { get; set; }
    }
}
