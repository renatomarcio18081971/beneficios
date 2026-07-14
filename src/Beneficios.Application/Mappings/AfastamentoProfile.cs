using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class AfastamentoProfile : Profile
{
    public AfastamentoProfile()
    {
        CreateMap<FuncionarioAfastamentoQueryResult, AfastamentoDto>()
            .ForMember(d => d.Tipo, o => o.MapFrom(s => TipoAfastamentoCatalog.DeBanco(s.Tipo)));
    }
}
