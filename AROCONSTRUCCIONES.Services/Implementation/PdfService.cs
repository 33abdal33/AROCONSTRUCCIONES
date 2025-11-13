using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Hosting; // Para IWebHostEnvironment
using QuestPDF.Fluent; // <-- AÑADIR
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
            // (Asegúrate de tener esta carpeta y archivo en wwwroot)
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

            // Asegurarse de que el directorio exista
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            // 5. Generar el PDF y guardarlo en el servidor
            // Usamos GeneratePdf en lugar de GeneratePdfAsync para evitar problemas de concurrencia
            document.GeneratePdf(rutaCompleta);

            // 6. Devolver la ruta web (la que usará el <a>)
            return await Task.FromResult($"/ordenes_pdf/{nombreArchivo}");
        }
    }
}