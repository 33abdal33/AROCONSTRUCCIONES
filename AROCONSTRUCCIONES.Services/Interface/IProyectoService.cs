using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IProyectoService
    {
        Task<IEnumerable<ProyectoDto>> GetAllAsync();
        Task<ProyectoDto?> GetByIdAsync(int id);
        Task CreateAsync(ProyectoDto dto);
        Task<Proyecto> UpdateAsync(int id, ProyectoDto dto);
        // Por ahora no implementaremos "Delete". Un proyecto se "Finaliza" o "Cancela".
    }
}