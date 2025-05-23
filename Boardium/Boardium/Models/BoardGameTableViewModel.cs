// This is for the Games Controller to load the pages with all the board games that we have.
// THIS IS NOT PART OF THE DATABASE!
using Boardium.Models.Game;

namespace Boardium.Models {
    public class BoardGameTableViewModel {
        public int CurrentPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public List<BoardGame> Games { get; set; }
    }

    public class BoardGame {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Description { get; set; }
        public string PathToImage { get; set; }
    }
}
