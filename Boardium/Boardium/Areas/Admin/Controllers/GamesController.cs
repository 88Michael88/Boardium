using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boardium.Areas.Admin.Models;
using Boardium.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Game;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Boardium.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Area("Admin")]
    public class GamesController : Controller
    {
        private readonly IGamesService _gamesService;
        private readonly ILogger<GamesController> _logger;

        public GamesController(BoardiumContext context, ILogger<GamesController> logger, IGamesService gamesService)
        {
            _gamesService = gamesService ?? throw new ArgumentNullException(nameof(gamesService));
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            var results = await _gamesService.AutocompleteAsync(term);
            return Json(new { results });
        }

        // GET: Games
        public async Task<IActionResult> Index()
        {
            var games = await _gamesService.GetAllGamesAsync();
            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _gamesService.GetGameDetailsAsync(id.Value);
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
                AllCategories = await _gamesService.GetAllCategoriesAsync(),
                PublisherList = await _gamesService.GetAllPublishersAsync(),
                ExistingImagePaths = new List<string>()
            };
            return View("GameForm", vm);
        }


        // GET: Games/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vm = await _gamesService.PrepareGameFormViewModelAsync(id.Value);

            if (vm == null)
                return NotFound();

            return View("GameForm", vm);
        }


        [HttpPost]
        public async Task<IActionResult> Save(GameFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await _gamesService.PopulateFormViewDataAsync(vm);
                return View("GameForm");
            }

            await _gamesService.SaveGameFormViewModel(vm);
            return RedirectToAction(nameof(Index));
        }

        

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _gamesService.GetGameDetailsAsync(id.Value);
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
            await _gamesService.RemoveGameAsync(id);
            return RedirectToAction(nameof(Index));
        }
        
    }
}