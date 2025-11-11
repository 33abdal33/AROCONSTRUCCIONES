using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requerimiento_Proyecto_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Proyecto",
                table: "Proyecto");

            migrationBuilder.RenameTable(
                name: "Proyecto",
                newName: "Proyectos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Proyectos",
                table: "Proyectos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requerimiento_Proyectos_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requerimiento_Proyectos_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Proyectos",
                table: "Proyectos");

            migrationBuilder.RenameTable(
                name: "Proyectos",
                newName: "Proyecto");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Proyecto",
                table: "Proyecto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requerimiento_Proyecto_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId",
                principalTable: "Proyecto",
                principalColumn: "Id");
        }
    }
}
