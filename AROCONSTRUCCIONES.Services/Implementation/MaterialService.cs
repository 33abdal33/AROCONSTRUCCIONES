using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class MaterialService : IMaterialServices
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly ApplicationDbContext _dbContext; // AUN LO NECESITAMOS
        private readonly IMapper _mapper;

        public MaterialService(IMaterialRepository materialRepository, ApplicationDbContext dbContext, IMapper mapper)
        {
            _materialRepository = materialRepository;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllAsync()
        {
            var materiales = await _materialRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }

        public async Task<MaterialDto?> GetByIdAsync(int id)
        {
            var materialEntity = await _materialRepository.GetByIdAsync(id);
            if (materialEntity is null) return null;
            return _mapper.Map<MaterialDto>(materialEntity);
        }

        // --- REFACTORIZADO ---
        // Ya no devuelve la entidad y NO guarda cambios.
        public async Task CreateAsync(MaterialDto dto)
        {
            var entity = _mapper.Map<Material>(dto);
            // El mapeo debería encargarse de todo, pero por si acaso:
            entity.Estado = true; // Asegurarse de que esté activo al crear

            await _materialRepository.AddAsync(entity);
            // LA LÍNEA DE SAVESCHANGES FUE ELIMINADA
        }

        // --- REFACTORIZADO ---
        // Ya no devuelve DTO y NO guarda cambios.
        public async Task<Material> UpdateAsync(int id, MaterialDto dto)
        {
            var existing = await _materialRepository.GetByIdAsync(id);
            if (existing is null) return null;

            _mapper.Map(dto, existing); // Mapea los cambios al objeto rastreado
            await _materialRepository.UpdateAsync(existing); // Marca la entidad como modificada
            // LA LÍNEA DE SAVESCHANGES FUE ELIMINADA
            return existing; // Devuelve la entidad para que el controlador la tenga
        }

        // --- REFACTORIZADO ---
        // NO guarda cambios.
        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _materialRepository.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Estado = false;
            await _materialRepository.UpdateAsync(existing);
            // LA LÍNEA DE SAVESCHANGES FUE ELIMINADA
            return true;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllActiveAsync()
        {
            var materiales = await _materialRepository.FindAsync(m => m.Estado, m => m.Nombre);
            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }

        // (Tus métodos GetMaterialCategoriesAsync y GetMaterialUnitsAsync están perfectos, no se tocan)
        public Task<List<string>> GetMaterialCategoriesAsync()
        {
            return Task.FromResult(new List<string> {
        // Obra Gruesa
         "Agregados",             // Arena, piedra, hormigón
         "Acero y Alambres",      // Fierro corrugado, alambre
         "Cementos y Aditivos",   // Cemento, cal, impermeabilizantes
         "Albañilería",           // Ladrillos, bloques de concreto
         "Maderas (Estructura)",  // Vigas, tablas para encofrado

        // Obra Fina (Acabados)
         "Tabiquería Seca",       // Drywall, perfiles metálicos
         "Pisos y Revestimientos",// Cerámicos, porcelanatos, vinílicos
         "Pinturas y Accesorios", // Látex, esmalte, brochas
         "Carpintería",           // Puertas, ventanas, marcos
         "Vidrios y Espejos",

        // Instalaciones
         "Inst. Eléctricas",      // Cables, tubería conduit, tomacorrientes
         "Inst. Sanitarias",      // Tuberías PVC, grifería, sanitarios
         "Inst. Gasfitería",

        // Otros
         "Ferretería General",    // Clavos, tornillos, pernos
         "Seguridad y EPP",       // Cascos, guantes, arneses
         "Herramientas y Equipos",// Lampas, picos, taladros (si se controlan por stock)
         "Oficina y Limpieza"     // Útiles de oficina para la obra
             });
        }

        public Task<List<string>> GetMaterialUnitsAsync()
        {
            return Task.FromResult(new List<string> {
        // Conteo
         "und",   // Unidad
         "pza",   // Pieza
        "kit",   // Kit
         "jgo",   // Juego (ej. de grifería)

        // Empaques
         "bol",   // Bolsa (tu "bolsa" original)
         "cja",   // Caja
         "rll",   // Rollo
        "paq",   // Paquete
        "ciento",// Ciento (ej. ladrillos)
        "millar",// Millar (ej. tornillos)
         "pln",   // Plancha (ej. Drywall)

        // Peso
         "kg",    // Kilogramo
         "ton",   // Tonelada

        // Volumen
         "m3",    // Metro Cúbico (para agregados)
         "L",     // Litro
         "gal",   // Galón (tu "gal" original)

        // Longitud
         "m",     // Metro (tu "m" original)
         "ml",    // Metro Lineal (para tuberías)
        
        // Área
        "m2"     // Metro Cuadrado (para pisos)
              });
        }
        // --- ¡AÑADE ESTE MÉTODO COMPLETO! ---
        public async Task<IEnumerable<MaterialDto>> GetMaterialesPorProveedorAsync(int proveedorId)
        {
            // 1. Vamos a la tabla de unión
            var idsMateriales = await _dbContext.ProveedorMateriales
                .Where(pm => pm.ProveedorId == proveedorId) // Filtramos por el proveedor
                .Select(pm => pm.MaterialId) // Obtenemos solo los IDs de los materiales
                .ToListAsync();

            if (!idsMateriales.Any())
            {
                return new List<MaterialDto>(); // Devuelve lista vacía si no vende nada
            }

            // 2. Buscamos esos materiales en la tabla Material
            var materiales = await _materialRepository.FindAsync(
                m => idsMateriales.Contains(m.Id) && m.Estado == true, // Filtramos por los IDs Y que estén activos
                m => m.Nombre // Ordenamos por nombre
            );

            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }
    }
}