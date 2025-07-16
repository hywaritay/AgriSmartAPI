using AgriSmartAPI.Data;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgriSmartAPI.Controllers;

[Route("api/market")]
[ApiController]
//[Authorize]
public class MarketAccessController : ControllerBase
{
    private readonly AgriSmartContext _context;

    public MarketAccessController(AgriSmartContext context)
    {
        _context = context;
    }

    [HttpGet("prices/{cropName}")]
    public async Task<ActionResult<MarketPrice>> GetMarketPrice(string cropName)
    {
        var price = await _context.MarketPrices.FirstOrDefaultAsync(p => p.CropName == cropName);
        if (price == null)
        {
            price = new MarketPrice
            {
                CropName = cropName,
                MarketLocation = "Local Market",
                Price = 50.0,
                LastUpdated = DateTime.Now
            };
            _context.MarketPrices.Add(price);
            await _context.SaveChangesAsync();
        }
        return Ok(price);
    }

    [HttpPost("listing")]
    public async Task<ActionResult<MarketplaceListing>> CreateListing([FromBody] MarketplaceListing listing)
    {
        _context.MarketplaceListings.Add(listing);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetListing), new { id = listing.Id }, listing);
    }

    [HttpGet("listing/{id}")]
    public async Task<ActionResult<MarketplaceListing>> GetListing(int id)
    {
        var listing = await _context.MarketplaceListings.FindAsync(id);
        if (listing == null) return NotFound();
        return Ok(listing);
    }

    [HttpPost("groupsale")]
    public async Task<ActionResult<GroupSale>> CreateGroupSale([FromBody] GroupSale groupSale)
    {
        _context.GroupSales.Add(groupSale);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGroupSale), new { id = groupSale.Id }, groupSale);
    }

    [HttpGet("groupsale/{id}")]
    public async Task<ActionResult<GroupSale>> GetGroupSale(int id)
    {
        var groupSale = await _context.GroupSales.FindAsync(id);
        if (groupSale == null) return NotFound();
        return Ok(groupSale);
    }
}
