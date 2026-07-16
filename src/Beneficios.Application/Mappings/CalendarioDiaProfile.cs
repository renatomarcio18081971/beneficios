using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class CalendarioDiaProfile : Profile
{
    public CalendarioDiaProfile()
    {
        CreateMap<CalendarioDiaQueryResult, CalendarioDiaDto>();
    }
}
