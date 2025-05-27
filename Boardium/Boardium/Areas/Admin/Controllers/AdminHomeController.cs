using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Boardium.Areas.Admin.Controllers
{
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
