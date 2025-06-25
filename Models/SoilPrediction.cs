namespace AgriSmartAPI.Models;

public class SoilPrediction
{
    public int Id { get; set; }
    public string SoilType { get; set; }
    public string Description { get; set; }
    public string RecommendedCrops { get; set; } // Store as JSON or comma-separated string
    public DateTime PredictionDate { get; set; }
}
