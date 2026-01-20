using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriSmartAPI.Models;

public class Cart
{
    public int Id { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required, MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    public Product? Product { get; set; }

}