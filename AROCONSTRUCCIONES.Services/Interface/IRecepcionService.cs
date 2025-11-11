using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IRecepcionService
    {
        Task RegistrarRecepcionAsync(RecepcionMaestroDto dto);
        // Este método contendrá la lógica que antes estaba en el controlador
        Task<RecepcionMaestroDto?> GetDatosParaModalRecepcionAsync(int id);
    }
}
