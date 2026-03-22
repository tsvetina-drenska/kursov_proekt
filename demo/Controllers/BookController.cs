using Microsoft.AspNetCore.Mvc;

namespace catalog.Controllers
{
    public class BookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
