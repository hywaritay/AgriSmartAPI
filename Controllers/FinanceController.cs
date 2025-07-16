using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/finance")]
[ApiController]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public FinanceController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("microloan")]
    public async Task<ActionResult<Microloan>> ApplyForMicroloan([FromBody] Microloan microloan)
    {
        _context.Microloans.Add(microloan);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMicroloan), new { id = microloan.Id }, microloan);
    }

    [HttpGet("microloan/{id}")]
    public async Task<ActionResult<Microloan>> GetMicroloan(int id)
    {
        var microloan = await _context.Microloans.FindAsync(id);
        if (microloan == null) return NotFound();
        return Ok(microloan);
    }

    [HttpPost("insurance")]
    public async Task<ActionResult<CropInsurance>> AddCropInsurance([FromBody] CropInsurance insurance)
    {
        _context.CropInsurances.Add(insurance);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCropInsurance), new { id = insurance.Id }, insurance);
    }

    [HttpGet("insurance/{id}")]
    public async Task<ActionResult<CropInsurance>> GetCropInsurance(int id)
    {
        var insurance = await _context.CropInsurances.FindAsync(id);
        if (insurance == null) return NotFound();
        return Ok(insurance);
    }
}
