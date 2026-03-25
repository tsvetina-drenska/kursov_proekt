using Microsoft.AspNetCore.Mvc;
using catalog.Entities;
using catalog.Services;


namespace catalog.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieService _movieService;
        public MovieController()
        {
            _movieService = new MovieService();
        }
        
        // GET: /Movie
        public IActionResult Index()
        {
            var movies = _movieService.GetAll();
            return View(movies);
        }
        
        // GET: /Movie/Details/ID
        public IActionResult Details(int id)
        {
            var movie = _movieService.GetById(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);

        }

        // GET: /Movie/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: /Movie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                _movieService.Add(movie);
                return RedirectToAction(nameof(Index));
            }

            return View(movie);

        }

        // GET: /Movie/Delete/ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Movie movie)
        {
            if (ModelState.IsValid)
            {
                _movieService.Update(movie);
                return RedirectToAction(nameof(Index));

            }

            return View(movie);
        }

        // POST: /Movie/Delete/ID
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public IActionResult DeleteConfirmed(int id)
        {
            _movieService.Delete(id);
            return RedirectToAction(nameof(Index));

        }

    }

}


