using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevasEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComprobantePagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoComprobante = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    OrdenCompraId = table.Column<int>(type: "int", nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGV = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoGravado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TieneDetraccion = table.Column<bool>(type: "bit", nullable: false),
                    PorcentajeDetraccion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoDetraccion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumeroConstanciaDetraccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaPagoDetraccion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoRetencion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstadoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantePagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantePagos_OrdenCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenCompra",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComprobantePagos_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuentaBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BancoNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CCI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentaBancarias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantePagos_OrdenCompraId",
                table: "ComprobantePagos",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantePagos_ProveedorId",
                table: "ComprobantePagos",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantePagos");

            migrationBuilder.DropTable(
                name: "CuentaBancarias");
        }
    }
}
