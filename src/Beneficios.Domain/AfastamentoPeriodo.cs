namespace Beneficios.Domain;

public static class AfastamentoPeriodo
{
    public static bool Sobrepoe(DateOnly iniA, DateOnly? fimA, DateOnly iniB, DateOnly? fimB)
    {
        var fimAEfetivo = fimA ?? DateOnly.MaxValue;
        var fimBEfetivo = fimB ?? DateOnly.MaxValue;
        return iniA <= fimBEfetivo && iniB <= fimAEfetivo;
    }

    public static bool EstaAtivoEm(DateOnly dataInicio, DateOnly? dataFim, DateOnly referencia) =>
        dataInicio <= referencia && (dataFim is null || dataFim >= referencia);
}