using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevasEntidadesTareosDetalleTareos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tareos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleTareos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TareoId = table.Column<int>(type: "int", nullable: false),
                    TrabajadorId = table.Column<int>(type: "int", nullable: false),
                    HorasNormales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HorasExtras60 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HorasExtras100 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Asistio = table.Column<bool>(type: "bit", nullable: false),
                    JornalBasicoDiario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CargoDia = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleTareos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleTareos_Tareos_TareoId",
                        column: x => x.TareoId,
                        principalTable: "Tareos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleTareos_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTareos_TareoId",
                table: "DetalleTareos",
                column: "TareoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTareos_TrabajadorId",
                table: "DetalleTareos",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tareos_ProyectoId",
                table: "Tareos",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleTareos");

            migrationBuilder.DropTable(
                name: "Tareos");
        }
    }
}
