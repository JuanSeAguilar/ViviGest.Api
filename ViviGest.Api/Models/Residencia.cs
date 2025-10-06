namespace ViviGest.Api.Models
{
    public class Residencia
    {
        public int IdResidencia { get; set; }
        public int IdUsuario { get; set; }
        public Guid IdUnidad { get; set; }
        public DateTime FechaInicio { get; set; }
    }
}
