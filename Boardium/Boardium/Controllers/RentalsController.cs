using Boardium.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Boardium.Data;

namespace Boardium.Controllers {
    public class RentalsController : Controller {
        private readonly BoardiumContext _context;
        private readonly ILogger<GamesController> _logger;
        
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Index(int GameID, int GameCopyID) {
            GameAvailableCopy[] gameCopies = await (from gc in _context.GameCopies
                                                    join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalsGroup
                                                    from rental in rentalsGroup.DefaultIfEmpty()
                                                    where gc.GameId == GameID && gc.Id == GameCopyID
                                                    select new GameAvailableCopy {
                                                        GameCopyID = gc.Id,
                                                        GameID = gc.GameId,
                                                        Condition = gc.Condition,
                                                        InventoryNumber = gc.InventoryNumber,
                                                        RentalFee = gc.RentalFee,
                                                        DueDate = rental.DueDate
                                                    }
                                                   ).ToArrayAsync();

            return View();
        }
    }
}
