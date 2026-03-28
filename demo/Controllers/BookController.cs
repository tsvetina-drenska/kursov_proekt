using Microsoft.AspNetCore.Mvc;
using catalog.Entities;
using catalog.Services;

namespace catalog.Controllers 
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: /Book
        public IActionResult Index()
        {
            var books = _bookService.GetAll();
            return View(books);
        }

        // GET: /Book/Details/ID
        public IActionResult Details(int id)
        {
            var book = _bookService.GetById(id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: /Book/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _bookService.Add(book);
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // GET: /Book/Delete/ID
        public IActionResult Delete(int id)
        {
            var book = _bookService.GetById(id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Book/Delete/ID
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _bookService.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Book/Edit/ID
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _bookService.GetById(id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: /Book/Edit/ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Намираме съществуващата книга
                var existingBook = _bookService.GetById(id);
                if (existingBook != null)
                {
                    // Обновяваме данните
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Year = book.Year;
                    existingBook.Description = book.Description;
                    existingBook.ImageUrl = book.ImageUrl;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }



    }

}
