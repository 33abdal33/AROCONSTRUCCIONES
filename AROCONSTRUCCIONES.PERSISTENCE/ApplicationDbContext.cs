using AROCONSTRUCCIONES.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<DetalleOrdenCompra> DetalleOrdenesCompra { get; set; }
        public DbSet<DetalleRequerimiento> DetalleRequerimientos { get; set; }
        public DbSet<Requerimiento> Requerimientos { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<ProveedorMaterial> ProveedorMateriales { get; set; }
        public DbSet<DetalleSolicitudPago> DetalleSolicitudPagos { get; set; }
        public DbSet<SolicitudPago> SolicitudesPagos { get; set; }


        //FLUENT API

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<ProveedorMaterial>()
               .HasKey(pm => new { pm.ProveedorId, pm.MaterialId });

            // Configura la relación Proveedor -> ProveedorMaterial
            modelBuilder.Entity<ProveedorMaterial>()
                .HasOne(pm => pm.Proveedor)
                .WithMany(p => p.ProveedorMateriales)
                .HasForeignKey(pm => pm.ProveedorId);

            // Configura la relación Material -> ProveedorMaterial
            modelBuilder.Entity<ProveedorMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProveedorMateriales)
                .HasForeignKey(pm => pm.MaterialId);
        }
    }
}
