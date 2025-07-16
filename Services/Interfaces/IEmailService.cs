namespace AgriSmartAPI.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName);
    Task<(bool Success, string ErrorMessage)> SendEmailAsync(string toEmail, string subject, string body);
} 