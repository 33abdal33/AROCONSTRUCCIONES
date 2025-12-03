using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Implementation.PdfTemplates;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class PdfService : IPdfService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GenerarPdfOrdenCompra(OrdenCompra ordenCompra)
        {
            // 1. Definir la ruta del logo
            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Iconos", "ARO.png");

            // 2. Crear el modelo para la plantilla
            var model = new OrdenCompraPdfModel
            {
                OrdenCompra = ordenCompra,
                LogoPath = logoPath
            };

            // 3. Instanciar la plantilla
            var document = new OrdenCompraPdfTemplate(model);

            // 4. Crear la ruta para guardar el archivo
            string nombreArchivo = $"{ordenCompra.Codigo}-{ordenCompra.Id}.pdf";
            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "ordenes_pdf");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            // 5. Generar el PDF
            document.GeneratePdf(rutaCompleta);

            // 6. Devolver la ruta web
            return await Task.FromResult($"/ordenes_pdf/{nombreArchivo}");
        }

        public async Task<string> GenerarPdfSolicitudPago(SolicitudPago solicitudPago)
        {
            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Iconos", "ARO.png");

            var model = new SolicitudPagoPdfModel
            {
                Solicitud = solicitudPago,
                LogoPath = logoPath
            };

            var document = new SolicitudPagoPdfTemplate(model);

            // Sanear nombre de archivo
            string codigoLimpio = solicitudPago.Codigo.Replace("/", "-").Replace("\\", "-");
            string nombreArchivo = $"SP-{codigoLimpio}.pdf";

            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "solicitudes_pago_pdf");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            document.GeneratePdf(rutaCompleta);

            return await Task.FromResult($"/solicitudes_pago_pdf/{nombreArchivo}");
        }

        // --- NUEVO MÉTODO: Generar Boleta de Pago ---
        public async Task<string> GenerarBoletaPago(DetallePlanilla detalle)
        {
            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Iconos", "ARO.png");

            var model = new BoletaPagoPdfModel
            {
                Detalle = detalle,
                Cabecera = detalle.PlanillaSemanal,
                Trabajador = detalle.Trabajador,
                LogoPath = logoPath
            };

            var document = new BoletaPagoPdfTemplate(model);

            // Nombre único: Boleta-DNI-SEMANA-ID.pdf
            string nombreArchivo = $"Boleta-{detalle.Trabajador.NumeroDocumento}-{detalle.Id}.pdf";
            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "boletas_pdf");

            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);
            document.GeneratePdf(rutaCompleta);

            return await Task.FromResult($"/boletas_pdf/{nombreArchivo}");
        }
    }
}