using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Helpers;
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
        private readonly CultureInfo _culture;

        // --- ESTILOS DE FUENTE (Ajustados a la imagen) ---
        static TextStyle HeaderTitleStyle => TextStyle.Default.FontSize(16).Bold().FontColor(Colors.Black);
        static TextStyle CompanyStyle => TextStyle.Default.FontSize(8).Bold();
        static TextStyle ProjectStyle => TextStyle.Default.FontSize(6).Bold(); // Letra pequeña para el nombre largo del proyecto
        static TextStyle LabelStyle => TextStyle.Default.FontSize(7).FontColor(Colors.Black);
        static TextStyle ValueStyle => TextStyle.Default.FontSize(8).FontColor(Colors.Black).Bold();
        static TextStyle BigNumberStyle => TextStyle.Default.FontSize(22).Bold().FontColor(Colors.Black);
        static TextStyle TableHeaderStyle => TextStyle.Default.FontSize(6).Bold();
        static TextStyle SmallTextStyle => TextStyle.Default.FontSize(6);

        public SolicitudPagoPdfTemplate(SolicitudPagoPdfModel model)
        {
            _model = model;
            _sp = model.Solicitud;
            _culture = CultureInfo.CreateSpecificCulture("es-PE");
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(25);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));

                // Todo el contenido va dentro de un borde principal como en la imagen
                page.Content().Element(ComposeContent);

                // Footer simple
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            // Borde exterior grueso (1.5f)
            container.Border(1.5f).BorderColor(Colors.Black).Column(col =>
            {
                // 1. TÍTULO GLOBAL
                col.Item().BorderBottom(1).PaddingVertical(4).AlignCenter().Text("SOLICITUD DE PAGO").Style(HeaderTitleStyle);

                // 2. CABECERA (Consorcio - Obra - Correlativo)
                col.Item().Element(ComposeCabeceraSuperior);

                // 3. DATOS (Emisión - Monto - Cargar A)
                col.Item().Element(ComposeBloqueDatos);

                // 4. DESCRIPCIÓN (Tabla Central)
                col.Item().Element(ComposeTablaDescripcion);

                // 5. TABLA INFERIOR (Documentos, Resumen, Bancos)
                col.Item().Element(ComposeTablaInferior);

                // 6. PIE (Notas, Neto, Firmas)
                col.Item().Element(ComposePieDatos);
            });
        }

        void ComposeCabeceraSuperior(IContainer container)
        {
            container.BorderBottom(1).Row(row =>
            {
                // [IZQUIERDA] Consorcio
                row.RelativeItem(2.5f).BorderRight(1).Padding(5).AlignCenter().AlignMiddle().Column(c =>
                {
                    c.Item().AlignCenter().Text("CONSORCIO EJECUTOR").FontSize(7).Bold();
                    // Aquí ponemos el nombre de tu empresa
                    c.Item().AlignCenter().Text("ARO CONSTRUCTORA Y MINEROS E.I.R.L.").Style(CompanyStyle);
                });

                // [CENTRO] Nombre del Proyecto / Obra
                row.RelativeItem(6f).BorderRight(1).Padding(5).AlignCenter().AlignMiddle().Column(c =>
                {
                    // Usamos comillas y mayúsculas como en el ejemplo
                    var nombreProyecto = _sp.Proyecto?.NombreProyecto?.ToUpper() ?? "SIN PROYECTO ASIGNADO";
                    c.Item().Text($"\"{nombreProyecto}\"").Style(ProjectStyle).AlignCenter();

                    // CUI
                    c.Item().PaddingTop(2).Text("CUI N° 2644941").Style(ProjectStyle).AlignCenter();
                });

                // [DERECHA] Número Correlativo (0006)
                row.RelativeItem(1.5f).Padding(5).AlignCenter().AlignMiddle().Text(t =>
                {
                    // Extraemos solo el número del código (ej: SP-0006 -> 0006)
                    var correlativo = _sp.Codigo.Contains("-") ? _sp.Codigo.Split('-').Last() : _sp.Codigo;
                    t.Span(correlativo).Style(BigNumberStyle);
                });
            });
        }

        void ComposeBloqueDatos(IContainer container)
        {
            container.Column(col =>
            {
                // Fila 1: Emisión | Fecha | MONEDA | TC
                col.Item().BorderBottom(1).Row(row =>
                {
                    row.RelativeItem(2).BorderRight(1).PaddingHorizontal(4).AlignMiddle().Text("Emisión").Style(LabelStyle);
                    row.RelativeItem(5).BorderRight(1).PaddingHorizontal(4).AlignMiddle()
                        .Text(_sp.FechaSolicitud.ToString("dddd dd de MMMM de yyyy", _culture)).Style(LabelStyle); // Fecha normal, no negrita en el ejemplo

                    row.RelativeItem(2).BorderRight(1).PaddingHorizontal(4).AlignMiddle().AlignCenter().Text("MONEDA").Style(LabelStyle);
                    row.RelativeItem(1).PaddingHorizontal(4).AlignMiddle().AlignCenter().Text("TC").Style(LabelStyle);
                });

                // Fila 2: LA CANTIDAD DE | S/. | Monto | NUEVOS SOLES
                col.Item().BorderBottom(1).Row(row =>
                {
                    row.RelativeItem(2).BorderRight(1).PaddingHorizontal(4).AlignMiddle().Text("LA CANTIDAD DE").Style(LabelStyle);

                    row.RelativeItem(1).PaddingHorizontal(4).AlignMiddle().AlignCenter().Text("S/.").FontSize(10).Bold();
                    row.RelativeItem(4).BorderRight(1).PaddingHorizontal(4).AlignMiddle().AlignRight()
                        .Text(_sp.MontoTotal.ToString("N2", _culture)).FontSize(12).Bold();

                    row.RelativeItem(2).BorderRight(1).PaddingHorizontal(4).AlignMiddle().AlignCenter().Text("NUEVOS SOLES").Style(LabelStyle);
                    row.RelativeItem(1).PaddingHorizontal(4).AlignMiddle().Text("");
                });

                // Fila 3: CARGAR A | Beneficiario | Fecha Pago
                col.Item().BorderBottom(1).Row(row =>
                {
                    // Izquierda
                    row.RelativeItem(8).Row(r =>
                    {
                        r.RelativeItem(2).BorderRight(1).Padding(4).AlignMiddle().Text("CARGAR A").Style(LabelStyle);
                        r.RelativeItem(6).Padding(4).AlignMiddle().Text(_sp.BeneficiarioNombre?.ToUpper()).FontSize(9).ExtraBold();
                    });

                    // Derecha (Caja Fecha Pago)
                    row.RelativeItem(2).BorderLeft(1).Column(c =>
                    {
                        c.Item().BorderBottom(1).Padding(2).AlignCenter().Text("Fecha de Pago").Style(LabelStyle);
                        // Si está pagado, mostramos fecha, sino vacío o pendiente
                        var fechaPago = _sp.FechaPago.HasValue ? _sp.FechaPago.Value.ToString("dd 'de' MMMM 'de' yyyy", _culture) : "";
                        c.Item().Padding(2).AlignCenter().Text(fechaPago).FontSize(7);
                    });
                });
            });
        }

        void ComposeTablaDescripcion(IContainer container)
        {
            decimal total = _sp.MontoTotal;
            decimal baseImponible = total / 1.18m;
            decimal igv = total - baseImponible;

            container.Column(col =>
            {
                // Header "DESCRIPCIÓN"
                col.Item().BorderBottom(1).Background(Colors.Grey.Lighten4).PaddingVertical(2).AlignCenter().Text("DESCRIPCIÓN").Style(TableHeaderStyle).FontColor(Colors.Black);

                // Sub-header (Gastos Generales | PARCIAL)
                col.Item().BorderBottom(1).Row(row =>
                {
                    row.RelativeItem(8).BorderRight(1).PaddingHorizontal(4).Text("Gastos Generales").FontSize(7).Bold();
                    row.RelativeItem(2).PaddingHorizontal(4).AlignCenter().Text("PARCIAL").FontSize(7).Bold();
                });

                // Fila Concepto Principal
                string conceptoCorto = _sp.Concepto.Split('-').FirstOrDefault()?.ToUpper() ?? "VARIOS";
                col.Item().BorderBottom(1).PaddingHorizontal(4).Text(conceptoCorto).FontSize(7).Bold();

                // Tabla Interna
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(6); // Descripción Larga
                        c.RelativeColumn(1); // Metrado
                        c.RelativeColumn(1); // P.U.
                        c.RelativeColumn(2); // Parcial (Columna derecha)
                    });

                    // 1. Descripción Detallada
                    table.Cell().PaddingHorizontal(4).PaddingVertical(4).Text(_sp.Concepto).FontSize(7);

                    // 2. Metrado y PU (Subrayados en la imagen)
                    table.Cell().BorderBottom(1).AlignCenter().Text("Metrado").FontSize(6).Underline();
                    table.Cell().BorderBottom(1).AlignCenter().Text("P.U.").FontSize(6).Underline();

                    // 3. Columna Derecha (Totales)
                    table.Cell().RowSpan(2).BorderLeft(1).Column(c =>
                    {
                        // Parcial (Arriba)
                        c.Item().BorderBottom(1).AlignRight().PaddingHorizontal(4).Text(baseImponible.ToString("N2", _culture)).FontSize(7);

                        // PARCIAL Label y Valor
                        c.Item().BorderBottom(1).Row(r =>
                        {
                            r.RelativeItem().Text("PARCIAL").FontSize(6).Bold().AlignRight();
                            r.ConstantItem(5);
                            r.AutoItem().Text(baseImponible.ToString("N2", _culture)).FontSize(7).Bold().AlignRight();
                            r.ConstantItem(4);
                        });

                        // IGV Label y Valor
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("IGV").FontSize(6).Bold().AlignRight();
                            r.ConstantItem(5);
                            r.AutoItem().Text(igv.ToString("N2", _culture)).FontSize(7).Bold().AlignRight();
                            r.ConstantItem(4);
                        });
                    });

                    // Segunda fila de la izquierda (Valores numéricos)
                    table.Cell().Text(""); // Espacio bajo descripción
                    table.Cell().AlignCenter().Text("1.00").FontSize(7);
                    table.Cell().AlignCenter().Text(baseImponible.ToString("N2", _culture)).FontSize(7);
                });

                col.Item().LineHorizontal(1);
            });
        }

        void ComposeTablaInferior(IContainer container)
        {
            container.Row(row =>
            {
                // [IZQUIERDA] Documentos
                row.RelativeItem(4).BorderRight(1).Column(c =>
                {
                    c.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols => { cols.RelativeColumn(2); cols.RelativeColumn(1); cols.RelativeColumn(2); cols.RelativeColumn(2); });

                        // Headers extraños de la imagen (Sin Orden N | 5-2024)
                        table.Cell().ColumnSpan(2).BorderBottom(1).BorderRight(1).AlignCenter().Text("Sin Orden N°").Style(SmallTextStyle).FontColor(Colors.Grey.Medium);
                        table.Cell().ColumnSpan(2).BorderBottom(1).AlignCenter().Text("5 - 2024").Style(SmallTextStyle);

                        // Filas de Documentos
                        foreach (var doc in _sp.Detalles)
                        {
                            table.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text(doc.TipoDocumento).Style(SmallTextStyle);
                            table.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("-").Style(SmallTextStyle);
                            table.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text(doc.FechaEmisionDocumento.ToString("dd/MM/yyyy")).Style(SmallTextStyle);
                            table.Cell().BorderBottom(1).Padding(2).AlignRight().Text(doc.Monto.ToString("N2", _culture)).Style(SmallTextStyle);
                        }

                        // CORRECCIÓN DEL ERROR: Filas vacías sin .Text("")
                        for (int i = 0; i < 4; i++)
                        {
                            table.Cell().BorderBottom(1).BorderRight(1).Height(12); // Solo altura
                            table.Cell().BorderBottom(1).BorderRight(1).Height(12);
                            table.Cell().BorderBottom(1).BorderRight(1).Height(12);
                            table.Cell().BorderBottom(1).Height(12);
                        }
                    });
                });

                // [CENTRO] Resumen Saldos
                row.RelativeItem(4).BorderRight(1).Column(c =>
                {
                    c.Item().Table(t => {
                        t.ColumnsDefinition(cols => { cols.RelativeColumn(3); cols.RelativeColumn(3); cols.RelativeColumn(2); cols.RelativeColumn(2); });

                        t.Cell().ColumnSpan(4).BorderBottom(1).AlignCenter().Text("Total de Pagos (SD) por SO").Style(SmallTextStyle).FontColor(Colors.Grey.Medium);

                        // Contratado
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("Contratado").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).Padding(2).Text("S/.").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).Padding(2).AlignRight().Text(_sp.MontoTotal.ToString("N2", _culture)).Style(SmallTextStyle);

                        // Pagado a cuenta (0.00 placeholder)
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("Pagado a cuenta").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("Saldo a Amortizar (C.D.)").FontSize(5).FontColor(Colors.Grey.Medium);
                        t.Cell().BorderBottom(1).Padding(2).Text("S/.").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).Padding(2).AlignRight().Text("0.00").Style(SmallTextStyle);

                        // Saldo a Pagar
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("Saldo a Pagar").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).BorderRight(1).Padding(2).Text("Fondo de Garantia (C.D.)").FontSize(5).FontColor(Colors.Grey.Medium);
                        t.Cell().BorderBottom(1).Padding(2).Text("S/.").Style(SmallTextStyle);
                        t.Cell().BorderBottom(1).Padding(2).AlignRight().Text(_sp.MontoNetoAPagar.ToString("N2", _culture)).FontSize(7).Bold();

                        // REEMBOLSAR A
                        string nombreCorto = _sp.BeneficiarioNombre?.Split(' ').FirstOrDefault() ?? "BENEFICIARIO";
                        t.Cell().ColumnSpan(4).Height(24).AlignMiddle().AlignCenter().Text($"REEMBOLSAR A {nombreCorto.ToUpper()}").FontSize(8).Bold();
                    });
                });

                // [DERECHA] Avance
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().BorderBottom(1).AlignCenter().Text("Avance SO").Style(SmallTextStyle).FontColor(Colors.Grey.Medium);
                    c.Item().Background(Colors.Grey.Lighten2).BorderBottom(1).PaddingVertical(2).AlignCenter().Text("100.00%").FontSize(7).Bold();
                    c.Item().PaddingVertical(2).AlignCenter().Text("0.00%").Style(SmallTextStyle);
                });
            });
        }

        void ComposePieDatos(IContainer container)
        {
            container.Column(col =>
            {
                // 1. Tabla de Bancos (Simulada)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });

                    // Headers
                    table.Cell().ColumnSpan(2).BorderRight(1).BorderBottom(1).AlignCenter().Text("INGRESO ACUMULADO").Style(SmallTextStyle).FontColor(Colors.Grey.Darken2);
                    table.Cell().ColumnSpan(2).BorderRight(1).BorderBottom(1).AlignCenter().Text("GASTO ACUMULADO").Style(SmallTextStyle).FontColor(Colors.Grey.Darken2);
                    table.Cell().ColumnSpan(2).BorderBottom(1).AlignCenter().Text("SALDO EN BANCO").Style(SmallTextStyle).FontColor(Colors.Grey.Darken2);

                    // Sub-headers (Nacion / Banbif)
                    for (int i = 0; i < 3; i++)
                    {
                        table.Cell().BorderRight(1).BorderBottom(1).AlignCenter().Text("NACION ARO").FontSize(5).FontColor(Colors.Grey.Medium);
                        table.Cell().BorderRight(i < 2 ? 1 : 0).BorderBottom(1).AlignCenter().Text("BANBIF ARO").FontSize(5).FontColor(Colors.Grey.Medium);
                    }

                    // Valores Vacíos
                    for (int i = 0; i < 6; i++)
                    {
                        string txt = (i == 4 || i == 5) ? "#N/D" : "-"; // Simula el error de excel de la imagen xD
                        table.Cell().BorderRight(i < 5 ? 1 : 0).BorderBottom(1).AlignCenter().Text(txt).FontSize(6);
                    }
                });

                // 2. Notas y Neto a Pagar
                col.Item().Row(row =>
                {
                    // Izquierda: Notas y SON
                    row.RelativeItem(7).BorderRight(1).Column(c =>
                    {
                        c.Item().Padding(4).Row(r =>
                        {
                            r.ConstantItem(35).Text("NOTAS:").Style(LabelStyle);
                            string infoBanco = $"{_sp.Banco ?? "BANCO"}: {_sp.NumeroCuenta ?? "-"} // CCI: {_sp.CCI ?? "-"}";
                            r.RelativeItem().Text(infoBanco).FontSize(7).Bold();
                        });

                        c.Item().BorderTop(1).Padding(4).Text(t => {
                            t.Span("SON: ").Style(LabelStyle);
                            t.Span(NumeroALetras.Convertir(_sp.MontoNetoAPagar, "PEN")).FontSize(7).Bold();
                        });
                    });

                    // Derecha: NETO A PAGAR
                    row.RelativeItem(3).Padding(4).Row(r =>
                    {
                        r.RelativeItem().Column(tc => {
                            tc.Item().Text("NETO A").FontSize(8).Bold();
                            tc.Item().Text("PAGAR").FontSize(8).Bold();
                        });
                        r.AutoItem().PaddingRight(4).Text("S/").FontSize(8).Bold();
                        r.AutoItem().Text(_sp.MontoNetoAPagar.ToString("N2", _culture)).FontSize(12).Bold();
                    });
                });

                col.Item().LineHorizontal(1);

                // 3. Firmas
                col.Item().PaddingTop(40).PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(c => { c.Item().Width(100).BorderBottom(1); c.Item().AlignCenter().Text("V°B° DPTO TECNICO").FontSize(6); });
                    row.RelativeItem().AlignCenter().Column(c => { c.Item().Width(100).BorderBottom(1); c.Item().AlignCenter().Text("V°B° CONTABILIDAD").FontSize(6); });
                    row.RelativeItem().AlignCenter().Column(c => { c.Item().Width(100).BorderBottom(1); c.Item().AlignCenter().Text("ADMINISTRACION").FontSize(6); });
                });
            });
        }
    }
}