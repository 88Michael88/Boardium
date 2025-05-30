using Boardium.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Boardium.Data;
using System.Security.Claims;
using Boardium.Models.Inventory;
using Boardium.Models.Rental;
using Microsoft.VisualBasic;

namespace Boardium.Controllers {
    public class RentalsController : Controller {
        private readonly BoardiumContext _context;
        private readonly ILogger<GamesController> _logger;
        public RentalsController(BoardiumContext context, ILogger<GamesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Index(int GameID, int GameCopyID) {
            GameAvailableCopyDetailsViewModel? gameCopyDetail =
                await (from gc in _context.GameCopies
                       join g in _context.Games on gc.GameId equals g.Id
                       join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalGroup
                       from rental in rentalGroup.DefaultIfEmpty() // LEFT JOIN
                       join gi in _context.GameImages on gc.GameId equals gi.GameId
                       where gc.GameId == GameID && gc.Id == GameCopyID 
                       && gi.IsCoverImage
                       && rental.RentedAt == null
                       select new GameAvailableCopyDetailsViewModel {
                                                                     GameCopyID = gc.Id,
                                                                       GameID = gc.GameId,
                                                                       Title = g.Title,
                                                                       Condition = gc.Condition,
                                                                       InventoryNumber = gc.InventoryNumber,
                                                                       RentalFee = gc.RentalFee,
                                                                       BorrowDate = rental.RentedAt,
                                                                       DueDate = rental.DueDate,
                                                                       FutureBorrows = new List<BorrowInfo>(),
                                                                       PathToImage = gi.ImagePath
                       }).FirstOrDefaultAsync();

            if (gameCopyDetail == null)
                return NotFound();

            return View(gameCopyDetail);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Confirm(int GameID, int GameCopyID, DateTime DesiredBorrowDate, DateTime DesiredDueDate) {
            // TODO:
            // Confirm if such a time period is allowed.
            // If so then add the rental to the database.
            // Change the IsAvailable status of the GameCopy.
            decimal? rentalFee = await (from gc in _context.GameCopies
                                        where gc.GameId == GameID && gc.Id == GameCopyID
                                        select gc.RentalFee
                                        ).FirstOrDefaultAsync();
            if (rentalFee == null) return NotFound();

            GameDataBeforeRental[] gameCopy = await (from gc in _context.GameCopies
                                                   join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalGroup
                                                   from rental in rentalGroup.DefaultIfEmpty() // LEFT JOIN
                                                   where rental.ReturnedAt == null
                                                   && rental.GameCopyId == GameCopyID
                                                   select new GameDataBeforeRental {
                                                       RentDate = rental.RentedAt,
                                                       DueDate = rental.DueDate,
                                                   }
                                                    ).ToArrayAsync();

            if (gameCopy != null)
                gameCopy = gameCopy;

            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var newRental = new Rental {
                GameCopyId = GameCopyID,
                ApplicationUserId = userId,
                RentedAt = DesiredBorrowDate,
                DueDate = DesiredDueDate,
                ReturnedAt = null,
                Status = RentalStatus.WaitingForAcceptance, 
                Notes = "",
                RentalFee = rentalFee,
                LateFee = 0,
                DamageFee = 0,
                PaidFee = 0
            };

            _context.Rentals.Add(newRental);
            await _context.SaveChangesAsync();

            return View();
        }
    }
}
