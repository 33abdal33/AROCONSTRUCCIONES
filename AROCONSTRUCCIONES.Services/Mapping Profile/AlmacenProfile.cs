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
    public class AlmacenProfile : Profile
    {
        public AlmacenProfile()
        {
            CreateMap<Almacen, AlmacenDto>().ReverseMap();
        }
    }
}
