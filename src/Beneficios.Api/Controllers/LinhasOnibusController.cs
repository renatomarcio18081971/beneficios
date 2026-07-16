using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/linhas-onibus")]
public class LinhasOnibusController : ControllerBase
{
    private const string CodigoMenu = "linhas_onibus";

    private readonly ILinhaOnibusService _service;
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<LinhasOnibusController> _logger;

    public LinhasOnibusController(
        ILinhaOnibusService service,
        IPermissaoService permissaoService,
        ILogger<LinhasOnibusController> logger)
    {
        _service = service;
        _permissaoService = permissaoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Filtrar(
        [FromQuery] string? descricao,
        [FromQuery] bool? somenteVigentes)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var lista = await _service.FiltrarAsync(descricao, somenteVigentes);
            return Ok(lista);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar linhas de ônibus");
            return StatusCode(500, new { message = "Erro ao listar linhas de ônibus" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var item = await _service.ObterPorIdAsync(id);
            if (item is null)
                return NotFound(new { message = LinhaOnibusService.MensagemNaoEncontrada });

            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter linha de ônibus {Id}", id);
            return StatusCode(500, new { message = "Erro ao obter linha de ônibus" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LinhaOnibusSalvarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
                return denied;

            var id = await _service.SalvarAsync(dto, GetUsuarioIdFromToken());
            return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrad", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar linha de ônibus");
            return StatusCode(500, new { message = "Erro ao criar linha de ônibus" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LinhaOnibusAtualizarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
                return denied;

            await _service.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("não encontrad", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar linha de ônibus {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar linha de ônibus" });
        }
    }

    private async Task<IActionResult?> DenyIfUnauthorizedAsync(AcaoPermissao acao)
    {
        if (IsAdminTenant())
            return null;

        var usuarioId = GetUsuarioIdFromToken()
            ?? throw new UnauthorizedAccessException("Sem permissão para esta operação.");

        await _permissaoService.GarantirPermissaoAsync(usuarioId, CodigoMenu, acao);
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
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim?.Value, out var id) ? id : null;
    }

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";
        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
