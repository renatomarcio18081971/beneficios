using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Entities;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;

namespace Beneficios.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ITenantProvisioner _tenantProvisioner;
    private readonly IMapper _mapper;

    public EmpresaService(
        IEmpresaRepository empresaRepository,
        ITenantProvisioner tenantProvisioner,
        IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _tenantProvisioner = tenantProvisioner;
        _mapper = mapper;
    }

    public async Task<Guid> SalvarAsync(EmpresaSalvarDto dto)
    {
        var empresa = _mapper.Map<Empresa>(dto);
        var salvarParams = _mapper.Map<EmpresaSalvarParams>(empresa);

        await _empresaRepository.SalvarAsync(salvarParams);

        try
        {
            await _tenantProvisioner.ProvisionarAsync(salvarParams.RazaoSocial, salvarParams.Id);
        }
        catch
        {
            await _empresaRepository.DeleteAsync(salvarParams.Id);
            throw;
        }

        return empresa.Id;
    }

    public async Task<bool> AtualizarAsync(Guid id, EmpresaAtualizarDto dto, Guid? usuarioAlteracaoId)
    {
        return await _empresaRepository.AtualizarAsync(
            _mapper.Map<EmpresaAtualizarParams>(dto) with
            {
                Id = id,
                UsuarioAlteracaoId = usuarioAlteracaoId
            });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _empresaRepository.DeleteAsync(id);
    }

    public async Task<EmpresaDto?> ObterUmAsync(Guid id)
    {
        var empresa = await _empresaRepository.ObterUmAsync(id);
        if (empresa == null)
            return null;

        return _mapper.Map<EmpresaDto>(empresa);
    }

    public async Task<EmpresaDto[]> ObterTodosAsync()
    {
        var empresas = await _empresaRepository.ObterTodosAsync();
        return _mapper.Map<EmpresaDto[]>(empresas);
    }

    public async Task<EmpresaDto[]> FiltrarAsync(EmpresaFiltroDto filtro)
    {
        var empresas = await _empresaRepository.FiltrarAsync(new EmpresaFiltroParams
        {
            RazaoSocial = filtro.RazaoSocial,
            Dominio = filtro.Dominio,
        });
        return _mapper.Map<EmpresaDto[]>(empresas);
    }
}
