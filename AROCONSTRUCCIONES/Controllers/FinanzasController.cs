using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador")] // Solo Admin maneja bancos
    public class FinanzasController : Controller
    {
        private readonly IFinanzasService _finanzasService;

        public FinanzasController(IFinanzasService finanzasService)
        {
            _finanzasService = finanzasService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Carga el contenedor principal
        }

        [HttpGet]
        public async Task<IActionResult> ListaCuentasPartial()
        {
            var cuentas = await _finanzasService.GetAllCuentasAsync();
            return PartialView("_ListaCuentasPartial", cuentas);
        }

        [HttpGet]
        public async Task<IActionResult> GetCuentaParaEditar(int id)
        {
            var dto = id == 0 ? new CuentaBancariaDto { Activo = true } : await _finanzasService.GetCuentaByIdAsync(id);
            return PartialView("_CuentaFormPartial", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCuenta(CuentaBancariaDto dto)
        {
            if (!ModelState.IsValid) return PartialView("_CuentaFormPartial", dto);

            if (dto.Id == 0)
                await _finanzasService.CreateCuentaAsync(dto);
            else
                await _finanzasService.UpdateCuentaAsync(dto.Id, dto);

            TempData["SuccessMessage"] = "Cuenta bancaria guardada.";
            TempData["OpenTab"] = "#bancos-tab"; // Para recargar la pestaña

            return Json(new { success = true, message = "Cuenta guardada." });
        }
        [HttpGet]
        public async Task<IActionResult> MovimientosIndex()
        {
            // Cargamos la lista de cuentas para el filtro
            var cuentas = await _finanzasService.GetAllCuentasAsync();
            ViewBag.Cuentas = new SelectList(cuentas, "Id", "BancoNombre");

            return PartialView("_MovimientosIndexPartial");
        }

        [HttpGet]
        public async Task<IActionResult> ListaMovimientosPartial(int? cuentaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Si no vienen fechas, sugerimos los últimos 30 días por defecto
            if (!fechaInicio.HasValue) fechaInicio = DateTime.Now.AddDays(-30);
            if (!fechaFin.HasValue) fechaFin = DateTime.Now;

            var movimientos = await _finanzasService.GetMovimientosAsync(cuentaId, fechaInicio, fechaFin);
            return PartialView("_ListaMovimientosPartial", movimientos);
        }
        [HttpGet]
        public async Task<IActionResult> ExportarMovimientosCsv(int? cuentaId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // 1. Reutilizamos la lógica del servicio para obtener los mismos datos que se ven en pantalla
            var movimientos = await _finanzasService.GetMovimientosAsync(cuentaId, fechaInicio, fechaFin);

            // 2. Construir el CSV
            var builder = new StringBuilder();

            // Cabecera
            builder.AppendLine("FECHA,BANCO,CUENTA,MONEDA,TIPO,DESCRIPCION,OPERACION,INGRESO,EGRESO,SALDO");

            foreach (var m in movimientos)
            {
                // Preparamos valores para columnas de Ingreso/Egreso
                string ingreso = m.TipoMovimiento == "INGRESO" ? m.Monto.ToString("F2") : "0.00";
                string egreso = m.TipoMovimiento == "EGRESO" ? m.Monto.ToString("F2") : "0.00";

                // Limpiamos descripción de comas para no romper el CSV
                string descripcion = $"\"{m.Descripcion?.Replace("\"", "\"\"")}\"";

                var linea = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    m.FechaMovimiento.ToString("yyyy-MM-dd HH:mm"),
                    m.BancoNombre,
                    m.NumeroCuenta,
                    m.Moneda,
                    m.TipoMovimiento,
                    descripcion,
                    m.NumeroOperacion,
                    ingreso,
                    egreso,
                    m.SaldoDespues.ToString("F2")
                );

                builder.AppendLine(linea);
            }

            // 3. Generar archivo
            string nombreArchivo = $"MovimientosBancarios_{DateTime.Now:yyyyMMdd_HHmm}.csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            return File(bytes, "text/csv", nombreArchivo);
        }
    }
}