using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;

namespace Beneficios.Application.Mappings;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        CreateMap<EmpresaQueryResult, EmpresaDto>();

        CreateMap<EmpresaSalvarDto, Empresa>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.NomeBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.NomeBanco)))
            .ForMember(dest => dest.UsuarioBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.UsuarioBanco)))
            .ForMember(dest => dest.SenhaBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.SenhaBanco)));

        CreateMap<EmpresaAtualizarDto, EmpresaAtualizarParams>()
            .ForMember(dest => dest.NomeBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.NomeBanco)))
            .ForMember(dest => dest.UsuarioBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.UsuarioBanco)))
            .ForMember(dest => dest.SenhaBanco, opt => opt.MapFrom(src => Criptografia.Encrypt(src.SenhaBanco)));

        CreateMap<Empresa, EmpresaSalvarParams>();
    }
}
