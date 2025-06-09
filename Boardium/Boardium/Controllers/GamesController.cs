using Microsoft.AspNetCore.Mvc;
using Boardium.Models;
using Boardium.Data;
using Microsoft.EntityFrameworkCore;
using Boardium.Models.Game;
using Boardium.Services.ControllerServices;

namespace Boardium.Controllers {
    public class GamesController : Controller {
        private readonly BoardiumContext _context;
        private readonly GameService _service;
        private readonly ILogger<GamesController> _logger;

        public GamesController(ILogger<GamesController> logger, GameService service) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new AbandonedMutexException(nameof(service));
        }

        [HttpGet("Games/BoardGame")]
        public async Task<IActionResult> BoardGame(int? gameIndex) {
            if (gameIndex == null) return NotFound(); 
            var model = await _service.getBoardGameAsync((int)gameIndex);   
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpGet("Games/")]
        public async Task<IActionResult> Index(string? category, int? page) {
            int pageSize = 10;
            int currentPage = page ?? 1;
            currentPage = currentPage <= 0 ? 1 : currentPage;
            var model = await _service.getBoardGamesDataAsync(category, currentPage, pageSize);
            return View(model);
        }
    }
}