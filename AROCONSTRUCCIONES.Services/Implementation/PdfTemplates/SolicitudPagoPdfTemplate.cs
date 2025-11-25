using AROCONSTRUCCIONES.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AROCONSTRUCCIONES.Services.Implementation.PdfTemplates
{
    public class SolicitudPagoPdfTemplate : IDocument
    {
        private readonly SolicitudPagoPdfModel _model;
        private readonly SolicitudPago _sp;
        private readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("es-PE");

        public SolicitudPagoPdfTemplate(SolicitudPagoPdfModel model)
        {
            _model = model;
            _sp = model.Solicitud;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Content().Element(ComposeContent);
            });
        }

        void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                // 1. ENCABEZADO PRINCIPAL (Título y Proyecto)
                col.Item().Element(ComposeHeader);

                col.Spacing(5);

                // 2. DATOS GENERALES (Emisión, Moneda, Beneficiario)
                col.Item().Element(ComposeDatosGenerales);

                // 3. DESCRIPCIÓN (Tabla central)
                col.Item().Element(ComposeDescripcionTable);

                // 4. DETALLE DE DOCUMENTOS (Tabla inferior)
                col.Item().Element(ComposeDocumentosTable);

                // 5. PIE DE PÁGINA (Bancos, Neto a Pagar, Firmas)
                col.Item().Element(ComposePiePagina);
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Consorcio/Empresa
                    columns.RelativeColumn(5); // Proyecto (Recuadro Rojo)
                    columns.RelativeColumn(2); // Código
                });

                // Celda 1: Empresa
                table.Cell().Border(1).AlignCenter().AlignMiddle().Padding(5).Column(col =>
                {
                    col.Item().Text("CONSORCIO EJECUTOR").Bold().FontSize(9).AlignCenter();
                    col.Item().Text("DISAMART").Bold().FontSize(10).AlignCenter();
                    // (O puedes poner "ARO CONSTRUCCIONES" aquí si prefieres)
                });

                // Celda 2: PROYECTO (RECUADRO ROJO #1)
                table.Cell().Border(2).BorderColor(Colors.Red.Medium).Padding(5).AlignCenter().AlignMiddle().Column(col =>
                {
                    col.Item().Text("SOLICITUD DE PAGO").Bold().FontSize(14).AlignCenter();
                    col.Item().PaddingTop(5).Text(_sp.Proyecto?.NombreProyecto.ToUpper() ?? "SIN PROYECTO").Bold().FontSize(8).AlignCenter();
                });

                // Celda 3: Código
                table.Cell().Border(1).AlignCenter().AlignMiddle().Padding(5).Text(_sp.Codigo).FontSize(16).Bold();
            });
        }

        void ComposeDatosGenerales(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80); // Label
                    columns.RelativeColumn();   // Value
                    columns.ConstantColumn(60); // Label Moneda
                    columns.ConstantColumn(40); // TC
                });

                // Fila 1: Emisión
                table.Cell().Border(1).Padding(2).Text("Emisión").FontSize(8);
                table.Cell().Border(1).Padding(2).Text(_sp.FechaSolicitud.ToString("dddd dd de MMMM de yyyy", culture)).FontSize(9).Bold();
                table.Cell().Border(1).Padding(2).Text("MONEDA").FontSize(8).AlignCenter();
                table.Cell().Border(1).Padding(2).Text("TC").FontSize(8).AlignCenter();

                // Fila 2: Cantidad
                table.Cell().Border(1).Padding(2).Text("LA CANTIDAD DE").FontSize(8);
                table.Cell().Border(1).Padding(2).Text($"S/. {_sp.MontoTotal.ToString("N2", culture)}").FontSize(11).Bold();
                table.Cell().Border(1).Padding(2).Text("NUEVOS SOLES").FontSize(7).AlignCenter();
                table.Cell().Border(1).Padding(2).Text("").FontSize(8);

                // Fila 3: Cargar A (Beneficiario)
                table.Cell().Border(1).Padding(2).Text("CARGAR A").FontSize(8);
                // Ocupa 3 columnas
                table.Cell().ColumnSpan(3).Border(1).Padding(2).Text(_sp.BeneficiarioNombre?.ToUpper()).FontSize(10).Bold();
            });
        }

        void ComposeDescripcionTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(6); // Descripción
                    columns.RelativeColumn(1); // Metrado
                    columns.RelativeColumn(1); // P.U.
                    columns.RelativeColumn(1); // Parcial
                });

                // Header
                table.Cell().ColumnSpan(4).Border(1).Background(Colors.Grey.Lighten3).Padding(2).Text("DESCRIPCIÓN").FontSize(9).Bold().AlignCenter();

                // Sub-header
                table.Cell().ColumnSpan(4).Border(1).Padding(2).Text("Gastos Generales").FontSize(8).Bold();

                // Concepto Principal
                table.Cell().ColumnSpan(4).Border(1).Padding(2).Text(_sp.Concepto).FontSize(8).Bold();

                // Detalle Row (Simulado con el total, ya que SP agrupa)
                table.Cell().Border(1).Padding(2).Text(_sp.Concepto).FontSize(8);
                table.Cell().Border(1).Padding(2).AlignRight().Text("1.00").FontSize(8);
                table.Cell().Border(1).Padding(2).AlignRight().Text(_sp.MontoNetoAPagar.ToString("N2")).FontSize(8);
                table.Cell().Border(1).Padding(2).AlignRight().Text(_sp.MontoNetoAPagar.ToString("N2")).FontSize(8);

                // Total Parcial
                table.Cell().ColumnSpan(3).Border(0).AlignRight().PaddingRight(5).Text("PARCIAL").FontSize(8).Bold();
                table.Cell().Border(1).Padding(2).AlignRight().Text(_sp.MontoNetoAPagar.ToString("N2")).FontSize(8).Bold();

                // IGV (Si aplica, aquí simplificamos asumiendo incluido o calculado)
                // Para tu caso, si el monto es neto:
                // table.Cell().ColumnSpan(3).AlignRight().PaddingRight(5).Text("IGV").FontSize(8).Bold();
                // table.Cell().Border(1).Padding(2).AlignRight().Text("...").FontSize(8);
            });
        }

        void ComposeDocumentosTable(IContainer container)
        {
            container.PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Doc
                    columns.RelativeColumn(2); // Fecha
                    columns.RelativeColumn(2); // Monto
                    columns.RelativeColumn(2); // Label
                    columns.RelativeColumn(2); // Monto2
                    columns.RelativeColumn(2); // Avance
                });

                // Headers de la tabla interna del PDF
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("Documento").FontSize(7).AlignCenter();
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("Fecha").FontSize(7).AlignCenter();
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("Monto").FontSize(7).AlignCenter();
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("Concepto").FontSize(7).AlignCenter();
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("S/.").FontSize(7).AlignCenter();
                table.Cell().Border(1).Background(Colors.Grey.Lighten4).Padding(2).Text("%").FontSize(7).AlignCenter();

                // Filas (Detalles de la SP)
                foreach (var det in _sp.Detalles)
                {
                    table.Cell().Border(1).Padding(2).Text($"{det.TipoDocumento} {det.NumeroDocumento}").FontSize(7);
                    table.Cell().Border(1).Padding(2).Text(det.FechaEmisionDocumento.ToString("dd/MM/yyyy")).FontSize(7);
                    table.Cell().Border(1).Padding(2).AlignRight().Text(det.Monto.ToString("N2")).FontSize(7);

                    // Columnas de resumen a la derecha (Estáticas por ahora o calculadas)
                    table.Cell().Border(1).Padding(2).Text("Contratado").FontSize(7);
                    table.Cell().Border(1).Padding(2).AlignRight().Text(det.Monto.ToString("N2")).FontSize(7);
                    table.Cell().Border(1).Padding(2).AlignRight().Text("100%").FontSize(7);
                }
            });
        }

        void ComposePiePagina(IContainer container)
        {
            container.PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(7); // Notas y Bancos
                    columns.RelativeColumn(3); // Neto a Pagar
                });

                // Columna Izquierda: Bancos y Notas
                table.Cell().Border(1).Padding(5).Column(col =>
                {
                    col.Item().Text("NOTAS:").Bold().FontSize(8);
                    col.Item().Text($"{_sp.Banco}: {_sp.NumeroCuenta} // CCI: {_sp.CCI}").FontSize(8);
                    col.Item().PaddingTop(5).Text($"SON: [MONTO EN LETRAS] {_sp.Moneda}").Bold().FontSize(8);
                });

                // Columna Derecha: Neto a Pagar
                table.Cell().Border(1).Padding(5).AlignMiddle().Column(col =>
                {
                    col.Item().Text("NETO A PAGAR").Bold().FontSize(10).AlignCenter();
                    col.Item().Text($"S/ {_sp.MontoNetoAPagar.ToString("N2", culture)}").Bold().FontSize(14).AlignCenter();
                });

                // FIRMAS (Fila inferior)
                table.Cell().ColumnSpan(2).PaddingTop(30).Row(row =>
                {
                    row.RelativeItem().Column(c => { c.Item().BorderBottom(1).Height(1); c.Item().PaddingTop(2).Text("V°B° DPTO TECNICO").FontSize(7).AlignCenter(); });
                    row.RelativeItem().Text(""); // Espacio
                    row.RelativeItem().Column(c => { c.Item().BorderBottom(1).Height(1); c.Item().PaddingTop(2).Text("V°B° CONTABILIDAD").FontSize(7).AlignCenter(); });
                    row.RelativeItem().Text(""); // Espacio
                    row.RelativeItem().Column(c => { c.Item().BorderBottom(1).Height(1); c.Item().PaddingTop(2).Text("ADMINISTRACION").FontSize(7).AlignCenter(); });
                });
            });
        }
    }
}