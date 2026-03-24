using Microsoft.AspNetCore.Mvc;

namespace catalog.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
