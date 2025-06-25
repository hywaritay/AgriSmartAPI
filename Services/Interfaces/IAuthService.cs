using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;

namespace AgriSmartAPI.Services.Interfaces;

public interface IAuthService
{
    Task<User> Register(RegisterModel registerModel);
    Task<string> Login(LoginModel loginModel);
}
