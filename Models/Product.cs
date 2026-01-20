using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AgriSmartAPI.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(160)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    // Foreign key to Category
    public int? CategoryId { get; set; }

    [Precision(18, 2)]
    [Range(0, double.MaxValue)]
    public decimal PricePerKg { get; set; }

    [Precision(3, 2)]
    [Range(0, 5)]
    public decimal Rating { get; set; }

    [MaxLength(160)]
    public string? Location { get; set; }

    [MaxLength(160)]
    public string? Supplier { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation property
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
}