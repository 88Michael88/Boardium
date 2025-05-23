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


            BoardGameViewModel model = new BoardGameViewModel {
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
