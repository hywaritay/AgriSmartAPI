using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using AgriSmartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _authService.Login(loginModel);
        return Ok(new { Token = token, username = loginModel.Username, status = 1 });
    }
}
