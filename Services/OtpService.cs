using AgriSmartAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AgriSmartAPI.Services;

public class OtpService : IOtpService
{
    private readonly IConfiguration _configuration;
    private readonly string _encryptionKey;

    public OtpService(IConfiguration configuration)
    {
        _configuration = configuration;
        _encryptionKey = _configuration["OtpEncryption:Key"] ?? "YourSecretKey12345678901234567890123456789012";
        
        // Log key information (without exposing the actual key)
        var keyLength = Encoding.UTF8.GetBytes(_encryptionKey).Length;
        Console.WriteLine($"OtpService initialized with key length: {keyLength} bytes");
    }

    public string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // 6-digit OTP
    }

    public string EncryptOtp(string otp)
    {
        try
        {
            using var aes = Aes.Create();
            
            // Validate key length
            var keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
            if (keyBytes.Length != 16 && keyBytes.Length != 24 && keyBytes.Length != 32)
            {
                throw new ArgumentException($"Invalid key length: {keyBytes.Length} bytes. Key must be 16, 24, or 32 bytes for AES.");
            }
            
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var otpBytes = Encoding.UTF8.GetBytes(otp);
            var encryptedBytes = encryptor.TransformFinalBlock(otpBytes, 0, otpBytes.Length);

            // Combine IV and encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"OTP encryption failed: {ex.Message}", ex);
        }
    }

    public string DecryptOtp(string encryptedOtp)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedOtp);
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

        // Extract IV from the beginning of the encrypted data
        var iv = new byte[16];
        var encryptedData = new byte[encryptedBytes.Length - 16];
        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(encryptedBytes, 16, encryptedData, 0, encryptedData.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public bool VerifyOtp(string inputOtp, string encryptedStoredOtp)
    {
        try
        {
            var decryptedOtp = DecryptOtp(encryptedStoredOtp);
            return inputOtp == decryptedOtp;
        }
        catch
        {
            return false;
        }
    }

    public bool IsOtpExpired(DateTime? expiryTime)
    {
        if (!expiryTime.HasValue)
            return true;

        return DateTime.UtcNow > expiryTime.Value;
    }
} 