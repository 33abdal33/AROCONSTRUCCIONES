using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class FinanzasService : IFinanzasService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinanzasService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CuentaBancariaDto>> GetAllCuentasAsync()
        {
            var cuentas = await _unitOfWork.Context.CuentasBancarias
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<CuentaBancariaDto>>(cuentas);
        }

        public async Task<CuentaBancariaDto?> GetCuentaByIdAsync(int id)
        {
            var entity = await _unitOfWork.Context.CuentasBancarias.FindAsync(id);
            return entity == null ? null : _mapper.Map<CuentaBancariaDto>(entity);
        }

        public async Task CreateCuentaAsync(CuentaBancariaDto dto)
        {
            var entity = _mapper.Map<CuentaBancaria>(dto);

            // Al crear, el saldo actual es igual al inicial
            entity.SaldoActual = entity.SaldoInicial;

            await _unitOfWork.Context.CuentasBancarias.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCuentaAsync(int id, CuentaBancariaDto dto)
        {
            var entity = await _unitOfWork.Context.CuentasBancarias.FindAsync(id);
            if (entity != null)
            {
                // Solo permitimos editar nombre, cuenta, cci, titular. 
                // NO EL SALDO (eso se mueve por transacciones)
                entity.BancoNombre = dto.BancoNombre;
                entity.NumeroCuenta = dto.NumeroCuenta;
                entity.CCI = dto.CCI;
                entity.Titular = dto.Titular;
                entity.Activo = dto.Activo;

                await _unitOfWork.SaveChangesAsync();
            }
        }
        // Agrega este método a la clase FinanzasService:

        public async Task<IEnumerable<MovimientoBancarioDto>> GetMovimientosAsync(int? cuentaId, DateTime? inicio, DateTime? fin)
        {
            var query = _unitOfWork.Context.MovimientosBancarios
                .Include(m => m.CuentaBancaria)
                .AsNoTracking()
                .AsQueryable();

            // Filtros
            if (cuentaId.HasValue && cuentaId > 0)
            {
                query = query.Where(m => m.CuentaBancariaId == cuentaId);
            }

            if (inicio.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento.Date >= inicio.Value.Date);
            }

            if (fin.HasValue)
            {
                query = query.Where(m => m.FechaMovimiento.Date <= fin.Value.Date);
            }

            // Ordenar: Lo más reciente primero
            var movimientos = await query.OrderByDescending(m => m.FechaMovimiento).ToListAsync();

            return _mapper.Map<IEnumerable<MovimientoBancarioDto>>(movimientos);
        }
    }
}