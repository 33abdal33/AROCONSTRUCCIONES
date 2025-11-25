using AROCONSTRUCCIONES.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AROCONSTRUCCIONES.Services.Implementation.PdfTemplates
{
    

    // La plantilla del PDF
    public class OrdenCompraPdfTemplate : IDocument
    {
        private readonly OrdenCompraPdfModel _model;
        private readonly OrdenCompra _oc;
        private readonly CultureInfo culture;

        public OrdenCompraPdfTemplate(OrdenCompraPdfModel model)
        {
            _model = model;
            _oc = model.OrdenCompra;
            culture = CultureInfo.CreateSpecificCulture("es-PE");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        // Define la estructura: Header, Content, Footer
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30); // 30 puntos de margen
                page.Size(PageSizes.A4);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // --- 1. HEADER ---
        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                // Columna 1: Logo
                row.RelativeItem(2).Column(col =>
                {
                    if (File.Exists(_model.LogoPath))
                        col.Item().Image(_model.LogoPath).FitWidth();
                    else
                        col.Item().Height(40).Text(text => text.Span("ARO").Bold().FontSize(18).FontColor(Colors.Red.Medium).LineHeight(0.8f)); 
                });

                // Columna 2: Datos Empresa
                row.RelativeItem(5).AlignCenter().Column(col =>
                {
                    col.Item().Text(text => text.Span("ARO CONSTRUCTORA Y MINEROS E.I.R.L.").Bold().FontSize(10));
                    col.Item().Text(text => text.Span("RUC: 20547282559").FontSize(7));
                    col.Item().Text(text => text.Span("Av. Jose Galvez N° 1146 Lima - Lima - Lima").FontSize(7));
                });

                // Columna 3: Nro de Orden (OC Box)
                row.RelativeItem(3).Border(2).BorderColor(Colors.Black).Padding(5).Column(col =>
                {
                    // Encabezado
                    col.Item().Background(Colors.Grey.Lighten3).AlignCenter()
                        .Text(text => text.Span("ORDEN DE COMPRA / SERVICIO").Bold().FontSize(9));
                    
                    // Código de OC (Se elimina la restricción Height(10))
                    col.Item().AlignCenter() 
                        .Text(text => text.Span(_oc.Codigo).Bold().FontSize(12).FontColor(Colors.Red.Medium));
                });
            });
        }

        // --- 2. CONTENT (Proveedor y Detalles) ---
        void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                // --- CORRECCIÓN DE MARGIN: Reemplazado por Spacing ---
                col.Item().Element(ComposeProveedor);
                col.Spacing(10); // <-- CAMBIO: Añade espacio DESPUÉS del proveedor
                col.Item().Element(ComposeDetalles);
                col.Spacing(10); // <-- CAMBIO: Añade espacio DESPUÉS de los detalles
                col.Item().Element(ComposeTotales);
            });
        }

        void ComposeProveedor(IContainer container)
        {
            // --- CORRECCIÓN DE MARGIN: Se eliminó .MarginTop(10) de aquí ---
            container.Border(1).Column(col =>
            {
                col.Item().Background(Colors.Grey.Darken3).Padding(3)
                    .Text(text =>
                    {
                        text.Span("DATOS DEL PROVEEDOR").Bold().FontSize(9).FontColor(Colors.White);
                    });

                col.Item().Padding(5).Column(inner =>
                {
                    inner.Item().Row(row =>
                    {
                        row.RelativeItem().Text(txt => { txt.Span("PROVEEDOR: ").Bold().FontSize(8); txt.Span(_oc.Proveedor?.RazonSocial).FontSize(8); });
                        row.RelativeItem().Text(txt => { txt.Span("RUC: ").Bold().FontSize(8); txt.Span(_oc.Proveedor?.RUC).FontSize(8); });
                    });
                    inner.Item().Row(row =>
                    {
                        row.RelativeItem().Text(txt => { txt.Span("DIRECCIÓN: ").Bold().FontSize(8); txt.Span(_oc.Proveedor?.Direccion).FontSize(8); });
                        row.RelativeItem().Text(txt => { txt.Span("TELÉFONO: ").Bold().FontSize(8); txt.Span(_oc.Proveedor?.Telefono).FontSize(8); });
                    });
                    inner.Item().Row(row =>
                    {
                        row.RelativeItem().Text(txt => { txt.Span("FECHA EMISIÓN: ").Bold().FontSize(8); txt.Span(_oc.FechaEmision.ToString("dd/MM/yyyy")).FontSize(8); });
                        row.RelativeItem().Text(txt => { txt.Span("EMAIL: ").Bold().FontSize(8); txt.Span(_oc.Proveedor?.Email).FontSize(8); });
                    });
                });
            });
        }

        void ComposeDetalles(IContainer container)
        {
            // --- CORRECCIÓN DE MARGIN: Se eliminó .MarginTop(10) de aquí ---
            container.Table(table =>
            {
                // Columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Codigo
                    columns.RelativeColumn(7); // Nombre
                    columns.RelativeColumn(2); // Unidad
                    columns.RelativeColumn(2); // Cantidad
                    columns.RelativeColumn(3); // P.U.
                    columns.RelativeColumn(3); // Subtotal
                });

                // Header de la tabla
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("CÓDIGO");
                    header.Cell().Element(HeaderCellStyle).Text("PRODUCTO");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("UND");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("CANT.");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("P. UNITARIO");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("SUBTOTAL");
                });

                // Items
                foreach (var item in _oc.Detalles)
                {
                    table.Cell().Element(BodyCellStyle).Text(item.Material?.Codigo);
                    table.Cell().Element(BodyCellStyle).Text(item.Material?.Nombre);
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(item.Material?.UnidadMedida);
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(item.Cantidad.ToString("N2"));
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(item.PrecioUnitario.ToString("C2")); // Formato Moneda
                    table.Cell().Element(BodyCellStyle).AlignRight().Text(item.Subtotal.ToString("C2"));
                }
            });
        }

        void ComposeTotales(IContainer container)
        {
            // Cálculos
            decimal subtotal = _oc.Total;
            decimal igv = subtotal * 0.18m; // Asumiendo 18%
            decimal totalFinal = subtotal + igv;

            // --- CORRECCIÓN DE MARGIN: Se eliminó .MarginTop(10) de aquí ---
            container.Row(row =>
            {
                // Columna Izquierda: Observaciones
                row.RelativeItem(6).Column(col =>
                {
                    col.Item().Text("OBSERVACIONES:").Bold().FontSize(9);
                    col.Item().Border(1).Padding(5).MinHeight(50).Text(_oc.Observaciones ?? "Sin observaciones.").FontSize(8);
                });

                row.RelativeItem(1).Text(""); // Espaciador

                // Columna Derecha: Totales
                row.RelativeItem(3).Column(col =>
                {
                    // --- CORRECCIÓN DE FONTSIZE: Usando la sintaxis lambda (más segura) ---
                    col.Item().Row(r => {
                        r.RelativeItem().Text(text => text.Span("SUBTOTAL:").FontSize(9));
                        r.RelativeItem().AlignRight().Text(text => text.Span(subtotal.ToString("C2")).FontSize(9));
                    });
                    col.Item().Row(r => {
                        r.RelativeItem().Text(text => text.Span("IGV (18%):").FontSize(9));
                        r.RelativeItem().AlignRight().Text(text => text.Span(igv.ToString("C2")).FontSize(9));
                    });
                    col.Item().Row(r => {
                        r.RelativeItem().Text(text => text.Span("TOTAL:").Bold().FontSize(10));
                        r.RelativeItem().AlignRight().Text(text => text.Span(totalFinal.ToString("C2")).Bold().FontSize(10));
                    });
                    // --- FIN DE CORRECCIÓN ---
                });
            });
        }

        // --- 3. FOOTER ---
        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Generado por AROCONSTRUCCIONES ERP - Página ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
            });
        }

        // --- Métodos de Estilo ---
        static IContainer HeaderCellStyle(IContainer container)
        {
            // Este método AHORA solo define el estilo del CONTENEDOR (la celda)
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(3);
        }

        // --- CORRECCIÓN: Se quita .FontSize() ---
        static IContainer BodyCellStyle(IContainer container)
        {
            // Este método AHORA solo define el estilo del CONTENEDOR (la celda)
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3);
        }
    }
}