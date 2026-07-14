using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class FuncionarioProfile : Profile
{
    public FuncionarioProfile()
    {
        CreateMap<FuncionarioQueryResult, FuncionarioDto>()
            .ForMember(d => d.Beneficios, o => o.Ignore());
        CreateMap<FuncionarioBeneficioQueryResult, FuncionarioBeneficioDto>();
    }
}
