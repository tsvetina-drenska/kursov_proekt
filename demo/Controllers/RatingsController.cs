using catalog.Data;
using catalog.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace catalog.Controllers;

[Authorize]
public class RatingController : Controller
{
    private readonly ApplicationDbContext _context;

    public RatingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /Rating/AddMovieRating
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMovieRating(int movieId, int value, string? comment)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Проверка дали вече е оценявал
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);

        if (existingRating != null)
        {
            TempData["ErrorMessage"] = "Вече сте оценили този филм!";
            return RedirectToAction("Details", "Movie", new { id = movieId });
        }

        // Създаване на нова оценка
        var rating = new Rating
        {
            Value = value,
            Comment = comment,
            UserId = userId,
            MovieId = movieId,
            CreatedAt = DateTime.Now
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Благодарим за оценката! ⭐";
        return RedirectToAction("Details", "Movie", new { id = movieId });
    }

    // POST: /Rating/AddBookRating
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBookRating(int bookId, int value, string? comment)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);

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

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Благодарим за оценката! ⭐";
        return RedirectToAction("Details", "Book", new { id = bookId });
    }

    // POST: /Rating/DeleteRating/ID
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRating(int ratingId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.Id == ratingId && r.UserId == userId);

        if (rating != null)
        {
            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Оценката беше премахната.";
        }
        else
        {
            TempData["ErrorMessage"] = "Оценката не беше намерена.";
        }

        return RedirectToAction("Index", "Home");
    }
}