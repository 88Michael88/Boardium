using Boardium.Models.Game;
using Boardium.Models.Inventory;
using Boardium.Models.Rental;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Data;

public class BoardiumContext : DbContext
{
    public BoardiumContext(DbContextOptions<BoardiumContext> options) : base(options)
    {
    }

    public DbSet<Models.Game.Game> Games { get; set; }
    public DbSet<Models.Game.Publisher> Publishers { get; set; }
    public DbSet<Models.Game.GameCategory> GameCategories { get; set; }
    public DbSet<Models.Rental.Rental> Rentals { get; set; }
    public DbSet<Models.Inventory.GameCopy> GameCopies { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Game.Game>()
            .HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Categories)
            .WithMany(gc => gc.Games);
        modelBuilder.Entity<Rental>()
            .HasOne(r => r.ApplicationUser)
            .WithMany(u => u.Rentals)
            .HasForeignKey(r => r.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Rental>()
            .HasOne(r => r.GameCopy)
            .WithMany(gc => gc.Rentals)
            .HasForeignKey(r => r.GameCopyId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<GameCopy>()
            .HasOne(gc=>gc.Game)
            .WithMany(g=>g.GameCopies)
            .HasForeignKey(gc=>gc.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}