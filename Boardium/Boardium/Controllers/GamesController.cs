using Microsoft.AspNetCore.Mvc;
using Boardium.Models;
using Boardium.Data;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Controllers {
    public class GamesController : Controller {
        private readonly BoardiumContext _context;
        private readonly ILogger<GamesController> _logger;

        public GamesController(BoardiumContext context, ILogger<GamesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult BoardGames() {
            return View();
        }

        public async Task<IActionResult> Index(int? page) {
            int pageSize = 10;
            int currentPage = page ?? 1;
            List<Models.Game.Game> games = await _context.Games
                                                         .OrderBy(g => g.Id)
                                                         .Skip((currentPage - 1) * pageSize)
                                                         .Take(pageSize)
                                                         .ToListAsync();
            int totalGames = await _context.Games
                                           .CountAsync();


            BoardGameViewModel model = new BoardGameViewModel {
                CurrentPage = currentPage,
                HasPreviousPage = currentPage > 1,
                HasNextPage = currentPage * pageSize < totalGames,
                Games = games.Select(g => new BoardGame {
                    Title = g.Title,
                    Publisher = "PublisherName",
                    Description = g.Description,
                    PathToImage = "ImagePath"
                }).ToList()
            };

            return View(model);
        }
    }
}
