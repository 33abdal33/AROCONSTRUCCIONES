using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class nuevas_entidades_para_modeloOC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequerimientoId",
                table: "OrdenCompra",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_RequerimientoId",
                table: "OrdenCompra",
                column: "RequerimientoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenCompra_Requerimiento_RequerimientoId",
                table: "OrdenCompra",
                column: "RequerimientoId",
                principalTable: "Requerimiento",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenCompra_Requerimiento_RequerimientoId",
                table: "OrdenCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenCompra_RequerimientoId",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "RequerimientoId",
                table: "OrdenCompra");
        }
    }
}
