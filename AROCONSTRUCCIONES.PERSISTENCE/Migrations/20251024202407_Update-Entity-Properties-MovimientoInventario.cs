using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityPropertiesMovimientoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Almacen_IdAlmacen",
                table: "MovimientoInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Material_IdMaterial",
                table: "MovimientoInventario");

            migrationBuilder.RenameColumn(
                name: "IdMaterial",
                table: "MovimientoInventario",
                newName: "MaterialId");

            migrationBuilder.RenameColumn(
                name: "IdAlmacen",
                table: "MovimientoInventario",
                newName: "AlmacenId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoInventario_IdMaterial",
                table: "MovimientoInventario",
                newName: "IX_MovimientoInventario_MaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoInventario_IdAlmacen",
                table: "MovimientoInventario",
                newName: "IX_MovimientoInventario_AlmacenId");

            migrationBuilder.AddColumn<string>(
                name: "NroFacturaGuia",
                table: "MovimientoInventario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "MovimientoInventario",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProveedorId",
                table: "MovimientoInventario",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_ProveedorId",
                table: "MovimientoInventario",
                column: "ProveedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Almacen_AlmacenId",
                table: "MovimientoInventario",
                column: "AlmacenId",
                principalTable: "Almacen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Proveedor_ProveedorId",
                table: "MovimientoInventario",
                column: "ProveedorId",
                principalTable: "Proveedor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Almacen_AlmacenId",
                table: "MovimientoInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Proveedor_ProveedorId",
                table: "MovimientoInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientoInventario_ProveedorId",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "NroFacturaGuia",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "MovimientoInventario");

            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "MovimientoInventario",
                newName: "IdMaterial");

            migrationBuilder.RenameColumn(
                name: "AlmacenId",
                table: "MovimientoInventario",
                newName: "IdAlmacen");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoInventario_MaterialId",
                table: "MovimientoInventario",
                newName: "IX_MovimientoInventario_IdMaterial");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientoInventario_AlmacenId",
                table: "MovimientoInventario",
                newName: "IX_MovimientoInventario_IdAlmacen");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Almacen_IdAlmacen",
                table: "MovimientoInventario",
                column: "IdAlmacen",
                principalTable: "Almacen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Material_IdMaterial",
                table: "MovimientoInventario",
                column: "IdMaterial",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
