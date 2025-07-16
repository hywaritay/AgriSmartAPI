using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/sustainablefarming")]
[ApiController]
//[Authorize]
public class SustainableFarmingController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public SustainableFarmingController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("organicguide")]
    public async Task<ActionResult<OrganicFarmingGuide>> CreateOrganicGuide([FromBody] OrganicFarmingGuide guide)
    {
        _context.OrganicFarmingGuides.Add(guide);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrganicGuide), new { id = guide.Id }, guide);
    }

    [HttpGet("organicguide/{id}")]
    public async Task<ActionResult<OrganicFarmingGuide>> GetOrganicGuide(int id)
    {
        var guide = await _context.OrganicFarmingGuides.FindAsync(id);
        if (guide == null) return NotFound();
        return Ok(guide);
    }

    [HttpPost("agroforestry")]
    public async Task<ActionResult<AgroforestryGuide>> CreateAgroforestryGuide([FromBody] AgroforestryGuide guide)
    {
        _context.AgroforestryGuides.Add(guide);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAgroforestryGuide), new { id = guide.Id }, guide);
    }

    [HttpGet("agroforestry/{id}")]
    public async Task<ActionResult<AgroforestryGuide>> GetAgroforestryGuide(int id)
    {
        var guide = await _context.AgroforestryGuides.FindAsync(id);
        if (guide == null) return NotFound();
        return Ok(guide);
    }
}
