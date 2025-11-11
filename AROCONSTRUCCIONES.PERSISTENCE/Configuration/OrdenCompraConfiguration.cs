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
    public class OrdenCompraConfiguration:IEntityTypeConfiguration<OrdenCompra>
    {
        public void Configure(EntityTypeBuilder<OrdenCompra> entity)
        {
            entity.ToTable("OrdenCompra");

            entity.Property(o => o.Codigo)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(o => o.Codigo)
                  .IsUnique();

            entity.Property(o => o.Estado)
                  .HasMaxLength(50)
                  .HasDefaultValue("Pendiente");

            entity.Property(o => o.Total)
                  .HasPrecision(10, 2)
                  .HasDefaultValue(0);

            entity.Property(o => o.Observaciones)
                  .HasMaxLength(250);

            entity.HasOne(o => o.Proveedor)
                  .WithMany(p => p.OrdenesCompra)
                  .HasForeignKey(o => o.IdProveedor);

        }
    }
}
