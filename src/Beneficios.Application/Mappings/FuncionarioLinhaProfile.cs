using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class FuncionarioLinhaProfile : Profile
{
    public FuncionarioLinhaProfile()
    {
        CreateMap<FuncionarioLinhaQueryResult, FuncionarioLinhaDto>();
    }
}
