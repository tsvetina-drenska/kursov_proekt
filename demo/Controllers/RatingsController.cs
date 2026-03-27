using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using catalog.Entities;
using catalog.Services;

namespace catalog.Controllers;

[Authorize]
public class RatingController : Controller
{
    private readonly IMovieService _movieService;
    private readonly IBookService _bookService;

    public RatingController(IMovieService movieService, IBookService bookService)
    {
        _movieService = movieService;
        _bookService = bookService;
    }

    // POST: /Rating/AddMovieRating
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddMovieRating(int movieId, int value, string? comment)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var movie = _movieService.GetById(movieId);

        if (movie == null)
        {
            return NotFound();
        }

        // Проверка дали вече е оценявал
        var existingRating = movie.Ratings.FirstOrDefault(r => r.UserId == userId);
        if (existingRating != null)
        {
            TempData["ErrorMessage"] = "Вече сте оценили този филм!";
            return RedirectToAction("Details", "Movie", new { id = movieId });
        }

        // Създаване на оценката
        var rating = new Rating
        {
            Value = value,
            Comment = comment,
            UserId = userId,
            MovieId = movieId,
            CreatedAt = DateTime.Now
        };

        movie.Ratings.Add(rating);

        TempData["SuccessMessage"] = "Благодарим за оценката!";
        return RedirectToAction("Details", "Movie", new { id = movieId });
    }

    // POST: /Rating/AddBookRating
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddBookRating(int bookId, int value, string? comment)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = _bookService.GetById(bookId);

        if (book == null)
        {
            return NotFound();
        }

        var existingRating = book.Ratings.FirstOrDefault(r => r.UserId == userId);
        if (existingRating != null)
        {
            TempData["ErrorMessage"] = "Вече сте оценили тази книга!";
            return RedirectToAction("Details", "Book", new { id = bookId });
        }

        var rating = new Rating
        {
            Value = value,
            Comment = comment,
            UserId = userId,
            BookId = bookId,
            CreatedAt = DateTime.Now
        };

        book.Ratings.Add(rating);

        TempData["SuccessMessage"] = "Благодарим за оценката!";
        return RedirectToAction("Details", "Book", new { id = bookId });
    }

    // POST: /Rating/DeleteRating/ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteRating(int ratingId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Търсим във филмите
        var allMovies = _movieService.GetAll();
        foreach (var movie in allMovies)
        {
            var rating = movie.Ratings.FirstOrDefault(r => r.Id == ratingId && r.UserId == userId);
            if (rating != null)
            {
                movie.Ratings.Remove(rating);
                TempData["SuccessMessage"] = "Оценката беше премахната.";
                return RedirectToAction("Details", "Movie", new { id = movie.Id });
            }
        }

        // Търсим в книгите
        var allBooks = _bookService.GetAll();
        foreach (var book in allBooks)
        {
            var rating = book.Ratings.FirstOrDefault(r => r.Id == ratingId && r.UserId == userId);
            if (rating != null)
            {
                book.Ratings.Remove(rating);
                TempData["SuccessMessage"] = "Оценката беше премахната.";
                return RedirectToAction("Details", "Book", new { id = book.Id });
            }
        }

        TempData["ErrorMessage"] = "Оценката не беше намерена.";
        return RedirectToAction("Index", "Home");
    }
}