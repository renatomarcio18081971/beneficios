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
[Route("api/calendario")]
public class CalendarioController : ControllerBase
{
    private const string CodigoMenu = "calendario";

    private readonly ICalendarioDiaService _calendarioDiaService;
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<CalendarioController> _logger;

    public CalendarioController(
        ICalendarioDiaService calendarioDiaService,
        IPermissaoService permissaoService,
        ILogger<CalendarioController> logger)
    {
        _calendarioDiaService = calendarioDiaService;
        _permissaoService = permissaoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObterPorMes([FromQuery] int ano, [FromQuery] int mes)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            if (ano < 1900 || mes is < 1 or > 12)
                return BadRequest(new { message = "Ano ou mês inválido." });

            var dias = await _calendarioDiaService.ObterPorMesAsync(ano, mes);
            return Ok(dias);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar calendário {Ano}/{Mes}", ano, mes);
            return StatusCode(500, new { message = "Erro ao listar calendário" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var dia = await _calendarioDiaService.ObterPorIdAsync(id);
            if (dia is null)
                return NotFound(new { message = CalendarioDiaService.MensagemDiaNaoEncontrado });

            return Ok(dia);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dia do calendário {Id}", id);
            return StatusCode(500, new { message = "Erro ao obter dia do calendário" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CalendarioDiaAtualizarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
                return denied;

            await _calendarioDiaService.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
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
            _logger.LogError(ex, "Erro ao atualizar dia do calendário {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar dia do calendário" });
        }
    }

    [HttpPost("gerar-ano")]
    public async Task<IActionResult> GerarAno([FromBody] GerarAnoCalendarioDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
                return denied;

            await _calendarioDiaService.GerarAnoAsync(dto.Ano);
            return Ok(new { message = "Ano gerado com sucesso." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar ano {Ano}", dto.Ano);
            return StatusCode(500, new { message = "Erro ao gerar ano do calendário" });
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
