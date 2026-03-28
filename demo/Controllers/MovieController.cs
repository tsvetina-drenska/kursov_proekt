using Microsoft.AspNetCore.Mvc;
using catalog.Entities;
using catalog.Services;


namespace catalog.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieService _movieService;
        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
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
        public IActionResult Delete(int id)
        {
            var movie = _movieService.GetById(id);

            if (movie == null)
            {
                return NotFound();
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

        // GET: /Movie/Edit/ID
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _movieService.GetById(id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: /Movie/Edit/ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Намираме съществуващия филм
                var existingMovie = _movieService.GetById(id);
                if (existingMovie != null)
                {
                    // Обновяваме данните
                    existingMovie.Title = movie.Title;
                    existingMovie.Director = movie.Director;
                    existingMovie.Year = movie.Year;
                    existingMovie.Description = movie.Description;
                    existingMovie.ImageUrl = movie.ImageUrl;

                }

                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

    }

}


