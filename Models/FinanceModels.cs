namespace AgriSmartAPI.Models;

public class Microloan
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string InstitutionName { get; set; }
    public double LoanAmount { get; set; }
    public double InterestRate { get; set; }
    public string LoanPurpose { get; set; }
}

public class CropInsurance
{
    public int Id { get; set; }
    public string FarmerId { get; set; }
    public string InsuranceProvider { get; set; }
    public string CoverageDetails { get; set; }
    public double Premium { get; set; }
}