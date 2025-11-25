using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class edicion_Entidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FondoGarantia",
                table: "SolicitudesPagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoNetoAPagar",
                table: "SolicitudesPagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoAmortizar",
                table: "SolicitudesPagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FondoGarantia",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "MontoNetoAPagar",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "SaldoAmortizar",
                table: "SolicitudesPagos");
        }
    }
}
