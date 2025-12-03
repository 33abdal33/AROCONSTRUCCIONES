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
    public class FinanzasProfile : Profile
    {
        public FinanzasProfile()
        {
            CreateMap<CuentaBancaria, CuentaBancariaDto>().ReverseMap();
            // Dentro del constructor, agrega este nuevo mapa:
            CreateMap<MovimientoBancario, MovimientoBancarioDto>()
                .ForMember(dest => dest.BancoNombre, opt => opt.MapFrom(src => src.CuentaBancaria.BancoNombre))
                .ForMember(dest => dest.NumeroCuenta, opt => opt.MapFrom(src => src.CuentaBancaria.NumeroCuenta))
                .ForMember(dest => dest.Moneda, opt => opt.MapFrom(src => src.CuentaBancaria.Moneda));
        }
    }
}