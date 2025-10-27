// Models/Visitante.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("Visitante")]
    public class Visitante
    {
        [Key]
        public Guid IdVisitante { get; set; }

        // FK 1-1 a Persona (UNIQUE en BD)
        public Guid IdPersona { get; set; }

        // Navegación requerida por tu AppDbContext y controladores
        public Persona Persona { get; set; } = null!;
    }
}
