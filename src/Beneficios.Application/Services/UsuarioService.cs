using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;

namespace Beneficios.Application.Services;

public class UsuarioService : IUsuarioService
{
    private const string MensagemRedefinicaoSenha =
        "Olá, você solicitou redefinição de senha. Informe este código quando solicitado.";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IMapper mapper,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _usuarioRepository = usuarioRepository;
        _mapper = mapper;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<Guid> SalvarAsync(UsuarioSalvarDto dto)
    {
        var usuario = _mapper.Map<Usuario>(dto);
        var salvarParams = _mapper.Map<UsuarioSalvarParams>(usuario);
        await _usuarioRepository.SalvarAsync(salvarParams);
        return usuario.Id;
    }

    public async Task<bool> AtualizarAsync(Guid id, UsuarioAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        var usuario = await _usuarioRepository.ObterUmAsync(id);
        if (usuario is null)
            return false;

        if (TenantDefaultUser.IsDefaultUser(usuario.Email))
            return false;

        return await _usuarioRepository.AtualizarAsync(
            _mapper.Map<UsuarioAtualizarParams>(dto) with
            {
                Id = id,
                UsuarioAlteracaoId = usuarioAlteracaoId
            });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUmAsync(id);
        if (usuario is null)
            return false;

        if (TenantDefaultUser.IsDefaultUser(usuario.Email))
            return false;

        return await _usuarioRepository.DeleteAsync(id);
    }

    public async Task<UsuarioDto?> ObterUmAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterUmAsync(id);
        if (usuario == null)
            return null;

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto[]> ObterTodosAsync()
    {
        var usuarios = await _usuarioRepository.ObterTodosAsync();
        return _mapper.Map<UsuarioDto[]>(usuarios);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string tenantSubdomain)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(loginDto.Email);
        if (usuario == null)
            return null;

        var senhaDescriptografada = Criptografia.Decrypt(usuario.Senha);
        if (senhaDescriptografada != loginDto.Senha)
            return null;

        if (!ValidateTenantAccess(usuario, tenantSubdomain))
            return null;

        var empresaId = usuario.Perfil == UsuarioPerfil.Admin ? null : (Guid?)usuario.EmpresaId;
        var token = _tokenService.GenerateToken(usuario.Id, usuario.Email, usuario.Perfil, empresaId);
        await _usuarioRepository.UpdateTokenAsync(usuario.Id, token);

        var response = _mapper.Map<LoginResponseDto>(usuario);
        response.Token = token;
        if (usuario.Perfil == UsuarioPerfil.Admin)
            response.EmpresaId = null;

        return response;
    }

    public async Task<bool> EmailExisteAsync(string email, string tenantSubdomain)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        return usuario is not null && ValidateTenantAccess(usuario, tenantSubdomain);
    }

    public async Task SolicitarAlteracaoSenhaAsync(SolicitarAlteracaoSenhaDto dto, string tenantSubdomain)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
        if (usuario is null || !ValidateTenantAccess(usuario, tenantSubdomain))
            return;

        var codigo = Random.Shared.Next(100000, 1_000_000).ToString();
        await _usuarioRepository.UpdateCodigoAlterarSenhaAsync(usuario.Id, codigo);
        var mensagem = $"{MensagemRedefinicaoSenha} {codigo}";
        await _emailService.EnviarAsync(dto.Email, "Redefinição de senha", mensagem);
    }

    public async Task<bool> AlterarSenhaAsync(AlterarSenhaDto dto, string tenantSubdomain)
    {
        if (dto.NovaSenha != dto.ConfirmarNovaSenha)
            return false;

        var usuario = await _usuarioRepository.GetByCodigoAlterarSenhaAsync(dto.Codigo);
        if (usuario is null || !ValidateTenantAccess(usuario, tenantSubdomain))
            return false;

        var senhaCriptografada = Criptografia.Encrypt(dto.NovaSenha);
        return await _usuarioRepository.AtualizarSenhaAsync(usuario.Id, senhaCriptografada);
    }

    private static bool ValidateTenantAccess(UsuarioAuthResult usuario, string tenantSubdomain)
    {
        if (string.Equals(tenantSubdomain, "admin", StringComparison.OrdinalIgnoreCase))
            return usuario.Perfil == UsuarioPerfil.Admin;

        return usuario.Perfil == UsuarioPerfil.Empresa
            && string.Equals(usuario.EmpresaDominio, tenantSubdomain, StringComparison.OrdinalIgnoreCase);
    }
}
