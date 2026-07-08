using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        CreateMap<EmpresaQueryResult, EmpresaDto>();

        CreateMap<EmpresaSalvarDto, Empresa>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()));

        CreateMap<EmpresaAtualizarDto, EmpresaAtualizarParams>();

        CreateMap<Empresa, EmpresaSalvarParams>();
    }
}
