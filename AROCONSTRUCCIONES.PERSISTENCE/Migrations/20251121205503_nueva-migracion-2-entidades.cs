using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class nuevamigracion2entidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Proveedor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CCI",
                table: "Proveedor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCuenta",
                table: "Proveedor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneDetraccion",
                table: "Proveedor",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SolicitudesPagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    OrdenCompraId = table.Column<int>(type: "int", nullable: true),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGV = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoAmortizar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FondoGarantia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoNetoPagar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitadoPorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesPagos_OrdenCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenCompra",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesPagos_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesPagos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleSolicitudPagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudPagoId = table.Column<int>(type: "int", nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaDocumento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleSolicitudPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleSolicitudPagos_SolicitudesPagos_SolicitudPagoId",
                        column: x => x.SolicitudPagoId,
                        principalTable: "SolicitudesPagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSolicitudPagos_SolicitudPagoId",
                table: "DetalleSolicitudPagos",
                column: "SolicitudPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPagos_OrdenCompraId",
                table: "SolicitudesPagos",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPagos_ProveedorId",
                table: "SolicitudesPagos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPagos_ProyectoId",
                table: "SolicitudesPagos",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleSolicitudPagos");

            migrationBuilder.DropTable(
                name: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "CCI",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "NumeroCuenta",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "TieneDetraccion",
                table: "Proveedor");
        }
    }
}
