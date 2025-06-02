namespace Boardium.Models.Rental;

public enum RentalStatus
{
    WaitingForAcceptance,
    WaitingForPickup,
    InUse,
    Returned,
    Overdue,
    Canceled,
}