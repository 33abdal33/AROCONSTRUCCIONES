using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Actualizacion_Entidad_CuentaBancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titular",
                table: "CuentasBancarias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Titular",
                table: "CuentasBancarias");
        }
    }
}
