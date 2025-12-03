using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevaEntidadCargotrabajador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JornalBasico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BUC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trabajadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CargoId = table.Column<int>(type: "int", nullable: false),
                    SistemaPension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cuspp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Banco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCese = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    ProyectoActualId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trabajadores_Cargos_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trabajadores_Proyectos_ProyectoActualId",
                        column: x => x.ProyectoActualId,
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_CargoId",
                table: "Trabajadores",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_ProyectoActualId",
                table: "Trabajadores",
                column: "ProyectoActualId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trabajadores");

            migrationBuilder.DropTable(
                name: "Cargos");
        }
    }
}
