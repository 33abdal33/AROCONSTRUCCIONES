using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Update_MODELS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "FondoGarantia",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "IGV",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "MontoNetoPagar",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "SaldoAmortizar",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "SolicitadoPorId",
                table: "SolicitudesPagos");

            migrationBuilder.RenameColumn(
                name: "FechaEmision",
                table: "SolicitudesPagos",
                newName: "FechaSolicitud");

            migrationBuilder.RenameColumn(
                name: "FechaDocumento",
                table: "DetalleSolicitudPagos",
                newName: "FechaEmisionDocumento");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "SolicitudesPagos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AutorizadoPorUserId",
                table: "SolicitudesPagos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "SolicitudesPagos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiarioNombre",
                table: "SolicitudesPagos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiarioRUC",
                table: "SolicitudesPagos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CCI",
                table: "SolicitudesPagos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                table: "SolicitudesPagos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAutorizacion",
                table: "SolicitudesPagos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCuenta",
                table: "SolicitudesPagos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolicitadoPorUserId",
                table: "SolicitudesPagos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "DetalleSolicitudPagos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "OrdenCompraId",
                table: "DetalleSolicitudPagos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieDocumento",
                table: "DetalleSolicitudPagos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPagos_AutorizadoPorUserId",
                table: "SolicitudesPagos",
                column: "AutorizadoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesPagos_SolicitadoPorUserId",
                table: "SolicitudesPagos",
                column: "SolicitadoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSolicitudPagos_OrdenCompraId",
                table: "DetalleSolicitudPagos",
                column: "OrdenCompraId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleSolicitudPagos_OrdenCompra_OrdenCompraId",
                table: "DetalleSolicitudPagos",
                column: "OrdenCompraId",
                principalTable: "OrdenCompra",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesPagos_AspNetUsers_AutorizadoPorUserId",
                table: "SolicitudesPagos",
                column: "AutorizadoPorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesPagos_AspNetUsers_SolicitadoPorUserId",
                table: "SolicitudesPagos",
                column: "SolicitadoPorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleSolicitudPagos_OrdenCompra_OrdenCompraId",
                table: "DetalleSolicitudPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesPagos_AspNetUsers_AutorizadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesPagos_AspNetUsers_SolicitadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesPagos_AutorizadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesPagos_SolicitadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropIndex(
                name: "IX_DetalleSolicitudPagos_OrdenCompraId",
                table: "DetalleSolicitudPagos");

            migrationBuilder.DropColumn(
                name: "AutorizadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "BeneficiarioNombre",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "BeneficiarioRUC",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "CCI",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "Concepto",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "FechaAutorizacion",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "NumeroCuenta",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "SolicitadoPorUserId",
                table: "SolicitudesPagos");

            migrationBuilder.DropColumn(
                name: "OrdenCompraId",
                table: "DetalleSolicitudPagos");

            migrationBuilder.DropColumn(
                name: "SerieDocumento",
                table: "DetalleSolicitudPagos");

            migrationBuilder.RenameColumn(
                name: "FechaSolicitud",
                table: "SolicitudesPagos",
                newName: "FechaEmision");

            migrationBuilder.RenameColumn(
                name: "FechaEmisionDocumento",
                table: "DetalleSolicitudPagos",
                newName: "FechaDocumento");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "SolicitudesPagos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "SolicitudesPagos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FondoGarantia",
                table: "SolicitudesPagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "IGV",
                table: "SolicitudesPagos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "SolicitudesPagos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoNetoPagar",
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

            migrationBuilder.AddColumn<string>(
                name: "SolicitadoPorId",
                table: "SolicitudesPagos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "DetalleSolicitudPagos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
