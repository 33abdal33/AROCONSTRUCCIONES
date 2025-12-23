using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AROCONSTRUCCIONES.Services.Implementation.PdfTemplates
{
    public class OrdenCompraPdfTemplate : IDocument
    {
        private readonly OrdenCompraPdfModel _model;
        private readonly OrdenCompra _oc;
        private readonly CultureInfo _culture;

        // --- ESTILOS ---
        static TextStyle TitleStyle => TextStyle.Default.FontSize(11).Bold().FontColor(Colors.Black);
        static TextStyle SubTitleStyle => TextStyle.Default.FontSize(8).FontColor(Colors.Black);

        static TextStyle SectionHeaderStyle => TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
        static TextStyle LabelStyle => TextStyle.Default.FontSize(7).Bold();
        static TextStyle ValueStyle => TextStyle.Default.FontSize(7);

        static TextStyle TableHeaderStyle => TextStyle.Default.FontSize(7).Bold();
        static TextStyle TableContentStyle => TextStyle.Default.FontSize(7);

        public OrdenCompraPdfTemplate(OrdenCompraPdfModel model)
        {
            _model = model;
            _oc = model.OrdenCompra;
            _culture = CultureInfo.CreateSpecificCulture("es-PE");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));

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
                // IZQUIERDA
                row.RelativeItem(7).Row(r =>
                {
                    r.AutoItem().Width(80).Height(50).Column(c =>
                    {
                        if (File.Exists(_model.LogoPath))
                            c.Item().Image(_model.LogoPath).FitArea();
                        else
                            c.Item().Text("LOGO").FontSize(20).Bold();
                    });

                    r.RelativeItem().PaddingLeft(10).Column(col =>
                    {
                        col.Item().Text("ARO CONSTRUCTORA Y MINEROS E.I.R.L.").Style(TitleStyle);
                        col.Item().Text("RUC: 20547282559").Style(SubTitleStyle);
                        col.Item().Text("Av. Jose Galvez N° 1146 Lima - Lima - Lima").Style(SubTitleStyle);
                    });
                });

                // DERECHA
                row.RelativeItem(3).Border(2).BorderColor(Colors.Black).Column(col =>
                {
                    col.Item().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter()
                        .Text("ORDEN DE COMPRA / SERVICIO").FontSize(8).Bold();

                    col.Item().PaddingVertical(10).AlignCenter()
                        .Text(_oc.Codigo).FontSize(11).Bold().FontColor(Colors.Red.Medium);
                });
            });
        }

        // --- 2. CONTENT ---
        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(col =>
            {
                col.Item().Element(ComposeDatosObra);
                col.Spacing(10);
                col.Item().Element(ComposeDatosProveedor);
                col.Spacing(10);
                col.Item().Element(ComposeTable);
                col.Spacing(10);
                col.Item().Element(ComposeTotalesSection);
            });
        }

        // --- SECCIÓN OBRA (ACTUALIZADA CON DATOS REALES) ---
        void ComposeDatosObra(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Black).Column(col =>
            {
                col.Item().Background(Colors.Black).Padding(2).PaddingLeft(5)
                    .Text("DATOS DE LA OBRA / SERVICIO").Style(SectionHeaderStyle);

                col.Item().Padding(4).Table(table =>
                {
                    table.ColumnsDefinition(c => {
                        c.ConstantColumn(40); // Etiqueta
                        c.RelativeColumn();   // Valor
                        c.ConstantColumn(40); // Etiqueta
                        c.ConstantColumn(100); // Valor
                    });

                    // Variables para datos seguros
                    var nombreObra = _oc.Proyecto?.NombreProyecto ?? "---";
                    var ubicacionObra = _oc.Proyecto?.Ubicacion ?? "---";
                    var residente = _oc.Proyecto?.Responsable ?? "---";

                    // Fila 1
                    table.Cell().Text("OBRA:").Style(LabelStyle);
                    table.Cell().Text(nombreObra).Style(ValueStyle); // <-- DATO REAL

                    table.Cell().Text("FECHA:").Style(LabelStyle);
                    table.Cell().Text(_oc.FechaEmision.ToString("dd/MM/yyyy")).Style(ValueStyle);

                    // Fila 2
                    table.Cell().Text("EMPRESA:").Style(LabelStyle);
                    table.Cell().Text(" ARO CONSTRUCTORA Y MINEROS E.I.R.L.").Style(ValueStyle);

                    table.Cell().Text("RESP:").Style(LabelStyle);
                    table.Cell().Text(residente).Style(ValueStyle); // <-- DATO REAL

                    // Fila 3 (Ubicación ocupando todo el ancho restante)
                    table.Cell().Text("UBICACION:" ).Style(LabelStyle);
                    table.Cell().Text(ubicacionObra).Style(ValueStyle); // <-- DATO REAL
                });
            });
        }

        // --- SECCIÓN PROVEEDOR (ACTUALIZADA CON DATOS REALES) ---
        void ComposeDatosProveedor(IContainer container)
        {
            container.Border(1).BorderColor(Colors.Black).Column(col =>
            {
                col.Item().Background(Colors.Black).Padding(2).PaddingLeft(5)
                    .Text("DATOS DEL PROVEEDOR").Style(SectionHeaderStyle);

                col.Item().Padding(4).Table(table =>
                {
                    table.ColumnsDefinition(c => {
                        c.ConstantColumn(60); // Etiqueta
                        c.RelativeColumn();   // Valor
                        c.ConstantColumn(70); // Etiqueta
                        c.RelativeColumn();   // Valor
                    });

                    // Variables seguras
                    var contacto = _oc.Proveedor?.NombreContacto ?? "---";
                    var telefono = _oc.Proveedor?.Telefono ?? "---";

                    // Fila 1
                    table.Cell().Text("PROVEEDOR:").Style(LabelStyle);
                    table.Cell().Text(_oc.Proveedor?.RazonSocial).Style(ValueStyle);

                    table.Cell().Text("FORMA PAGO:").Style(LabelStyle);
                    table.Cell().Text(_oc.FormaPago ?? "No especificado").Style(ValueStyle); // DATO REAL

                    // Fila 2
                    table.Cell().Text("RUC:").Style(LabelStyle);
                    table.Cell().Text(_oc.Proveedor?.RUC).Style(ValueStyle);

                    table.Cell().Text("ENTREGA EN:").Style(LabelStyle);
                    table.Cell().Text("ALMACEN OBRA").Style(ValueStyle);

                    // Fila 3
                    table.Cell().Text("DIRECCIÓN:").Style(LabelStyle);
                    table.Cell().Text(_oc.Proveedor?.Direccion).Style(ValueStyle);

                    table.Cell().Text("MONEDA:").Style(LabelStyle);
                    table.Cell().Text(_oc.Moneda == "USD" ? "DÓLARES (USD)" : "SOLES (PEN)").Style(ValueStyle);

                    // Fila 4 (NUEVOS CAMPOS SOLICITADOS)
                    table.Cell().Text("CONTACTO:").Style(LabelStyle);
                    table.Cell().Text(contacto).Style(ValueStyle); // <-- DATO REAL

                    table.Cell().Text("TELÉFONO:").Style(LabelStyle);
                    table.Cell().Text(telefono).Style(ValueStyle); // <-- DATO REAL
                });
            });
        }

        // --- TABLA ---
        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.ConstantColumn(30);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(60);
                });

                // Header
                table.Header(header =>
                {
                    HeaderStyle(header.Cell()).Text("N°");
                    HeaderStyle(header.Cell()).Text("PRODUCTO");
                    HeaderStyle(header.Cell()).Text("MARCA");
                    HeaderStyle(header.Cell()).AlignCenter().Text("UND");
                    HeaderStyle(header.Cell()).AlignCenter().Text("CANT");
                    HeaderStyle(header.Cell()).AlignRight().Text("P.U");
                    HeaderStyle(header.Cell()).AlignRight().Text("DSCTO %");
                    HeaderStyle(header.Cell()).AlignRight().Text("PARCIAL");
                });

                // Filas
                int index = 1;
                if (_oc.Detalles != null) // Pequeña validación de seguridad
                {
                    foreach (var item in _oc.Detalles)
                    {
                        BodyStyle(table.Cell()).AlignCenter().Text($"{index++}");
                        BodyStyle(table.Cell()).Text(item.Material?.Nombre ?? "---");
                        BodyStyle(table.Cell()).Text(""); // Marca
                        BodyStyle(table.Cell()).AlignCenter().Text(item.Material?.UnidadMedida ?? "UND");
                        BodyStyle(table.Cell()).AlignRight().Text(item.Cantidad.ToString("N2", _culture));
                        BodyStyle(table.Cell()).AlignRight().Text(item.PrecioUnitario.ToString("N2", _culture));

                        // Mostrar descuento real si existe
                        var dsctoTexto = item.PorcentajeDescuento > 0 ? $"{item.PorcentajeDescuento:N0}%" : "0%";
                        BodyStyle(table.Cell()).AlignRight().Text(dsctoTexto);

                        // --- CORRECCIÓN AQUÍ: Usar ImporteTotal en vez de Subtotal ---
                        BodyStyle(table.Cell()).AlignRight().Text(item.ImporteTotal.ToString("N2", _culture));
                    }
                }

                // Relleno (Visual)
                int itemsCount = _oc.Detalles?.Count ?? 0;
                for (int i = 0; i < (15 - itemsCount); i++)
                {
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                    BodyStyle(table.Cell()).Text("");
                }
            });
        }
        // Helpers de Estilo
        IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background("#cfe2f3")
                .Border(1).BorderColor(Colors.Black)
                .Padding(2)
                .DefaultTextStyle(TableHeaderStyle);
        }

        IContainer BodyStyle(IContainer container)
        {
            return container
                .Border(1).BorderColor(Colors.Grey.Lighten1)
                .Padding(2)
                .DefaultTextStyle(TableContentStyle);
        }

        // --- TOTALES ---
        // --- TOTALES ---
        void ComposeTotalesSection(IContainer container)
        {
            // --- CORRECCIÓN LÓGICA ---
            // Usamos las propiedades que agregamos al modelo OrdenCompra.
            // Si tu modelo OrdenCompra aún no tiene 'SubTotal' e 'Impuesto', avísame.
            // Asumiendo que aceptaste los cambios del modelo sugeridos:

            decimal subtotal = _oc.SubTotal;
            decimal igv = _oc.Impuesto;
            decimal total = _oc.Total;

            // NOTA: Si te da error en .SubTotal o .Impuesto, es porque no actualizaste 
            // la clase OrdenCompra.cs. Si es así, avísame para darte el código alternativo.

            container.Row(row =>
            {
                // Izquierda (Texto y Observaciones)
                row.RelativeItem(6).PaddingRight(10).Column(col =>
                {
                    col.Item().Border(1).BorderColor(Colors.Black).Padding(4).Text(t => {
                        // USAMOS EL HELPER AQUÍ
                        string textoMonto = NumeroALetras.Convertir(_oc.Total, _oc.Moneda ?? "PEN");
                        t.Span(textoMonto).Bold().FontSize(7);
                    });

                    col.Spacing(5);

                    col.Item().Text("OBSERVACIONES GENERALES:").FontSize(7).Bold();
                    col.Item().Border(1).BorderColor(Colors.Black).Padding(4).MinHeight(40).Column(c => {
                        c.Item().Text("NOTA: Condiciones comerciales según acuerdo...").FontSize(6);
                        if (!string.IsNullOrEmpty(_oc.Observaciones))
                            c.Item().Text($"OBS: {_oc.Observaciones}").FontSize(6);
                    });
                });

                // Derecha (Cuadro numérico)
                row.RelativeItem(4).Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("Sub Total").Style(LabelStyle);
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text(subtotal.ToString("N2", _culture)).Style(ValueStyle);

                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).Text("IGV (18.00%)").Style(LabelStyle);
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text(igv.ToString("N2", _culture)).Style(ValueStyle);

                    table.Cell().Background("#cfe2f3").Border(1).BorderColor(Colors.Black).Padding(3).Text("Total a Pagar").FontSize(8).Bold();
                    table.Cell().Background("#cfe2f3").Border(1).BorderColor(Colors.Black).Padding(3).AlignRight().Text($"{_oc.Moneda} {total.ToString("N2", _culture)}").FontSize(8).Bold();
                });
            });
        }
        // --- FOOTER ---
        void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(40).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(2).BorderColor(Colors.Black).Width(100);
                        c.Item().PaddingTop(2).Text("JEFE DE LOGISTICA").FontSize(7).Bold();
                    });

                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().AlignCenter().BorderBottom(2).BorderColor(Colors.Black).Width(100);
                        c.Item().AlignCenter().PaddingTop(2).Text("V°B° PROVEEDOR").FontSize(7).Bold();
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().BorderBottom(2).BorderColor(Colors.Black).Width(100);
                        c.Item().AlignRight().PaddingTop(2).Text("V°B° GERENCIA DE PROYECTOS").FontSize(7).Bold();
                    });
                });

                col.Item().PaddingTop(10).AlignCenter().Text(text => {
                    text.Span("Generado por AROCONSTRUCCIONES ERP - Página ").FontSize(6);
                    text.CurrentPageNumber().FontSize(6);
                    text.Span(" de ").FontSize(6);
                    text.TotalPages().FontSize(6);
                });
            });
        }
    }
}