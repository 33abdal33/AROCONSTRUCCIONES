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
    public class DetalleRequerimientoConfiguration:IEntityTypeConfiguration<DetalleRequerimiento>
    {
        public void Configure(EntityTypeBuilder<DetalleRequerimiento> entity)
        {
            entity.ToTable("DetalleRequerimiento");

            entity.Property(d => d.CantidadSolicitada)
                  .HasPrecision(10, 2);

            entity.Property(d => d.CantidadAtendida)
                  .HasPrecision(10, 2)
                  .HasDefaultValue(0);

            entity.HasOne(d => d.Requerimiento)
                  .WithMany(r => r.Detalles)
                  .HasForeignKey(d => d.IdRequerimiento);

            entity.HasOne(d => d.Material)
                  .WithMany(m => m.DetallesRequerimiento)
                  .HasForeignKey(d => d.IdMaterial);

        }
    }
}
