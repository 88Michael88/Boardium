using Boardium.Models.Rental;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Boardium.Areas.Admin.Services;

public interface IRentalsService
{
    Task<List<Rental>> GetAllRentalsAsync();
    Task<RentalProcessIndexViewModel> ProcessIndex(int? status);
    Task<Rental> GetRentalByIdAsync(int id);
    Task<int?> GetRentalIdByPickupCodeAsync(int? pickupCode);
    SelectList GetRentalStatusSelectList(RentalStatus? selectedStatus = null);
    Task<bool> DeleteAsync(int id);
    Task<bool> RentalExistsAsync(int id);
    Task<bool> UpdateRentalAsync(Rental rental);
    SelectList GetUsersSelectList(string selectedId = null);
    SelectList GetGameCopiesSelectList(int? selectedId = null);
    Task<bool> CreateRentalAsync(Rental rental);
    Task<(bool Success, Rental? UpdatedRental, bool SendMail)> ProcessRentalAsync(Rental rental);
    Task SendConfirmationEmailIfNeededAsync(Rental rental);
}