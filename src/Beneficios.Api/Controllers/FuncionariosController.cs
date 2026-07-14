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
[Route("api/funcionarios")]
public class FuncionariosController : ControllerBase
{
    private const string CodigoMenu = "funcionarios";

    private readonly IFuncionarioService _funcionarioService;
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<FuncionariosController> _logger;

    public FuncionariosController(
        IFuncionarioService funcionarioService,
        IPermissaoService permissaoService,
        ILogger<FuncionariosController> logger)
    {
        _funcionarioService = funcionarioService;
        _permissaoService = permissaoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Filtrar([FromQuery] FuncionarioFiltroDto filtro)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var lista = await _funcionarioService.FiltrarAsync(filtro);
            return Ok(lista);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar funcionários");
            return StatusCode(500, new { message = "Erro ao listar funcionários" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var item = await _funcionarioService.ObterPorIdAsync(id);
            if (item is null)
                return NotFound(new { message = FuncionarioService.MensagemNaoEncontrado });

            return Ok(item);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter funcionário {Id}", id);
            return StatusCode(500, new { message = "Erro ao obter funcionário" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] FuncionarioSalvarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
                return denied;

            var id = await _funcionarioService.SalvarAsync(dto, GetUsuarioIdFromToken());
            return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
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
            _logger.LogError(ex, "Erro ao criar funcionário");
            return StatusCode(500, new { message = "Erro ao criar funcionário" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] FuncionarioAtualizarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
                return denied;

            await _funcionarioService.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
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
            _logger.LogError(ex, "Erro ao atualizar funcionário {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar funcionário" });
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
