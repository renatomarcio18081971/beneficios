using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PerfisController : ControllerBase
{
    private const string CodigoMenu = "perfis";

    private readonly IPerfilService _perfilService;
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<PerfisController> _logger;

    public PerfisController(
        IPerfilService perfilService,
        IPermissaoService permissaoService,
        ILogger<PerfisController> logger)
    {
        _perfilService = perfilService;
        _permissaoService = permissaoService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PerfilSalvarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
                return denied;

            _logger.LogInformation("Criando perfil: {Nome}", dto.Nome);
            var id = await _perfilService.SalvarAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar perfil: {Nome}", dto.Nome);
            return StatusCode(500, new { message = "Erro ao criar perfil" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PerfilAtualizarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
                return denied;

            _logger.LogInformation("Atualizando perfil: {Id}", id);
            await _perfilService.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil: {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar perfil" });
        }
    }

    [HttpGet("filtrar")]
    public async Task<IActionResult> Filtrar([FromQuery] PerfilFiltroDto filtro)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var perfis = await _perfilService.FiltrarAsync(filtro);
            return Ok(perfis);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao filtrar perfis");
            return StatusCode(500, new { message = "Erro ao filtrar perfis" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var perfil = await _perfilService.ObterUmAsync(id);
            if (perfil is null)
                return NotFound(new { message = "Perfil não encontrado" });

            return Ok(perfil);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter perfil: {Id}", id);
            return StatusCode(500, new { message = "Erro ao obter perfil" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var perfis = await _perfilService.ObterTodosAsync();
            return Ok(perfis);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar perfis");
            return StatusCode(500, new { message = "Erro ao listar perfis" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Excluir) is { } denied)
                return denied;

            _logger.LogInformation("Excluindo perfil: {Id}", id);
            await _perfilService.DeleteAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir perfil: {Id}", id);
            return StatusCode(500, new { message = "Erro ao excluir perfil" });
        }
    }

    private async Task<IActionResult?> DenyIfUnauthorizedAsync(AcaoPermissao acao)
    {
        if (IsAdminTenant())
            return null;

        var usuarioId = GetUsuarioIdFromToken()
            ?? throw new UnauthorizedAccessException("Sem permissão para esta operação.");

        await _permissaoService.EnsureAsync(usuarioId, CodigoMenu, acao);
        return null;
    }

    private bool IsAdminTenant()
    {
        if (HttpContext?.Request is null)
            return true;

        var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
            ?? ExtractSubdomain(Request.Host.Host);
        return string.Equals(tenant, "admin", StringComparison.OrdinalIgnoreCase);
    }

    private Guid? GetUsuarioIdFromToken()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";

        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
