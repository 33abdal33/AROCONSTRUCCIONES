using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevaEntidadMovimientoBancario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovimientosBancarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuentaBancariaId = table.Column<int>(type: "int", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoDespues = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NumeroOperacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SolicitudPagoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosBancarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimientosBancarios_SolicitudesPagos_SolicitudPagoId",
                        column: x => x.SolicitudPagoId,
                        principalTable: "SolicitudesPagos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_CuentaBancariaId",
                table: "MovimientosBancarios",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancarios_SolicitudPagoId",
                table: "MovimientosBancarios",
                column: "SolicitudPagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosBancarios");
        }
    }
}
