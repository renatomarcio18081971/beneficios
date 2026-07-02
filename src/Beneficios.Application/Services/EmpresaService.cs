using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;

namespace Beneficios.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IMapper _mapper;

    public EmpresaService(IEmpresaRepository empresaRepository, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(EmpresaCreateDto dto)
    {
        var id = Guid.NewGuid();
        var nomeBancoCriptografado = Criptografia.Encrypt(dto.NomeBanco);
        var usuarioBancoCriptografado = Criptografia.Encrypt(dto.UsuarioBanco);
        var senhaBancoCriptografada = Criptografia.Encrypt(dto.SenhaBanco);

        await _empresaRepository.CreateAsync(new EmpresaCreateParams(
            id, dto.RazaoSocial, dto.Dominio, nomeBancoCriptografado, usuarioBancoCriptografado, senhaBancoCriptografada));
        return id;
    }

    public async Task<bool> UpdateAsync(Guid id, EmpresaUpdateDto dto, Guid? usuarioAlteracaoId)
    {
        var nomeBancoCriptografado = Criptografia.Encrypt(dto.NomeBanco);
        var usuarioBancoCriptografado = Criptografia.Encrypt(dto.UsuarioBanco);
        var senhaBancoCriptografada = Criptografia.Encrypt(dto.SenhaBanco);

        return await _empresaRepository.UpdateAsync(new EmpresaUpdateParams(
            id, dto.RazaoSocial, dto.Dominio, nomeBancoCriptografado, usuarioBancoCriptografado, senhaBancoCriptografada, usuarioAlteracaoId));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _empresaRepository.DeleteAsync(id);
    }

    public async Task<EmpresaDto?> GetByIdAsync(Guid id)
    {
        var empresa = await _empresaRepository.GetByIdAsync(id);
        if (empresa == null)
            return null;

        return _mapper.Map<EmpresaDto>(empresa);
    }

    public async Task<EmpresaDto[]> GetAllAsync()
    {
        var empresas = await _empresaRepository.GetAllAsync();
        return _mapper.Map<EmpresaDto[]>(empresas);
    }
}
