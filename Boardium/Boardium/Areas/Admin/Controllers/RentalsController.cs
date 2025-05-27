using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Boardium.Data;
using Boardium.Models.Rental;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RentalsController : Controller
    {
        private readonly BoardiumContext _context;

        public RentalsController(BoardiumContext context)
        {
            _context = context;
        }

        // GET: Admin/Rentals
        public async Task<IActionResult> Index()
        {
            var boardiumContext = _context.Rentals.Include(r => r.ApplicationUser).Include(r => r.GameCopy);
            return View(await boardiumContext.ToListAsync());
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
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
