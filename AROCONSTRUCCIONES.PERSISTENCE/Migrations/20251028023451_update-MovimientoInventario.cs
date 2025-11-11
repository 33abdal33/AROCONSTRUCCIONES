using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updateMovimientoInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "MovimientoInventario");

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "MovimientoInventario",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Responsable",
                table: "MovimientoInventario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockFinal",
                table: "MovimientoInventario",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notas",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "Responsable",
                table: "MovimientoInventario");

            migrationBuilder.DropColumn(
                name: "StockFinal",
                table: "MovimientoInventario");

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "MovimientoInventario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
