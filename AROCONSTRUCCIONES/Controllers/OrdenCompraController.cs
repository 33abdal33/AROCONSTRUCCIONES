
using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface; // ¡Ahora usamos todos los servicios!
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Permitir que todos los roles logueados vean el Dashboard
    [Authorize(Roles = "Administrador,Usuario,Almacenero")]
    public class OrdenCompraController : Controller
    {
        // --- DEPENDENCIAS (SOLO SERVICIOS) ---
        private readonly IOrdenCompraServices _ordenCompraService;
        private readonly IRecepcionService _recepcionService;
        private readonly IProveedorService _proveedorService;
        private readonly IMaterialServices _materialService;
        private readonly IAlmacenService _almacenService;
        private readonly IUnitOfWork _unitOfWork; // <-- 1. AÑADIR
        private readonly IMapper _mapper; // <-- 2. AÑADIR

        public OrdenCompraController(
            IOrdenCompraServices ordenCompraService,
            IRecepcionService recepcionService,
            IProveedorService proveedorService,      // <-- NUEVO
            IMaterialServices materialService,   // <-- NUEVO
            IAlmacenService almacenService,
            IMapper mapper,
            IUnitOfWork unitOfWork)      // <-- NUEVO
        {
            _ordenCompraService = ordenCompraService;
            _recepcionService = recepcionService;
            _proveedorService = proveedorService;
            _materialService = materialService;
            _almacenService = almacenService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        // --- ACCIONES DE LECTURA (UI) ---

        // GET: /OrdenCompra/ListaOrdenes
        [HttpGet]
        public async Task<IActionResult> ListaOrdenes()
        {
            var ordenes = await _ordenCompraService.GetAllOrdenesCompraAsync();
            return PartialView("_ListaOrdenesPartial", ordenes);
        }

        // GET: /OrdenCompra/CargarFormularioOC
        [Authorize(Roles = "Administrador,Usuario")]
        [HttpGet]
        public async Task<IActionResult> CargarFormularioOC()
        {
            await CargarViewBagsFormulario();
            // Ahora devuelve el PARCIAL que crearemos en el Paso 3
            return PartialView("_OrdenCompraFormPartial", new OrdenCompraCreateDto());
        }

        [Authorize(Roles = "Administrador,Usuario")]
        [HttpGet]
        public async Task<IActionResult> CargarFormularioOCDesdeRequerimiento(int id)
        {
            // 1. Cargar el Requerimiento con sus detalles (Materiales)
            var requerimiento = await _unitOfWork.Requerimientos.GetByIdWithDetailsAsync(id);
            if (requerimiento == null || requerimiento.Estado != "Aprobado")
            {
                return NotFound("Requerimiento no encontrado o no está aprobado.");
            }

            // 2. Mapear Requerimiento -> OrdenCompraCreateDto
            var prefilledDto = _mapper.Map<OrdenCompraCreateDto>(requerimiento);

            // 3. Cargar los ViewBags que el modal necesita
            await CargarViewBagsFormulario();

            // 4. Devolver el mismo modal, pero ahora con los datos precargados
            return PartialView("_OrdenCompraFormPartial", prefilledDto);
        }

        // GET: /OrdenCompra/AbrirModalRecepcion/5
        [HttpGet]
        public async Task<IActionResult> AbrirModalRecepcion(int id)
        {
            // La lógica de "GetById" y "Map" se mueve al servicio.
            var dto = await _recepcionService.GetDatosParaModalRecepcionAsync(id); // <-- Lógica movida al servicio
            if (dto == null)
                return NotFound();

            // La carga del ViewBag también se mueve al servicio o se llama desde aquí
            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");

            return PartialView("_RecepcionOrdenModal", dto);
        }

        // GET: /OrdenCompra/GetMaterialesPorProveedor?proveedorId=5
        [HttpGet]
        public async Task<IActionResult> GetMaterialesPorProveedor(int proveedorId)
        {
            // Esta lógica le pertenece al MaterialService, no al controlador
            var materiales = await _materialService.GetMaterialesPorProveedorAsync(proveedorId);

            var items = materiales.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
            }).ToList();

            return Json(items);
        }

        // --- ACCIONES DE ESCRITURA (NEGOCIO) ---
        // (Estas ya estaban perfectas, devuelven JSON)
        [Authorize(Roles = "Administrador,Usuario")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenCompraCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Si la validación falla, recargamos los dropdowns y devolvemos
                // el formulario parcial para que el modal muestre los errores.
                await CargarViewBagsFormulario();
                return PartialView("_OrdenCompraFormPartial", dto);
            }
            try
            {
                await _ordenCompraService.CreateOrdenCompraAsync(dto);
                TempData["Exito"] = "¡Orden de Compra creada exitosamente!";
                TempData["OpenTab"] = "#ordencompra-tab";
                var redirectUrl = Url.Action("Index", "Inventario");
                return Json(new { success = true, redirectUrl = redirectUrl, message = "¡Orden de Compra creada!" });
            }
            catch (DbUpdateException ex) // Captura errores de EF Core
            {
                // Revisa si la "InnerException" es un error de SQL
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    // 2601 es el código de error para VIOLACIÓN DE ÍNDICE ÚNICO (UNIQUE INDEX)
                    return Json(new { success = false, message = $"Error: Ya existe una Orden de Compra con el código '{dto.Codigo}'. No se puede duplicar." });
                }
                return Json(new { success = false, message = "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message });
            }
            catch (ApplicationException appEx) // Errores de negocio (lanzados por el servicio)
            {
                return Json(new { success = false, message = appEx.InnerException?.Message ?? appEx.Message });
            }
            catch (Exception ex) // Errores inesperados
            {
                return Json(new { success = false, message = "Error inesperado: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarRecepcion(RecepcionMaestroDto dto)
        {
            // (Esta acción ya era "pura" y está perfecta, no se toca)
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

        // --- HELPER ---
        private async Task CargarViewBagsFormulario()
        {
            // El helper ahora llama a los SERVICIOS, no a los Repositorios.
            ViewBag.Proveedores = new SelectList(
                await _proveedorService.GetAllActiveAsync(), "Id", "RazonSocial");

            ViewBag.Materiales = new SelectList(
                await _materialService.GetAllActiveAsync(), "Id", "Nombre");
        }
    }
}
