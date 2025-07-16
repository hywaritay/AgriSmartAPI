using AgriSmartAPI.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace AgriSmartAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderPassword;
    private readonly string _senderName;
    private readonly bool _bypassSsl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Get email configuration from appsettings.json
        _smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _senderEmail = _configuration["Email:SenderEmail"] ?? "";
        _senderPassword = _configuration["Email:SenderPassword"] ?? "";
        _senderName = _configuration["Email:SenderName"] ?? "AgriSmart";
        _bypassSsl = bool.Parse(_configuration["Email:BypassSsl"] ?? "false");
        
        _logger.LogInformation("Email Service initialized with SMTP: {Server}:{Port}, Sender: {Email}, BypassSSL: {BypassSsl}", 
            _smtpServer, _smtpPort, _senderEmail, _bypassSsl);
    }

    public async Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName)
    {
        var subject = "Email Verification OTP - AgriSmart API";
        var body = GenerateOtpEmailBody(otp, userName);
        
        var (success, errorMessage) = await SendEmailAsync(toEmail, subject, body);
        if (!success)
        {
            _logger.LogError("Failed to send OTP email: {ErrorMessage}", errorMessage);
        }
        return success;
    }

    public async Task<(bool Success, string ErrorMessage)> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {ToEmail} from {SenderEmail}", toEmail, _senderEmail);

            if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
            {
                var errorMsg = "Email configuration is missing. Please configure SenderEmail and SenderPassword in appsettings.json";
                _logger.LogError(errorMsg);
                return (false, errorMsg);
            }

            _logger.LogInformation("Using SMTP Server: {SmtpServer}:{SmtpPort}", _smtpServer, _smtpPort);

            // Bypass SSL certificate validation if configured
            if (_bypassSsl)
            {
                ServicePointManager.ServerCertificateValidationCallback = 
                    new RemoteCertificateValidationCallback(ValidateServerCertificate);
                _logger.LogWarning("SSL certificate validation is bypassed for development purposes");
            }

            using var client = new SmtpClient();
            
            // Configure SMTP client based on port
            if (_smtpPort == 465)
            {
                // SSL connection
                client.Host = _smtpServer;
                client.Port = _smtpPort;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
            }
            else
            {
                // TLS connection (port 587)
                client.Host = _smtpServer;
                client.Port = _smtpPort;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
            }

            client.Timeout = 10000; // 10 seconds timeout

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            _logger.LogInformation("Attempting to send email...");
            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return (true, "");
        }
        catch (SmtpException smtpEx)
        {
            var errorMsg = $"SMTP Error: StatusCode={smtpEx.StatusCode}, Message={smtpEx.Message}";
            _logger.LogError(smtpEx, "SMTP Error sending email to {ToEmail}. {ErrorMsg}", toEmail, errorMsg);
            return (false, errorMsg);
        }
        catch (Exception ex)
        {
            var errorMsg = $"General Error: {ex.Message}";
            _logger.LogError(ex, "Error sending email to {ToEmail}. {ErrorMsg}", toEmail, errorMsg);
            return (false, errorMsg);
        }
    }

    private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // Always return true to bypass certificate validation
        return true;
    }

    private string GenerateOtpEmailBody(string otp, string userName)
    {
        var htmlBody = new StringBuilder();
        htmlBody.AppendLine("<!DOCTYPE html>");
        htmlBody.AppendLine("<html>");
        htmlBody.AppendLine("<head>");
        htmlBody.AppendLine("    <meta charset='utf-8'>");
        htmlBody.AppendLine("    <title>Email Verification OTP</title>");
        htmlBody.AppendLine("    <style>");
        htmlBody.AppendLine("        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
        htmlBody.AppendLine("        .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
        htmlBody.AppendLine("        .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }");
        htmlBody.AppendLine("        .content { padding: 20px; background-color: #f9f9f9; }");
        htmlBody.AppendLine("        .otp-box { background-color: #fff; border: 2px solid #4CAF50; padding: 15px; text-align: center; margin: 20px 0; border-radius: 5px; }");
        htmlBody.AppendLine("        .otp-code { font-size: 24px; font-weight: bold; color: #4CAF50; letter-spacing: 5px; }");
        htmlBody.AppendLine("        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }");
        htmlBody.AppendLine("    </style>");
        htmlBody.AppendLine("</head>");
        htmlBody.AppendLine("<body>");
        htmlBody.AppendLine("    <div class='container'>");
        htmlBody.AppendLine("        <div class='header'>");
        htmlBody.AppendLine("            <h1>AgriSmart API</h1>");
        htmlBody.AppendLine("            <h2>Email Verification OTP</h2>");
        htmlBody.AppendLine("        </div>");
        htmlBody.AppendLine("        <div class='content'>");
        htmlBody.AppendLine($"            <p>Hello {userName},</p>");
        htmlBody.AppendLine("            <p>Thank you for registering with AgriSmart API. To complete your email verification, please use the following OTP:</p>");
        htmlBody.AppendLine("            <div class='otp-box'>");
        htmlBody.AppendLine($"                <div class='otp-code'>{otp}</div>");
        htmlBody.AppendLine("            </div>");
        htmlBody.AppendLine("            <p><strong>Important:</strong></p>");
        htmlBody.AppendLine("            <ul>");
        htmlBody.AppendLine("                <li>This OTP is valid for 15 minutes only</li>");
        htmlBody.AppendLine("                <li>Do not share this OTP with anyone</li>");
        htmlBody.AppendLine("                <li>If you didn't request this OTP, please ignore this email</li>");
        htmlBody.AppendLine("            </ul>");
        htmlBody.AppendLine("            <p>If you have any questions, please contact our support team.</p>");
        htmlBody.AppendLine("            <p>Best regards,<br>The AgriSmart API Team</p>");
        htmlBody.AppendLine("        </div>");
        htmlBody.AppendLine("        <div class='footer'>");
        htmlBody.AppendLine("            <p>This is an automated email. Please do not reply to this message.</p>");
        htmlBody.AppendLine("        </div>");
        htmlBody.AppendLine("    </div>");
        htmlBody.AppendLine("</body>");
        htmlBody.AppendLine("</html>");

        return htmlBody.ToString();
    }
} 