using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Mappings;

public class LinhaOnibusProfile : Profile
{
    public LinhaOnibusProfile()
    {
        CreateMap<LinhaOnibusQueryResult, LinhaOnibusDto>();
    }
}