using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IRequerimientoRepository :IRepositoryBase<Requerimiento>
    {
        // Obtiene todos los requerimientos de un proyecto específico
        Task<IEnumerable<Requerimiento>> GetRequerimientosPorProyectoAsync(int proyectoId);

        // Obtiene un requerimiento con todos sus materiales (para el despacho)
        Task<Requerimiento> GetByIdWithDetailsAsync(int id);
    }
}
