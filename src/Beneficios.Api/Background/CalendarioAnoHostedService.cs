using Beneficios.Domain.Interfaces;

namespace Beneficios.Api.Background;

public class CalendarioAnoHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CalendarioAnoHostedService> _logger;

    public CalendarioAnoHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CalendarioAnoHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var garantia = scope.ServiceProvider.GetRequiredService<ICalendarioAnoGarantia>();
                await garantia.GarantirAnoCorrenteEmTenantsExistentesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Falha ao garantir calendário do ano corrente nos tenants.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
