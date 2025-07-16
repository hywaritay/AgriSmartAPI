using System.ComponentModel.DataAnnotations;

namespace AgriSmartAPI.Models;

public class User
{
    public int Id { get; set; }
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
    public bool EmailVerified { get; set; } = false;
    public DateTime? EmailVerifiedAt { get; set; }
    
    // OTP fields
    public string? EncryptedOtp { get; set; }
    public DateTime? OtpExpiryTime { get; set; }
    public int OtpAttempts { get; set; } = 0;
    public DateTime? LastOtpAttempt { get; set; }
}