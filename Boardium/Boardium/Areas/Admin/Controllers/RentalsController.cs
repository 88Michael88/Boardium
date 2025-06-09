using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Boardium.Areas.Admin.Mappers;
using Boardium.Areas.Admin.Services;
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
        private IRentalsService _rentalsService;
        private ILogger<RentalsController> _logger;

        public RentalsController(IRentalsService rentalsService, ILogger<RentalsController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rentalsService = rentalsService?? throw new ArgumentNullException(nameof(rentalsService));    
        }

        // GET: Admin/Rentals
        public async Task<IActionResult> Index()
        {
            var rentals = await  _rentalsService.GetAllRentalsAsync();
            return View(rentals);
        }

        public async Task<IActionResult> ProcessIndex(int? status)
        {
            var vm = await _rentalsService.ProcessIndex(status);
            return View(vm);
        }

        // GET: Admin/Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _rentalsService.GetRentalByIdAsync(id.Value);

            return View(rental);
        }

        public async Task<IActionResult> Pickup(int? pickupCode)
        {
            if (pickupCode == null)
            {
                _logger.LogError("Pickup code is null");
                return View(); // bez przekazywania null do widoku
            }

            var rentalId = await _rentalsService.GetRentalIdByPickupCodeAsync(pickupCode);

            if (rentalId != null)
            {
                return RedirectToAction(nameof(Process), new { id = rentalId.Value });
            }

            ViewBag.Error = "Nie znaleziono wypożyczenia dla podanego kodu.";
            return View();
        }


        public async Task<IActionResult> Process(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _rentalsService.GetRentalByIdAsync(id.Value);
            if (rental == null)
            {
                return NotFound();
            }

            ViewBag.RentalStatus = _rentalsService.GetRentalStatusSelectList(rental.Status);
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
                ViewBag.RentalStatus = _rentalsService.GetRentalStatusSelectList(rental.Status);
                var fallbackRental = await _rentalsService.GetRentalByIdAsync(rental.Id);
                return View(fallbackRental);
            }

            var (success, updatedRental, sendMail) = await _rentalsService.ProcessRentalAsync(rental);
            if (!success)
                return NotFound();

            if (sendMail && updatedRental != null)
            {
                await _rentalsService.SendConfirmationEmailIfNeededAsync(updatedRental);
            }

            return RedirectToAction(nameof(ProcessIndex));
        }

        // GET: Admin/Rentals/Create
        public IActionResult Create()
        {
            ViewData["ApplicationUserId"] = _rentalsService.GetUsersSelectList();
            ViewData["GameCopyId"] = _rentalsService.GetGameCopiesSelectList();
            return View();
        }

        // POST: Admin/Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,GameCopyId,ApplicationUserId,PickupCode,RentedAt,DueDate,ReturnedAt,Status,Notes,RentalFee,LateFee,DamageFee,PaidFee")]
            Rental rental)
        {
            if (ModelState.IsValid)
            {
                var success = await _rentalsService.CreateRentalAsync(rental);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Wystąpił błąd podczas zapisu danych.");
            }

            ViewData["ApplicationUserId"] = _rentalsService.GetUsersSelectList(rental.ApplicationUserId);
            ViewData["GameCopyId"] = _rentalsService.GetGameCopiesSelectList(rental.GameCopyId);

            return View(rental);
        }

        // GET: Admin/Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _rentalsService.GetRentalByIdAsync(id.Value);
            if (rental == null)
            {
                return NotFound();
            }

            ViewData["ApplicationUserId"] = _rentalsService.GetUsersSelectList(rental.ApplicationUserId);
            ViewData["GameCopyId"] = _rentalsService.GetGameCopiesSelectList(rental.GameCopyId);
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
                var updated = await _rentalsService.UpdateRentalAsync(rental);
                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index), "Rentals", new { area = "Admin" });
            }

            ViewData["ApplicationUserId"] = _rentalsService.GetUsersSelectList(rental.ApplicationUserId);
            ViewData["GameCopyId"] = _rentalsService.GetGameCopiesSelectList(rental.GameCopyId);

            return View(rental);
        
        }

        // GET: Admin/Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _rentalsService.GetRentalByIdAsync(id.Value);
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
            await _rentalsService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        
    }
}