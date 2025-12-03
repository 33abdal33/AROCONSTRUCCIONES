using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevaMigrasion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprobantePagos_OrdenCompra_OrdenCompraId",
                table: "ComprobantePagos");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprobantePagos_Proveedor_ProveedorId",
                table: "ComprobantePagos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CuentaBancarias",
                table: "CuentaBancarias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComprobantePagos",
                table: "ComprobantePagos");

            migrationBuilder.RenameTable(
                name: "CuentaBancarias",
                newName: "CuentasBancarias");

            migrationBuilder.RenameTable(
                name: "ComprobantePagos",
                newName: "ComprobantesPago");

            migrationBuilder.RenameIndex(
                name: "IX_ComprobantePagos_ProveedorId",
                table: "ComprobantesPago",
                newName: "IX_ComprobantesPago_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_ComprobantePagos_OrdenCompraId",
                table: "ComprobantesPago",
                newName: "IX_ComprobantesPago_OrdenCompraId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CuentasBancarias",
                table: "CuentasBancarias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComprobantesPago",
                table: "ComprobantesPago",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprobantesPago_OrdenCompra_OrdenCompraId",
                table: "ComprobantesPago",
                column: "OrdenCompraId",
                principalTable: "OrdenCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprobantesPago_Proveedor_ProveedorId",
                table: "ComprobantesPago",
                column: "ProveedorId",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprobantesPago_OrdenCompra_OrdenCompraId",
                table: "ComprobantesPago");

            migrationBuilder.DropForeignKey(
                name: "FK_ComprobantesPago_Proveedor_ProveedorId",
                table: "ComprobantesPago");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CuentasBancarias",
                table: "CuentasBancarias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComprobantesPago",
                table: "ComprobantesPago");

            migrationBuilder.RenameTable(
                name: "CuentasBancarias",
                newName: "CuentaBancarias");

            migrationBuilder.RenameTable(
                name: "ComprobantesPago",
                newName: "ComprobantePagos");

            migrationBuilder.RenameIndex(
                name: "IX_ComprobantesPago_ProveedorId",
                table: "ComprobantePagos",
                newName: "IX_ComprobantePagos_ProveedorId");

            migrationBuilder.RenameIndex(
                name: "IX_ComprobantesPago_OrdenCompraId",
                table: "ComprobantePagos",
                newName: "IX_ComprobantePagos_OrdenCompraId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CuentaBancarias",
                table: "CuentaBancarias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComprobantePagos",
                table: "ComprobantePagos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprobantePagos_OrdenCompra_OrdenCompraId",
                table: "ComprobantePagos",
                column: "OrdenCompraId",
                principalTable: "OrdenCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprobantePagos_Proveedor_ProveedorId",
                table: "ComprobantePagos",
                column: "ProveedorId",
                principalTable: "Proveedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
