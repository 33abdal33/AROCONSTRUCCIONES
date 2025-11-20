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
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        public MaterialService(IUnitOfWork unitOfWork, IMapper mapper) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllAsync()
        {
            var materiales = await _unitOfWork.Materiales.GetAllAsync();
            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }

        public async Task<MaterialDto?> GetByIdAsync(int id)
        {
            var materialEntity = await _unitOfWork.Materiales.GetByIdAsync(id);
            if (materialEntity is null) return null;
            return _mapper.Map<MaterialDto>(materialEntity);
        }

        // --- REFACTORIZADO ---
        // Ya no devuelve la entidad y NO guarda cambios.
        public async Task CreateAsync(MaterialDto dto)
        {
            var entity = _mapper.Map<Material>(dto);
            entity.Estado = true;
            await _unitOfWork.Materiales.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // <-- GUARDA
        }

        public async Task<Material> UpdateAsync(int id, MaterialDto dto)
        {
            var existing = await _unitOfWork.Materiales.GetByIdAsync(id);
            if (existing is null) return null;

            _mapper.Map(dto, existing);
            await _unitOfWork.Materiales.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(); // <-- GUARDA
            return existing;
        }
        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _unitOfWork.Materiales.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Estado = false;
            await _unitOfWork.Materiales.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(); // <-- GUARDA
            return true;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllActiveAsync()
        {
            var materiales = await _unitOfWork.Materiales.FindAsync(m => m.Estado, m => m.Nombre);
            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }
        // (Tus métodos GetMaterialCategoriesAsync y GetMaterialUnitsAsync están perfectos, no se tocan)
        public Task<List<string>> GetMaterialCategoriesAsync()
        {
            return Task.FromResult(new List<string> {

        // -------------------------
        // OBRA GRUESA
        // -------------------------
        "Agregados",                  // Arena, piedra, hormigón
        "Acero y Alambres",           // Acero corrugado, mallas, alambres
        "Cementos y Aditivos",        // Cemento, cal, sika, plastificantes
        "Albañilería",                // Ladrillos, bloques
        "Maderas (Estructura)",       // Para encofrado y estructuras
        "Encofrados y Andamios",      // Tablones, puntales, andamios

        // -------------------------
        // OBRA FINA (ACABADOS)
        // -------------------------
        "Tabiquería Seca (Drywall)",  // Placas, perfiles, tornillos
        "Pisos y Revestimientos",     // Cerámicos, porcelanatos, vinílicos
        "Pinturas y Accesorios",      // Látex, esmalte, selladores
        "Carpintería Madera",         // Puertas, marcos, zócalos
        "Carpintería Metálica",       // Rejas, barandas, puertas metálicas
        "Vidrios y Espejos",
        "Yeso y Masillas",            // Yeso, pasta muro

        // -------------------------
        // INSTALACIONES
        // -------------------------
        "Inst. Eléctricas",           // Cables, interruptores
        "Inst. Sanitarias",           // PVC, conexiones, válvulas
        "Inst. Gasfitería",           // Accesorios, grifería
        "Inst. Mecánicas y HVAC",     // Aire acondicionado, ductería
        "Inst. Especiales",           // Datos, CCTV, alarmas

        // -------------------------
        // IMPERMEABILIZACIÓN Y ACÚSTICA
        // -------------------------
        "Impermeabilizantes",         // Mantas, sika, membranas
        "Aislantes Térmicos/Acústicos", // Lana de vidrio, poliestireno

        // -------------------------
        // FERRETERÍA Y INSUMOS
        // -------------------------
        "Ferretería General",         // Clavos, tornillos, discos
        "Herramientas Manuales",      // Martillos, llaves, planos
        "Herramientas Eléctricas",    // Taladros, amoladoras
        "Accesorios de Herramientas", // Brocas, discos, lijas

        // -------------------------
        // SEGURIDAD
        // -------------------------
        "Seguridad y EPP",            // Cascos, guantes, arneses
        "Señalización y Control",     // Cinta, conos, mallas

        // -------------------------
        // MAQUINARIA Y EQUIPOS
        // -------------------------
        "Alquiler de Equipos",        // Compactadoras, mezcladoras
        "Consumibles de Maquinaria",  // Aceites, filtros

        // -------------------------
        // OTROS
        // -------------------------
        "Oficina y Limpieza",         // Papelería, detergentes
        "Misceláneos",                // Todo lo no categorizado

        // OPCIONAL (es real en obras medianas-grandes)
        "Parrillas y Prefabricados",  // Viguetas, aligerantes
        "Geotextiles y Geomallas",    // Obras viales
        "Paisajismo",                 // Jardinería, tierra de chacra
        "Urbanismo y Vías"            // Sardineles, adoquines
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
            // Usamos el Context del UoW para la tabla de unión
            var idsMateriales = await _unitOfWork.Context.ProveedorMateriales
                .Where(pm => pm.ProveedorId == proveedorId)
                .Select(pm => pm.MaterialId)
                .ToListAsync();

            if (!idsMateriales.Any())
            {
                return new List<MaterialDto>();
            }

            var materiales = await _unitOfWork.Materiales.FindAsync(
                m => idsMateriales.Contains(m.Id) && m.Estado == true,
                m => m.Nombre
            );

            return _mapper.Map<IEnumerable<MaterialDto>>(materiales);
        }
    }
}