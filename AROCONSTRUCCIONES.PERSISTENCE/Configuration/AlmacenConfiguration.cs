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
    public class AlmacenConfiguration: IEntityTypeConfiguration<Almacen>
    {
        public void Configure(EntityTypeBuilder<Almacen> entity)
        {
            entity.ToTable("Almacen");

            entity.Property(a => a.Nombre)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(a => a.Ubicacion)
                  .HasMaxLength(200);

            entity.Property(a => a.Responsable)
                  .HasMaxLength(100);

            entity.Property(a => a.Estado)
                  .HasDefaultValue(true);

        }
    }
}
