using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class LogisticaDashboardService : ILogisticaDashboardService
    {
        private readonly ApplicationDbContext _context;

        public LogisticaDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LogisticaDashboardDto> GetSummaryAsync()
        {
            // 1. Valor del Inventario (Stock * Costo Promedio)
            var valorInventario = await _context.Inventario
                .SumAsync(i => i.StockActual * i.CostoPromedio);

            // 2. Órdenes Pendientes (Las que no están completadas o canceladas)
            var ordenesPendientes = await _context.OrdenesCompra
                .CountAsync(oc => oc.Estado != "Completado" && oc.Estado != "Cancelado");

            // 3. Movimientos del Mes (Últimos 30 días)
            var fechaHace30Dias = DateTime.Now.AddDays(-30);
            var movimientosMes = await _context.MovimientosInventario
                .CountAsync(m => m.FechaMovimiento >= fechaHace30Dias);

            // 4. Alertas de Stock (Stock actual por debajo del mínimo)
            var alertasStock = await _context.Inventario
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