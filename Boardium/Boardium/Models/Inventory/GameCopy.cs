using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Boardium.Models.Inventory;

public class GameCopy
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game.Game Game { get; set; }
    [Required]
    public string InventoryNumber { get; set; }// barcode or  internal number

    public bool IsAvailable { get; set; } = true;
    public GameCondition Condition { get; set; }
    [Precision(10,2)]
    public decimal RentalFee { get; set; }
    public ICollection<Rental.Rental> Rentals { get; set; } = new List<Rental.Rental>();

}