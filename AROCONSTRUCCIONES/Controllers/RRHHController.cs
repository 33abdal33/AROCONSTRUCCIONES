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
            // 1. Obtener la planilla y sus detalles
            var planilla = await _unitOfWork.Context.PlanillasSemanales
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Trabajador)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (planilla == null) return NotFound();

            // 2. Construir el CSV
            var builder = new StringBuilder();

            // Cabecera
            builder.AppendLine("DNI,TRABAJADOR,DIAS,H.NORM,H.EXT,BASICO,PAGO EXTRAS,BUC,MOVILIDAD,TOTAL BRUTO,AFP/ONP,CONAF,TOTAL DSCTO,NETO A PAGAR");

            foreach (var det in planilla.Detalles)
            {
                // Formateamos para evitar problemas con comas en los nombres
                string trabajador = $"\"{det.Trabajador.NombreCompleto}\"";

                var linea = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                    det.Trabajador.NumeroDocumento,
                    trabajador,
                    det.DiasTrabajados,
                    det.TotalHorasNormales,
                    det.TotalHorasExtras60 + det.TotalHorasExtras100,
                    det.SueldoBasico.ToString("F2"),
                    det.PagoHorasExtras.ToString("F2"),
                    det.BonificacionBUC.ToString("F2"),
                    det.Movilidad.ToString("F2"),
                    det.TotalBruto.ToString("F2"),
                    det.AportePension.ToString("F2"),
                    det.Conafovicer.ToString("F2"),
                    det.TotalDescuentos.ToString("F2"),
                    det.NetoAPagar.ToString("F2")
                );

                builder.AppendLine(linea);
            }

            // 3. Generar archivo (con BOM para que Excel lea tildes)
            var nombreArchivo = $"Planilla-{planilla.Codigo}.csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

            return File(bytes, "text/csv", nombreArchivo);
        }
    }
}