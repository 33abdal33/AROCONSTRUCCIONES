using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdProyecto",
                table: "Requerimiento",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProyectoId",
                table: "Requerimiento",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Proyecto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoProyecto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreProyecto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostoAcumuladoMateriales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostoAcumuladoManoObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PresupuestoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyecto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requerimiento_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requerimiento_Proyecto_ProyectoId",
                table: "Requerimiento",
                column: "ProyectoId",
                principalTable: "Proyecto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requerimiento_Proyecto_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropTable(
                name: "Proyecto");

            migrationBuilder.DropIndex(
                name: "IX_Requerimiento_ProyectoId",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "IdProyecto",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                table: "Requerimiento");
        }
    }
}
