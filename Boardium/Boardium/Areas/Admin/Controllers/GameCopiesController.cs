using Boardium.Areas.Admin.Services;
using Boardium.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class GameCopiesController : Controller
    {
        private readonly IGameCopiesService _service;
 
        public GameCopiesController(IGameCopiesService service)
        {
           _service = service?? throw new ArgumentNullException(nameof(service));
        }


        // GET: Admin/GameCopies
        public async Task<IActionResult> Index(int? gameId, int page = 1)
        {
            var pageSize = 5;
            var (items, totalPages) = await _service.GetGameCopiesAsync(gameId, page, pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(items);
        }

        // GET: Admin/GameCopies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _service.GetGameCopyDetailsAsync(id.Value);
            if (gameCopy == null)
            {
                return NotFound();
            }

            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Create
        public IActionResult Create()
        {
            ViewData["GameId"] = _service.GetGameSelectList();
            ViewData["Condition"] = _service.GetConditionSelectList();
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
                var result = await _service.TryCreateGameCopyAsync(gameCopy);
                if (!result.Success)
                {
                    ModelState.AddModelError("GameId", "Chosen game doesn't exist.");
                    ViewData["GameId"] = _service.GetGameSelectList(gameCopy.GameId);
                    ViewData["Condition"] = _service.GetConditionSelectList(gameCopy.Condition);
                    return View(gameCopy);
                }
                return RedirectToAction(nameof(Index));
                
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("Model error: " + error.ErrorMessage);
            }
            ViewData["GameId"] = _service.GetGameSelectList(gameCopy.GameId);
            ViewData["Condition"] = _service.GetConditionSelectList(gameCopy.Condition);
            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _service.GetGameCopyDetailsAsync(id.Value);
            if (gameCopy == null)
            {
                return NotFound();
            }
            
            ViewData["GameId"] = _service.GetGameSelectList(gameCopy.GameId);
            ViewData["Condition"] = _service.GetConditionSelectList(gameCopy.Condition);

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
                    var result = await _service.UpdateGameCopyAsync(gameCopy);
                    if (!result)
                    {
                        return NotFound();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return StatusCode(500, "Concurrency confilict");
                }

                return RedirectToAction(nameof(Index));
            }
            

            ViewData["GameId"] = _service.GetGameSelectList(gameCopy.GameId);
            ViewData["Condition"] = _service.GetConditionSelectList(gameCopy.Condition);

            return View(gameCopy);
        }

        // GET: Admin/GameCopies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameCopy = await _service.GetGameCopyDetailsAsync(id.Value);
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
            await _service.DeleteGameCopyAsync(id);
            return RedirectToAction(nameof(Index));
        }
        
        
        [HttpGet]
        public async Task<IActionResult> Barcode(int id)
        {
            var imageBytesResult = await _service.GenerateBarcodeFromId(id);
            
            return File(imageBytesResult, "image/png");
        }
      
        


       
    }
}
