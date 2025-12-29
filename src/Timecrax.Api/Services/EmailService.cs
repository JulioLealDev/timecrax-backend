using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Timecrax.Api.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string language)
    {
        var smtp = _config.GetSection("Smtp");
        var enabled = smtp.GetValue<bool>("Enabled");

        if (!enabled)
        {
            _logger.LogWarning("SMTP is disabled. Reset token for {Email}: {Token}", toEmail, resetToken);
            return;
        }

        var frontendUrl = _config["FrontendUrl"] ?? "http://localhost:5173";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

        var (subject, htmlBody, textBody) = GetEmailContent(language, resetLink);

        await SendEmailAsync(toEmail, subject, htmlBody, textBody);
    }

    private (string subject, string htmlBody, string textBody) GetEmailContent(string language, string resetLink)
    {
        return language switch
        {
            "pt_br" or "pt-br" => (
                "Redefinir sua senha - TimeCrax",
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #b45309;"">Redefinir Senha</h2>
        <p>Você solicitou a redefinição da sua senha no TimeCrax.</p>
        <p>Clique no botão abaixo para criar uma nova senha:</p>
        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #b45309; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;"">Redefinir Senha</a>
        </p>
        <p style=""font-size: 14px; color: #666;"">Ou copie e cole este link no seu navegador:</p>
        <p style=""font-size: 14px; color: #b45309; word-break: break-all;"">{resetLink}</p>
        <p style=""font-size: 14px; color: #666;"">Este link expira em 1 hora.</p>
        <p style=""font-size: 14px; color: #666;"">Se você não solicitou esta redefinição, ignore este email.</p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999;"">TimeCrax - Plataforma de Aprendizagem</p>
    </div>
</body>
</html>",
                $@"Redefinir Senha - TimeCrax

Você solicitou a redefinição da sua senha no TimeCrax.

Clique no link abaixo para criar uma nova senha:
{resetLink}

Este link expira em 1 hora.

Se você não solicitou esta redefinição, ignore este email.

---
TimeCrax - Plataforma de Aprendizagem"
            ),
            "pt_pt" or "pt-pt" => (
                "Redefinir a sua palavra-passe - TimeCrax",
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #b45309;"">Redefinir Palavra-passe</h2>
        <p>Solicitou a redefinição da sua palavra-passe no TimeCrax.</p>
        <p>Clique no botão abaixo para criar uma nova palavra-passe:</p>
        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #b45309; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;"">Redefinir Palavra-passe</a>
        </p>
        <p style=""font-size: 14px; color: #666;"">Ou copie e cole este link no seu navegador:</p>
        <p style=""font-size: 14px; color: #b45309; word-break: break-all;"">{resetLink}</p>
        <p style=""font-size: 14px; color: #666;"">Este link expira em 1 hora.</p>
        <p style=""font-size: 14px; color: #666;"">Se não solicitou esta redefinição, ignore este email.</p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999;"">TimeCrax - Plataforma de Aprendizagem</p>
    </div>
</body>
</html>",
                $@"Redefinir Palavra-passe - TimeCrax

Solicitou a redefinição da sua palavra-passe no TimeCrax.

Clique no link abaixo para criar uma nova palavra-passe:
{resetLink}

Este link expira em 1 hora.

Se não solicitou esta redefinição, ignore este email.

---
TimeCrax - Plataforma de Aprendizagem"
            ),
            "fr" => (
                "Réinitialiser votre mot de passe - TimeCrax",
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #b45309;"">Réinitialiser le mot de passe</h2>
        <p>Vous avez demandé la réinitialisation de votre mot de passe sur TimeCrax.</p>
        <p>Cliquez sur le bouton ci-dessous pour créer un nouveau mot de passe :</p>
        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #b45309; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;"">Réinitialiser le mot de passe</a>
        </p>
        <p style=""font-size: 14px; color: #666;"">Ou copiez et collez ce lien dans votre navigateur :</p>
        <p style=""font-size: 14px; color: #b45309; word-break: break-all;"">{resetLink}</p>
        <p style=""font-size: 14px; color: #666;"">Ce lien expire dans 1 heure.</p>
        <p style=""font-size: 14px; color: #666;"">Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999;"">TimeCrax - Plateforme d'apprentissage</p>
    </div>
</body>
</html>",
                $@"Réinitialiser le mot de passe - TimeCrax

Vous avez demandé la réinitialisation de votre mot de passe sur TimeCrax.

Cliquez sur le lien ci-dessous pour créer un nouveau mot de passe :
{resetLink}

Ce lien expire dans 1 heure.

Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.

---
TimeCrax - Plateforme d'apprentissage"
            ),
            "es" => (
                "Restablecer tu contraseña - TimeCrax",
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #b45309;"">Restablecer Contraseña</h2>
        <p>Has solicitado restablecer tu contraseña en TimeCrax.</p>
        <p>Haz clic en el botón de abajo para crear una nueva contraseña:</p>
        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #b45309; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;"">Restablecer Contraseña</a>
        </p>
        <p style=""font-size: 14px; color: #666;"">O copia y pega este enlace en tu navegador:</p>
        <p style=""font-size: 14px; color: #b45309; word-break: break-all;"">{resetLink}</p>
        <p style=""font-size: 14px; color: #666;"">Este enlace expira en 1 hora.</p>
        <p style=""font-size: 14px; color: #666;"">Si no solicitaste este restablecimiento, ignora este correo.</p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999;"">TimeCrax - Plataforma de Aprendizaje</p>
    </div>
</body>
</html>",
                $@"Restablecer Contraseña - TimeCrax

Has solicitado restablecer tu contraseña en TimeCrax.

Haz clic en el enlace de abajo para crear una nueva contraseña:
{resetLink}

Este enlace expira en 1 hora.

Si no solicitaste este restablecimiento, ignora este correo.

---
TimeCrax - Plataforma de Aprendizaje"
            ),
            _ => (
                "Reset your password - TimeCrax",
                $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0;"">
    <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h2 style=""color: #b45309;"">Reset Password</h2>
        <p>You have requested to reset your password on TimeCrax.</p>
        <p>Click the button below to create a new password:</p>
        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetLink}"" style=""display: inline-block; background-color: #b45309; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px;"">Reset Password</a>
        </p>
        <p style=""font-size: 14px; color: #666;"">Or copy and paste this link into your browser:</p>
        <p style=""font-size: 14px; color: #b45309; word-break: break-all;"">{resetLink}</p>
        <p style=""font-size: 14px; color: #666;"">This link expires in 1 hour.</p>
        <p style=""font-size: 14px; color: #666;"">If you did not request this reset, please ignore this email.</p>
        <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">
        <p style=""font-size: 12px; color: #999;"">TimeCrax - Learning Platform</p>
    </div>
</body>
</html>",
                $@"Reset Password - TimeCrax

You have requested to reset your password on TimeCrax.

Click the link below to create a new password:
{resetLink}

This link expires in 1 hour.

If you did not request this reset, please ignore this email.

---
TimeCrax - Learning Platform"
            )
        };
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody)
    {
        var smtp = _config.GetSection("Smtp");

        var host = smtp["Host"]!;
        var port = smtp.GetValue<int>("Port");
        var username = smtp["Username"]!;
        var password = smtp["Password"]!;
        var fromEmail = smtp["FromEmail"]!;
        var fromName = smtp["FromName"] ?? "TimeCrax";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject
        };
        message.To.Add(toEmail);

        // Add both plain text and HTML views for better compatibility
        var textView = AlternateView.CreateAlternateViewFromString(textBody, null, MediaTypeNames.Text.Plain);
        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

        message.AlternateViews.Add(textView);
        message.AlternateViews.Add(htmlView);

        await client.SendMailAsync(message);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
