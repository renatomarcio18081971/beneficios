using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.EmpresaNome, opt => opt.MapFrom(src => src.Empresa != null ? src.Empresa.RazaoSocial : string.Empty));

        CreateMap<UsuarioCreateDto, Usuario>();
        CreateMap<UsuarioUpdateDto, Usuario>();

        CreateMap<UsuarioQueryResult, UsuarioDto>();

        CreateMap<Empresa, EmpresaDto>();
        CreateMap<EmpresaCreateDto, Empresa>();
        CreateMap<EmpresaUpdateDto, Empresa>();

        CreateMap<EmpresaQueryResult, EmpresaDto>();
    }
}
