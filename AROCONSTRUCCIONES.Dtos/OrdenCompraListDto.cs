namespace AROCONSTRUCCIONES.Dtos
{
    public class OrdenCompraListDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string ProveedorRazonSocial { get; set; } // Aplanado
        public DateTime FechaEmision { get; set; }
        public string Estado { get; set; }

        public decimal Total { get; set; }
        // --- ¡AÑADE ESTA LÍNEA! ---
        public string? RutaPdf { get; set; }
    }
}
