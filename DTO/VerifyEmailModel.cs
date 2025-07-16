using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.DTO;

public class VerifyEmailModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string VerificationCode { get; set; }
} 