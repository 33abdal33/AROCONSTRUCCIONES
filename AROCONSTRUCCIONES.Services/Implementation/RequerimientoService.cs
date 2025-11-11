using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RequerimientoService : IRequerimientoService
    {
        private readonly IRequerimientoRepository _requerimientoRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public RequerimientoService(
            IRequerimientoRepository requerimientoRepo,
            ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _requerimientoRepo = requerimientoRepo;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RequerimientoListDto>> GetRequerimientosPorProyectoAsync(int proyectoId)
        {
            var requerimientos = await _requerimientoRepo.GetRequerimientosPorProyectoAsync(proyectoId);
            return _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);
        }

        public async Task CreateAsync(RequerimientoCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new ApplicationException("El requerimiento debe tener al menos un material.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var requerimiento = _mapper.Map<Requerimiento>(dto);
                requerimiento.Estado = "Pendiente"; // Estado inicial

                // EF Core es lo suficientemente inteligente para guardar el "Padre" (Requerimiento)
                // y todos los "Hijos" (Detalles) en una sola transacción.
                await _requerimientoRepo.AddAsync(requerimiento);

                await _dbContext.SaveChangesAsync(); // Guardamos el Padre y los Hijos
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanza el error
            }
        }
    }
}
