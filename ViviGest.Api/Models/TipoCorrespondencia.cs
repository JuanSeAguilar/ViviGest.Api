
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViviGest.Api.Models
{
    [Table("TipoCorrespondencia")]
    public class TipoCorrespondencia
    {
        [Key]
        public int IdTipoCorrespondencia { get; set; }

        // 👈 esta propiedad DEBE llamarse igual que la columna,
        //     o estar marcada con [Column("Nombre")]
        public string Nombre { get; set; } = null!;
    }
}
