using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boardium.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class AdminHomeController : Controller
    {
        // GET: AdminHomeController
        [Area("Admin")]
        [Authorize(Roles = "Admin,Employee")]
        public ActionResult Index()
        {
            return View();
        }

    }
}
