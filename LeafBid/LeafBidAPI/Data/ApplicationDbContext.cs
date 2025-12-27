using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Toolbelt.ComponentModel.DataAnnotations;

namespace LeafBidAPI.Data;

// EF-Core exposes collections as DbSet<T>, which implements IQueryable<T> and thus can utilize LINQ.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<AuctionProduct> AuctionProducts { get; set; }
    public DbSet<AuctionSale> AuctionSales { get; set; }
    public DbSet<AuctionSaleProduct> AuctionSaleProducts { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<RegisteredProduct> RegisteredProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.BuildDecimalColumnTypeFromAnnotations();

        // AuctionProduct (join table with payload)
        modelBuilder.Entity<AuctionProduct>()
            .HasKey(ap => new { ap.AuctionId, ap.RegisteredProductId });

        modelBuilder.Entity<AuctionProduct>()
            .HasOne(ap => ap.Auction)
            .WithMany() // or .WithMany(a => a.AuctionProduct) if you add the collection
            .HasForeignKey(ap => ap.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuctionProduct>()
            .HasOne(ap => ap.RegisteredProduct)
            .WithMany() // or .WithMany(p => p.AuctionProduct)
            .HasForeignKey(ap => ap.RegisteredProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuctionSale -> Auction
        modelBuilder.Entity<AuctionSale>()
            .HasOne(s => s.Auction)
            .WithMany()
            .HasForeignKey(s => s.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuctionSale -> User
        modelBuilder.Entity<AuctionSale>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuctionSaleProduct -> AuctionSale
        modelBuilder.Entity<AuctionSaleProduct>()
            .HasOne(sp => sp.AuctionSale)
            .WithMany()
            .HasForeignKey(sp => sp.AuctionSaleId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuctionSaleProduct -> Product
        modelBuilder.Entity<AuctionSaleProduct>()
            .HasOne(sp => sp.Product)
            .WithMany()
            .HasForeignKey(sp => sp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Auction -> User (auctioneer)
        modelBuilder.Entity<Auction>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Registered Products -> User (provider)
        modelBuilder.Entity<RegisteredProduct>()
            .HasOne(rp => rp.User)
            .WithMany()
            .HasForeignKey(rp => rp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Registered Products -> Product
        modelBuilder.Entity<RegisteredProduct>()
            .HasOne(rp => rp.Product)
            .WithMany()
            .HasForeignKey(rp => rp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}