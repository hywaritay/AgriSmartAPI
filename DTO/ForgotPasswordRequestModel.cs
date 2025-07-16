using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.DTO;

public class ForgotPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
} 