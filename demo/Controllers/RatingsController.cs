using Microsoft.AspNetCore.Mvc;

namespace catalog.Controllers
{
    public class RatingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
