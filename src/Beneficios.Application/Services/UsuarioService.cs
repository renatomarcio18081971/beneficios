using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;

namespace Beneficios.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;

    public UsuarioService(IUsuarioRepository usuarioRepository, IMapper mapper, ITokenService tokenService)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<Guid> CreateAsync(UsuarioCreateDto dto)
    {
        var id = Guid.NewGuid();
        var senhaCriptografada = Criptografia.Encrypt(dto.Senha);

        await _usuarioRepository.CreateAsync(new UsuarioCreateParams(
            id, dto.Nome, senhaCriptografada, dto.Email, dto.EmpresaId));
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, UsuarioUpdateDto dto, Guid? usuarioAlteracaoId)
    {
        return await _usuarioRepository.UpdateAsync(new UsuarioUpdateParams(
            id, dto.Nome, dto.Email, dto.EmpresaId, usuarioAlteracaoId));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _usuarioRepository.DeleteAsync(id);
    }

    public async Task<UsuarioDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
            return null;

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto[]> GetAllAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return _mapper.Map<UsuarioDto[]>(usuarios);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(loginDto.Email);
        if (usuario == null)
            return null;

        var senhaDescriptografada = Criptografia.Decrypt(usuario.Senha);
        if (senhaDescriptografada != loginDto.Senha)
            return null;

        var token = _tokenService.GenerateToken(usuario.Id, usuario.Email);
        await _usuarioRepository.UpdateTokenAsync(usuario.Id, token);

        return new LoginResponseDto
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email
        };
    }
}
