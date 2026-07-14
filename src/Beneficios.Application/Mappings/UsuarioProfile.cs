using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;

namespace Beneficios.Application.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<UsuarioQueryResult, UsuarioDto>();

        CreateMap<UsuarioSalvarDto, Usuario>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Senha, opt => opt.MapFrom(src => Criptografia.Encrypt(src.Senha)));

        CreateMap<UsuarioAtualizarDto, UsuarioAtualizarParams>();

        CreateMap<Usuario, UsuarioSalvarParams>();

        CreateMap<UsuarioAuthResult, LoginResponseDto>()
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EmpresaId, opt => opt.MapFrom(src =>
                src.Perfil == UsuarioPerfil.Admin ? (Guid?)null : src.EmpresaId))
            .ForMember(dest => dest.PerfilAcessoId, opt => opt.MapFrom(src => src.PerfilId))
            .ForMember(dest => dest.PerfilAcessoNome, opt => opt.MapFrom(src => src.PerfilAcessoNome ?? string.Empty))
            .ForMember(dest => dest.Permissoes, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore());
    }
}
