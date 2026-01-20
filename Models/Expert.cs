using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Models;

public class Expert
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Specialty { get; set; }

    // 0.0 - 5.0
    public double Rating { get; set; } = 4.5;

    // Number of answers provided in community
    public int Answers { get; set; } = 0;

    // CSV of badges like: "Top Contributor,Soil Specialist"
    [MaxLength(1000)]
    public string? Badges { get; set; }

}