using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevaMigracionEntidadesPlanillasSemanalesDetallePlanillas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanillasSemanales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetoAPagar = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanillasSemanales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanillasSemanales_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallePlanillas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanillaSemanalId = table.Column<int>(type: "int", nullable: false),
                    TrabajadorId = table.Column<int>(type: "int", nullable: false),
                    DiasTrabajados = table.Column<int>(type: "int", nullable: false),
                    TotalHorasNormales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalHorasExtras60 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalHorasExtras100 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JornalPromedio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SueldoBasico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PagoHorasExtras = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonificacionBUC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Movilidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SistemaPension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AportePension = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Conafovicer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetoAPagar = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallePlanillas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallePlanillas_PlanillasSemanales_PlanillaSemanalId",
                        column: x => x.PlanillaSemanalId,
                        principalTable: "PlanillasSemanales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallePlanillas_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallePlanillas_PlanillaSemanalId",
                table: "DetallePlanillas",
                column: "PlanillaSemanalId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePlanillas_TrabajadorId",
                table: "DetallePlanillas",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanillasSemanales_ProyectoId",
                table: "PlanillasSemanales",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallePlanillas");

            migrationBuilder.DropTable(
                name: "PlanillasSemanales");
        }
    }
}
