namespace ViviGest.Api.Dtos
{
    public class MarcarEntregaCorrespondenciaDto
    {
        public string EntregadoA { get; set; } = null!;   // nombre de quien retira
        public string? Observacion { get; set; }          // ej. “Entrega a familiar”
    }
}