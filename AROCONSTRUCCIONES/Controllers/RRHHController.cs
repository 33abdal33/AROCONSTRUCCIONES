using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces; // Para IUnitOfWork
using AROCONSTRUCCIONES.Services.Interface;      // Para Servicios
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // <--- ESTE ES EL USING CLAVE PARA EL .Include()
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario")]
    public class RRHHController : Controller
    {
        private readonly IRecursosHumanosService _rrhhService;
        private readonly IProyectoService _proyectoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPdfService _pdfService;

        public RRHHController(
            IRecursosHumanosService rrhhService,
            IProyectoService proyectoService,
            IUnitOfWork unitOfWork,
            IPdfService pdfService)
        {
            _rrhhService = rrhhService;
            _proyectoService = proyectoService;
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Contenedor principal
        }

        [HttpGet]
        public async Task<IActionResult> ListaTrabajadoresPartial()
        {
            var trabajadores = await _rrhhService.GetAllTrabajadoresAsync();
            return PartialView("_ListaTrabajadoresPartial", trabajadores);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrabajadorParaEditar(int id)
        {
            var dto = id == 0 ? new TrabajadorDto { Estado = true } : await _rrhhService.GetTrabajadorByIdAsync(id);

            ViewBag.Cargos = new SelectList(await _rrhhService.GetAllCargosAsync(), "Id", "Nombre");
            ViewBag.Pensiones = new SelectList(new[] { "ONP", "INTEGRA", "PRIMA", "HABITAT", "PROFUTURO" });
            ViewBag.Bancos = new SelectList(new[] { "BCP", "BBVA", "INTERBANK", "NACION", "SCOTIABANK" });

            return PartialView("_TrabajadorFormPartial", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTrabajador(TrabajadorDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cargos = new SelectList(await _rrhhService.GetAllCargosAsync(), "Id", "Nombre");
                ViewBag.Pensiones = new SelectList(new[] { "ONP", "INTEGRA", "PRIMA", "HABITAT", "PROFUTURO" });
                ViewBag.Bancos = new SelectList(new[] { "BCP", "BBVA", "INTERBANK", "NACION", "SCOTIABANK" });
                return PartialView("_TrabajadorFormPartial", dto);
            }

            if (dto.Id == 0)
                await _rrhhService.CreateTrabajadorAsync(dto);
            else
                await _rrhhService.UpdateTrabajadorAsync(dto.Id, dto);

            TempData["SuccessMessage"] = "Trabajador guardado correctamente.";
            TempData["OpenTab"] = "#trabajadores-tab";

            return Json(new { success = true, message = "Guardado exitoso." });
        }

        // --- TAREO ---
        [HttpGet]
        public async Task<IActionResult> TareoIndex(int proyectoId = 0)
        {
            var proyectos = await _proyectoService.GetAllAsync();
            var proyectosActivos = proyectos.Where(p => p.Estado == "En Ejecución" || p.Estado == "Planificación");

            ViewBag.Proyectos = new SelectList(proyectosActivos, "Id", "NombreProyecto");
            ViewBag.ProyectoIdSeleccionado = proyectoId;

            return PartialView("_TareoIndexPartial");
        }

        [HttpGet]
        public async Task<IActionResult> ListaTareos(int proyectoId)
        {
            if (proyectoId == 0) return Content("Seleccione un proyecto.");
            var tareos = await _rrhhService.GetHistorialTareosAsync(proyectoId);
            return PartialView("_ListaTareosPartial", tareos);
        }

        [HttpGet]
        public async Task<IActionResult> GetTareoForm(int id, int proyectoId)
        {
            if (proyectoId == 0) return Content("Error: Falta Proyecto ID");
            var dto = await _rrhhService.GetTareoParaEditarAsync(id, proyectoId);
            return PartialView("_TareoFormPartial", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarTareo(TareoDto dto)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Datos inválidos" });

            try
            {
                await _rrhhService.GuardarTareoAsync(dto);
                TempData["SuccessMessage"] = "Tareo guardado correctamente.";
                return Json(new { success = true, message = "Tareo registrado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- PLANILLAS ---
        [HttpGet]
        public async Task<IActionResult> PlanillaIndex()
        {
            var proyectos = await _proyectoService.GetAllAsync();
            var activos = proyectos.Where(p => p.Estado == "En Ejecución");
            ViewBag.Proyectos = new SelectList(activos, "Id", "NombreProyecto");
            return PartialView("_PlanillaIndexPartial");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarPrePlanilla(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var planillaDto = await _rrhhService.GenerarPrePlanillaAsync(proyectoId, fechaInicio, fechaFin);
                planillaDto.ProyectoId = proyectoId;

                var proyecto = await _proyectoService.GetByIdAsync(proyectoId);
                planillaDto.ProyectoNombre = proyecto?.NombreProyecto ?? "Desconocido";

                return PartialView("_PlanillaCalculadaPartial", planillaDto);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-warning'>{ex.Message}</div>");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AprobarPlanilla([FromBody] PlanillaSemanalDto dto)
        {
            if (dto == null) return Json(new { success = false, message = "No se recibieron datos." });
            if (dto.ProyectoId <= 0) return Json(new { success = false, message = $"Error crítico: ProyectoId inválido." });

            try
            {
                await _rrhhService.GuardarPlanillaAsync(dto);
                TempData["SuccessMessage"] = "Planilla aprobada y enviada a Tesorería.";
                TempData["OpenTab"] = "#planilla-tab";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- HISTORIAL Y BOLETAS ---
        [HttpGet]
        public async Task<IActionResult> HistorialIndex(int proyectoId = 0)
        {
            var proyectos = await _proyectoService.GetAllAsync();
            ViewBag.Proyectos = new SelectList(proyectos, "Id", "NombreProyecto");
            return PartialView("_HistorialIndexPartial");
        }

        [HttpGet]
        public async Task<IActionResult> ListaHistorial(int proyectoId)
        {
            var historial = await _rrhhService.GetHistorialPlanillasAsync(proyectoId);
            return PartialView("_ListaHistorialPartial", historial);
        }

        [HttpGet]
        public async Task<IActionResult> DescargarBoleta(int id)
        {
            // Acceso directo al DbSet a través del UnitOfWork
            var detalle = await _unitOfWork.Context.Set<Models.DetallePlanilla>()
                .Include(d => d.PlanillaSemanal)
                .Include(d => d.Trabajador).ThenInclude(t => t.Cargo)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detalle == null) return NotFound();

            // Generar el PDF usando el servicio
            var url = await _pdfService.GenerarBoletaPago(detalle);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarPlanillaCsv(int id)
        {
            var planilla = await _unitOfWork.Context.PlanillasSemanales
                .Include(p => p.Detalles).ThenInclude(d => d.Trabajador).ThenInclude(t => t.Cargo)
                .AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (planilla == null) return NotFound();

            var tareos = await _unitOfWork.Context.Tareos
                .Include(t => t.Detalles)
                .Where(t => t.ProyectoId == planilla.ProyectoId && t.Fecha >= planilla.FechaInicio && t.Fecha <= planilla.FechaFin)
                .ToListAsync();

            var sb = new StringBuilder();
            // Cabecera
            sb.AppendLine("DNI,TRABAJADOR,CARGO,SALARIO,DIAS LAB,DIAS FER,BASICO,DOMINICAL,BUC,MOVILIDAD,INDEM(15%),VAC(10%),GRATI,BONIF,H.E. 60%,H.E. 100%,INDEM H.E.,TOTAL BRUTO,BASE AFP,SNP 13%,AFP APORTE,AFP PRIMA,AFP COMISION,CONAFOVICER,TOTAL DSCTO,NETO,ESSALUD");

            foreach (var det in planilla.Detalles)
            {
                // 1. Datos básicos
                var asis = tareos.SelectMany(t => t.Detalles).Where(d => d.TrabajadorId == det.TrabajadorId).ToList();
                int diasLab = asis.Count(d => d.TipoAsistencia == "LB");
                int diasFer = asis.Count(d => d.TipoAsistencia == "FE" || d.TipoAsistencia == "DM");
                decimal jornal = det.JornalPromedio;

                // 2. BUC Real
                decimal bucReal = (jornal * diasLab) * (det.Trabajador.Cargo.Nombre.ToUpper().Contains("OPERARIO") ? 0.32m : 0.30m);

                // 3. INDEM H.E. (Factores Fijos)
                decimal factorIndem = 1.15m; // Default Peón
                string cargo = det.Trabajador.Cargo?.Nombre.ToUpper() ?? "";
                if (cargo.Contains("OPERARIO")) factorIndem = 1.63m;
                else if (cargo.Contains("OFICIAL")) factorIndem = 1.28m;
                else if (cargo.Contains("PEON")) factorIndem = 1.15m;

                // Suma de horas físicas para aplicar el factor
                decimal totalHorasFisicas = det.TotalHorasExtras60 + det.TotalHorasExtras100;
                decimal indemHE = totalHorasFisicas * factorIndem;

                // 4. Montos Dinero Extras
                decimal monto60 = det.TotalHorasExtras60 * (jornal / 8m * 1.60m);
                decimal monto100 = det.TotalHorasExtras100 * (jornal / 8m * 2.00m);

                // 5. Total Bruto (Reconstruido para Excel)
                decimal brutoExcel = det.SueldoBasico + monto60 + monto100 + indemHE + bucReal + det.Movilidad + det.Indemnizacion + det.Vacaciones + det.Gratificacion + det.BonificacionExtraordinaria;

                // 6. Base AFP (CORREGIDO: Restamos INDEM H.E.)
                decimal baseAfp = brutoExcel - det.Movilidad - det.Indemnizacion - det.Gratificacion - det.BonificacionExtraordinaria - indemHE;

                // 7. Cálculos de Descuentos
                string snp = "-", afpObl = "-", afpPrima = "-", afpCom = "-";

                if (det.SistemaPension.Contains("ONP") || det.SistemaPension.Contains("SNP"))
                {
                    snp = (baseAfp * 0.13m).ToString("F2");
                }
                else // AFP
                {
                    // Simulación Desglose AFP (Ajustado a Habitat/Integra)
                    afpObl = (baseAfp * 0.10m).ToString("F2");      // 10% Aporte
                    afpPrima = (baseAfp * 0.0137m).ToString("F2");  // 1.37% Prima

                    // Comisión es el restante hasta llegar al total descontado en Planilla
                    // (Ya que en BD guardamos el total calculado con la tasa exacta de la AFP)
                    // Si quieres que el Excel recalcule exacto según la base nueva:
                    decimal comision = (det.AportePension) - (baseAfp * 0.1137m);
                    if (comision < 0) comision = baseAfp * 0.0155m; // Fallback 1.55% si no cuadra

                    afpCom = comision.ToString("F2");
                }

                string linea = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}",
                    det.Trabajador.NumeroDocumento,
                    $"\"{det.Trabajador.NombreCompleto}\"",
                    cargo,
                    jornal.ToString("F2"),
                    diasLab, diasFer,
                    (jornal * (diasLab + diasFer)).ToString("F2"), // Basico Dias
                    (det.SueldoBasico - (jornal * (diasLab + diasFer))).ToString("F2"), // Dominical
                    bucReal.ToString("F2"),
                    det.Movilidad.ToString("F2"),
                    det.Indemnizacion.ToString("F2"),
                    det.Vacaciones.ToString("F2"),
                    det.Gratificacion.ToString("F2"),
                    det.BonificacionExtraordinaria.ToString("F2"),
                    monto60.ToString("F2"),
                    monto100.ToString("F2"),
                    indemHE.ToString("F2"),
                    brutoExcel.ToString("F2"),
                    baseAfp.ToString("F2"), // <--- AQUI YA ESTARÁ RESTADO EL INDEM HE
                    snp, afpObl, afpPrima, afpCom,
                    det.Conafovicer.ToString("F2"),
                    det.TotalDescuentos.ToString("F2"),
                    (brutoExcel - det.TotalDescuentos).ToString("F2"),
                    det.AporteEsSalud.ToString("F2")
                );
                sb.AppendLine(linea);
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"Planilla_Final_{planilla.Codigo}.csv");
        }
    }
}