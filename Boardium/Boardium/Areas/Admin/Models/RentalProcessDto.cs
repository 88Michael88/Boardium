using Boardium.Models.Rental;

public class RentalProcessDto
{
    public int Id { get; set; }
    public string GameTitle { get; set; }
    public string InventoryNumber { get; set; }
    public string CustomerEmail { get; set; }
    public RentalStatus Status { get; set; }
    public DateTime RentedAt { get; set; }
}