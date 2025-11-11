using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class newentitybase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdRequerimiento",
                table: "Requerimiento",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdProveedor",
                table: "Proveedor",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdOrdenCompra",
                table: "OrdenCompra",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdMovimiento",
                table: "MovimientoInventario",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdMaterial",
                table: "Material",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdDetalle",
                table: "DetalleRequerimiento",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdDetalle",
                table: "DetalleOrdenCompra",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdAlmacen",
                table: "Almacen",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Requerimiento",
                newName: "IdRequerimiento");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Proveedor",
                newName: "IdProveedor");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OrdenCompra",
                newName: "IdOrdenCompra");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MovimientoInventario",
                newName: "IdMovimiento");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Material",
                newName: "IdMaterial");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DetalleRequerimiento",
                newName: "IdDetalle");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DetalleOrdenCompra",
                newName: "IdDetalle");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Almacen",
                newName: "IdAlmacen");
        }
    }
}
