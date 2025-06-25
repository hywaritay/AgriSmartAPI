namespace AgriSmartAPI.Models;

public class FarmProductivity
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public double CropYield { get; set; }
    public double InputUsage { get; set; }
    public double ProfitMargin { get; set; }
    public DateTime RecordedDate { get; set; }
}

public class SustainabilityReport
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string EnvironmentalImpact { get; set; }
    public string Recommendations { get; set; }
    public DateTime GeneratedDate { get; set; }
}