using Microsoft.AspNetCore.Mvc;

namespace Boardium.Controllers {
    public class RentalsController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
