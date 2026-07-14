using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private const string CodigoMenu = "usuarios";

    private readonly IUsuarioService _usuarioService;
    private readonly IPermissaoService _permissaoService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        IUsuarioService usuarioService,
        IPermissaoService permissaoService,
        ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _permissaoService = permissaoService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] UsuarioSalvarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Criar) is { } denied)
                return denied;

            _logger.LogInformation("Criando novo usuário: {Email}", dto.Email);
            var id = await _usuarioService.SalvarAsync(dto);
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
            _logger.LogError(ex, "Erro ao criar usuário: {Email}", dto.Email);
            return StatusCode(500, new { message = "Erro ao criar usuário" });
        }
    }

    [HttpPut("solicitarAlteracaoSenha")]
    [AllowAnonymous]
    public async Task<IActionResult> SolicitarAlteracaoSenha([FromBody] SolicitarAlteracaoSenhaDto dto)
    {
        try
        {
            var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
                ?? ExtractSubdomain(Request.Host.Host);
            _logger.LogInformation("Solicitação de alteração de senha: {Email} (tenant {Tenant})", dto.Email, tenant);

            if (!await _usuarioService.EmailExisteAsync(dto.Email, tenant))
                return NotFound(new { message = "E-mail não localizado !" });

            await _usuarioService.SolicitarAlteracaoSenhaAsync(dto, tenant);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar alteração de senha: {Email}", dto.Email);
            return StatusCode(500, new { message = "Erro ao solicitar alteração de senha" });
        }
    }

    [HttpPut("alterarSenha")]
    [AllowAnonymous]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        try
        {
            var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
                ?? ExtractSubdomain(Request.Host.Host);
            _logger.LogInformation("Alteração de senha solicitada (tenant {Tenant})", tenant);
            var success = await _usuarioService.AlterarSenhaAsync(dto, tenant);
            if (!success)
                return BadRequest(new { message = "Código inválido." });

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha");
            return StatusCode(500, new { message = "Erro ao alterar senha" });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioAtualizarDto dto)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Editar) is { } denied)
                return denied;

            var usuarioAlteracaoId = GetUsuarioIdFromToken();
            _logger.LogInformation("Atualizando usuário: {Id}", id);

            var success = await _usuarioService.AtualizarAsync(id, dto, usuarioAlteracaoId);
            if (!success)
                return NotFound(new { message = "Usuário não encontrado" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário: {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar usuário" });
        }
    }

    [HttpGet("filtrar")]
    [Authorize]
    public async Task<IActionResult> Filtrar([FromQuery] string? nome, [FromQuery] string? email)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var usuarios = await _usuarioService.FiltrarAsync(new UsuarioFiltroDto(nome, email));
            return Ok(usuarios);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao filtrar usuários");
            return StatusCode(500, new { message = "Erro ao filtrar usuários" });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var usuario = await _usuarioService.ObterUmAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usuário não encontrado" });

            return Ok(usuario);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário: {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar usuário" });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Visualizar) is { } denied)
                return denied;

            var usuarios = await _usuarioService.ObterTodosAsync();
            return Ok(usuarios);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários");
            return StatusCode(500, new { message = "Erro ao buscar usuários" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            if (await DenyIfUnauthorizedAsync(AcaoPermissao.Excluir) is { } denied)
                return denied;

            _logger.LogInformation("Deletando usuário: {Id}", id);
            var success = await _usuarioService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Usuário não encontrado" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário: {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar usuário" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Tentativa de login: {Email}", loginDto.Email);
            var tenant = Request.Headers["X-Tenant"].FirstOrDefault()
                ?? ExtractSubdomain(Request.Host.Host);
            var response = await _usuarioService.LoginAsync(loginDto, tenant);

            if (response == null)
                return Unauthorized(new { message = "Email ou senha inválidos" });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login: {Email}", loginDto.Email);
            return StatusCode(500, new { message = "Erro ao fazer login" });
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
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";

        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
