using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation property (optional - allows navigation from Category to Products)
    public ICollection<Product> Products { get; set; } = new List<Product>();
}