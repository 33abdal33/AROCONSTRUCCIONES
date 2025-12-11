using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionEntidaddeMovimientoI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartidaId",
                table: "MovimientoInventario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "MovimientoInventario",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_PartidaId",
                table: "MovimientoInventario",
                column: "PartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoInventario_ProyectoId",
                table: "MovimientoInventario",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Partidas_PartidaId",
                table: "MovimientoInventario",
                column: "PartidaId",
                principalTable: "Partidas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientoInventario_Proyectos_ProyectoId",
                table: "MovimientoInventario",
                column: "ProyectoId",
                principalTable: "Proyectos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Partidas_PartidaId",
                table: "MovimientoInventario");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientoInventario_Proyectos_ProyectoId",
                table: "MovimientoInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientoInventario_PartidaId",
                table: "MovimientoInventario");

            migrationBuilder.DropIndex(
                name: "IX_MovimientoInventario_ProyectoId",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "PartidaId",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "MovimientoInventario");
        }
    }
}
