using Boardium.Data;
using Boardium.Models;
using Boardium.Models.Game;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Services.ControllerServices {
    public class GameService {
        private readonly BoardiumContext _context;
        public GameService(BoardiumContext context) { 
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<BoardGameViewModel?> getBoardGameAsync(int gameIndex) {
            Game? game = await _context.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == gameIndex);

            if (game == null) return null;

            string[] categories = game.Categories.Select(c => c.Name).ToArray();
            string[] pathsToImages = await _context.GameImages.Where(gi => gi.GameId == gameIndex).Select(gi => gi.ImagePath).ToArrayAsync();

            Publisher publisher = await _context.Publishers.Where(p => p.Id == game.PublisherId).FirstAsync();


            var gameCopies = await (from r in _context.GameCopies
                                    where r.GameId == gameIndex
                                    select new GameAvailableCopy {
                                        Condition = r.Condition,
                                        GameCopyID = r.Id,
                                        GameID = r.GameId,
                                        InventoryNumber = r.InventoryNumber,
                                        RentalFee = r.RentalFee
                                    }).ToArrayAsync();


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

            return model;
        }

        public async Task<BoardGameTableViewModel> getBoardGamesDataAsync(string? category, int currentPage, int pageSize) {
            List<BoardGame> boardGames;
            if (string.IsNullOrEmpty(category)) {
                 boardGames = await (from g in _context.Games
                                                    join gi in _context.GameImages on g.Id equals gi.GameId
                                                    join p in _context.Publishers on g.PublisherId equals p.Id
                                                    where gi.IsCoverImage
                                                    select new BoardGame {
                                                        Id = g.Id,
                                                        Title = g.Title,
                                                        Description = g.Description,
                                                        PathToImage = gi.ImagePath,
                                                        Publisher = p.Name
                                                    })
                                                   .Skip((currentPage - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToListAsync();
            } else {
                 boardGames = await (from g in _context.Games
                                                    join gi in _context.GameImages on g.Id equals gi.GameId
                                                    join p in _context.Publishers on g.PublisherId equals p.Id
                                                    where gi.IsCoverImage
                                                    && g.Categories.Any(c => c.Name == category)
                                                    select new BoardGame {
                                                        Id = g.Id,
                                                        Title = g.Title,
                                                        Description = g.Description,
                                                        PathToImage = gi.ImagePath,
                                                        Publisher = p.Name
                                                    })
                                                   .Skip((currentPage - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToListAsync();
            }

            int totalGames = await _context.Games
                                           .CountAsync();

            BoardGameTableViewModel model = new BoardGameTableViewModel {
                Categories = await (from c in _context.GameCategories
                                    select c.Name).ToArrayAsync(),
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

            return model;
        }

    }
}
