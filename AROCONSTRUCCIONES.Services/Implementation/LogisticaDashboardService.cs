using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class LogisticaDashboardService : ILogisticaDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogisticaDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LogisticaDashboardDto> GetSummaryAsync()
        {
            var dbContext = _unitOfWork.Context;

            var valorInventario = await dbContext.Inventario
                .SumAsync(i => i.StockActual * i.CostoPromedio);

            var ordenesPendientes = await dbContext.OrdenesCompra
                .CountAsync(oc => oc.Estado != "Completado" && oc.Estado != "Cancelado");

            var fechaHace30Dias = DateTime.Now.AddDays(-30);
            var movimientosMes = await dbContext.MovimientosInventario
                .CountAsync(m => m.FechaMovimiento >= fechaHace30Dias);

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

        // ⭐ NUEVO MÉTODO PARA LOS GRÁFICOS
        public async Task<LogisticaChartDataDto> GetLogisticaChartDataAsync()
        {
            var dbContext = _unitOfWork.Context;
            var chartData = new LogisticaChartDataDto();

            // 1. Inversión por Almacén (Dona)
            var datosInventario = await dbContext.Inventario
                .Include(i => i.Almacen)
                .ToListAsync();

            chartData.InversionPorAlmacen = datosInventario
                .GroupBy(i => i.Almacen.Nombre)
                .Select(g => new ChartItemDto
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.StockActual * x.CostoPromedio)
                }).ToList();

            // 2. Top 5 Materiales con mayor gasto (Barras)
            var movimientos = await dbContext.MovimientosInventario
                .Include(m => m.Material)
                .Where(m => m.TipoMovimiento == "SALIDA")
                .ToListAsync();

            chartData.ConsumoTopMateriales = movimientos
                .GroupBy(m => m.Material.Nombre)
                .Select(g => new ChartItemDto
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.Cantidad * x.CostoUnitarioMovimiento)
                })
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            return chartData;
        }
    }
}