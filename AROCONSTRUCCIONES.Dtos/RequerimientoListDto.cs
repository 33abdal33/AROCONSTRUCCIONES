namespace AROCONSTRUCCIONES.Dtos
{
    public class RequerimientoListDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public DateTime Fecha { get; set; }
        public string Solicitante { get; set; }
        public string Estado { get; set; } // Pendiente, Aprobado, Despachado, Cancelado
        public string ProyectoNombre { get; set; } // Para la nueva columna de la tabla
        public string Prioridad { get; set; } // Ejemplo: "Normal", "Urgente"

    }
}
