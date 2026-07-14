using Beneficios.Domain;
using Xunit;

namespace Beneficios.Tests.Domain;

public class AfastamentoPeriodoTests
{
    [Fact]
    public void Sobrepoe_IntervalosAbertos_DeveDetectarConflito()
    {
        Assert.True(AfastamentoPeriodo.Sobrepoe(
            new DateOnly(2026, 1, 1), null,
            new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 30)));
    }

    [Fact]
    public void Sobrepoe_IntervalosDisjuntos_NaoConflita()
    {
        Assert.False(AfastamentoPeriodo.Sobrepoe(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28)));
    }

    [Fact]
    public void Sobrepoe_ToqueNaBorda_Conflita()
    {
        Assert.True(AfastamentoPeriodo.Sobrepoe(
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 10)));
    }

    [Fact]
    public void EstaAtivoEm_FimNulo_TrueSeJaComecou()
    {
        Assert.True(AfastamentoPeriodo.EstaAtivoEm(
            new DateOnly(2026, 1, 1), null, new DateOnly(2026, 7, 14)));
    }

    [Fact]
    public void EstaAtivoEm_AntesDoInicio_False()
    {
        Assert.False(AfastamentoPeriodo.EstaAtivoEm(
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 10), new DateOnly(2026, 1, 15)));
    }

    [Fact]
    public void EstaAtivoEm_DepoisDoFim_False()
    {
        Assert.False(AfastamentoPeriodo.EstaAtivoEm(
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 11)));
    }
}