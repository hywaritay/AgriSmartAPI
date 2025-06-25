using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/resources")]
[ApiController]
//[Authorize]
public class ResourceManagementController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public ResourceManagementController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("farmsize")]
    public async Task<ActionResult<FarmSize>> CalculateFarmSize([FromBody] FarmSize farmSize)
    {
        _context.FarmSizes.Add(farmSize);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFarmSize), new { id = farmSize.Id }, farmSize);
    }

    [HttpGet("farmsize/{id}")]
    public async Task<ActionResult<FarmSize>> GetFarmSize(int id)
    {
        var farmSize = await _context.FarmSizes.FindAsync(id);
        if (farmSize == null) return NotFound();
        return Ok(farmSize);
    }

    [HttpPost("watermanagement")]
    public async Task<ActionResult<WaterManagement>> CreateWaterManagement([FromBody] WaterManagement waterManagement)
    {
        waterManagement.IrrigationSchedule = "Irrigate every 3 days";
        waterManagement.ConservationTechniques = "Use mulching";
        _context.WaterManagements.Add(waterManagement);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWaterManagement), new { id = waterManagement.Id }, waterManagement);
    }

    [HttpGet("watermanagement/{id}")]
    public async Task<ActionResult<WaterManagement>> GetWaterManagement(int id)
    {
        var waterManagement = await _context.WaterManagements.FindAsync(id);
        if (waterManagement == null) return NotFound();
        return Ok(waterManagement);
    }

    [HttpPost("fertilizerpesticide")]
    public async Task<ActionResult<FertilizerPesticideCalculation>> CalculateFertilizerPesticide([FromBody] FertilizerPesticideCalculation calculation)
    {
        calculation.FertilizerAmount = calculation.FarmSize * 2.5;
        calculation.PesticideAmount = calculation.FarmSize * 1.5;
        _context.FertilizerPesticideCalculations.Add(calculation);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFertilizerPesticide), new { id = calculation.Id }, calculation);
    }

    [HttpGet("fertilizerpesticide/{id}")]
    public async Task<ActionResult<FertilizerPesticideCalculation>> GetFertilizerPesticide(int id)
    {
        var calculation = await _context.FertilizerPesticideCalculations.FindAsync(id);
        if (calculation == null) return NotFound();
        return Ok(calculation);
    }
}
