using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AROCONSTRUCCIONES.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class nuevasEdicionesParaLaEntidadDetalleOCyMuchosMas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrdenCompra_Material_IdMaterial",
                table: "DetalleOrdenCompra");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Requerimiento",
                newName: "FechaSolicitud");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Requerimiento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRequerida",
                table: "Requerimiento",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Requerimiento",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAprobador",
                table: "Requerimiento",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntregaPactada",
                table: "OrdenCompra",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                table: "OrdenCompra",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Impuesto",
                table: "OrdenCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "OrdenCompra",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "OrdenCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambio",
                table: "OrdenCompra",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetalleOrdenCompra",
                type: "decimal(18,2)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadRecibida",
                table: "DetalleOrdenCompra",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "DetalleOrdenCompra",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "IdDetalleRequerimiento",
                table: "DetalleOrdenCompra",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImporteTotal",
                table: "DetalleOrdenCompra",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeDescuento",
                table: "DetalleOrdenCompra",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleOrdenCompra_IdDetalleRequerimiento",
                table: "DetalleOrdenCompra",
                column: "IdDetalleRequerimiento");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrdenCompra_DetalleRequerimiento_IdDetalleRequerimiento",
                table: "DetalleOrdenCompra",
                column: "IdDetalleRequerimiento",
                principalTable: "DetalleRequerimiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrdenCompra_Material_IdMaterial",
                table: "DetalleOrdenCompra",
                column: "IdMaterial",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrdenCompra_DetalleRequerimiento_IdDetalleRequerimiento",
                table: "DetalleOrdenCompra");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrdenCompra_Material_IdMaterial",
                table: "DetalleOrdenCompra");

            migrationBuilder.DropIndex(
                name: "IX_DetalleOrdenCompra_IdDetalleRequerimiento",
                table: "DetalleOrdenCompra");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "FechaRequerida",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "UsuarioAprobador",
                table: "Requerimiento");

            migrationBuilder.DropColumn(
                name: "FechaEntregaPactada",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "Impuesto",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "TipoCambio",
                table: "OrdenCompra");

            migrationBuilder.DropColumn(
                name: "IdDetalleRequerimiento",
                table: "DetalleOrdenCompra");

            migrationBuilder.DropColumn(
                name: "ImporteTotal",
                table: "DetalleOrdenCompra");

            migrationBuilder.DropColumn(
                name: "PorcentajeDescuento",
                table: "DetalleOrdenCompra");

            migrationBuilder.RenameColumn(
                name: "FechaSolicitud",
                table: "Requerimiento",
                newName: "Fecha");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioUnitario",
                table: "DetalleOrdenCompra",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadRecibida",
                table: "DetalleOrdenCompra",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "DetalleOrdenCompra",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrdenCompra_Material_IdMaterial",
                table: "DetalleOrdenCompra",
                column: "IdMaterial",
                principalTable: "Material",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
