namespace AgriSmartAPI.Models;

public class TrainingContent
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }
    public string Topic { get; set; }
}

public class VirtualFieldSchool
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime SessionDate { get; set; }
    public string Content { get; set; }
    public string FacilitatorId { get; set; }
}