using AROCONSTRUCCIONES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Persistence.Configuration
{
    public class DetalleOrdenCompraConfiguration:IEntityTypeConfiguration<DetalleOrdenCompra>
    {
        public void Configure(EntityTypeBuilder<DetalleOrdenCompra> entity)
        {
            entity.ToTable("DetalleOrdenCompra");

            entity.Property(d => d.Cantidad)
                  .HasPrecision(10, 2);

            entity.Property(d => d.PrecioUnitario)
                  .HasPrecision(10, 2);

            entity.Ignore(d => d.Subtotal); // calculado en la lógica, no en BD

            entity.HasOne(d => d.OrdenCompra)
                  .WithMany(o => o.Detalles)
                  .HasForeignKey(d => d.IdOrdenCompra);

            entity.HasOne(d => d.Material)
                  .WithMany(m => m.DetallesOrdenCompra)
                  .HasForeignKey(d => d.IdMaterial);

        }
    }
}
