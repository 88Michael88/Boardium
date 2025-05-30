using Boardium.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Boardium.Data;
using System.Security.Claims;
using Boardium.Models.Inventory;
using Boardium.Models.Rental;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Identity;

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
                       && gi.IsCoverImage == true
                       && rental.ReturnedAt == null
                       orderby rental.RentedAt
                       select new GameAvailableCopyDetailsViewModel {
                                                                    GameCopyID = gc.Id,
                                                                    GameID = gc.GameId,
                                                                    Title = g.Title,
                                                                    Condition = gc.Condition,
                                                                    InventoryNumber = gc.InventoryNumber,
                                                                    RentalFee = gc.RentalFee,
                                                                    BorrowDate = rental.RentedAt,
                                                                    DueDate = rental.DueDate,
                                                                    CurrentBorrows = (from r in _context.Rentals
                                                                                     where r.GameCopyId == GameCopyID
                                                                                     && r.ReturnedAt == null
                                                                                     && r.DueDate > DateTime.Now
                                                                                     orderby r.RentedAt
                                                                                     select new BorrowInfo {
                                                                                            BorrowDate = r.RentedAt,
                                                                                            DueDate = r.DueDate
                                                                                     }
                                                                                    ).ToList(),
                                                                    PathToImage = gi.ImagePath
                                                                    }).FirstOrDefaultAsync();

            if (gameCopyDetail == null)
                return NotFound();

            return View(gameCopyDetail);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Confirm(int GameID, int GameCopyID, DateTime DesiredBorrowDate, DateTime DesiredDueDate) {
            if (DesiredBorrowDate < DateTime.Now.Date || DesiredDueDate < DateTime.Now || DesiredDueDate < DesiredBorrowDate) // Basic Date confirmation.
                return RedirectToAction(nameof(Index), new { GameID = GameID, GameCopyID = GameCopyID });

            decimal? rentalFee = await (from gc in _context.GameCopies // Check if such a game exists.
                                        where gc.GameId == GameID && gc.Id == GameCopyID
                                        select gc.RentalFee
                                        ).FirstOrDefaultAsync();
            if (rentalFee == null) return NotFound();

            BorrowInfo[] gameBorrowInfo = await (from gc in _context.GameCopies // Get all the current rentals of this game copy.
                                                 join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalGroup
                                                 from rental in rentalGroup.DefaultIfEmpty() // LEFT JOIN
                                                 where rental.ReturnedAt == null
                                                 && rental.GameCopyId == GameCopyID
                                                 && rental.DueDate > DateTime.Now
                                                 select new BorrowInfo {
                                                     BorrowDate = rental.RentedAt,
                                                     DueDate = rental.DueDate,
                                                 }
                                          ).ToArrayAsync();

            if (gameBorrowInfo != null) { // Thorough Date confirmation
                if (dateIsBetweenDates(DesiredBorrowDate, gameBorrowInfo) || dateIsBetweenDates(DesiredDueDate, gameBorrowInfo))  {
                    return RedirectToAction(nameof(Index), new { GameID = GameID, GameCopyID = GameCopyID });
                }
            }

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

            // TODO:
            // Sent an email with the order.
            // Is there a transaction made automatically, so that a different user can't rent a board game at the same time?

            return View(newRental);
        }

        private bool dateIsBetweenDates(DateTime date, BorrowInfo[] borrowInfo) {
            foreach (BorrowInfo borrowRow in borrowInfo) {
                if (date <= borrowRow.DueDate.AddDays(1) && date >= borrowRow.BorrowDate.AddDays(-1)) {
                    return true;
                }
            }
            return false;
        }
        [Authorize]
        public async Task<IActionResult> MyRentals() {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rentals = await (from r in _context.Rentals
                                 join gc in _context.GameCopies on r.GameCopyId equals gc.Id
                                 join g in _context.Games on gc.GameId equals g.Id
                                 where r.ApplicationUserId == userId
                                 select new ShowUserRentalData {
                                     GameTitle = g.Title,
                                     InventoryNumber = gc.InventoryNumber,
                                     RentedAt = r.RentedAt,
                                     DueDate = r.DueDate,
                                     ReturnedAt = r.ReturnedAt,
                                     Status = r.Status,
                                     RentalFee = r.RentalFee,
                                     LateFee = r.LateFee,
                                     DamageFee = r.DamageFee,
                                     PaidFee = r.PaidFee,
                                     PickupCode = r.PickupCode
                                 }
                                ).ToListAsync();

            return View(rentals);
        }

    }
}
