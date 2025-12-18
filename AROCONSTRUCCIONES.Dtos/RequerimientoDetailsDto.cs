namespace AROCONSTRUCCIONES.Dtos
{
    public class RequerimientoDetailsDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string ProyectoNombre { get; set; }
        public string Solicitante { get; set; }
        public string Area { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public List<DetalleRequerimientoDetailsDto> Detalles { get; set; } = new();
    }
}