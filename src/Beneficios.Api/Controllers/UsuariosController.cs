using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] UsuarioSalvarDto dto)
    {
        try
        {
            _logger.LogInformation("Criando novo usu�rio: {Email}", dto.Email);
            var id = await _usuarioService.SalvarAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usu�rio: {Email}", dto.Email);
            return StatusCode(500, new { message = "Erro ao criar usu�rio" });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioAtualizarDto dto)
    {
        try
        {
            var usuarioAlteracaoId = GetUsuarioIdFromToken();
            _logger.LogInformation("Atualizando usu�rio: {Id}", id);

            var success = await _usuarioService.AtualizarAsync(id, dto, usuarioAlteracaoId);
            if (!success)
                return NotFound(new { message = "Usu�rio n�o encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usu�rio: {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar usu�rio" });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var usuario = await _usuarioService.ObterUmAsync(id);
            if (usuario == null)
                return NotFound(new { message = "Usu�rio n�o encontrado" });

            return Ok(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usu�rio: {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar usu�rio" });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var usuarios = await _usuarioService.ObterTodosAsync();
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usu�rios");
            return StatusCode(500, new { message = "Erro ao buscar usu�rios" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            _logger.LogInformation("Deletando usu�rio: {Id}", id);
            var success = await _usuarioService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Usu�rio n�o encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usu�rio: {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar usu�rio" });
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer login: {Email}", loginDto.Email);
            return StatusCode(500, new { message = "Erro ao fazer login" });
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

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";

        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
