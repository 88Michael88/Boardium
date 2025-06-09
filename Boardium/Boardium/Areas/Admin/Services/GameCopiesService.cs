using Boardium.Areas.DTOs.GameCopies;
using Boardium.Data;
using Boardium.Models.Game;
using Boardium.Models.Inventory;
using Boardium.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Services;

public class GameCopiesService:IGameCopiesService
{
    private readonly BoardiumContext _context;
    
    private readonly BarcodeService _barcodeService;
    
    public GameCopiesService(BoardiumContext context, BarcodeService barcodeService)
    {
        _context = context;
        _barcodeService = barcodeService;
    }

  
    public async Task<(IEnumerable<GameCopy> Items, int TotalPages)> GetGameCopiesAsync(int? gameId, int page, int pageSize)
    {
        var query = _context.GameCopies
            .Include(gc => gc.Game)
            .AsQueryable();

        if (gameId.HasValue)
        {
            query = query.Where(gc => gc.GameId == gameId.Value);
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalPages);
    }

    public async Task<GameCopy?> GetGameCopyDetailsAsync(int id)
    {
        return await _context.GameCopies
            .Include(g => g.Game)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<GameCopy?> GetGameCopyByIdAsync(int id)
    {
        return await _context.GameCopies.FindAsync(id);
    }

    public async Task<string> GenerateInventoryNumberAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game == null) throw new ArgumentException("Game not found");

        string inventoryNumber = $"{game.Id}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        return inventoryNumber;
    }

    public async Task<bool> CreateGameCopyAsync(GameCopy gameCopy)
    {
        _context.GameCopies.Add(gameCopy);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<GameCopyResult> TryCreateGameCopyAsync(GameCopy gameCopy)
    {
        var game = await _context.Games.FindAsync(gameCopy.GameId);
        if (game == null)
        {
            return new GameCopyResult
            {
                Success = false,
                ErrorMessage = "Game not found."
            };
        }
        gameCopy.InventoryNumber = GenerateInventoryNumber(game);
        await CreateGameCopyAsync(gameCopy);
        return new GameCopyResult
        {
            Success = true
        };
    }
    public async Task<bool> UpdateGameCopyAsync(GameCopy gameCopy)
    {
        _context.GameCopies.Update(gameCopy);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteGameCopyAsync(int id)
    {
        var gameCopy = await GetGameCopyByIdAsync(id);
        if (gameCopy == null) return false;

        _context.GameCopies.Remove(gameCopy);
        return await _context.SaveChangesAsync() > 0;
    }
    public SelectList GetGameSelectList(int? selectedId = null)
    {
        var games = _context.Games.AsNoTracking().ToList();
        return new SelectList(games, "Id", "Title", selectedId);
    }
    public SelectList GetConditionSelectList(GameCondition? selected = null)
    {
        var values = Enum.GetValues(typeof(GameCondition)).Cast<GameCondition>();
        return new SelectList(values, selected);
    }
    public async Task<byte[]> GenerateBarcodeFromId(int gameCopyId)
    {
        var gameCopy = await GetGameCopyByIdAsync(gameCopyId);
        if (gameCopy == null || string.IsNullOrWhiteSpace(gameCopy.InventoryNumber))
        {
            return null;
        }
        var imageBytes = _barcodeService.GenerateBarcode(gameCopy.InventoryNumber);
        return imageBytes;
    }
    private bool GameCopyExists(int id)
    {
        return _context.GameCopies.Any(e => e.Id == id);
    }
    
    private string GenerateInventoryNumber(Game game)
    {
        var prefixLetters = new string(game.Title
                .Where(char.IsLetter)
                .Take(3)
                .ToArray())
            .ToUpper();

        if (prefixLetters.Length < 3)
            prefixLetters = prefixLetters.PadRight(3, 'X');
        var prefix = $"{prefixLetters}-{game.Id.ToString("D2")}";
        var existingCopies = _context.GameCopies
            .Where(gc => gc.GameId == game.Id && gc.InventoryNumber.StartsWith(prefix + "-"))
            .ToList();
        int maxNumber = existingCopies
            .Select(gc =>
            {
                var parts = gc.InventoryNumber.Split('-');
                if (parts.Length < 2) return 0;
                return int.TryParse(parts.Last(), out int num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();
        int newNumber = maxNumber + 1;
        string newNumberString = newNumber.ToString("D3");
        return $"{prefix}-{newNumberString}";

    }
    
}