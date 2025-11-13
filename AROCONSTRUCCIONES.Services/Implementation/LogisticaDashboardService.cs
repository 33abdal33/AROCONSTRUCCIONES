using AROCONSTRUCCIONES.Dtos;
// using AROCONSTRUCCIONES.Persistence; // <-- SE VA
using AROCONSTRUCCIONES.Repository.Interfaces; // <-- AÑADIDO
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class LogisticaDashboardService : ILogisticaDashboardService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO

        public LogisticaDashboardService(IUnitOfWork unitOfWork) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
        }

        public async Task<LogisticaDashboardDto> GetSummaryAsync()
        {
            // Usamos el Contexto expuesto por el Unit of Work
            var dbContext = _unitOfWork.Context;

            // 1. Valor del Inventario
            var valorInventario = await dbContext.Inventario
                .SumAsync(i => i.StockActual * i.CostoPromedio);

            // 2. Órdenes Pendientes
            var ordenesPendientes = await dbContext.OrdenesCompra
                .CountAsync(oc => oc.Estado != "Completado" && oc.Estado != "Cancelado");

            // 3. Movimientos del Mes
            var fechaHace30Dias = DateTime.Now.AddDays(-30);
            var movimientosMes = await dbContext.MovimientosInventario
                .CountAsync(m => m.FechaMovimiento >= fechaHace30Dias);

            // 4. Alertas de Stock
            var alertasStock = await dbContext.Inventario
                .CountAsync(i => i.StockActual > 0 && i.StockActual <= i.StockMinimo);

            return new LogisticaDashboardDto
            {
                ValorInventario = valorInventario,
                OrdenesPendientes = ordenesPendientes,
                MovimientosMes = movimientosMes,
                AlertasStock = alertasStock
            };
        }
    }
}