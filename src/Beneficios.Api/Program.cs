using Beneficios.Api.Background;
using Beneficios.Api.DependencyInjection;
using Beneficios.Api.Middleware;
using Beneficios.Application.Configuration;
using Beneficios.Application.Interfaces;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Infrastructure.Configurations;
using Beneficios.Infrastructure.Repositories;
using Beneficios.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/beneficios-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApiSwagger();
builder.Services.AddApiForwardedHeaders();
builder.Services.AddApiCors(builder.Configuration);

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(UsuarioProfile));

builder.Services.AddScoped<ITenantCatalogRepository, TenantCatalogRepository>();
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<ITenantProvisioner, TenantProvisioner>();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = httpContextAccessor.HttpContext?.Items[TenantContextKeys.ConnectionString] as string
        ?? configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    return DatabaseConfiguration.CreateConnection(connectionString);
});

builder.Services.AddScoped<ITenantSchemaAccessor>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var schema = httpContextAccessor.HttpContext?.Items[TenantContextKeys.Schema] as string
        ?? TenantSchemaNames.CatalogSchema;
    return new TenantSchemaAccessor(schema);
});

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IPerfilRepository, PerfilRepository>();
builder.Services.AddScoped<ICalendarioDiaRepository, CalendarioDiaRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IFuncionarioAfastamentoRepository, FuncionarioAfastamentoRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<ICalendarioDiaService, CalendarioDiaService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IFuncionarioAfastamentoService, FuncionarioAfastamentoService>();
builder.Services.AddScoped<IPermissaoService, PermissaoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICalendarioAnoGarantia, CalendarioAnoGarantia>();
builder.Services.AddHostedService<CalendarioAnoHostedService>();

builder.Services.AddSingleton<ITokenService>(sp =>
{
    var secretKey = builder.Configuration["JwtSettings:SecretKey"]
        ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurada.");
    var issuer = builder.Configuration["JwtSettings:Issuer"]
        ?? throw new InvalidOperationException("JwtSettings:Issuer não configurada.");
    var audience = builder.Configuration["JwtSettings:Audience"]
        ?? throw new InvalidOperationException("JwtSettings:Audience não configurada.");
    return new TokenService(secretKey, issuer, audience);
});

var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey não configurada.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer não configurada.");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience não configurada.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var provisioner = scope.ServiceProvider.GetRequiredService<ITenantProvisioner>();
    await provisioner.GarantirPerfisEmTenantsExistentesAsync();
    var calendarioGarantia = scope.ServiceProvider.GetRequiredService<ICalendarioAnoGarantia>();
    await calendarioGarantia.GarantirAnoCorrenteEmTenantsExistentesAsync();
}
catch (Exception ex)
{
    Log.Warning(ex, "Não foi possível migrar perfis/calendário nos tenants existentes na inicialização.");
}

app.UseApiForwardedHeaders();

if (!app.Environment.IsDevelopment() && !string.Equals(app.Environment.EnvironmentName, "Docker", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsServiceExtensions.PolicyName);
app.UseMiddleware<TenantMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseApiSwagger();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
