using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace AgriSmartAPI.Models;

public class Tutorial
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    // e.g., "10:00"
    [MaxLength(16)]
    public string? Duration { get; set; }

    public int Views { get; set; } = 0;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? Difficulty { get; set; } // Beginner, Intermediate, Advanced

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(400)]
    public string? VideoPath { get; set; }
    
    [NotMapped]
    public IFormFile? VideoFile { get; set; }


}