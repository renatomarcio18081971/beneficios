using Beneficios.Application.DTOs;
using Beneficios.Application.Interfaces;
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
    private readonly IPerfilService _perfilService;
    private readonly ILogger<PerfisController> _logger;

    public PerfisController(IPerfilService perfilService, ILogger<PerfisController> logger)
    {
        _perfilService = perfilService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PerfilSalvarDto dto)
    {
        try
        {
            _logger.LogInformation("Criando perfil: {Nome}", dto.Nome);
            var id = await _perfilService.SalvarAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
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
            _logger.LogInformation("Atualizando perfil: {Id}", id);
            await _perfilService.AtualizarAsync(id, dto, GetUsuarioIdFromToken());
            return NoContent();
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
            var perfis = await _perfilService.FiltrarAsync(filtro);
            return Ok(perfis);
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
            var perfil = await _perfilService.ObterUmAsync(id);
            if (perfil is null)
                return NotFound(new { message = "Perfil não encontrado" });

            return Ok(perfil);
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
            var perfis = await _perfilService.ObterTodosAsync();
            return Ok(perfis);
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
            _logger.LogInformation("Excluindo perfil: {Id}", id);
            await _perfilService.DeleteAsync(id);
            return NoContent();
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

    private Guid? GetUsuarioIdFromToken()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
