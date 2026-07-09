using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly ILogger<EmpresasController> _logger;

    public EmpresasController(IEmpresaService empresaService, ILogger<EmpresasController> logger)
    {
        _empresaService = empresaService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpresaSalvarDto dto)
    {
        try
        {
            _logger.LogInformation("Criando nova empresa: {RazaoSocial}", dto.RazaoSocial);
            var id = await _empresaService.SalvarAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar empresa: {RazaoSocial}", dto.RazaoSocial);
            return StatusCode(500, new { message = "Erro ao criar empresa" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmpresaAtualizarDto dto)
    {
        try
        {
            var usuarioAlteracaoId = GetUsuarioIdFromToken();
            _logger.LogInformation("Atualizando empresa: {Id}", id);

            var success = await _empresaService.AtualizarAsync(id, dto, usuarioAlteracaoId);
            if (!success)
                return NotFound(new { message = "Empresa n�o encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa: {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar empresa" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var empresa = await _empresaService.ObterUmAsync(id);
            if (empresa == null)
                return NotFound(new { message = "Empresa n�o encontrada" });

            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresa: {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar empresa" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var empresas = await _empresaService.ObterTodosAsync();
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas");
            return StatusCode(500, new { message = "Erro ao buscar empresas" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogInformation("Deletando empresa: {Id}", id);
            var success = await _empresaService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Empresa n�o encontrada" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar empresa: {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar empresa" });
        }
    }

    private Guid? GetUsuarioIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}
