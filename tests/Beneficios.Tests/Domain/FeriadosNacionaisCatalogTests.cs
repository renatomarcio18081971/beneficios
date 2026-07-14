using Beneficios.Domain;
using Xunit;

namespace Beneficios.Tests.Domain;

public class FeriadosNacionaisCatalogTests
{
    [Fact]
    public void ObterParaAno_2026_DeveConterFeriadosFixosEMoveisConhecidos()
    {
        var feriados = FeriadosNacionaisCatalog.ObterParaAno(2026);

        Assert.Contains(feriados, f =>
            f.Data == new DateOnly(2026, 1, 1)
            && f.Nome.Contains("Confraterniza", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 12, 25));
        Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 4, 3));
        Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 2, 17));
        Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 11, 20));
        Assert.Contains(feriados, f => f.Data == new DateOnly(2026, 6, 4)); // Corpus Christi 2026
    }

    [Fact]
    public void ObterParaAno_DeveCalcularPascoaConhecida()
    {
        // Páscoa 2026 = 5 de abril → validado indiretamente pelos móveis
        var feriados = FeriadosNacionaisCatalog.ObterParaAno(2026);
        Assert.DoesNotContain(feriados, f => f.Data == new DateOnly(2026, 4, 5)); // Páscoa não é feriado civil nacional obrigatório na lista
    }
}
