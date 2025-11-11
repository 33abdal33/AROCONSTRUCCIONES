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
    public class InventarioConfiguration : IEntityTypeConfiguration<Inventario>
    {
        // Ejemplo de InventarioConfiguration
        public void Configure(EntityTypeBuilder<Inventario> entity)
        {
            entity.ToTable("Inventario");

            // ⭐ 1. DEFINICIÓN DE LA CLAVE PRIMARIA COMPUESTA (MaterialId, AlmacenId)
            entity.HasKey(i => new { i.MaterialId, i.AlmacenId });

            // ⭐ 2. RELACIONES CON RESTRICCIÓN DE BORRADO (RESTRICT)

            // Relación con Material: Si un Material tiene stock, no se puede borrar.
            entity.HasOne(i => i.Material)
                  .WithMany()
                  .HasForeignKey(i => i.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict); // ¡Importante para la integridad!

            // Relación con Almacén: Si un Almacén tiene stock, no se puede borrar.
            entity.HasOne(i => i.Almacen)
                  .WithMany()
                  .HasForeignKey(i => i.AlmacenId)
                  .OnDelete(DeleteBehavior.Restrict); // ¡Importante para la integridad!

            // ⭐ 3. DEFINICIÓN DE PROPIEDADES DE VALOR

            entity.Property(i => i.StockActual)
                  .IsRequired()
                  .HasPrecision(18, 2);

            entity.Property(i => i.StockMinimo)
                  .HasPrecision(18, 2); // Permite null, pero definimos precisión

            entity.Property(i => i.CostoPromedio)
                  .IsRequired()
                  .HasPrecision(18, 8);

            entity.Property(i => i.FechaUltimoMovimiento)
                  .IsRequired();
        }
    }
}
