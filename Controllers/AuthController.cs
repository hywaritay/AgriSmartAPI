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
