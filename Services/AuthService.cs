using AgriSmartAPI.Data;
using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using AgriSmartAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgriSmartAPI.Services;

public class AuthService : IAuthService
{
    private readonly AgriSmartContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public AuthService(
        AgriSmartContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IOtpService otpService,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<User> Register(RegisterModel registerModel)
    {
        try
        {
            _logger.LogInformation("Registering user {Username}", registerModel.Username);

            if (await _context.Users.AnyAsync(u => u.Username == registerModel.Username))
                throw new ValidationException("Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == registerModel.Email))
                throw new ValidationException("Email already exists");

            var user = new User
            {
                FullName = registerModel.FullName,
                Username = registerModel.Username,
                Email = registerModel.Email,
                Role = registerModel.Role,
                Password = HashPassword(registerModel.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", user.Username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {Username}", registerModel.Username);
            throw;
        }
    }

    public async Task<string> Login(LoginModel loginModel)
    {
        try
        {
            _logger.LogInformation("Login attempt for {Username}", loginModel.Username);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginModel.Username);

            if (user == null || !VerifyPassword(loginModel.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = GenerateJwtToken(user);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Username}", loginModel.Username);
            throw;
        }
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailModel verifyEmailModel)
    {
        try
        {
            _logger.LogInformation("Email verification attempt for {Email}", verifyEmailModel.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == verifyEmailModel.Email);

            if (user == null)
            {
                _logger.LogWarning("Email verification failed: User not found for {Email}", verifyEmailModel.Email);
                return false;
            }

            // Check if OTP is expired
            if (_otpService.IsOtpExpired(user.OtpExpiryTime))
            {
                _logger.LogWarning("Email verification failed: OTP expired for {Email}", verifyEmailModel.Email);
                return false;
            }

            // Check OTP attempts limit (max 5 attempts)
            if (user.OtpAttempts >= 5)
            {
                _logger.LogWarning("Email verification failed: Too many attempts for {Email}", verifyEmailModel.Email);
                return false;
            }

            // Verify OTP
            if (string.IsNullOrEmpty(user.EncryptedOtp) || !_otpService.VerifyOtp(verifyEmailModel.VerificationCode, user.EncryptedOtp))
            {
                user.OtpAttempts++;
                user.LastOtpAttempt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("Email verification failed: Invalid OTP for {Email}. Attempts: {Attempts}", 
                    verifyEmailModel.Email, user.OtpAttempts);
                return false;
            }

            // OTP is valid - mark email as verified and clear OTP data
            user.EmailVerified = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EncryptedOtp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            user.LastOtpAttempt = null;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Email verified successfully for {Email}", verifyEmailModel.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for {Email}", verifyEmailModel.Email);
            return false;
        }
    }

    public async Task<(bool Success, string ErrorMessage)> SendOtpAsync(SendOtpModel sendOtpModel)
    {
        try
        {
            _logger.LogInformation("Sending OTP for {Email}", sendOtpModel.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == sendOtpModel.Email);

            if (user == null)
            {
                var errorMsg = $"User not found for email: {sendOtpModel.Email}";
                _logger.LogWarning("Send OTP failed: {ErrorMsg}", errorMsg);
                return (false, errorMsg);
            }

            // Generate OTP
            var otp = _otpService.GenerateOtp();
            var encryptedOtp = _otpService.EncryptOtp(otp);
            
            // Set OTP expiry (15 minutes from now)
            var otpExpiry = DateTime.UtcNow.AddMinutes(15);

            // Update user with OTP data
            user.EncryptedOtp = encryptedOtp;
            user.OtpExpiryTime = otpExpiry;
            user.OtpAttempts = 0;
            user.LastOtpAttempt = null;

            await _context.SaveChangesAsync();

            // Send OTP via email
            var emailSent = await _emailService.SendOtpEmailAsync(sendOtpModel.Email, otp, user.FullName);
            
            if (!emailSent)
            {
                var errorMsg = "Failed to send OTP email. Please check email configuration.";
                _logger.LogError("Failed to send OTP email to {Email}", sendOtpModel.Email);
                return (false, errorMsg);
            }

            _logger.LogInformation("OTP sent successfully for {Email}", sendOtpModel.Email);
            return (true, "");
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error sending OTP: {ex.Message}";
            _logger.LogError(ex, "Error sending OTP for {Email}: {ErrorMsg}", sendOtpModel.Email, errorMsg);
            return (false, errorMsg);
        }
    }

    public async Task<(bool Success, string ErrorMessage)> ForgotPasswordAsync(ForgotPasswordRequestModel model)
    {
        // Reuse SendOtpAsync logic
        var sendOtpModel = new SendOtpModel { Email = model.Email };
        return await SendOtpAsync(sendOtpModel);
    }

    public async Task<(bool Success, string ErrorMessage)> ResetPasswordAsync(ResetPasswordRequestModel model)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return (false, "User not found.");

            // Check if OTP is expired
            if (_otpService.IsOtpExpired(user.OtpExpiryTime))
                return (false, "OTP expired. Please request a new one.");

            // Check OTP attempts limit (max 5 attempts)
            if (user.OtpAttempts >= 5)
                return (false, "Too many invalid OTP attempts. Please request a new one.");

            // Verify OTP
            if (string.IsNullOrEmpty(user.EncryptedOtp) || !_otpService.VerifyOtp(model.Otp, user.EncryptedOtp))
            {
                user.OtpAttempts++;
                user.LastOtpAttempt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return (false, "Invalid OTP.");
            }

            // OTP is valid - update password and clear OTP data
            user.Password = HashPassword(model.NewPassword);
            user.EncryptedOtp = null;
            user.OtpExpiryTime = null;
            user.OtpAttempts = 0;
            user.LastOtpAttempt = null;
            await _context.SaveChangesAsync();
            return (true, "Password reset successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for {Email}", model.Email);
            return (false, $"Error during password reset: {ex.Message}");
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: new[] {
                new Claim(ClaimTypes.Name, user.Username), 
                new Claim(ClaimTypes.Role, user.Role), 
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) 
            },
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
