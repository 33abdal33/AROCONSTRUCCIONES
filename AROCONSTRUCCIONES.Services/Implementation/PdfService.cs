using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Interface;
using IronPdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.IO;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class PdfService : IPdfService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PdfService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;

            // Configurar la licencia (para desarrollo)
           // IronPdf.License.LicenseKey = "IRONPDF.AROCONSTRUCCIONES.SP-30.11.2025-2C56272E27-E1C7E0B0DF5F6F-DEVELOPMENT.ONLY. watermark.key";
        }

        public async Task<string> GenerarPdfOrdenCompra(OrdenCompra ordenCompra)
        {
            // 1. Renderizar la vista de Razor a un string HTML
            string htmlString = await RenderViewToStringAsync(
                "~/Views/OrdenCompra/_OrdenCompraPdfTemplate.cshtml",
                ordenCompra
            );

            // 2. Usar IronPdf para convertir el HTML a PDF
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(htmlString);

            // 3. Crear la ruta para guardar el archivo
            string nombreArchivo = $"{ordenCompra.Codigo}-{ordenCompra.Id}.pdf";
            string carpeta = Path.Combine(_webHostEnvironment.WebRootPath, "ordenes_pdf");

            // Asegurarse de que el directorio exista
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            // 4. Guardar el PDF en el servidor (en wwwroot/ordenes_pdf/)
            pdf.SaveAs(rutaCompleta);

            // 5. Devolver la ruta web (la que usará el <a>)
            return $"/ordenes_pdf/{nombreArchivo}";
        }

        // --- Método Helper para renderizar Vistas de Razor a String ---
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var httpContext = _httpContextAccessor.HttpContext ??
                new DefaultHttpContext { RequestServices = _httpContextAccessor.HttpContext.RequestServices };

            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());

            await using var sw = new StringWriter();
            var viewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} no encontrada");
            }

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            viewDictionary["WebRootPath"] = _webHostEnvironment.WebRootPath;

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}