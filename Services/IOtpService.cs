namespace AgriSmartAPI.Services.Interfaces;

public interface IOtpService
{
    string GenerateOtp();
    string EncryptOtp(string otp);
    string DecryptOtp(string encryptedOtp);
    bool VerifyOtp(string inputOtp, string encryptedStoredOtp);
    bool IsOtpExpired(DateTime? expiryTime);
} 