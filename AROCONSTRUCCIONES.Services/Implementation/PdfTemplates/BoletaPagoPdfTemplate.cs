using AROCONSTRUCCIONES.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AROCONSTRUCCIONES.Services.Implementation.PdfTemplates
{
    public class BoletaPagoPdfTemplate : IDocument
    {
        private readonly BoletaPagoPdfModel _model;
        private readonly CultureInfo _culture;

        // Estilos
        static TextStyle TitleStyle => TextStyle.Default.FontSize(14).Bold().FontColor(Colors.Black);
        static TextStyle LabelStyle => TextStyle.Default.FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
        static TextStyle ValueStyle => TextStyle.Default.FontSize(9).FontColor(Colors.Black);
        static TextStyle TableHeaderStyle => TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);

        public BoletaPagoPdfTemplate(BoletaPagoPdfModel model)
        {
            _model = model;
            _culture = CultureInfo.CreateSpecificCulture("es-PE");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                // Tamaño A5 Horizontal (Mitad de A4)
                page.Size(PageSizes.A5.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));

                page.Content().Element(ComposeContent);
            });
        }

        void ComposeContent(IContainer container)
        {
            var det = _model.Detalle;
            var trab = _model.Trabajador;
            var cab = _model.Cabecera;

            container.Border(1).BorderColor(Colors.Black).Padding(10).Column(col =>
            {
                // 1. Encabezado
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        // Logo si existe
                        if (!string.IsNullOrEmpty(_model.LogoPath) && System.IO.File.Exists(_model.LogoPath))
                            c.Item().Height(30).Image(_model.LogoPath).FitArea();

                        c.Item().Text("ARO CONSTRUCTORA Y MINEROS E.I.R.L.").Bold().FontSize(10);
                        c.Item().Text("RUC: 20547282559").FontSize(8);
                    });
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text("BOLETA DE PAGO").Style(TitleStyle);
                        c.Item().Text($"Semana: {cab.FechaInicio:dd/MM} al {cab.FechaFin:dd/MM/yyyy}").FontSize(8);
                        c.Item().Text($"Planilla N°: {cab.Codigo}").FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                });

                col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // 2. Datos del Trabajador
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("TRABAJADOR: ").Style(LabelStyle); t.Span(trab.NombreCompleto).Style(ValueStyle).Bold(); });
                        c.Item().Text(t => { t.Span("CARGO: ").Style(LabelStyle); t.Span(trab.Cargo?.Nombre ?? "Obrero").Style(ValueStyle); });
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("DNI: ").Style(LabelStyle); t.Span(trab.NumeroDocumento).Style(ValueStyle); });
                        c.Item().Text(t => { t.Span("DIAS TRAB.: ").Style(LabelStyle); t.Span(det.DiasTrabajados.ToString()).Style(ValueStyle); });
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("SISTEMA: ").Style(LabelStyle); t.Span(det.SistemaPension).Style(ValueStyle); });
                        c.Item().Text(t => { t.Span("CUSPP: ").Style(LabelStyle); t.Span(trab.Cuspp ?? "-").Style(ValueStyle); });
                    });
                });

                col.Spacing(10);

                // 3. Tabla de Conceptos
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(4); // Concepto
                        c.RelativeColumn(2); // Ingresos
                        c.RelativeColumn(2); // Descuentos
                        c.RelativeColumn(2); // Neto (Solo para total)
                    });

                    // Header
                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderStyle).Text("CONCEPTOS REMUNERATIVOS");
                        h.Cell().Element(HeaderStyle).AlignRight().Text("INGRESOS");
                        h.Cell().Element(HeaderStyle).AlignRight().Text("DESCUENTOS");
                        h.Cell().Element(HeaderStyle).AlignRight().Text("NETO");
                    });

                    // --- INGRESOS ---
                    Row(table, $"Jornal Básico ({det.TotalHorasNormales} hrs)", det.SueldoBasico, 0);

                    if (det.PagoHorasExtras > 0)
                        Row(table, $"Horas Extras (60%: {det.TotalHorasExtras60} | 100%: {det.TotalHorasExtras100})", det.PagoHorasExtras, 0);

                    if (det.BonificacionBUC > 0)
                        Row(table, "Bonif. Unificada Construcción (BUC)", det.BonificacionBUC, 0);

                    if (det.Movilidad > 0)
                        Row(table, "Movilidad Supeditada", det.Movilidad, 0);

                    // --- DESCUENTOS ---
                    Row(table, $"Aporte {det.SistemaPension}", 0, det.AportePension);
                    Row(table, "Conafovicer (2%)", 0, det.Conafovicer);

                    // Relleno visual mínimo
                    table.Cell().ColumnSpan(4).PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // --- TOTALES ---
                    table.Cell().Element(TotalLabelStyle).Text("TOTALES");
                    table.Cell().Element(TotalValueStyle).AlignRight().Text(det.TotalBruto.ToString("N2", _culture));
                    table.Cell().Element(TotalValueStyle).AlignRight().Text(det.TotalDescuentos.ToString("N2", _culture)).FontColor(Colors.Red.Medium);

                    // Neto en grande
                    table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Black).Padding(5).AlignRight().AlignMiddle()
                        .Text(det.NetoAPagar.ToString("N2", _culture)).FontSize(11).ExtraBold();
                });

                col.Spacing(5);

                // Neto en letras (usando tu helper existente si está disponible, o placeholder)
                // string netoLetras = NumeroALetras.Convertir(det.NetoAPagar);
                // col.Item().Text($"SON: {netoLetras}").FontSize(8).Italic();

                // 4. Firmas
                col.Item().PaddingTop(35).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().BorderBottom(1).Width(120).AlignCenter();
                        c.Item().AlignCenter().Text("EMPLEADOR").FontSize(7);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().BorderBottom(1).Width(120).AlignCenter();
                        c.Item().AlignCenter().Text("TRABAJADOR (Firma/Huella)").FontSize(7);
                        c.Item().AlignCenter().Text($"DNI: {trab.NumeroDocumento}").FontSize(7);
                    });
                });
            });
        }

        void Row(TableDescriptor table, string label, decimal ing, decimal desc)
        {
            table.Cell().PaddingVertical(2).Text(label).FontSize(8);
            table.Cell().PaddingVertical(2).AlignRight().Text(ing > 0 ? ing.ToString("N2", _culture) : "").FontSize(8);
            table.Cell().PaddingVertical(2).AlignRight().Text(desc > 0 ? desc.ToString("N2", _culture) : "").FontSize(8);
            table.Cell().Text(""); // Espacio de la columna neto
        }

        // Helpers de Estilo
        IContainer HeaderStyle(IContainer container)
            => container.Background("#2c3e50").Padding(3).DefaultTextStyle(TableHeaderStyle);

        IContainer TotalLabelStyle(IContainer container)
            => container.Padding(4).AlignRight().DefaultTextStyle(x => x.Bold().FontSize(9));

        IContainer TotalValueStyle(IContainer container)
            => container.Padding(4).DefaultTextStyle(x => x.Bold().FontSize(9));
    }
}