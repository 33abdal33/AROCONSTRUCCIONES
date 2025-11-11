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
    public class MaterialConfiguration:IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> entity)
        {
            entity.ToTable("Material");

            entity.Property(m => m.Codigo)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(m => m.Codigo)
                  .IsUnique();

            entity.Property(m => m.Nombre)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(m => m.Descripcion)
                  .HasMaxLength(250);

            entity.Property(m => m.UnidadMedida)
                  .HasMaxLength(50);

            entity.Property(m => m.Categoria)
                  .HasMaxLength(100);

            entity.Property(m => m.PrecioUnitario)
                  .HasPrecision(10, 2);


        }
    }
}
