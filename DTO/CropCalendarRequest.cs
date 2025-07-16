namespace AgriSmartAPI.DTO;

public class CropCalendarRequest
{
    public string CropName { get; set; }
    public string Location { get; set; }
    public DateTime PlantingDate { get; set; }
}
