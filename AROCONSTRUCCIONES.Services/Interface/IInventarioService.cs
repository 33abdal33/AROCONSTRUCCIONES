using AROCONSTRUCCIONES.Dtos;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IInventarioService
    {
        // Método para obtener todos los saldos y mapearlos al DTO enriquecido
        Task<IEnumerable<InventarioDto>> GetAllStockViewAsync();

        // Método para obtener el stock de un material/almacén específico (si se necesita)
        Task<InventarioDto?> GetStockByKeysAsync(int materialId, int almacenId);

        // NOTA: Los métodos Add/Update del saldo no van aquí
        // porque se llaman internamente desde MovimientoInventarioService, no desde el Controlador.
    }
}
