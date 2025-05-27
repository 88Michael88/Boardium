using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boardium.Controllers {
    [Authorize(Roles = "Admin,Employee,User")]
    public class RentalsController : Controller {
        public IActionResult Index(string? id) {
            return View();
        }
    }
}
