using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/training")]
[ApiController]
//[Authorize]
public class TrainingController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public TrainingController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("content")]
    public async Task<ActionResult<TrainingContent>> AddTrainingContent([FromBody] TrainingContent content)
    {
        _context.TrainingContents.Add(content);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTrainingContent), new { id = content.Id }, content);
    }

    [HttpGet("content/{id}")]
    public async Task<ActionResult<TrainingContent>> GetTrainingContent(int id)
    {
        var content = await _context.TrainingContents.FindAsync(id);
        if (content == null) return NotFound();
        return Ok(content);
    }

    [HttpPost("fieldschool")]
    public async Task<ActionResult<VirtualFieldSchool>> CreateFieldSchool([FromBody] VirtualFieldSchool fieldSchool)
    {
        _context.VirtualFieldSchools.Add(fieldSchool);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFieldSchool), new { id = fieldSchool.Id }, fieldSchool);
    }

    [HttpGet("fieldschool/{id}")]
    public async Task<ActionResult<VirtualFieldSchool>> GetFieldSchool(int id)
    {
        var fieldSchool = await _context.VirtualFieldSchools.FindAsync(id);
        if (fieldSchool == null) return NotFound();
        return Ok(fieldSchool);
    }
}
