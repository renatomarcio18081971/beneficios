using System.Net;
using System.Net.Mail;
using Beneficios.Application.Configuration;
using Beneficios.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beneficios.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task EnviarAsync(string destinatario, string assunto, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning(
                "SMTP não configurado. E-mail para {Destinatario} | Assunto: {Assunto} | Mensagem: {Mensagem}",
                destinatario,
                assunto,
                mensagem);
            return Task.CompletedTask;
        }

        _ = Task.Run(() => EnviarInternoAsync(destinatario, assunto, mensagem));
        return Task.CompletedTask;
    }

    private async Task EnviarInternoAsync(string destinatario, string assunto, string mensagem)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_settings.Username, _settings.Password),
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromDisplayName),
                Subject = assunto,
                Body = mensagem,
                IsBodyHtml = false,
            };
            mail.To.Add(destinatario);

            await client.SendMailAsync(mail);
            _logger.LogInformation("E-mail enviado para {Destinatario}", destinatario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {Destinatario}", destinatario);
        }
    }
}
