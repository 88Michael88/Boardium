using Boardium.Areas.Admin.Mappers;
using Boardium.Data;
using Boardium.Models.Rental;
using Boardium.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Areas.Admin.Services;

public class RentalsService : IRentalsService
{
    private ILogger<IRentalsService> _logger;
    private readonly BoardiumContext _context;
    private readonly RentalMapper _rentalMapper;
    private readonly EmailService _emailService;
    private readonly QrCodeService _qrCodeService;

    public RentalsService(BoardiumContext context, ILogger<IRentalsService> logger, RentalMapper rentalMapper,
        EmailService emailService, QrCodeService qrCodeService)
    {
        _logger = logger;
        _context = context;
        _rentalMapper = rentalMapper;
        _emailService = emailService;
        _qrCodeService = qrCodeService;
    }

    public async Task<List<Rental>> GetAllRentalsAsync()
    {
        var rentals = await _context.Rentals.Include(r => r.ApplicationUser).Include(r => r.GameCopy).ToListAsync();
        return rentals;
    }
    public async Task<RentalProcessIndexViewModel> ProcessIndex(int? status)
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
        return vm;
    }
    public async Task<Rental> GetRentalByIdAsync(int id)
    {
        var rental = await _context.Rentals
            .Include(r => r.GameCopy)
            .ThenInclude(gc => gc.Game)
            .Include(r => r.ApplicationUser)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (rental == null)
        {
            _logger.LogWarning($"Rental with ID {id} not found.");
        }
        
        return rental;
    }
    public async Task<int?> GetRentalIdByPickupCodeAsync(int? pickupCode)
    {
        if (pickupCode == null) return null;

        return await _context.Rentals
            .Where(r => r.PickupCode == pickupCode)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync();
    }
    public SelectList GetRentalStatusSelectList(RentalStatus? selectedStatus = null)
    {
        var statuses = Enum.GetValues(typeof(RentalStatus))
            .Cast<RentalStatus>()
            .Select(s => new { Id = s, Name = s.ToString() });

        return new SelectList(statuses, "Id", "Name", selectedStatus);
    }
    private bool RentalExists(int id)
    {
        return _context.Rentals.Any(e => e.Id == id);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var rental = await _context.Rentals.FindAsync(id);
        if (rental == null)
        {
            return false;
        }

        _context.Rentals.Remove(rental);
        return await _context.SaveChangesAsync() > 0;
    }
    public async Task<bool> UpdateRentalAsync(Rental rental)
    {
        try
        {
            _context.Update(rental);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RentalExistsAsync(rental.Id))
            {
                return false;
            }
            else
            {
                throw;
            }
        }
    }

    public async Task<bool> RentalExistsAsync(int id)
    {
        return await _context.Rentals.AnyAsync(e => e.Id == id);
    }
    public SelectList GetUsersSelectList(string selectedId = null)
    {
        return new SelectList(_context.Users, "Id", "Id", selectedId);
    }

    public SelectList GetGameCopiesSelectList(int? selectedId = null)
    {
        return new SelectList(_context.GameCopies, "Id", "InventoryNumber", selectedId);
    }
    public async Task<bool> CreateRentalAsync(Rental rental)
    {
        try
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating rental");
            return false;
        }
    }
    public async Task<(bool Success, Rental? UpdatedRental, bool SendMail)> ProcessRentalAsync(Rental rental)
    {
        var rentalFromDb = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rental.Id);
        if (rentalFromDb == null)
            return (false, null, false);

        bool sendMail = rental.Status == RentalStatus.WaitingForPickup && rentalFromDb.Status != RentalStatus.WaitingForPickup;

        rentalFromDb.RentedAt = rental.RentedAt;
        rentalFromDb.DueDate = rental.DueDate;
        rentalFromDb.ReturnedAt = rental.ReturnedAt;
        rentalFromDb.Status = rental.Status;
        rentalFromDb.Notes = rental.Notes ?? string.Empty;
        rentalFromDb.RentalFee = rental.RentalFee;
        rentalFromDb.LateFee = rental.LateFee;
        rentalFromDb.DamageFee = rental.DamageFee;
        rentalFromDb.PaidFee = rental.PaidFee;

        await _context.SaveChangesAsync();

        return (true, rentalFromDb, sendMail);
    }

    public async Task SendConfirmationEmailIfNeededAsync(Rental rental)
    {
        var rentalMail = await _context.Rentals
            .Include(r => r.ApplicationUser)
            .Include(r => r.GameCopy)
            .ThenInclude(gc => gc.Game)
            .FirstOrDefaultAsync(r => r.Id == rental.Id);

        if (rentalMail == null)
            return;

        try
        {
            var qrCodeBytes = _qrCodeService.GenerateQrCodeBytes(rentalMail.PickupCode.ToString());
            await _emailService.SendConfirmationAsync(
                rentalMail.ApplicationUser.Email,
                rentalMail.ApplicationUser.FirstName,
                rentalMail.GameCopy.Game.Title,
                qrCodeBytes,
                rentalMail.PickupCode.ToString()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending confirmation email for rental {RentalId}.", rentalMail.Id);
        }
    }
}