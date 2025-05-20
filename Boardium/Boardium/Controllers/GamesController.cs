using Microsoft.AspNetCore.Mvc;

namespace Boardium.Controllers {
    public class GamesController : Controller {
        public IActionResult Index() {
            return View();
        }

        public IActionResult BoardGame(int id) {
            return View();
        }
    }
}
