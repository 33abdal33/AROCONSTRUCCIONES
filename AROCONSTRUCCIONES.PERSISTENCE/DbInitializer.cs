using AROCONSTRUCCIONES.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Persistence
{
    public static class DbInitializer
    {
        // Este es el método que llamaremos desde Program.cs
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                logger.LogInformation("Iniciando la siembra de datos de Identity...");

                // --- 1. CREAR ROLES ---
                string[] roleNames = { "Administrador", "Usuario", "Almacenero" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        logger.LogInformation($"Rol '{roleName}' creado.");
                    }
                }

                // --- 2. CREAR USUARIO ADMINISTRADOR ---
                string adminEmail = "admin@hotmail.com";
                string adminPassword = "password123"; // (Puedes cambiar esto)

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    ApplicationUser adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        NombreCompleto = "Admin General",
                        EmailConfirmed = true // Confirma el email automáticamente
                    };

                    IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Usuario Administrador '{adminEmail}' creado.");
                        // Asignar el rol "Administrador"
                        await userManager.AddToRoleAsync(adminUser, "Administrador");
                        logger.LogInformation($"Rol 'Administrador' asignado a '{adminEmail}'.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            logger.LogError($"Error creando admin: {error.Description}");
                        }
                    }
                }

                logger.LogInformation("Siembra de datos de Identity completada.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error durante la siembra de datos de Identity.");
            }

            // --- 3. SEMBRAR CARGOS DE CONSTRUCCIÓN CIVIL ---
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (!context.Cargos.Any())
            {
                context.Cargos.AddRange(
                    // Valores referenciales de la Federación de Construcción Civil
                    new Cargo { Nombre = "Operario", JornalBasico = 87.30m, BUC = 32.00m },
                    new Cargo { Nombre = "Oficial", JornalBasico = 68.50m, BUC = 30.00m },
                    new Cargo { Nombre = "Peón", JornalBasico = 61.65m, BUC = 30.00m },
                    new Cargo { Nombre = "Capataz", JornalBasico = 100.00m, BUC = 32.00m }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Cargos de construcción civil sembrados.");
            }
        }


    }
}
