using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Game;
using Boardium.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SkiaSharp;
using ZXing.SkiaSharp.Rendering;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class GameCopiesController : Controller
    {
        private readonly BoardiumContext _context;

        public GameCopiesController(BoardiumContext context)
        {
            _context = context;
        }


        // GET: Admin/GameCopies
        public async Task<IActionResult> Index(int? gameId, int page = 1)
        {
            var pageSize = 5;
            var query = _context.GameCopies
                .Include(gc => gc.Game)
                .AsQueryable();
            if (gameId.HasValue)
            {
                query = query.Where(gc => gc.GameId == gameId.Value);
            }

            int totalItems = await _context.GameCopies.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(await items.ToListAsync());
        }

        // GET: Admin/GameCopies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _context.GameCopies
                .Include(g => g.Game)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameCopy == null)
            {
                return NotFound();
            }

            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Create
        public IActionResult Create()
        {
            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title");
            ViewData["Condition"] = new SelectList(Enum.GetValues(typeof(GameCondition)).Cast<GameCondition>());
            return View();
        }

        // POST: Admin/GameCopies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GameId,IsAvailable,Condition,RentalFee")] GameCopy gameCopy)
        {
            ModelState.Remove("Game");
            ModelState.Remove("InventoryNumber");

            if (ModelState.IsValid)
            {
                var game = await _context.Games.FindAsync(gameCopy.GameId);
                if (game == null)
                {
                    ModelState.AddModelError("GameId", "Chosen game doesn't exist.");
                    ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", gameCopy.GameId);
                    return View(gameCopy);
                }

                gameCopy.InventoryNumber = GenerateInventoryNumber(game);
                _context.Add(gameCopy);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Model error: " + error.ErrorMessage);
                }
            }

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", gameCopy.GameId);
            ViewData["Condition"] = new SelectList(Enum.GetValues(typeof(GameCondition)).Cast<GameCondition>(),
                gameCopy.Condition);
            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _context.GameCopies.FindAsync(id);
            if (gameCopy == null)
            {
                return NotFound();
            }

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", gameCopy.GameId);
            ViewData["Condition"] = new SelectList(Enum.GetValues(typeof(GameCondition)).Cast<GameCondition>(),
                gameCopy.Condition);

            return View(gameCopy);
        }

        // POST: Admin/GameCopies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,GameId,InventoryNumber,IsAvailable,Condition,RentalFee")] GameCopy gameCopy)
        {
            if (id != gameCopy.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Game");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gameCopy);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameCopyExists(gameCopy.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {

            }

            ViewData["GameId"] = new SelectList(_context.Games, "Id", "Title", gameCopy.GameId);
            ViewData["Condition"] = new SelectList(Enum.GetValues(typeof(GameCondition)).Cast<GameCondition>(),
                gameCopy.Condition);

            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _context.GameCopies
                .Include(g => g.Game)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gameCopy == null)
            {
                return NotFound();
            }

            return View(gameCopy);
        }

        // POST: Admin/GameCopies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gameCopy = await _context.GameCopies.FindAsync(id);
            if (gameCopy != null)
            {
                _context.GameCopies.Remove(gameCopy);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Barcode(int id)
        {
            var gameCopy = await _context.GameCopies.FindAsync(id);
            if (gameCopy == null || string.IsNullOrWhiteSpace(gameCopy.InventoryNumber))
            {
                return NotFound();
            }

            var imageBytes = GenerateBarcode(gameCopy.InventoryNumber);
            return File(imageBytes, "image/png");
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

        public byte[] GenerateBarcode(string inventoryNumber)
        {
            var writer = new BarcodeWriter<SKBitmap>
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 100,
                    Width = 300,
                    Margin = 10
                },
                Renderer = new SKBitmapRenderer()
            };

            var bitmap = writer.Write(inventoryNumber);

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
