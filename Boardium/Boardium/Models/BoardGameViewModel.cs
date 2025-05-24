using Boardium.Models.Game;
using Boardium.Models.Inventory;

namespace Boardium.Models {
    public class BoardGameViewModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public Publisher Publisher{ get; set; }
        public string Description { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public int MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int PlayingTimeMinutes { get; set; }
        public string[] Categories { get; set; }
        public string[] PathsToImages { get; set; }
        public GameAvailableCopy[] GameCopies { get; set; }
    }

    public class GameAvailableCopy {
        public int Id { get; set; }
        public string InventoryNumber { get; set; }
        public GameCondition Condition { get; set; }
        public decimal RentalFee { get; set; }
    }
}
