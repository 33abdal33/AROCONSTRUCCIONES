using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class ProyectoProfile : Profile
    {
        public ProyectoProfile()
        {
            // Mapeo simple de 1 a 1.
            // .ReverseMap() nos permite mapear Dto -> Entidad
            CreateMap<Proyecto, ProyectoDto>().ReverseMap();
        }
    }
}
