using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface; 
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario,Almacenero")]
    public class OrdenCompraController : Controller
    {
        // --- DEPENDENCIAS ---
        private readonly IOrdenCompraServices _ordenCompraService;
        private readonly IRecepcionService _recepcionService;
        private readonly IProveedorService _proveedorService;
        private readonly IMaterialServices _materialService;
        private readonly IAlmacenService _almacenService;
        private readonly IProyectoService _proyectoService; // <-- 1. NUEVO (Para asignar OC a Proyecto)
        private readonly ITesoreriaService _tesoreriaService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequerimientoService _requerimientoService;
        private readonly ILogger<OrdenCompraController> _logger; // <-- Agregar <OrdenCompraController>
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdenCompraController(
            IOrdenCompraServices ordenCompraService,
            IRecepcionService recepcionService,
            IProveedorService proveedorService,
            IMaterialServices materialService,
            ITesoreriaService tesoreriaService,
            IAlmacenService almacenService,
            IProyectoService proyectoService, // <-- INYECTAR
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IRequerimientoService requerimientoService,
            ILogger<OrdenCompraController> logger, // <-- Agregar <OrdenCompraController>
            UserManager<ApplicationUser> userManager)
        {
            _ordenCompraService = ordenCompraService;
            _recepcionService = recepcionService;
            _tesoreriaService = tesoreriaService;
            _proveedorService = proveedorService;
            _materialService = materialService;
            _almacenService = almacenService;
            _proyectoService = proyectoService; // <-- ASIGNAR
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _requerimientoService = requerimientoService;
            _logger = _logger;
            _userManager = userManager;
        }

        // --- ACCIONES DE LECTURA (UI) ---
        [HttpGet]
        public async Task<IActionResult> ListaOrdenes()
        {
            var ordenes = await _ordenCompraService.GetAllOrdenesCompraAsync();
            return PartialView("_ListaOrdenesPartial", ordenes);
        }

        [Authorize(Roles = "Administrador,Usuario")]
        [HttpGet]
        public async Task<IActionResult> CargarFormularioOC()
        {
            await CargarViewBagsFormulario();
            // Inicializamos fecha hoy por defecto
            return PartialView("_OrdenCompraFormPartial", new OrdenCompraCreateDto { FechaEmision = DateTime.Now });
        }

        // --- MÉTODO CLAVE PARA LA TRAZABILIDAD ---
        [Authorize(Roles = "Administrador,Usuario")]
        [HttpGet]
        public async Task<IActionResult> CargarFormularioOCDesdeRequerimiento(int id)
        {
            var requerimiento = await _unitOfWork.Requerimientos.GetByIdWithDetailsAsync(id);

            if (requerimiento == null || requerimiento.Estado != "Aprobado")
                return NotFound("Requerimiento no encontrado o no está aprobado.");

            var prefilledDto = new OrdenCompraCreateDto
            {
                RequerimientoId = id, // <-- ¡VITAL! Aquí se mapea el ID para que viaje al POST
                ProyectoId = requerimiento.IdProyecto,
                Observaciones = $"Atención al Requerimiento {requerimiento.Codigo}",
                FechaEmision = DateTime.Now,
                Moneda = "PEN",
                Detalles = new List<DetalleOrdenCompraCreateDto>()
            };

            foreach (var det in requerimiento.Detalles)
            {
                decimal saldoPendiente = det.CantidadSolicitada - det.CantidadAtendida;
                if (saldoPendiente > 0)
                {
                    prefilledDto.Detalles.Add(new DetalleOrdenCompraCreateDto
                    {
                        IdMaterial = det.IdMaterial,
                        MaterialNombre = det.Material != null ? $"{det.Material.Codigo} - {det.Material.Nombre}" : "Material Desconocido",
                        Cantidad = saldoPendiente,
                        PrecioUnitario = 0,
                        IdDetalleRequerimiento = det.Id
                    });
                }
            }

            if (!prefilledDto.Detalles.Any())
                return Content("Este requerimiento ya fue atendido totalmente.");

            // Cargar proveedores aptos (filtrado por materiales del requerimiento)
            var materialesNecesitadosIds = prefilledDto.Detalles.Select(d => d.IdMaterial).ToList();
            var proveedoresAptos = await _unitOfWork.Context.Set<ProveedorMaterial>()
                .Where(pm => materialesNecesitadosIds.Contains(pm.MaterialId))
                .Select(pm => pm.Proveedor)
                .Distinct()
                .Where(p => p.Estado)
                .ToListAsync();

            ViewBag.Proveedores = new SelectList(proveedoresAptos, "Id", "RazonSocial");
            ViewBag.Materiales = new SelectList(await _materialService.GetAllActiveAsync(), "Id", "Nombre");
            ViewBag.Proyectos = new SelectList(await _proyectoService.GetAllProyectosAsync(), "Id", "NombreProyecto");

            return PartialView("_OrdenCompraFormPartial", prefilledDto);
        }

        [HttpGet]
        public async Task<IActionResult> AbrirModalRecepcion(int id)
        {
            var dto = await _recepcionService.GetDatosParaModalRecepcionAsync(id);
            if (dto == null) return NotFound();

            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");
            return PartialView("_RecepcionOrdenModal", dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialesPorProveedor(int proveedorId)
        {
            var materiales = await _materialService.GetMaterialesPorProveedorAsync(proveedorId);
            var items = materiales.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
            }).ToList();
            return Json(items);
        }

        // --- ACCIÓN PARA VER DETALLES EN MODAL ---
        [HttpGet]
        public async Task<IActionResult> VerDetalles(int id)
        {
            // El controlador solo delega la responsabilidad al servicio
            var orden = await _ordenCompraService.GetByIdWithDetailsAsync(id);

            if (orden == null)
            {
                return NotFound("No se encontró la Orden de Compra.");
            }

            return PartialView("_DetallesOrdenModalPartial", orden);
        }
        // --- ACCIONES DE ESCRITURA ---

        [Authorize(Roles = "Administrador,Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenCompraCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                await CargarViewBagsFormulario();
                return PartialView("_OrdenCompraFormPartial", dto);
            }

            try
            {
                // 1. Llamamos al servicio. 
                // El servicio ya se encarga de: Crear OC, Generar PDF y Cambiar estado del REQ a "Con Orden"
                await _ordenCompraService.CreateOrdenCompraAsync(dto);

                return Json(new
                {
                    success = true,
                    message = "¡Orden de Compra procesada exitosamente!",
                    redirectUrl = Url.Action("Index", "Inventario")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la Orden de Compra.");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarRecepcion(RecepcionMaestroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errorDetails = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Json(new { success = false, message = "Datos inválidos: " + errorDetails });
                }
                await _recepcionService.RegistrarRecepcionAsync(dto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Usuario")]
        public async Task<IActionResult> GenerarSolicitudPago(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false, message = "Usuario no identificado." });

                bool resultado = await _tesoreriaService.GenerarSolicitudPagoDesdeOC(id, user.Id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Solicitud de Pago generada.";
                    TempData["OpenTab"] = "#ordencompra-tab";
                    return Json(new { success = true, message = "Solicitud enviada a Tesorería." });
                }
                else
                {
                    return Json(new { success = false, message = "Esta OC ya tiene una Solicitud de Pago activa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // --- HELPER ACTUALIZADO ---
        private async Task CargarViewBagsFormulario()
        {
            ViewBag.Proveedores = new SelectList(await _proveedorService.GetAllActiveAsync(), "Id", "RazonSocial");
            ViewBag.Materiales = new SelectList(await _materialService.GetAllActiveAsync(), "Id", "Nombre");
            ViewBag.Proyectos = new SelectList(await _proyectoService.GetAllProyectosAsync(), "Id", "NombreProyecto");
        }
    }
}