using Boardium.Models.Inventory;

namespace Boardium.Models {

    public class GameAvailableCopyDetailsViewModel {
        public int GameCopyID { get; set; }
        public int GameID { get; set; }
        public string? Title {  get; set; }
        public string InventoryNumber { get; set; }
        public GameCondition Condition { get; set; }
        public decimal RentalFee { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public List<BorrowInfo>? FutureBorrows { get; set; }
        public string PathToImage { get; set; }
    }

    public class BorrowInfo {
        public DateTime RentDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
