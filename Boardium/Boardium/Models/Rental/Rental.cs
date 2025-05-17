using System.ComponentModel.DataAnnotations;
using Boardium.Models.Auth;
using Boardium.Models.Inventory;

namespace Boardium.Models.Rental;

public class Rental
{
   public int Id { get; set; }
   public int GameCopyId { get; set; }
   public GameCopy GameCopy { get; set; }
   [Required]
   public string ApplicationUserId { get; set; }
   public ApplicationUser ApplicationUser { get; set; }
   public int PickupCode { get; set; }
   
   [DataType(DataType.Date)]
   public DateTime RentedAt { get; set; }
   [DataType(DataType.Date)]
   public DateTime DueDate { get; set; }
   [DataType(DataType.Date)]
   public DateTime? ReturnedAt { get; set; }
   public RentalStatus Status { get; set; }
   [MaxLength(200)]
   public string Notes { get; set; } = string.Empty;
   public decimal? RentalFee { get; set; } = 0;
   public decimal? LateFee { get; set; } = 0;
   public decimal? DamageFee { get; set; } = 0;
   public decimal PaidFee { get; set; } = 0;
   
}