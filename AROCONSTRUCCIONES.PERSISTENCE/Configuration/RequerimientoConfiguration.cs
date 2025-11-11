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
    public class RequerimientoConfiguration : IEntityTypeConfiguration<Requerimiento>
    {
        public void Configure(EntityTypeBuilder<Requerimiento> entity)
        {
            entity.ToTable("Requerimiento");

            entity.Property(r => r.Codigo)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(r => r.Codigo)
                  .IsUnique();

            entity.Property(r => r.Solicitante)
                  .HasMaxLength(150);

            entity.Property(r => r.Area)
                  .HasMaxLength(100);

            entity.Property(r => r.Estado)
                  .HasMaxLength(50)
                  .HasDefaultValue("Pendiente");

        }
    }
}
