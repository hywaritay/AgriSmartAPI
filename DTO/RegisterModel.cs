using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.DTO;

public class RegisterModel
{
    [Required, StringLength(100)]
    public string FullName { get; set; }
    [Required, StringLength(50)]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    [Required, StringLength(100)]
    public string Email { get; set; }
    [Required, StringLength(50)]
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; } 
}

