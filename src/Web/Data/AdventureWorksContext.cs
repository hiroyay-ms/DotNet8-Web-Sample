using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data;

public class AdventureWorksContext : DbContext
{
    public AdventureWorksContext(DbContextOptions<AdventureWorksContext> options) : base(options)
    {
    }

    public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
    public DbSet<Product> Product => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SalesLT");

        modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");
        modelBuilder.Entity<Product>().ToTable("Product");
    }
}