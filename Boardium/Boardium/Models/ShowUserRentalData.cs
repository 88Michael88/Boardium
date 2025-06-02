using Boardium.Models.Inventory;
using Boardium.Models.Rental;

namespace Boardium.Models {
    public class ShowUserRentalData {
        public string GameTitle {  get; set; }
        public string InventoryNumber { get; set; }
        public int PickupCode { get; set; }
        public DateTime RentedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public RentalStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal? RentalFee { get; set; } = 0;
        public decimal? LateFee { get; set; } = 0;
        public decimal? DamageFee { get; set; } = 0;
        public decimal PaidFee { get; set; } = 0;
    }
}
