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
            // 1. Cargar Requerimiento
            var requerimiento = await _unitOfWork.Requerimientos.GetByIdWithDetailsAsync(id);
            
            if (requerimiento == null || requerimiento.Estado != "Aprobado")
            {
                return NotFound("Requerimiento no encontrado o no está aprobado."); // Mejor respuesta que string plano
            }

            // 2. MAPEO MANUAL (CRÍTICO PARA TRAZABILIDAD)
            // Hacemos esto manual para asegurar que el ID pase correctamente y calcular saldos.
            var prefilledDto = new OrdenCompraCreateDto
            {
                ProyectoId = requerimiento.IdProyecto, // Heredamos el proyecto
                Observaciones = $"Atención al Requerimiento {requerimiento.Codigo}",
                FechaEmision = DateTime.Now,
                Moneda = "PEN",
                Detalles = new List<DetalleOrdenCompraCreateDto>()
            };

            if (requerimiento.Detalles != null)
            {
                foreach (var det in requerimiento.Detalles)
                {
                    // Solo agregamos si falta por atender
                    decimal saldoPendiente = det.CantidadSolicitada - det.CantidadAtendida;

                    if (saldoPendiente > 0)
                    {
                        prefilledDto.Detalles.Add(new DetalleOrdenCompraCreateDto
                        {
                            IdMaterial = det.IdMaterial,
                            // Sugerimos comprar solo lo que falta
                            Cantidad = saldoPendiente, 
                            // Buscamos el precio actual del sistema (Opcional, si tienes lista de precios)
                            PrecioUnitario = 0, 
                            // ¡ESTO ES LO MÁS IMPORTANTE! Vinculamos con el ID original
                            IdDetalleRequerimiento = det.Id 
                        });
                    }
                }
            }

            if (!prefilledDto.Detalles.Any())
            {
                 // Caso borde: El requerimiento existe pero ya todo fue comprado.
                 return Content("Este requerimiento ya fue atendido totalmente.");
            }

            // 3. Cargar ViewBags
            await CargarViewBagsFormulario();

            // 4. Retornar Vista
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
                await _ordenCompraService.CreateOrdenCompraAsync(dto);
                
                TempData["Exito"] = "¡Orden de Compra creada exitosamente!";
                TempData["OpenTab"] = "#ordencompra-tab"; // Mantiene al usuario en la pestaña
                
                // Redirigir a Inventory/Index suele ser seguro, o al dashboard de compras
                var redirectUrl = Url.Action("Index", "Inventario"); 
                return Json(new { success = true, redirectUrl = redirectUrl, message = "¡Orden de Compra creada!" });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    return Json(new { success = false, message = $"Error: Código duplicado." });
                }
                return Json(new { success = false, message = "Error BD: " + (ex.InnerException?.Message ?? ex.Message) });
            }
            catch (ApplicationException appEx)
            {
                return Json(new { success = false, message = appEx.Message }); // Mensajes controlados por nuestro Servicio
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado: " + ex.Message });
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
            // 1. Proveedores
            ViewBag.Proveedores = new SelectList(
                await _proveedorService.GetAllActiveAsync(), "Id", "RazonSocial");

            // 2. Materiales
            ViewBag.Materiales = new SelectList(
                await _materialService.GetAllActiveAsync(), "Id", "Nombre");

            // 3. Proyectos (¡IMPORTANTE! Agregado)
            // Necesitamos saber para qué obra es la compra
            ViewBag.Proyectos = new SelectList(
                await _proyectoService.GetAllProyectosAsync(), "Id", "NombreProyecto");
        }
    }
}