using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Repository.Repositories;
using AROCONSTRUCCIONES.Services.Implementation;
using AROCONSTRUCCIONES.Services.Interface;
using AROCONSTRUCCIONES.Services.Mapping_Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Runtime.Loader;


var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Esto crea una política que exige que CUALQUIER usuario logueado
    // sea requerido en CUALQUIER endpoint de la aplicación.
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();

    // Añadimos este filtro globalmente
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<MaterialProfile>();
    config.AddProfile<ProveedorProfile>();
    config.AddProfile<AlmacenProfile>();
    config.AddProfile<MovimientoInventarioProfile>();
    config.AddProfile<InventarioProfile>();
    config.AddProfile<OrdenCompraProfile>();
    config.AddProfile<ProyectoProfile>();
    config.AddProfile<RequerimientoProfile>();
    config.AddProfile<FinanzasProfile>();
    config.AddProfile<RRHHProfile>();
});

builder.Services.AddScoped<IMaterialServices, MaterialService>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IAlmacenService, AlmacenService>();
builder.Services.AddScoped<IAlmacenRepository, AlmacenRepository>();
builder.Services.AddScoped<IMovimientoInventarioServices, MovimientoInventarioService>();
builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
builder.Services.AddScoped<IOrdenCompraServices, OrdenCompraService>();
builder.Services.AddScoped<IRecepcionService, RecepcionService>();
builder.Services.AddScoped<ILogisticaDashboardService, LogisticaDashboardService>();
builder.Services.AddScoped<IProyectoRepository, ProyectoRepository>();
builder.Services.AddScoped<IProyectoService, ProyectoService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IProyectoDashboardService, ProyectoDashboardService>();
builder.Services.AddScoped<IRequerimientoRepository, RequerimientoRepository>();
builder.Services.AddScoped<IRequerimientoService, RequerimientoService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITesoreriaService, TesoreriaService>();
builder.Services.AddScoped<IRecursosHumanosService, RecursosHumanosService>();
builder.Services.AddScoped<IFinanzasService, FinanzasService>();

builder.Services.AddHttpContextAccessor();
//Configure Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Puedes configurar opciones de contraseña aquí si quieres
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Necesario para reseteo de contraseña, etc.

var app = builder.Build();
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await DbInitializer.InitializeAsync(services);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Un error ocurrió al sembrar la BD.");
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();