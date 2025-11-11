using AROCONSTRUCCIONES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Persistence.Configuration
{
    public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
    {
        public void Configure(EntityTypeBuilder<MovimientoInventario> entity)
        {
            entity.ToTable("MovimientoInventario");

            entity.Property(m => m.TipoMovimiento)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(m => m.Cantidad)
                  .HasPrecision(10, 2);

            // ⭐ CORRECCIÓN: Usar 'Notas' en lugar de 'Referencia'
            entity.Property(m => m.Notas)
                  .HasMaxLength(255); // Usar 255 es común para campos de texto corto/medio

            // ⭐ CONFIGURACIÓN ADICIONAL RECOMENDADA: Añadir las nuevas propiedades al mapeo
            entity.Property(m => m.Responsable)
                  .HasMaxLength(150);

            entity.Property(m => m.StockFinal)
                  .HasPrecision(10, 2);

            entity.Property(m => m.CostoUnitarioMovimiento)
                  .HasPrecision(10, 4); // Usar más decimales para costos unitarios

            // Relaciones
            entity.HasOne(m => m.Material)
                  .WithMany(ma => ma.MovimientosInventario)
                  .HasForeignKey(m => m.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Almacen)
                  .WithMany(a => a.Movimientos)
                  .HasForeignKey(m => m.AlmacenId);

            // Asegúrate de que las relaciones Proveedor (si existe) también estén aquí.
            // entity.HasOne(m => m.Proveedor)
            //       .WithMany(p => p.Movimientos)
            //       .HasForeignKey(m => m.ProveedorId);
        }
    }
}