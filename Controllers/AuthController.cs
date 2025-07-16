using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using AgriSmartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace AgriSmartAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] RegisterModel registerModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _authService.Register(registerModel);
        return CreatedAtAction(nameof(Login), new { username = user.Username }, user);
    }
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailModel verifyEmailModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.VerifyEmailAsync(verifyEmailModel);

        if (!result)
            return BadRequest(new { message = "Invalid verification code or email." });

        return Ok(new { message = "Email verified successfully." });
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpModel sendOtpModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errorMessage) = await _authService.SendOtpAsync(sendOtpModel);

        if (!success)
            return BadRequest(new { message = errorMessage });

        return Ok(new { message = "OTP sent successfully. Please check your email." });
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] SendOtpModel sendOtpModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Test email sending directly
            var testOtp = "123456";
            var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
            var (success, errorMessage) = await emailService.SendEmailAsync(sendOtpModel.Email, "Test Email", "This is a test email from AgriSmart API");

            if (success)
                return Ok(new { message = "Test email sent successfully!" });
            else
                return BadRequest(new { message = $"Test email failed: {errorMessage}" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Test email error: {ex.Message}" });
        }
    }

    [HttpPost("test-otp-generation")]
    public async Task<IActionResult> TestOtpGeneration()
    {
        try
        {
            var otpService = HttpContext.RequestServices.GetRequiredService<IOtpService>();
            
            // Test OTP generation and encryption
            var otp = otpService.GenerateOtp();
            var encryptedOtp = otpService.EncryptOtp(otp);
            var decryptedOtp = otpService.DecryptOtp(encryptedOtp);
            
            var isValid = otp == decryptedOtp;
            
            return Ok(new { 
                message = "OTP generation test completed",
                originalOtp = otp,
                encryptedOtp = encryptedOtp,
                decryptedOtp = decryptedOtp,
                isValid = isValid
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"OTP generation test failed: {ex.Message}" });
        }
    }

    [HttpPost("test-smtp-connection")]
    public async Task<IActionResult> TestSmtpConnection()
    {
        try
        {
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var smtpServer = configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = configuration["Email:SenderEmail"] ?? "";
            var senderPassword = configuration["Email:SenderPassword"] ?? "";

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword),
                Timeout = 10000
            };

            // Test connection without sending email
            await client.SendMailAsync(new MailMessage());

            return Ok(new { 
                message = "SMTP connection test successful",
                server = smtpServer,
                port = smtpPort,
                email = senderEmail
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"SMTP connection test failed: {ex.Message}" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errorMessage) = await _authService.ForgotPasswordAsync(model);
        if (!success)
            return BadRequest(new { message = errorMessage });
        return Ok(new { message = "OTP sent to your email for password reset." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, errorMessage) = await _authService.ResetPasswordAsync(model);
        if (!success)
            return BadRequest(new { message = errorMessage });
        return Ok(new { message = "Password reset successful." });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _authService.Login(loginModel);
        return Ok(new { Token = token, username = loginModel.Username, status = 1 });
    }
}
