using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/implementation")]
[ApiController]
//[Authorize]
public class ImplementationController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public ImplementationController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("partnership")]
    public async Task<ActionResult<Partnership>> AddPartnership([FromBody] Partnership partnership)
    {
        _context.Partnerships.Add(partnership);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPartnership), new { id = partnership.Id }, partnership);
    }

    [HttpGet("partnership/{id}")]
    public async Task<ActionResult<Partnership>> GetPartnership(int id)
    {
        var partnership = await _context.Partnerships.FindAsync(id);
        if (partnership == null) return NotFound();
        return Ok(partnership);
    }

    [HttpPost("localization")]
    public async Task<ActionResult<LocalizationContent>> AddLocalization([FromBody] LocalizationContent localization)
    {
        _context.LocalizationContents.Add(localization);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLocalization), new { id = localization.Id }, localization);
    }

    [HttpGet("localization/{id}")]
    public async Task<ActionResult<string>> GetLocalization(int id)
    {
        var localization = await _context.LocalizationContents.FindAsync(id);
        if (localization == null) return NotFound();

        var culture = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name ?? "en-US";
        if (localization.LocalizedContent.TryGetValue(culture, out var content))
        {
            return Ok(content);
        }
        return Ok(localization.LocalizedContent["en-US"]);
    }

    [HttpPost("training")]
    public async Task<ActionResult<FarmerTrainingSession>> ScheduleTraining([FromBody] FarmerTrainingSession training)
    {
        _context.FarmerTrainingSessions.Add(training);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTrainingSession), new { id = training.Id }, training);
    }

    [HttpGet("training/{id}")]
    public async Task<ActionResult<FarmerTrainingSession>> GetTrainingSession(int id)
    {
        var training = await _context.FarmerTrainingSessions.FindAsync(id);
        if (training == null) return NotFound();
        return Ok(training);
    }
}
