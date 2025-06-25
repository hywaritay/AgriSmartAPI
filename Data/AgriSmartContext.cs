using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriSmartAPI.Data;

public class AgriSmartContext : DbContext
{
    public AgriSmartContext(DbContextOptions<AgriSmartContext> options) : base(options)
    {  
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Crop> Crops { get; set; }
    public DbSet<PestDiagnosis> PestDiagnoses { get; set; }
    public DbSet<SoilPrediction> SoilPredictions { get; set; }
    public DbSet<FarmSize> FarmSizes { get; set; }
    public DbSet<WaterManagement> WaterManagements { get; set; }
    public DbSet<FertilizerPesticideCalculation> FertilizerPesticideCalculations { get; set; }
    public DbSet<MarketPrice> MarketPrices { get; set; }
    public DbSet<MarketplaceListing> MarketplaceListings { get; set; }
    public DbSet<GroupSale> GroupSales { get; set; }
    public DbSet<Microloan> Microloans { get; set; }
    public DbSet<CropInsurance> CropInsurances { get; set; }
    public DbSet<OrganicFarmingGuide> OrganicFarmingGuides { get; set; }
    public DbSet<AgroforestryGuide> AgroforestryGuides { get; set; }
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<ExpertQA> ExpertQAs { get; set; }
    public DbSet<TrainingContent> TrainingContents { get; set; }
    public DbSet<VirtualFieldSchool> VirtualFieldSchools { get; set; }
    public DbSet<InputSupplier> InputSuppliers { get; set; }
    public DbSet<InputOrder> InputOrders { get; set; }
    public DbSet<FarmProductivity> FarmProductivities { get; set; }
    public DbSet<SustainabilityReport> SustainabilityReports { get; set; }
    public DbSet<Partnership> Partnerships { get; set; }
    public DbSet<LocalizationContent> LocalizationContents { get; set; }
    public DbSet<FarmerTrainingSession> FarmerTrainingSessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalizationContent>()
            .Property(l => l.LocalizedContent)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null));
        
        modelBuilder.Entity<ForumPost>()
            .Property(f => f.Replies)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null));

        modelBuilder.Entity<InputSupplier>()
            .Property(i => i.Reviews)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null));
    }
}
