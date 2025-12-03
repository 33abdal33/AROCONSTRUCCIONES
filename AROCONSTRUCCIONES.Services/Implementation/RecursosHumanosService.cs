using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RecursosHumanosService : IRecursosHumanosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RecursosHumanosService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- TRABAJADORES ---

        public async Task<IEnumerable<TrabajadorDto>> GetAllTrabajadoresAsync()
        {
            var lista = await _unitOfWork.Context.Trabajadores
                .Include(t => t.Cargo)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<IEnumerable<TrabajadorDto>>(lista);
        }

        public async Task<TrabajadorDto?> GetTrabajadorByIdAsync(int id)
        {
            var entity = await _unitOfWork.Context.Trabajadores.FindAsync(id);
            return entity == null ? null : _mapper.Map<TrabajadorDto>(entity);
        }

        public async Task CreateTrabajadorAsync(TrabajadorDto dto)
        {
            var entity = _mapper.Map<Trabajador>(dto);
            await _unitOfWork.Context.Trabajadores.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateTrabajadorAsync(int id, TrabajadorDto dto)
        {
            var existing = await _unitOfWork.Context.Trabajadores.FindAsync(id);
            if (existing != null)
            {
                _mapper.Map(dto, existing);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CargoDto>> GetAllCargosAsync()
        {
            var cargos = await _unitOfWork.Context.Cargos.Where(c => c.Activo).ToListAsync();
            return _mapper.Map<IEnumerable<CargoDto>>(cargos);
        }

        // --- TAREO ---

        public async Task<TareoDto> GetTareoParaEditarAsync(int id, int proyectoId)
        {
            if (id > 0)
            {
                var tareo = await _unitOfWork.Context.Tareos
                    .Include(t => t.Detalles).ThenInclude(d => d.Trabajador)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tareo == null) return null;
                return _mapper.Map<TareoDto>(tareo);
            }

            var dto = new TareoDto
            {
                ProyectoId = proyectoId,
                Fecha = DateTime.Now,
                Detalles = new List<DetalleTareoDto>()
            };

            var trabajadoresActivos = await _unitOfWork.Context.Trabajadores
                .Include(t => t.Cargo)
                .Where(t => t.Estado == true && (t.ProyectoActualId == proyectoId || t.ProyectoActualId == null))
                .ToListAsync();

            foreach (var trab in trabajadoresActivos)
            {
                dto.Detalles.Add(new DetalleTareoDto
                {
                    TrabajadorId = trab.Id,
                    TrabajadorNombre = trab.NombreCompleto,
                    Cargo = trab.Cargo.Nombre,
                    HorasNormales = 8,
                    Asistio = true
                });
            }
            return dto;
        }

        public async Task GuardarTareoAsync(TareoDto dto)
        {
            if (dto.Id == 0)
            {
                var entity = _mapper.Map<Tareo>(dto);
                foreach (var det in entity.Detalles)
                {
                    var trabajador = await _unitOfWork.Context.Trabajadores.Include(t => t.Cargo).FirstOrDefaultAsync(t => t.Id == det.TrabajadorId);
                    if (trabajador != null)
                    {
                        det.JornalBasicoDiario = trabajador.Cargo.JornalBasico;
                        det.CargoDia = trabajador.Cargo.Nombre;
                    }
                }
                await _unitOfWork.Context.Tareos.AddAsync(entity);
            }
            else
            {
                var existing = await _unitOfWork.Context.Tareos
                    .Include(t => t.Detalles)
                    .FirstOrDefaultAsync(t => t.Id == dto.Id);

                if (existing != null)
                {
                    existing.Fecha = dto.Fecha;
                    existing.Responsable = dto.Responsable;
                    foreach (var detDto in dto.Detalles)
                    {
                        var detEntity = existing.Detalles.FirstOrDefault(d => d.TrabajadorId == detDto.TrabajadorId);
                        if (detEntity != null)
                        {
                            detEntity.HorasNormales = detDto.HorasNormales;
                            detEntity.HorasExtras60 = detDto.HorasExtras60;
                            detEntity.HorasExtras100 = detDto.HorasExtras100;
                            detEntity.Asistio = detDto.Asistio;
                        }
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<TareoDto>> GetHistorialTareosAsync(int proyectoId)
        {
            var list = await _unitOfWork.Context.Tareos
                .Where(t => t.ProyectoId == proyectoId)
                .OrderByDescending(t => t.Fecha)
                .Include(t => t.Detalles) // <--- ¡ESTA LÍNEA FALTABA!
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<TareoDto>>(list);
        }

        // --- PLANILLAS ---

        public async Task<PlanillaSemanalDto> GenerarPrePlanillaAsync(int proyectoId, DateTime inicio, DateTime fin)
        {
            var tareos = await _unitOfWork.Context.Tareos
                .Include(t => t.Detalles)
                .ThenInclude(d => d.Trabajador)
                .ThenInclude(tr => tr.Cargo)
                .Where(t => t.ProyectoId == proyectoId
                            && t.Fecha.Date >= inicio.Date
                            && t.Fecha.Date <= fin.Date)
                .ToListAsync();

            if (!tareos.Any()) throw new ApplicationException("No hay tareos registrados en este rango de fechas.");

            var planillaDto = new PlanillaSemanalDto
            {
                ProyectoId = proyectoId, // <-- ID ASEGURADO
                FechaInicio = inicio,
                FechaFin = fin,
                Codigo = $"PL-{inicio:yyyy}WK{System.Globalization.ISOWeek.GetWeekOfYear(inicio)}-{proyectoId}",
                Detalles = new List<DetallePlanillaDto>()
            };

            var detallesPlanos = tareos.SelectMany(t => t.Detalles).Where(d => d.Asistio);
            var gruposTrabajador = detallesPlanos.GroupBy(d => d.TrabajadorId);

            foreach (var grupo in gruposTrabajador)
            {
                var trabajador = grupo.First().Trabajador;
                var cargo = trabajador.Cargo;

                decimal horasNormales = grupo.Sum(d => d.HorasNormales);
                decimal horas60 = grupo.Sum(d => d.HorasExtras60);
                decimal horas100 = grupo.Sum(d => d.HorasExtras100);
                int diasTrabajados = grupo.Count();

                decimal valorHora = cargo.JornalBasico / 8m;
                decimal sueldoBasico = horasNormales * valorHora;
                decimal pagoExtras = (horas60 * valorHora * 1.60m) + (horas100 * valorHora * 2.00m);

                decimal porcentajeBUC = (cargo.Nombre.Contains("Operario") || cargo.Nombre.Contains("Capataz")) ? 0.32m : 0.30m;
                decimal buc = (cargo.JornalBasico * diasTrabajados) * porcentajeBUC;

                decimal movilidad = diasTrabajados * 8.00m;
                decimal totalBruto = sueldoBasico + pagoExtras + buc + movilidad;

                decimal aportePension = (sueldoBasico + pagoExtras + buc) * 0.13m;
                decimal conafovicer = sueldoBasico * 0.02m;
                decimal totalDescuentos = aportePension + conafovicer;
                decimal neto = totalBruto - totalDescuentos;

                planillaDto.Detalles.Add(new DetallePlanillaDto
                {
                    TrabajadorId = trabajador.Id,
                    TrabajadorNombre = trabajador.NombreCompleto,
                    Cargo = cargo.Nombre,
                    SistemaPension = trabajador.SistemaPension,
                    DiasTrabajados = diasTrabajados,
                    HorasNormales = horasNormales,
                    HorasExtras60 = horas60,
                    HorasExtras100 = horas100,
                    JornalBasico = cargo.JornalBasico,
                    SueldoBasico = sueldoBasico,
                    PagoExtras = pagoExtras,
                    BUC = buc,
                    Movilidad = movilidad,
                    TotalBruto = totalBruto,
                    AportePension = aportePension,
                    Conafovicer = conafovicer,
                    TotalDescuentos = totalDescuentos,
                    NetoAPagar = neto
                });
            }
            return planillaDto;
        }

        public async Task GuardarPlanillaAsync(PlanillaSemanalDto dto)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // Generación de código si falta
                if (string.IsNullOrEmpty(dto.Codigo))
                {
                    var semana = System.Globalization.ISOWeek.GetWeekOfYear(dto.FechaInicio);
                    dto.Codigo = $"PL-{dto.FechaInicio.Year}-SEM{semana}-P{dto.ProyectoId}";
                }

                var planilla = new PlanillaSemanal
                {
                    Codigo = dto.Codigo,
                    FechaInicio = dto.FechaInicio,
                    FechaFin = dto.FechaFin,
                    ProyectoId = dto.ProyectoId,
                    Proyecto = null, // <--- ¡SOLUCIÓN CRÍTICA PARA EVITAR DUPLICADOS!
                    Estado = "Aprobado",
                    TotalBruto = dto.TotalBruto,
                    TotalDescuentos = dto.Detalles.Sum(d => d.TotalDescuentos),
                    TotalNetoAPagar = dto.TotalNeto,
                    Detalles = new List<DetallePlanilla>()
                };

                foreach (var detDto in dto.Detalles)
                {
                    planilla.Detalles.Add(new DetallePlanilla
                    {
                        TrabajadorId = detDto.TrabajadorId,
                        DiasTrabajados = detDto.DiasTrabajados,
                        TotalHorasNormales = detDto.HorasNormales,
                        TotalHorasExtras60 = detDto.HorasExtras60,
                        TotalHorasExtras100 = detDto.HorasExtras100,
                        JornalPromedio = detDto.JornalBasico,
                        SueldoBasico = detDto.SueldoBasico,
                        PagoHorasExtras = detDto.PagoExtras,
                        BonificacionBUC = detDto.BUC,
                        Movilidad = detDto.Movilidad,
                        TotalBruto = detDto.TotalBruto,
                        SistemaPension = detDto.SistemaPension,
                        AportePension = detDto.AportePension,
                        Conafovicer = detDto.Conafovicer,
                        TotalDescuentos = detDto.TotalDescuentos,
                        NetoAPagar = detDto.NetoAPagar
                    });
                }

                await _unitOfWork.Context.PlanillasSemanales.AddAsync(planilla);
                await _unitOfWork.SaveChangesAsync();

                // Generar Solicitud de Pago
                int proveedorPlanillaId = 1; // Ajusta esto según tu BD

                var sp = new SolicitudPago
                {
                    Codigo = $"SP-{DateTime.Now.Year}-PL-{planilla.Id}",
                    FechaSolicitud = DateTime.Now,
                    Estado = "Pendiente",
                    Moneda = "NUEVOS SOLES",
                    ProyectoId = planilla.ProyectoId,
                    ProveedorId = proveedorPlanillaId,
                    BeneficiarioNombre = "PLANILLA SEMANAL OBREROS",
                    MontoTotal = planilla.TotalNetoAPagar,
                    MontoNetoAPagar = planilla.TotalNetoAPagar,
                    Concepto = $"PAGO PLANILLA SEMANA {dto.FechaInicio:dd/MM} AL {dto.FechaFin:dd/MM}"
                };

                var spDetalle = new DetalleSolicitudPago
                {
                    SolicitudPago = sp,
                    TipoDocumento = "PL",
                    SerieDocumento = "GEN",
                    NumeroDocumento = planilla.Codigo,
                    FechaEmisionDocumento = DateTime.Now,
                    Monto = planilla.TotalNetoAPagar,
                    Observacion = "Generado desde Módulo RR.HH."
                };

                await _unitOfWork.Context.SolicitudesPagos.AddAsync(sp);
                await _unitOfWork.Context.DetalleSolicitudPagos.AddAsync(spDetalle);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<IEnumerable<PlanillaSemanalDto>> GetHistorialPlanillasAsync(int proyectoId)
        {
            var planillas = await _unitOfWork.Context.PlanillasSemanales
                .Where(p => p.ProyectoId == proyectoId)
                .OrderByDescending(p => p.FechaInicio)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Trabajador) // <--- ¡ESTA LÍNEA FALTABA!
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<PlanillaSemanalDto>>(planillas);
        }
    }
}