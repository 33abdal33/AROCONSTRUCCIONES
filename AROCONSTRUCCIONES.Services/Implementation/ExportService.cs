using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ExportService : IExportService
    {
        public byte[] GenerarExcelStock(IEnumerable<InventarioDto> datos)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock Valorizado");

                // Encabezados con estilo ARO
                string[] headers = { "Almacén", "Código", "Material", "Stock Actual", "Unidad", "Costo Prom.", "Valor Total (S/.)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b"); // Slate 800
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int row = 2;
                foreach (var item in datos)
                {
                    worksheet.Cell(row, 1).Value = item.AlmacenUbicacion;
                    worksheet.Cell(row, 2).Value = item.MaterialCodigo;
                    worksheet.Cell(row, 3).Value = item.MaterialNombre;
                    worksheet.Cell(row, 4).Value = item.StockActual;
                    worksheet.Cell(row, 5).Value = item.MaterialUnidadMedida;
                    worksheet.Cell(row, 6).Value = item.CostoPromedio;
                    worksheet.Cell(row, 7).Value = item.ValorTotal;

                    // Formato moneda
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "S/#,##0.00";
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "S/#,##0.00";
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerarExcelConsumo(IEnumerable<ConsumoMaterialProyectoDto> datos, string nombreProyecto)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Consumo Obra");

                // Título Principal
                worksheet.Cell(1, 1).Value = "REPORTE DE CONSUMO DE MATERIALES - " + nombreProyecto.ToUpper();
                worksheet.Range("A1:D1").Merge().Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 14;

                // Headers
                string[] headers = { "Material", "Unidad", "Cantidad Consumida", "Inversión Acumulada" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(3, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#059669"); // Green 600
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Font.Bold = true;
                }

                int row = 4;
                foreach (var item in datos)
                {
                    worksheet.Cell(row, 1).Value = item.MaterialNombre;
                    worksheet.Cell(row, 2).Value = item.UnidadMedida;
                    worksheet.Cell(row, 3).Value = item.CantidadTotal;
                    worksheet.Cell(row, 4).Value = item.CostoTotalAcumulado;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "S/#,##0.00";
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}