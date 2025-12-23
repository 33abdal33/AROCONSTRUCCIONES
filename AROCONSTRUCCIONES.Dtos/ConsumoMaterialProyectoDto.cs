namespace AROCONSTRUCCIONES.Dtos
{
    public class ConsumoMaterialProyectoDto
    {
        public string MaterialNombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CantidadTotal { get; set; }
        public decimal CostoTotalAcumulado { get; set; }
        public decimal PorcentajeDelTotal { get; set; } // Qué tanto pesa este material en el gasto del proyecto
    }
}