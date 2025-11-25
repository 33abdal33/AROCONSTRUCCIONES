using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface ITesoreriaService
    {
        // Este método recibe una OC aprobada y crea la SP
        Task<bool> GenerarSolicitudPagoDesdeOC(int ordenCompraId, string userId);

        // --- NUEVO MÉTODO ---
        Task<IEnumerable<SolicitudPago>> GetAllSolicitudesAsync();

        // --- NUEVO MÉTODO (Para el PDF después) ---
        Task<SolicitudPago?> GetSolicitudByIdAsync(int id);
    }
}
