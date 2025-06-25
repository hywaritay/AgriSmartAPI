namespace AgriSmartAPI.Models;

public class OrganicFarmingGuide
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string CertificationProcess { get; set; }
}

public class AgroforestryGuide
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Advice { get; set; }
    public string Benefits { get; set; }
}