using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addOnDeleteSoftDeleteMovimientoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Material_MaterialId",
                table: "MovimientoInventario",
                column: "MaterialId",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
