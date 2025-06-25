namespace AgriSmartAPI.DTO;

public class PestDiagnosis
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public string ImageUrl { get; set; }
    public string Diagnosis { get; set; }
    public string TreatmentRecommendation { get; set; }
    public DateTime CreatedAt { get; set; }
}