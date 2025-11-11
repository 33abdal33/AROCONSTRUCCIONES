using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProyectoDashboardService : IProyectoDashboardService
    {
        private readonly ApplicationDbContext _context;

        public ProyectoDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProyectoDashboardDto> GetDashboardSummaryAsync()
        {
            // Filtra solo los proyectos que están "En Ejecución"
            var proyectosActivosQuery = _context.Proyectos
                .Where(p => p.Estado == "En Ejecución");

            // Calcula los KPIs
            var resumen = await proyectosActivosQuery
                .GroupBy(p => 1) // Agrupa todo en una sola fila
                .Select(g => new
                {
                    ProyectosActivos = g.Count(),
                    AvancePromedio = g.Average(p => p.AvancePorcentaje),
                    PresupuestoTotal = g.Sum(p => p.Presupuesto),
                    EjecutadoTotal = g.Sum(p => p.CostoEjecutado)
                })
                .FirstOrDefaultAsync();

            // Cuenta los que están en cronograma (Avance >= % esperado)
            // (Esta es una lógica simplificada. Una real compararía fechas)
            var enCronograma = await proyectosActivosQuery
                .CountAsync(p => p.AvancePorcentaje >= 75); // Asumiendo "75%" como "a tiempo"

            if (resumen == null)
            {
                return new ProyectoDashboardDto(); // Devuelve DTO vacío si no hay proyectos
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