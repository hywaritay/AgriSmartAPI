using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriSmartAPI.Controllers;

[Route("api/inputsupply")]
[ApiController]
//[Authorize]
public class InputSupplyController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public InputSupplyController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpPost("supplier")]
    public async Task<ActionResult<InputSupplier>> AddSupplier([FromBody] InputSupplier supplier)
    {
        _context.InputSuppliers.Add(supplier);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
    }

    [HttpGet("supplier/{id}")]
    public async Task<ActionResult<InputSupplier>> GetSupplier(int id)
    {
        var supplier = await _context.InputSuppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        return Ok(supplier);
    }

    [HttpPost("order")]
    public async Task<ActionResult<InputOrder>> PlaceOrder([FromBody] InputOrder order)
    {
        order.Status = "Pending";
        _context.InputOrders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("order/{id}")]
    public async Task<ActionResult<InputOrder>> GetOrder(int id)
    {
        var order = await _context.InputOrders.FindAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }
}