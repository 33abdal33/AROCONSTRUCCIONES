using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nuevaconfiguracion_InventarioAlmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "Almacen",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Almacen_ProyectoId",
                table: "Almacen",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Almacen_Proyectos_ProyectoId",
                table: "Almacen",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Almacen_Proyectos_ProyectoId",
                table: "Almacen");

            migrationBuilder.DropIndex(
                name: "IX_Almacen_ProyectoId",
                table: "Almacen");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "Almacen");
        }
    }
}
