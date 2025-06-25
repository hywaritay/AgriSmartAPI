using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/analytics")]
[ApiController]
//[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public AnalyticsController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("productivity")]
    public async Task<ActionResult<FarmProductivity>> AddProductivityData([FromBody] FarmProductivity productivity)
    {
        productivity.RecordedDate = DateTime.Now;
        _context.FarmProductivities.Add(productivity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProductivityData), new { id = productivity.Id }, productivity);
    }

    [HttpGet("productivity/{id}")]
    public async Task<ActionResult<FarmProductivity>> GetProductivityData(int id)
    {
        var productivity = await _context.FarmProductivities.FindAsync(id);
        if (productivity == null) return NotFound();
        return Ok(productivity);
    }

    [HttpPost("sustainability")]
    public async Task<ActionResult<SustainabilityReport>> GenerateSustainabilityReport([FromBody] SustainabilityReport report)
    {
        report.GeneratedDate = DateTime.Now;
        report.Recommendations = "Adopt organic fertilizers";
        _context.SustainabilityReports.Add(report);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSustainabilityReport), new { id = report.Id }, report);
    }

    [HttpGet("sustainability/{id}")]
    public async Task<ActionResult<SustainabilityReport>> GetSustainabilityReport(int id)
    {
        var report = await _context.SustainabilityReports.FindAsync(id);
        if (report == null) return NotFound();
        return Ok(report);
    }
}
