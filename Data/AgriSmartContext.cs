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
   
    public DbSet<ForumPost> ForumPosts { get; set; } 
    public DbSet<ForumComment> ForumComments { get; set; } 
    public DbSet<Expert> Experts { get; set; } 
    public DbSet<Tutorial> Tutorials { get; set; } 
    public DbSet<Product> Products { get; set; } 
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ForumComment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasIndex(c => new { c.UserId, c.ProductId })
            .HasDatabaseName("IX_Cart_User_Product")
            .IsUnique(false);
        
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);



        
    }
}
