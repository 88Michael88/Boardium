using Microsoft.AspNetCore.Mvc;
using Boardium.Models;
using Boardium.Data;
using Microsoft.EntityFrameworkCore;
using Boardium.Models.Game;

namespace Boardium.Controllers {
    public class GamesController : Controller {
        private readonly BoardiumContext _context;
        private readonly ILogger<GamesController> _logger;

        public GamesController(BoardiumContext context, ILogger<GamesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> BoardGame(int? gameIndex) {
            Game? game = await _context.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == gameIndex);

            if (game == null) return NotFound();

            string[] categories = game.Categories.Select(c => c.Name).ToArray();
            string[] pathsToImages = await _context.GameImages.Where(gi => gi.GameId == gameIndex).Select(gi => gi.ImagePath).ToArrayAsync();

            Publisher publisher = await _context.Publishers.Where(p => p.Id == game.PublisherId).FirstAsync();

            GameAvailableCopy[] gameCopies = await (from gc in _context.GameCopies
                                                   where !(from r in _context.Rentals
                                                           where r.ReturnedAt == null
                                                           select r.GameCopyId).Contains(gc.Id)
                                                   && gc.GameId == gameIndex
                                                   select new GameAvailableCopy {
                                                       Id = gc.Id,
                                                       Condition = gc.Condition,
                                                       InventoryNumber = gc.InventoryNumber,
                                                       RentalFee = gc.RentalFee
                                                   }
                                                   ).ToArrayAsync();

            BoardGameViewModel model = new BoardGameViewModel {
                Id = game.Id,
                Title = game.Title,
                Publisher = publisher,
                Description = game.Description,
                MinPlayers = game.MinPlayers,
                MaxPlayers = game.MaxPlayers,
                MinAge = game.MinAge,
                MaxAge = game.MaxAge,
                PlayingTimeMinutes = game.PlayingTimeMinutes,
                PathsToImages = pathsToImages,
                Categories = categories,
                GameCopies = gameCopies
            };

            return View(model);
        }

        public async Task<IActionResult> Index(int? page) {
            int pageSize = 10;
            int currentPage = page ?? 1;

            List<BoardGame> boardGames = await (from g in _context.Games
                                               join gi in _context.GameImages on g.Id equals gi.GameId
                                               join p in _context.Publishers on g.PublisherId equals p.Id
                                               where gi.IsCoverImage
                                               select new BoardGame {
                                                   Id = g.Id,
                                                   Title = g.Title,
                                                   Description = g.Description,
                                                   PathToImage = gi.ImagePath,
                                                   Publisher = p.Name
                                               }).ToListAsync();

            int totalGames = await _context.Games
                                           .CountAsync();

            BoardGameTableViewModel model = new BoardGameTableViewModel {
                CurrentPage = currentPage,
                HasPreviousPage = currentPage > 1,
                HasNextPage = currentPage * pageSize < totalGames,
                Games = boardGames.Select(g => new BoardGame {
                    Id = g.Id,
                    Title = g.Title,
                    Publisher = g.Publisher,
                    Description = g.Description,
                    PathToImage = g.PathToImage 
                }).ToList()
            };

            return View(model);
        }
    }
}