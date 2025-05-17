using Boardium.Models.Auth;
using Boardium.Models.Game;
using Boardium.Models.Inventory;
using Boardium.Models.Rental;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BoardiumContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.Migrate();

        //seeding Game Categories 
        if (!context.GameCategories.Any())
        {
            context.GameCategories.AddRange(
                new GameCategory { Name = "Karcianka" },
                new GameCategory { Name = "Strategia" },
                new GameCategory { Name = "Rodzinna" }
            );
            context.SaveChanges();
        }

        //seeding Publishers
        if (!context.Publishers.Any())
        {
            context.Publishers.AddRange(
                new Publisher { Name = "Rebel", Website = "https://www.rebel.pl" },
                new Publisher { Name = "Galakta", Website = "https://www.galakta.pl" },
                new Publisher { Name = "Catan Studio", Website = "https://www.catanstudio.com" }
            );
            context.SaveChanges();
        }

        //seeding Games
        if (!context.Games.Any())
        {
            var publisher = context.Publishers.First();
            var game = new Game
            {
                Title = "Catan",
                Description = "Gra planszowa, w której gracze rywalizują o zasoby i budują osady.",
                MinPlayers = 3,
                MaxPlayers = 4,
                MinAge = 10,
                MaxAge = 99,
                PlayingTimeMinutes = 90,
                PublisherId = publisher.Id,
                Categories = new List<GameCategory>
                {
                    context.GameCategories.First(g => g.Name == "Strategia"),
                    context.GameCategories.First(g => g.Name == "Rodzinna")
                }
            };
            context.Games.Add(game);
            context.SaveChanges();
        }

        //seeding Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        //Seed admin user
        var adminEmail = "admin@boardium.pl";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        //Seed regular user
        var userEmail = "user@boardium.pl";
        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = "user",
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "User",
                LastName = "User"
            };
            var result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        //Seed Game Copies
        if (!context.Games.Any())
        {
            var game = context.Games.First();
            context.GameCopies.AddRange(
                new GameCopy
                {
                    GameId = game.Id,
                    Condition = GameCondition.Good,
                    IsAvailable = true,
                    RentalFee = 10.00m,
                    InventoryNumber = "Cat-001"
                },
                new GameCopy
                {
                    GameId = game.Id,
                    Condition = GameCondition.Used,
                    IsAvailable = true,
                    RentalFee = 8.00m,
                    InventoryNumber = "Cat-002"
                }
            );
            context.SaveChanges();
        }
        //Seed Rentals
        if (!context.Rentals.Any())
        {
            var gameCopy = context.GameCopies.First();
            var user = await userManager.FindByEmailAsync(userEmail);
            if(user != null && gameCopy != null)
            {
                context.Rentals.Add(new Rental
                {
                    GameCopyId = gameCopy.Id,
                    ApplicationUserId = user.Id,
                    RentedAt = DateTime.UtcNow.AddDays(-1),
                    DueDate = DateTime.Now.AddDays(10),
                    Status = RentalStatus.Ongoing,
                    PickupCode = 12345678,
                    RentalFee = gameCopy.RentalFee,
                    PaidFee = gameCopy.RentalFee,
                });
            }
            
            context.SaveChanges();
        }
    }
}