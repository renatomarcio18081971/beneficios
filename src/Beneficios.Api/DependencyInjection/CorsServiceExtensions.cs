using Beneficios.Api.Configuration;

namespace Beneficios.Api.DependencyInjection;

public static class CorsServiceExtensions
{
    public const string PolicyName = "FrontPolicy";

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (corsSettings.AllowAnyOrigin)
                {
                    policy.AllowAnyOrigin();
                }
                else if (corsSettings.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsSettings.AllowedOrigins);
                }
                else
                {
                    policy.SetIsOriginAllowed(origin =>
                        origin.Contains("localhost:4200", StringComparison.OrdinalIgnoreCase)
                        || origin.EndsWith(".minhaempresa.com.br", StringComparison.OrdinalIgnoreCase)
                        || origin.EndsWith(".beneficios.servidor", StringComparison.OrdinalIgnoreCase));
                }

                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
