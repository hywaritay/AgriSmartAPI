using System.Text.Json.Serialization;

namespace AgriSmartAPI.DTO;

public class SoilPredictionResult
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("soil_type")]
    public string? SoilType { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("recommended_crops")]
    public List<string>? RecommendedCrops { get; set; }
}
