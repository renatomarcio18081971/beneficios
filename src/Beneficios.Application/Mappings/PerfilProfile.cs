using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class PerfilProfile : Profile
{
    public PerfilProfile()
    {
        CreateMap<PerfilPermissaoParams, PermissaoMenuDto>();
        CreateMap<PermissaoMenuDto, PerfilPermissaoParams>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.PerfilId, opt => opt.Ignore());

        CreateMap<PerfilQueryResult, PerfilDto>();
        CreateMap<PerfilFiltroDto, PerfilFiltroParams>();
    }
}
