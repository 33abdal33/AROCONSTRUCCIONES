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
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> entity)
        {
            entity.ToTable("Proveedor");

            entity.Property(p => p.RazonSocial)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(p => p.RUC)
                  .HasMaxLength(11)
                  .IsRequired();

            entity.HasIndex(p => p.RUC)
                  .IsUnique();

            entity.Property(p => p.Email)
                  .HasMaxLength(100);

            entity.Property(p => p.Telefono)
                  .HasMaxLength(15);

            entity.Property(p => p.Direccion)
                  .HasMaxLength(200);

            entity.Property(p => p.Especialidad)
           .HasMaxLength(100);

            entity.Property(p => p.NombreContacto)
                .HasMaxLength(100);

            entity.HasMany(p => p.OrdenesCompra)
                  .WithOne(o => o.Proveedor)
                  .HasForeignKey(o => o.IdProveedor);


        }
    }
}
