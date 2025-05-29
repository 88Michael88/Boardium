using Boardium.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Boardium.Data;

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
                       where gc.GameId == GameID && gc.Id == GameCopyID 
                       select new GameAvailableCopyDetailsViewModel {
                                                                     GameCopyID = gc.Id,
                                                                       GameID = gc.GameId,
                                                                       Title = g.Title,
                                                                       Condition = gc.Condition,
                                                                       InventoryNumber = gc.InventoryNumber,
                                                                       RentalFee = gc.RentalFee,
                                                                       DueDate = rental.DueDate,
                                                                       LateFee = 0.0m,
                                                                       DamageFee = 0.0m,
                                                                       FutureBorrows = new List<BorrowInfo>(),
                                                                       PathToImage = "Catan_Example_Game.jpg" 
                       }).FirstOrDefaultAsync();

            if (gameCopyDetail == null)
                return NotFound();

            return View(gameCopyDetail);
        }
    }
}
