using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.DTO;

public class SendOtpModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
} 