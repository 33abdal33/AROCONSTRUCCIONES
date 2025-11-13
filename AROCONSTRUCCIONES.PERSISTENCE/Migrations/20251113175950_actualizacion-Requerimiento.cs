using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class actualizacionRequerimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requerimiento_Proyectos_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropIndex(
                name: "IX_Requerimiento_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "Requerimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Requerimiento_IdProyecto",
                table: "Requerimiento",
                column: "IdProyecto");

            migrationBuilder.AddForeignKey(
                name: "FK_Requerimiento_Proyectos_IdProyecto",
                table: "Requerimiento",
                column: "IdProyecto",
                principalTable: "Proyectos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requerimiento_Proyectos_IdProyecto",
                table: "Requerimiento");

            migrationBuilder.DropIndex(
                name: "IX_Requerimiento_IdProyecto",
                table: "Requerimiento");

            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "Requerimiento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requerimiento_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requerimiento_Proyectos_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id");
        }
    }
}
