using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;

namespace AgriSmartAPI.Services.Interfaces;

public interface IAuthService
{
    Task<User> Register(RegisterModel registerModel);
    Task<string> Login(LoginModel loginModel);
    Task<bool> VerifyEmailAsync(VerifyEmailModel verifyEmailModel);
    Task<(bool Success, string ErrorMessage)> SendOtpAsync(SendOtpModel sendOtpModel);
    Task<(bool Success, string ErrorMessage)> ForgotPasswordAsync(ForgotPasswordRequestModel model);
    Task<(bool Success, string ErrorMessage)> ResetPasswordAsync(ResetPasswordRequestModel model);
}
