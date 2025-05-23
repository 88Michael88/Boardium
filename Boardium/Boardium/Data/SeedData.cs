using Boardium.Models.Auth;
using Boardium.Models.Game;
using Boardium.Models.Inventory;
using Boardium.Models.Rental;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Data;

public class SeedData
{
    private readonly BoardiumContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<SeedData> _logger;
    public SeedData(BoardiumContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<SeedData> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }
    public  async Task InitializeAsync()
    {
        _context.Database.Migrate();

        //Seeding Game Categories 
        if (!_context.GameCategories.Any())
        {
            _context.GameCategories.AddRange(
                new GameCategory { Name = "Karcianka" },
                new GameCategory { Name = "Strategia" },
                new GameCategory { Name = "Rodzinna" }
            );
            _context.SaveChanges();
        }

        //Seeding Publishers
        if (!_context.Publishers.Any())
        {
            _context.Publishers.AddRange(
                new Publisher { Name = "Rebel", Website = "https://www.rebel.pl" },
                new Publisher { Name = "Galakta", Website = "https://www.galakta.pl" },
                new Publisher { Name = "Catan Studio", Website = "https://www.catanstudio.com" }
            );
            _context.SaveChanges();
        }

        //Seeding Games
        if (!_context.Games.Any())
        {
            var publisher = _context.Publishers.First();
            var image = new GameImage
            {
                ImagePath = "Catan_Example_Game.jpg",
                IsCoverImage = true
            };
            _context.GameImages.Add(image);
            var game = new Game
            {
                Title = "Catan",
                Description = "Gra planszowa, w której gracze rywalizują o zasoby i budują osady.",
                Images = new List<GameImage> { image },
                MinPlayers = 3,
                MaxPlayers = 4,
                MinAge = 10,
                MaxAge = 99,
                PlayingTimeMinutes = 90,
                PublisherId = publisher.Id,
                Categories = new List<GameCategory>
                {
                    _context.GameCategories.First(g => g.Name == "Strategia"),
                    _context.GameCategories.First(g => g.Name == "Rodzinna")
                }
            };
            _context.Games.Add(game);
            _context.SaveChanges();
        }

        //Seeding Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        //Seed admin user
        var adminEmail = "admin@boardium.pl";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Admin"
            };
            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        //Seed regular user
        var userEmail = "user@boardium.pl";
        var regularUser = await _userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "User",
                LastName = "User"
            };
            var result = await _userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(regularUser, "User");
            }
        }

        //Seed Game Copies
        if (!_context.GameCopies.Any())
        {
            var game = _context.Games.First();
            _context.GameCopies.AddRange(
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
            _context.SaveChanges();
        }

        //Seed Rentals
        if (!_context.Rentals.Any())
        {
            var gameCopy = _context.GameCopies.First();
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user != null && gameCopy != null)
            {
                _context.Rentals.Add(new Rental
                {
                    GameCopyId = gameCopy.Id,
                    ApplicationUserId = user.Id,
                    RentedAt = DateTime.UtcNow.AddDays(-1),
                    DueDate = DateTime.Now.AddDays(10),
                    Status = RentalStatus.InUse,
                    PickupCode = 12345678,
                    RentalFee = gameCopy.RentalFee,
                    PaidFee = gameCopy.RentalFee,
                });
            }
            _context.SaveChanges();
        }
        _logger.Log(LogLevel.Information, "Seeding data finished");
    }
}