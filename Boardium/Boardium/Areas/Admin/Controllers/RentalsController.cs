using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boardium.Areas.Admin.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Rental;
using Boardium.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ZXing.QrCode.Internal;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class RentalsController : Controller
    {
        private ILogger<RentalsController> _logger;
        private readonly BoardiumContext _context;
        private readonly RentalMapper _rentalMapper;
        private readonly EmailService _emailService;

        public RentalsController(BoardiumContext context, ILogger<RentalsController> logger, RentalMapper rentalMapper, EmailService emailService)
        {
            _logger = logger;
            _context = context;
            _rentalMapper = rentalMapper;
            _emailService = emailService;
        }

        // GET: Admin/Rentals
        public async Task<IActionResult> Index()
        {
            var boardiumContext = _context.Rentals.Include(r => r.ApplicationUser).Include(r => r.GameCopy);
            return View(await boardiumContext.ToListAsync());
        }
        public async Task<IActionResult> ProcessIndex(int? status)
        {
            var query = _context.Rentals
                .Include(r => r.GameCopy)
                .ThenInclude(gc => gc.Game)
                .Include(r => r.ApplicationUser)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => (int)r.Status == status.Value);
            }

            var rentals = await query.ToListAsync();

            var rentalDtos = rentals.Select(r => _rentalMapper.Map(r)).ToList();

            var vm = new RentalProcessIndexViewModel
            {
                Rentals = rentalDtos,
                SelectedStatus = status
            };

            return View(vm);
        }
        // GET: Admin/Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.ApplicationUser)
                .Include(r => r.GameCopy)
                .ThenInclude(gc=> gc.Game)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }
            return View(rental);
        }

        public async Task<IActionResult> Pickup(int? pickupCode)
        {
            if (pickupCode == null)
            {
                _logger.Log(LogLevel.Error, "Pickup code is null");
                return View(pickupCode);
            }
            var rentalId = await _context.Rentals
                .Where(r => r.PickupCode == pickupCode)
                .Select(r => (int?)r.Id)
                .FirstOrDefaultAsync();
            Console.WriteLine(rentalId);
            if (rentalId != null)
            {
                return RedirectToAction(nameof(Process), new { id = rentalId });
            }
            ViewBag.Error = "Nie znaleziono wypożyczenia dla podanego kodu.";
            return View(pickupCode);
        }
        public async Task<IActionResult> Process(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var rental = await _context.Rentals
                .Include(r=> r.ApplicationUser)
                .Include(r => r.GameCopy)
                    .ThenInclude(gc => gc.Game)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewBag.RentalStatus = new SelectList(
                Enum.GetValues(typeof(RentalStatus)).Cast<RentalStatus>()
                    .Select(s => new { Id = s, Name = s.ToString() }), 
                "Id", 
                "Name",
                rental.Status);
            return View(rental);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(Rental rental)
        {
            ModelState.Remove("Notes");
            ModelState.Remove("ApplicationUserId");

            if (rental.Notes == null)
                rental.Notes = string.Empty;

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for rental {RentalId}.", rental.Id);

                ViewBag.RentalStatus = new SelectList(
                    Enum.GetValues(typeof(RentalStatus)).Cast<RentalStatus>()
                        .Select(s => new { Id = s, Name = s.ToString() }),
                    "Id", "Name", rental.Status);

                var rentalFromDbFallback = await _context.Rentals
                    .Include(r => r.ApplicationUser)
                    .Include(r => r.GameCopy)
                    .ThenInclude(gc => gc.Game)
                    .FirstOrDefaultAsync(r => r.Id == rental.Id);

                return View(rentalFromDbFallback);
            }

            var rentalFromDb = await _context.Rentals
                .FirstOrDefaultAsync(r => r.Id == rental.Id);

            if (rentalFromDb == null)
                return NotFound();

            rentalFromDb.RentedAt = rental.RentedAt;
            rentalFromDb.DueDate = rental.DueDate;
            rentalFromDb.ReturnedAt = rental.ReturnedAt;
            rentalFromDb.Status = rental.Status;
            rentalFromDb.Notes = rental.Notes;
            rentalFromDb.RentalFee = rental.RentalFee;
            rentalFromDb.LateFee = rental.LateFee;
            rentalFromDb.DamageFee = rental.DamageFee;
            rentalFromDb.PaidFee = rental.PaidFee;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ProcessIndex));
        }

        // GET: Admin/Rentals/Create
        public IActionResult Create()
        {
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["GameCopyId"] = new SelectList(_context.GameCopies, "Id", "InventoryNumber");
            return View();
        }

        // POST: Admin/Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GameCopyId,ApplicationUserId,PickupCode,RentedAt,DueDate,ReturnedAt,Status,Notes,RentalFee,LateFee,DamageFee,PaidFee")] Rental rental)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", rental.ApplicationUserId);
            ViewData["GameCopyId"] = new SelectList(_context.GameCopies, "Id", "InventoryNumber", rental.GameCopyId);
            return View(rental);
        }

        // GET: Admin/Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", rental.ApplicationUserId);
            ViewData["GameCopyId"] = new SelectList(_context.GameCopies, "Id", "InventoryNumber", rental.GameCopyId);
            return View(rental);
        }

        // POST: Admin/Rentals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GameCopyId,ApplicationUserId,PickupCode,RentedAt,DueDate,ReturnedAt,Status,Notes,RentalFee,LateFee,DamageFee,PaidFee")] Rental rental)
        {
            if (id != rental.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.Id))
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
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "Id", rental.ApplicationUserId);
            ViewData["GameCopyId"] = new SelectList(_context.GameCopies, "Id", "InventoryNumber", rental.GameCopyId);
            return View(rental);
        }

        // GET: Admin/Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.ApplicationUser)
                .Include(r => r.GameCopy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Admin/Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.Id == id);
        }
    }
}
