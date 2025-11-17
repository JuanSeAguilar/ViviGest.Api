
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("EstadoCorrespondencia")]
    public class EstadoCorrespondencia
    {
        [Key]
        public int IdEstadoCorrespondencia { get; set; }

        public string Nombre { get; set; } = null!;
    }
}
