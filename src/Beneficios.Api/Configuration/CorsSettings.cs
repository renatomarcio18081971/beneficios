namespace Beneficios.Api.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowAnyOrigin { get; set; }
}
