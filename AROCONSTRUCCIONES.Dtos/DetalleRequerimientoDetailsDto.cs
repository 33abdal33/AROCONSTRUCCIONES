namespace AROCONSTRUCCIONES.Dtos
{
    public class DetalleRequerimientoDetailsDto
    {
        public string MaterialCodigo { get; set; }
        public string MaterialNombre { get; set; }
        public string UnidadMedida { get; set; }
        public decimal CantidadSolicitada { get; set; }
        public decimal CantidadAtendida { get; set; }
    }
}