using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class FuncionarioProfile : Profile
{
    public FuncionarioProfile()
    {
        CreateMap<FuncionarioQueryResult, FuncionarioDto>()
            .ForCtorParam("Beneficios", opt => opt.MapFrom(_ => Array.Empty<FuncionarioBeneficioDto>()));
        CreateMap<FuncionarioBeneficioQueryResult, FuncionarioBeneficioDto>();
    }
}
