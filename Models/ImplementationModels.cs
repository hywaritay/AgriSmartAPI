namespace AgriSmartAPI.Models;

public class Partnership
{
    public int Id { get; set; }
    public string PartnerName { get; set; }
    public string PartnerType { get; set; }
    public string SupportType { get; set; }
}

public class LocalizationContent
{
    public int Id { get; set; }
    public string Region { get; set; }
    public string CropType { get; set; }
    public Dictionary<string, string> LocalizedContent { get; set; } = new Dictionary<string, string>();
}

public class FarmerTrainingSession
{
    public int Id { get; set; }
    public string Location { get; set; }
    public DateTime SessionDate { get; set; }
    public string Topic { get; set; }
    public string PartnerId { get; set; }
}