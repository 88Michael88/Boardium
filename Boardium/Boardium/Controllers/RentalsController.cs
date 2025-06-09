using Boardium.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Boardium.Data;
using System.Security.Claims;
using Boardium.Models.Rental;
using Boardium.HelperFuncs;
using DinkToPdf.Contracts;
using DinkToPdf;
using Boardium.PDFTemplates;
using Boardium.Services.ControllerServices;

namespace Boardium.Controllers {
    public class RentalsController : Controller {
        private readonly RentalService _service;
        private DateBetweenChecker _dateBetweenChecker;

        public RentalsController(RentalService service, DateBetweenChecker dateBetweenChecker, PickupCodeGenerator pickupCodeGenerator) {
            _service = service;
            _dateBetweenChecker = dateBetweenChecker;
        }

        [HttpGet("Rentals/")]
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Index(int GameID, int GameCopyID) {
            var gameCopyDetail = await _service.GetGameCopyDetailsAsync(GameID, GameCopyID);
            if (gameCopyDetail == null)
                return NotFound();

            return View(gameCopyDetail);
        }

        [HttpPost("Rentals/Confirm")]
        [Authorize(Roles = "Admin,Employee,User")]
        public async Task<IActionResult> Confirm(int GameID, int GameCopyID, DateTime DesiredBorrowDate, DateTime DesiredDueDate) {
            if (DesiredBorrowDate < DateTime.Now.Date || DesiredDueDate < DateTime.Now || DesiredDueDate < DesiredBorrowDate) // Basic Date confirmation.
                return RedirectToAction(nameof(Index), new { GameID = GameID, GameCopyID = GameCopyID });

            var rentalFee = await _service.GetRentalFee(GameID, GameCopyID);
            if (rentalFee == null) return NotFound();

            var gameBorrowInfo = await _service.GetBorrowInfoAsync(GameID, GameCopyID, DesiredBorrowDate, DesiredDueDate);

            if (gameBorrowInfo != null) { // Thorough Date confirmation
                if (_dateBetweenChecker.DateIsBetweenDates(DesiredBorrowDate, gameBorrowInfo) || _dateBetweenChecker.DateIsBetweenDates(DesiredDueDate, gameBorrowInfo)) {
                    return RedirectToAction(nameof(Index), new { GameID = GameID, GameCopyID = GameCopyID });
                }
            }

            string? userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null) return NotFound();

            var newRental = await _service.GetRentalConfirmationAsync((string)userID, GameID, GameCopyID, DesiredBorrowDate, DesiredDueDate, (decimal)rentalFee);

            return View(newRental);
        }

        [Authorize]
        [HttpGet("Rentals/MyRentals")]
        public async Task<IActionResult> MyRentals() {
            string? userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null) return NotFound();

            var rentals = await _service.GetMyRentalInfoAsync(userID);

            return View(rentals);
        }

        public async Task<IActionResult> DownloadPDF(int PickupCode) {
            string? userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userID == null) return NotFound();

            var pdf = await _service.GeneratePDFAsync((string)userID, PickupCode);

            return File(pdf, "application/pdf", $"Rental_{PickupCode}.pdf");
        }

    }
}
