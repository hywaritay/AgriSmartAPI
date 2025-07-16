namespace AgriSmartAPI.Models;

public class Crop
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public DateTime PlantingDate { get; set; }
    public string? CareSchedule { get; set; }
    public string? HarvestSchedule { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; }
}