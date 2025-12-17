using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NuevaMigracionDetallePlanilla35 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AporteEsSalud",
                table: "DetallePlanillas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BonificacionExtraordinaria",
                table: "DetallePlanillas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Gratificacion",
                table: "DetallePlanillas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AporteEsSalud",
                table: "DetallePlanillas");

            migrationBuilder.DropColumn(
                name: "BonificacionExtraordinaria",
                table: "DetallePlanillas");

            migrationBuilder.DropColumn(
                name: "Gratificacion",
                table: "DetallePlanillas");
        }
    }
}
