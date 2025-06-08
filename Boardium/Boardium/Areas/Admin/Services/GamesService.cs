using Boardium.Areas.Admin.Models;
using Boardium.Areas.DTOs.Games;
using Boardium.Data;
using Boardium.Models.Game;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Boardium.Areas.Admin.Services;

public class GamesService : IGamesService
{
    private readonly BoardiumContext _context;
    private readonly ILogger<IGamesService> _logger;

    public GamesService(BoardiumContext context, ILogger<IGamesService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<AutocompleteResult>> AutocompleteAsync(string term)
    {
        return await _context.Games
            .Where(g => g.Title.Contains(term))
            .Select(g => new AutocompleteResult { Id = g.Id, Text = g.Title })
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _context.Games.Include(g => g.Publisher).Include(g => g.Categories).ToListAsync();
    }

    public async Task<Game?> GetGameDetailsAsync(int id)
    {
        return await _context.Games
            .Include(g => g.Publisher)
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<SelectListItem>> GetAllCategoriesSelectListAsync()
    {
        return await _context.GameCategories
            .Select(gc => new SelectListItem
            {
                Value = gc.Id.ToString(),
                Text = gc.Name
            }).ToListAsync();
    }
    
    public async Task RemoveGameAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }
    private bool GameExists(int id)
    {
        return _context.Games.Any(e => e.Id == id);
    }
    public async Task<List<SelectListItem>> GetAllPublishersSelectListAsync()
    {
        return await _context.Publishers
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToListAsync();
    }

    public async Task<GameFormViewModel> PrepareGameFormViewModelAsync(int gameId)
    {
        var game = await _context.Games
            .Include(g => g.Categories)
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return null;
        }

        var selectedCategories = game.Categories.Select(c => c.Id).ToList();

        var allCategories = await GetAllCategoriesSelectListAsync();
        var allPublishers = await GetAllPublishersSelectListAsync();

        return new GameFormViewModel
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            MinPlayers = game.MinPlayers,
            MaxPlayers = game.MaxPlayers,
            MinAge = game.MinAge,
            MaxAge = game.MaxAge,
            PlayingTimeMinutes = game.PlayingTimeMinutes,
            PublisherId = game.PublisherId,
            SelectedCategoryIds = selectedCategories,
            AllCategories = allCategories,
            PublisherList = allPublishers,
            ExistingImagePaths = game.Images.Select(i => i.ImagePath).ToList() ?? new(),
            CoverImagePath = game.Images.FirstOrDefault(i => i.IsCoverImage)?.ImagePath
        };
    }

    public async Task<Game> GetOrCreateGameAsync(int gameId)
    {
        var game = await _context.Games
            .Include(g => g.Categories)
            .Include(g => g.Images)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            game = new Game();
            _context.Games.Add(game);
        }

        return game;
    }

    public async Task<List<SelectListItem>> GetAllCategoriesAsync()
    {
        return await _context.GameCategories
            .Select(gc => new SelectListItem
            {
                Value = gc.Id.ToString(),
                Text = gc.Name
            }).ToListAsync();
    }
    public async Task<List<SelectListItem>> GetAllPublishersAsync()
    {
        return await _context.Publishers
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToListAsync();
    }
    public async Task<Game> CreateOrUpdateGameAsync(GameFormViewModel vm)
    {
        var game = await GetOrCreateGameAsync(vm.Id);
        UpdateGameData(game, vm);
        await UpdateGameCategoriesAsync(game, vm.SelectedCategoryIds);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task PopulateFormViewDataAsync(GameFormViewModel vm)
    {
        vm.AllCategories = await _context.GameCategories
            .Select(gc => new SelectListItem
            {
                Value = gc.Id.ToString(),
                Text = gc.Name,
            }).ToListAsync();

        vm.PublisherList = await _context.Publishers
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToListAsync();
    }

    public async Task UpdateGameCategoriesAsync(Game game, List<int>? selectedCategoryIds)
    {
        game.Categories.Clear();
        foreach (var catId in selectedCategoryIds ?? new List<int>())
        {
            var cat = await _context.GameCategories.FindAsync(catId);
            if (cat != null)
            {
                game.Categories.Add(cat);
            }
        }
    }

    public async Task SaveUploadedImagesAsync(Game game, IEnumerable<IFormFile>? uploadedImages)
    {
        var formFiles = uploadedImages.ToList();
        if (formFiles?.Any() != true) return;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var gameFolder = Path.Combine("wwwroot", "pictures", game.Id.ToString());
        var lowResFolder = Path.Combine(gameFolder, "lowres");
        Directory.CreateDirectory(gameFolder);
        Directory.CreateDirectory(lowResFolder);

        foreach (var file in formFiles)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension)) continue;

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(gameFolder, fileName).Replace("\\", "/");
            var thumbnailPath = Path.Combine(lowResFolder, fileName).Replace("\\", "/");
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await GenerateThumbnailAsync(filePath, thumbnailPath);
                game.Images.Add(new GameImage
                {
                    ImagePath = fileName,
                    IsCoverImage = false
                });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, "Error writing image to images folder");
            }
        }
    }

    public async Task GenerateThumbnailAsync(string originalPath, string thumbnailPath, int width = 250)
    {
        using var image = await Image.LoadAsync(originalPath);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(width, 0)
        }));
        await image.SaveAsync(thumbnailPath);
    }

    private void UpdateGameData(Game game, GameFormViewModel vm)
    {
        game.Title = vm.Title;
        game.PublisherId = vm.PublisherId;
        game.MinPlayers = vm.MinPlayers;
        game.MaxPlayers = vm.MaxPlayers;
        game.MinAge = vm.MinAge;
        game.MaxAge = vm.MaxAge;
        game.PlayingTimeMinutes = vm.PlayingTimeMinutes;
        game.Description = vm.Description;
    }

    public async Task DeleteImagesAsync(Game game, IEnumerable<string>? imagePaths)
    {
        var enumerable = imagePaths.ToList();
        if (enumerable?.Any() != true) return;

        foreach (var imagePath in enumerable)
        {
            _logger.Log(LogLevel.Information, "Deleting image: {imagePath}", imagePath);
            var image = game.Images.FirstOrDefault(i => i.ImagePath == imagePath);
            if (image != null)
            {
                game.Images.Remove(image);
                _context.GameImages.Remove(image);

                var fullPath = Path.Combine("wwwroot", "pictures", game.Id.ToString(), imagePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }
    }

    public void UpdateCoverImage(Game game, string? coverImagePath)
    {
        foreach (var img in game.Images)
        {
            img.IsCoverImage = false;
        }

        var coverImage = game.Images.FirstOrDefault(i => i.ImagePath == coverImagePath);
        if (coverImage != null)
        {
            coverImage.IsCoverImage = true;
        }
    }

    public async Task<bool> SaveGameFormViewModel(GameFormViewModel vm)
    {
        var game = GetOrCreateGameAsync(vm.Id).Result;

        UpdateGameData(game, vm);

        UpdateGameCategoriesAsync(game, vm.SelectedCategoryIds).Wait();
        _context.SaveChangesAsync().Wait();

        SaveUploadedImagesAsync(game, vm.UploadedImages).Wait();
        DeleteImagesAsync(game, vm.DeletedImagePaths).Wait();
        UpdateCoverImage(game, vm.CoverImagePath);

        _context.SaveChangesAsync().Wait();
        return true;
    }
}