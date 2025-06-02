using Boardium.Models.Rental;
using Riok.Mapperly.Abstractions;

namespace Boardium.Areas.Admin.Mappers;

public class RentalMapper
{
    public RentalProcessDto Map(Rental rental)
    {
        return new RentalProcessDto
        {
            Id = rental.Id,
            GameTitle = rental.GameCopy?.Game?.Title,
            InventoryNumber = rental.GameCopy?.InventoryNumber,
            CustomerEmail = rental.ApplicationUser?.Email,
            Status = rental.Status,
            RentedAt = rental.RentedAt
        };
    }
}