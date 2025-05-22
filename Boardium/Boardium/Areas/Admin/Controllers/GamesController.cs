using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boardium.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Game;
using Microsoft.AspNetCore.Authorization;

namespace Boardium.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Area("Admin")]
    public class GamesController : Controller
    {
        private readonly BoardiumContext _context;
        private readonly ILogger<GamesController> _logger;

        public GamesController(BoardiumContext context, ILogger<GamesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Games
        public async Task<IActionResult> Index()
        {
            var boardiumContext = _context.Games.Include(g => g.Publisher).Include(g => g.Categories);
            return View(await boardiumContext.ToListAsync());
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        public async Task<IActionResult> Create()
        {
            var vm = new GameFormViewModel
            {
                AllCategories = await _context.GameCategories.Select(gc => new SelectListItem
                {
                    Value = gc.Id.ToString(),
                    Text = gc.Name,
                }).ToListAsync(),
                PublisherList = await _context.Publishers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync(),
                ExistingImagePaths = new List<string>()
            };
            return View("GameForm",vm);
        }
        

        // GET: Games/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Categories).Include(g => g.Images).FirstOrDefaultAsync(g => g.Id == id);
                
            if (game == null)
            {
                return NotFound();
            }

            var selectetCategoriesId = game.Categories.Select(c => c.Id).ToList();

            var vm = new GameFormViewModel
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
                SelectedCategoryIds = selectetCategoriesId,
                AllCategories = await _context.GameCategories.Select(gc => new SelectListItem
                {
                    Value = gc.Id.ToString(),
                    Text = gc.Name,
                }).ToListAsync(),
                PublisherList = await _context.Publishers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync(),
                ExistingImagePaths = game.Images.Select(i => i.ImagePath).ToList()?? new(),
                //CoverImagePath = game.Images.FirstOrDefault(i => i.IsCoverImage)?.ImagePath

            };

            return View("GameForm", vm);
        }

     
    
        [HttpPost]
        public async Task<IActionResult> Save(GameFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllCategories = await _context.GameCategories.Select(gc => new SelectListItem
                {
                    Value = gc.Id.ToString(),
                    Text = gc.Name,
                }).ToListAsync();
                vm.PublisherList = await _context.Publishers
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.Name
                    })
                    .ToListAsync();
                
                return View("GameForm");
            }
            
            var game = await _context.Games
                .Include(g => g.Categories)
                .Include(g=> g.Images)
                .FirstOrDefaultAsync(g => g.Id == vm.Id);

            if (game == null)
            {
                game = new Game();
                _context.Games.Add(game);
            }
            game.Title = vm.Title;
            game.PublisherId = vm.PublisherId;
            game.MinPlayers = vm.MinPlayers;
            game.MaxPlayers = vm.MaxPlayers;
            game.MinAge = vm.MinAge;
            game.MaxAge = vm.MaxAge;
            game.PlayingTimeMinutes = vm.PlayingTimeMinutes;
            game.Description = vm.Description;
            game.Categories.Clear();
            foreach (var catId in vm.SelectedCategoryIds ?? new List<int>())
            {
                var cat = await _context.GameCategories.FindAsync(catId);
                if (cat != null)
                {
                    game.Categories.Add(cat);    
                }
                
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (vm.UploadedImages?.Any() == true)
            {
                var gameFolder = Path.Combine("wwwroot", "images", "games", game.Id.ToString());
                Directory.CreateDirectory(gameFolder);
                foreach (var file in vm.UploadedImages)
                {
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(gameFolder, fileName).Replace("\\", "/");
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        game.Images.Add(new GameImage
                        {
                            ImagePath = Path.Combine("images", "games", game.Id.ToString(), fileName).Replace("\\", "/"),
                            IsCoverImage = false
                        });
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, ex, "Error writing image to images folder");
                    }
                    
                }
            }
                _logger.Log(LogLevel.Information, $"Deleting images from images folder Count {vm.DeletedImagePaths?.Count}");
            if (vm.DeletedImagePaths?.Any() == true)
            {
                foreach (var imagePath in vm.DeletedImagePaths)
                {
                    _logger.Log(LogLevel.Information, "Deleting image: {imagePath}", imagePath);
                    var image = game.Images.FirstOrDefault(i => i.ImagePath == imagePath);
                    if (image != null)
                    {
                        game.Images.Remove(image);
                        _context.GameImages.Remove(image);
                        var fullPath = Path.Combine("wwwroot", imagePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Publisher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}