using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Models;

public class ForumPost
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AuthorName { get; set; }

    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    // CSV of tags like: "maize,irrigation,soil"
    [MaxLength(1000)]
    public string? Tags { get; set; }

    public int Likes { get; set; } = 0;

    public int Views { get; set; } = 0;

    public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
}