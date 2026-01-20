using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.DTO;

public class PlantProductForm
{
    [Required, MaxLength(160)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PricePerKg { get; set; }

    [MaxLength(160)]
    public string? Location { get; set; }

    [MaxLength(160)]
    public string? Supplier { get; set; }

    [Required]
    public IFormFile? ImageFile { get; set; }
}
