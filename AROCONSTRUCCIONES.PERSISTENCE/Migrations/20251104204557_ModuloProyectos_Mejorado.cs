using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModuloProyectos_Mejorado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoAcumuladoManoObra",
                table: "Proyecto");

            migrationBuilder.RenameColumn(
                name: "PresupuestoTotal",
                table: "Proyecto",
                newName: "Presupuesto");

            migrationBuilder.RenameColumn(
                name: "CostoAcumuladoMateriales",
                table: "Proyecto",
                newName: "CostoEjecutado");

            migrationBuilder.AddColumn<int>(
                name: "AvancePorcentaje",
                table: "Proyecto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Responsable",
                table: "Proyecto",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvancePorcentaje",
                table: "Proyecto");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "Proyecto");

            migrationBuilder.RenameColumn(
                name: "Presupuesto",
                table: "Proyecto",
                newName: "PresupuestoTotal");

            migrationBuilder.RenameColumn(
                name: "CostoEjecutado",
                table: "Proyecto",
                newName: "CostoAcumuladoMateriales");

            migrationBuilder.AddColumn<decimal>(
                name: "CostoAcumuladoManoObra",
                table: "Proyecto",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
