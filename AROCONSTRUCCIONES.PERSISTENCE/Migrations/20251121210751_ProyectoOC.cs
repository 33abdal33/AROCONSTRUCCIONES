using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProyectoOC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "OrdenCompra",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenCompra_ProyectoId",
                table: "OrdenCompra",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenCompra_Proyectos_ProyectoId",
                table: "OrdenCompra",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenCompra_Proyectos_ProyectoId",
                table: "OrdenCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenCompra_ProyectoId",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "OrdenCompra");
        }
    }
}
