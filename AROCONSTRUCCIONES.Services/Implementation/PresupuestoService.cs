using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using ClosedXML.Excel; // ¡LIBRERÍA NUEVA!
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class PresupuestoService : IPresupuestoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PresupuestoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PartidaDto>> GetPartidasPorProyectoAsync(int proyectoId)
        {
            var partidas = await _unitOfWork.Context.Partidas
                .Where(p => p.ProyectoId == proyectoId)
                .OrderBy(p => p.Item) // Ordenar por Item (01, 01.01...) es clave
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<PartidaDto>>(partidas);
        }

        public async Task ImportarPresupuestoDesdeExcelAsync(Stream fileStream, int proyectoId)
        {
            // 1. Limpiar presupuesto anterior (si existe) para evitar duplicados
            await EliminarPresupuestoAsync(proyectoId);

            var listaPartidas = new List<Partida>();

            // 2. Leer Excel con ClosedXML
            using (var workbook = new XLWorkbook(fileStream))
            {
                var worksheet = workbook.Worksheet(1); // Primera hoja
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Saltamos cabecera

                foreach (var row in rows)
                {
                    // Asumimos columnas: A=Item, B=Descripción, C=Unidad, D=Metrado, E=Precio
                    var item = row.Cell(1).GetValue<string>().Trim();
                    var descripcion = row.Cell(2).GetValue<string>().Trim();
                    var unidad = row.Cell(3).GetValue<string>().Trim();

                    // Lectura segura de números
                    decimal metrado = 0;
                    decimal precio = 0;

                    if (!row.Cell(4).IsEmpty())
                        decimal.TryParse(row.Cell(4).GetString(), out metrado);

                    if (!row.Cell(5).IsEmpty())
                        decimal.TryParse(row.Cell(5).GetString(), out precio);

                    // Lógica: Si no tiene unidad ni precio, es un TÍTULO
                    bool esTitulo = string.IsNullOrEmpty(unidad) && precio == 0;

                    var partida = new Partida
                    {
                        ProyectoId = proyectoId,
                        Item = item,
                        Descripcion = descripcion,
                        Unidad = unidad,
                        Metrado = metrado,
                        PrecioUnitario = precio,
                        Parcial = metrado * precio,
                        EsTitulo = esTitulo
                    };

                    listaPartidas.Add(partida);
                }
            }

            // 3. Guardar Masivamente
            if (listaPartidas.Any())
            {
                await _unitOfWork.Context.Partidas.AddRangeAsync(listaPartidas);

                // Actualizar el presupuesto total del Proyecto (Cabecera)
                var totalPresupuesto = listaPartidas.Where(p => !p.EsTitulo).Sum(p => p.Parcial);
                var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(proyectoId);
                if (proyecto != null)
                {
                    proyecto.Presupuesto = totalPresupuesto; // Actualizamos la meta global
                    await _unitOfWork.Proyectos.UpdateAsync(proyecto);
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task EliminarPresupuestoAsync(int proyectoId)
        {
            var partidas = await _unitOfWork.Context.Partidas.Where(p => p.ProyectoId == proyectoId).ToListAsync();
            _unitOfWork.Context.Partidas.RemoveRange(partidas);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}