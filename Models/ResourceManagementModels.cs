namespace AgriSmartAPI.Models;

public class FarmSize
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public double AreaInAcres { get; set; }
}

public class WaterManagement
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string CropType { get; set; }
    public string IrrigationSchedule { get; set; }
    public string ConservationTechniques { get; set; }
}

public class FertilizerPesticideCalculation
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public double FarmSize { get; set; }
    public string CropType { get; set; }
    public double FertilizerAmount { get; set; }
    public double PesticideAmount { get; set; }
}
