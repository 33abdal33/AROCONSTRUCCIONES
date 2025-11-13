using AROCONSTRUCCIONES.Dtos;
// using AROCONSTRUCCIONES.Persistence; // <-- SE VA
using AROCONSTRUCCIONES.Repository.Interfaces; // <-- AÑADIDO
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProyectoDashboardService : IProyectoDashboardService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO

        public ProyectoDashboardService(IUnitOfWork unitOfWork) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
        }
        public async Task<ProyectoDashboardDto> GetDashboardSummaryAsync()
        {
            // Usamos el Contexto expuesto por el Unit of Work
            var dbContext = _unitOfWork.Context;

            var proyectosActivosQuery = dbContext.Proyectos
                .Where(p => p.Estado == "En Ejecución");

            var resumen = await proyectosActivosQuery
                .GroupBy(p => 1)
                .Select(g => new
                {
                    ProyectosActivos = g.Count(),
                    AvancePromedio = g.Average(p => p.AvancePorcentaje),
                    PresupuestoTotal = g.Sum(p => p.Presupuesto),
                    EjecutadoTotal = g.Sum(p => p.CostoEjecutado)
                })
                .FirstOrDefaultAsync();

            var enCronograma = await proyectosActivosQuery
                .CountAsync(p => p.AvancePorcentaje >= 75);

            if (resumen == null)
            {
                return new ProyectoDashboardDto();
            }

            return new ProyectoDashboardDto
            {
                ProyectosActivos = resumen.ProyectosActivos,
                AvancePromedio = (int)resumen.AvancePromedio,
                PresupuestoTotal = resumen.PresupuestoTotal,
                EjecutadoTotal = resumen.EjecutadoTotal,
                EnCronograma = enCronograma
            };
        }
    }
}