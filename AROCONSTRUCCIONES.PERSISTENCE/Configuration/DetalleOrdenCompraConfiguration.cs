using AROCONSTRUCCIONES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AROCONSTRUCCIONES.Persistence.Configuration
{
    public class DetalleOrdenCompraConfiguration : IEntityTypeConfiguration<DetalleOrdenCompra>
    {
        public void Configure(EntityTypeBuilder<DetalleOrdenCompra> entity)
        {
            entity.ToTable("DetalleOrdenCompra");

            // 1. Configuración de Precisión (Importante para construcción y dinero)
            // Usamos (18,4) para Cantidad y PrecioUnitario para mayor precisión (ej: 0.3333 m3)
            entity.Property(d => d.Cantidad)
                  .HasPrecision(18, 4);

            entity.Property(d => d.PrecioUnitario)
                  .HasPrecision(18, 4);

            entity.Property(d => d.CantidadRecibida)
                  .HasPrecision(18, 4);

            // ImporteTotal es dinero, (18,2) suele bastar, pero (18,2) asegura compatibilidad financiera
            entity.Property(d => d.ImporteTotal)
                  .HasPrecision(18, 2);

            entity.Property(d => d.PorcentajeDescuento)
                  .HasPrecision(5, 2); // Ej: 100.00%

            // --- ELIMINADO: entity.Ignore(d => d.Subtotal); --- 
            // Ya no existe 'Subtotal', ahora usamos 'ImporteTotal' y sí se guarda en BD.

            // 2. Relaciones

            // Relación con OrdenCompra (Cabecera)
            entity.HasOne(d => d.OrdenCompra)
                  .WithMany(o => o.Detalles)
                  .HasForeignKey(d => d.IdOrdenCompra)
                  .OnDelete(DeleteBehavior.Cascade); // Si borras la OC, se borran sus detalles.

            // Relación con Material
            entity.HasOne(d => d.Material)
                  // IMPORTANTE: Si en tu modelo Material NO tienes la lista 'DetallesOrdenCompra',
                  // cambia la siguiente línea a simplemente: .WithMany()
                  .WithMany(m => m.DetallesOrdenCompra)
                  .HasForeignKey(d => d.IdMaterial)
                  .OnDelete(DeleteBehavior.Restrict); // No puedes borrar un material si ya se usó en una compra.

            // 3. NUEVA RELACIÓN: Trazabilidad (El enlace con el Requerimiento)
            entity.HasOne(d => d.DetalleRequerimientoOrigen)
                  .WithMany() // DetalleRequerimiento no necesita tener una lista de OCs
                  .HasForeignKey(d => d.IdDetalleRequerimiento)
                  .OnDelete(DeleteBehavior.Restrict); // Borrar un requerimiento no debería borrar la historia de la compra
        }
    }
}