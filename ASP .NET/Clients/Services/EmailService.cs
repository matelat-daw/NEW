using System.Net;
using System.Net.Mail;

namespace Clients.Services;

/// <summary>
/// Servicio para envío de emails
/// Requiere configuración en appsettings.json
/// </summary>
public interface IEmailService
{
    Task SendVerificationEmailAsync(string userEmail, string userName, string verificationToken);
    Task SendWelcomeEmailAsync(string userEmail, string userName);
    Task SendPasswordResetEmailAsync(string userEmail, string userName, string resetToken);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Envía email de verificación de cuenta
    /// </summary>
    public async Task SendVerificationEmailAsync(string userEmail, string userName, string verificationToken)
    {
        try
        {
            var mailFrom = _configuration["Email:From"] ?? "noreply@example.com";
            var appName = _configuration["Email:AppName"] ?? "MyApp";
            var backendUrl = _configuration["AppUrls:BackendUrl"] ?? "http://localhost:5000";

            string normalizedBackendUrl = backendUrl.EndsWith("/")
                ? backendUrl.Substring(0, backendUrl.Length - 1)
                : backendUrl;

            string verificationLink = $"{normalizedBackendUrl}/api/auth/verify/{verificationToken}";

            string htmlContent = BuildVerificationEmailHTML(userName, verificationLink, appName);

            await SendEmailAsync(mailFrom, userEmail, $"Verifica tu cuenta - {appName}", htmlContent);

            _logger.LogInformation($"📧 Email de verificación enviado a: {userEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al enviar email de verificación a {userEmail}: {ex.Message}");
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Envía email de bienvenida después de verificación
    /// </summary>
    public async Task SendWelcomeEmailAsync(string userEmail, string userName)
    {
        try
        {
            var mailFrom = _configuration["Email:From"] ?? "noreply@example.com";
            var appName = _configuration["Email:AppName"] ?? "MyApp";

            string htmlContent = BuildWelcomeEmailHTML(userName, appName);

            await SendEmailAsync(mailFrom, userEmail, $"¡Bienvenido a {appName}!", htmlContent);

            _logger.LogInformation($"📧 Email de bienvenida enviado a: {userEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al enviar email de bienvenida a {userEmail}: {ex.Message}");
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Envía email de recuperación de contraseña
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string userEmail, string userName, string resetToken)
    {
        try
        {
            var mailFrom = _configuration["Email:From"] ?? "noreply@example.com";
            var appName = _configuration["Email:AppName"] ?? "MyApp";
            var frontendUrl = _configuration["AppUrls:FrontendUrl"] ?? "http://localhost";

            string resetLink = $"{frontendUrl}/reset-password?token={resetToken}";
            string htmlContent = BuildPasswordResetEmailHTML(userName, resetLink, appName);

            await SendEmailAsync(mailFrom, userEmail, $"Recuperar contraseña - {appName}", htmlContent);

            _logger.LogInformation($"📧 Email de recuperación de contraseña enviado a: {userEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al enviar email de recuperación a {userEmail}: {ex.Message}");
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Método genérico para enviar emails
    /// </summary>
    private async Task SendEmailAsync(string from, string to, string subject, string htmlBody)
    {
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPassword = _configuration["Email:SmtpPassword"];
        var enableSSL = bool.Parse(_configuration["Email:EnableSSL"] ?? "true");

        if (string.IsNullOrEmpty(smtpServer))
        {
            _logger.LogWarning($"⚠️ SMTP no configurado. Email NO será enviado a: {to}");
            return; // No lanzar excepción, permitir que el registro continúe
        }

        try
        {
            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = enableSSL;
                smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                using (var mailMessage = new MailMessage(from, to))
                {
                    mailMessage.Subject = subject;
                    mailMessage.Body = htmlBody;
                    mailMessage.IsBodyHtml = true;

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            _logger.LogInformation($"✅ Email enviado correctamente a: {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error SMTP al enviar email a {to}: {ex.Message}");
            throw;
        }
    }

    private string BuildVerificationEmailHTML(string name, string verificationLink, string appName)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background-color: #f4f4f4; 
            margin: 0; 
            padding: 0;
        }}
        .container {{ 
            max-width: 600px; 
            margin: 20px auto; 
            background-color: white; 
            padding: 0; 
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{ 
            padding: 30px 20px; 
        }}
        .content p {{
            line-height: 1.6;
            color: #333;
            margin: 10px 0;
        }}
        .button {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; 
            padding: 14px 40px; 
            text-decoration: none; 
            border-radius: 4px; 
            display: inline-block; 
            margin: 20px 0;
            font-weight: bold;
        }}
        .button:hover {{
            opacity: 0.9;
        }}
        .footer {{ 
            text-align: center; 
            font-size: 12px; 
            color: #666; 
            padding: 20px;
            background-color: #f9f9f9;
            border-top: 1px solid #eee;
        }}
        .warning {{
            background-color: #fff3cd;
            border: 1px solid #ffc107;
            padding: 10px;
            border-radius: 4px;
            margin: 15px 0;
            color: #856404;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✉️ Verifica tu Cuenta</h1>
        </div>
        <div class='content'>
            <p>¡Hola <strong>{name}</strong>!</p>
            <p>Gracias por registrarte en <strong>{appName}</strong>. Para completar tu registro, por favor verifica tu dirección de correo electrónico.</p>
            <center>
                <a href='{verificationLink}' class='button'>Verificar Email</a>
            </center>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='font-size: 12px; word-break: break-all; color: #666;'>{verificationLink}</p>
            <div class='warning'>
                <strong>⏰ Importante:</strong> Este enlace expirará en 24 horas.
            </div>
            <p>Si no hiciste esta solicitud, simplemente ignora este correo.</p>
        </div>
        <div class='footer'>
            <p>© 2024 {appName}. Todos los derechos reservados.</p>
            <p>Este es un email automatizado. Por favor, no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildWelcomeEmailHTML(string name, string appName)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background-color: #f4f4f4; 
            margin: 0; 
            padding: 0;
        }}
        .container {{ 
            max-width: 600px; 
            margin: 20px auto; 
            background-color: white; 
            padding: 0; 
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{ 
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{ 
            padding: 30px 20px; 
        }}
        .content p {{
            line-height: 1.6;
            color: #333;
            margin: 10px 0;
        }}
        .success-box {{
            background-color: #d4edda;
            border: 1px solid #c3e6cb;
            padding: 15px;
            border-radius: 4px;
            margin: 15px 0;
            color: #155724;
        }}
        .footer {{ 
            text-align: center; 
            font-size: 12px; 
            color: #666; 
            padding: 20px;
            background-color: #f9f9f9;
            border-top: 1px solid #eee;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 ¡Bienvenido!</h1>
        </div>
        <div class='content'>
            <p>¡Hola <strong>{name}</strong>!</p>
            <div class='success-box'>
                <strong>✅ ¡Tu cuenta ha sido verificada exitosamente!</strong>
            </div>
            <p>Ahora puedes acceder a todas las funcionalidades de <strong>{appName}</strong>.</p>
            <p>Estamos felices de tenerte en nuestra comunidad. Si tienes preguntas o necesitas ayuda, no dudes en contactarnos.</p>
        </div>
        <div class='footer'>
            <p>© 2024 {appName}. Todos los derechos reservados.</p>
            <p>Este es un email automatizado. Por favor, no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildPasswordResetEmailHTML(string name, string resetLink, string appName)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background-color: #f4f4f4; 
            margin: 0; 
            padding: 0;
        }}
        .container {{ 
            max-width: 600px; 
            margin: 20px auto; 
            background-color: white; 
            padding: 0; 
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{ 
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white; 
            padding: 30px 20px; 
            text-align: center; 
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{ 
            padding: 30px 20px; 
        }}
        .content p {{
            line-height: 1.6;
            color: #333;
            margin: 10px 0;
        }}
        .button {{ 
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white; 
            padding: 14px 40px; 
            text-decoration: none; 
            border-radius: 4px; 
            display: inline-block; 
            margin: 20px 0;
            font-weight: bold;
        }}
        .button:hover {{
            opacity: 0.9;
        }}
        .warning {{
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            padding: 10px;
            border-radius: 4px;
            margin: 15px 0;
            color: #721c24;
        }}
        .footer {{ 
            text-align: center; 
            font-size: 12px; 
            color: #666; 
            padding: 20px;
            background-color: #f9f9f9;
            border-top: 1px solid #eee;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Recuperar Contraseña</h1>
        </div>
        <div class='content'>
            <p>¡Hola <strong>{name}</strong>!</p>
            <p>Recibimos una solicitud para recuperar tu contraseña. Haz clic en el botón de abajo para establecer una nueva contraseña.</p>
            <center>
                <a href='{resetLink}' class='button'>Recuperar Contraseña</a>
            </center>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='font-size: 12px; word-break: break-all; color: #666;'>{resetLink}</p>
            <div class='warning'>
                <strong>⚠️ Importante:</strong> Este enlace expirará en 1 hora. Si no solicitaste esto, ignora este correo.
            </div>
        </div>
        <div class='footer'>
            <p>© 2024 {appName}. Todos los derechos reservados.</p>
            <p>Este es un email automatizado. Por favor, no respondas a este mensaje.</p>
        </div>
    </div>
</body>
</html>";
    }
}
