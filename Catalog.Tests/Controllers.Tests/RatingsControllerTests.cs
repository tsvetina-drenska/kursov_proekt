using catalog.Controllers;
using catalog.Data;
using catalog.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Security.Claims;

namespace Catalog.Tests.ControllersTests;

[TestFixture]
public class RatingControllerTests
{
    private ApplicationDbContext _context;
    private RatingController _controller;
    private User _testUser;
    private Movie _testMovie;
    private Book _testBook;

    [SetUp]
    public void SetUp()
    {
        // Създаване на InMemory база данни
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Добавяне на тестов потребител
        _testUser = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed123",
            CreatedAt = DateTime.Now
        };

        // Добавяне на тестов филм
        _testMovie = new Movie
        {
            Id = 1,
            Title = "Test Movie",
            Director = "Test Director",
            Year = 2024,
            CreatedAt = DateTime.Now
        };

        // Добавяне на тестова книга
        _testBook = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = "Test Author",
            Year = 2024,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(_testUser);
        _context.Movies.Add(_testMovie);
        _context.Books.Add(_testBook);
        _context.SaveChanges();

        _controller = new RatingController(_context);

        // Настройване на HttpContext с логнат потребител
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUser.Id.ToString()),
            new Claim(ClaimTypes.Name, _testUser.Username)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Създаване на TempData
        _controller.TempData = new TempDataDictionary(httpContext, new NullTempDataProvider());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ========== ADD MOVIE RATING TESTS ==========

    [Test]
    public async Task AddMovieRating_ValidRating_AddsRatingAndRedirects()
    {
        // Arrange
        var value = 5;
        var comment = "Great movie!";

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, value, comment);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
        Assert.That(redirectResult.ControllerName, Is.EqualTo("Movie"));

        var rating = _context.Ratings.FirstOrDefault(r => r.MovieId == _testMovie.Id);
        Assert.That(rating, Is.Not.Null);
        Assert.That(rating.Value, Is.EqualTo(value));
        Assert.That(rating.Comment, Is.EqualTo(comment));
        Assert.That(rating.UserId, Is.EqualTo(_testUser.Id));
    }

    [Test]
    public async Task AddMovieRating_DuplicateRating_ShowsErrorAndRedirects()
    {
        // Arrange
        var existingRating = new Rating
        {
            Value = 4,
            Comment = "Already rated",
            UserId = _testUser.Id,
            MovieId = _testMovie.Id,
            CreatedAt = DateTime.Now
        };
        _context.Ratings.Add(existingRating);
        _context.SaveChanges();

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, 5, "Another rating");

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
        Assert.That(redirectResult.ControllerName, Is.EqualTo("Movie"));

        var ratings = _context.Ratings.Where(r => r.MovieId == _testMovie.Id).ToList();
        Assert.That(ratings.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task AddMovieRating_NonExistingMovie_ReturnsNotFound()
    {
        // Act
        var result = await _controller.AddMovieRating(999, 5, "Comment");

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // ========== ADD BOOK RATING TESTS ==========

    [Test]
    public async Task AddBookRating_ValidRating_AddsRatingAndRedirects()
    {
        // Arrange
        var value = 4;
        var comment = "Good book!";

        // Act
        var result = await _controller.AddBookRating(_testBook.Id, value, comment);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
        Assert.That(redirectResult.ControllerName, Is.EqualTo("Book"));

        var rating = _context.Ratings.FirstOrDefault(r => r.BookId == _testBook.Id);
        Assert.That(rating, Is.Not.Null);
        Assert.That(rating.Value, Is.EqualTo(value));
        Assert.That(rating.Comment, Is.EqualTo(comment));
        Assert.That(rating.UserId, Is.EqualTo(_testUser.Id));
    }

    [Test]
    public async Task AddBookRating_DuplicateRating_ShowsErrorAndRedirects()
    {
        // Arrange
        var existingRating = new Rating
        {
            Value = 3,
            Comment = "Already rated",
            UserId = _testUser.Id,
            BookId = _testBook.Id,
            CreatedAt = DateTime.Now
        };
        _context.Ratings.Add(existingRating);
        _context.SaveChanges();

        // Act
        var result = await _controller.AddBookRating(_testBook.Id, 5, "Another rating");

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
        Assert.That(redirectResult.ControllerName, Is.EqualTo("Book"));

        var ratings = _context.Ratings.Where(r => r.BookId == _testBook.Id).ToList();
        Assert.That(ratings.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task AddBookRating_NonExistingBook_ReturnsNotFound()
    {
        // Act
        var result = await _controller.AddBookRating(999, 4, "Comment");

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    // ========== DELETE RATING TESTS ==========

    [Test]
    public async Task DeleteRating_ExistingRating_RemovesRatingAndRedirects()
    {
        // Arrange
        var rating = new Rating
        {
            Value = 5,
            Comment = "To be deleted",
            UserId = _testUser.Id,
            MovieId = _testMovie.Id,
            CreatedAt = DateTime.Now
        };
        _context.Ratings.Add(rating);
        _context.SaveChanges();

        var ratingId = rating.Id;

        // Act
        var result = await _controller.DeleteRating(ratingId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);

        var deletedRating = _context.Ratings.Find(ratingId);
        Assert.That(deletedRating, Is.Null);
    }

    [Test]
    public async Task DeleteRating_NonExistingRating_ReturnsRedirect()
    {
        // Act
        var result = await _controller.DeleteRating(999);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
    }

    // ========== BOUNDARY TESTS ==========

    [Test]
    public async Task AddMovieRating_WithEmptyComment_StillAddsRating()
    {
        // Arrange
        var value = 3;
        string? comment = null;

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, value, comment);

        // Assert
        var rating = _context.Ratings.FirstOrDefault(r => r.MovieId == _testMovie.Id);
        Assert.That(rating, Is.Not.Null);
    }

    [Test]
    public async Task AddMovieRating_MinimumValue_AddsRating()
    {
        // Arrange
        var value = 1;

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, value, null);

        // Assert
        var rating = _context.Ratings.FirstOrDefault(r => r.MovieId == _testMovie.Id);
        Assert.That(rating, Is.Not.Null);
        Assert.That(rating.Value, Is.EqualTo(1));
    }

    [Test]
    public async Task AddMovieRating_MaximumValue_AddsRating()
    {
        // Arrange
        var value = 5;

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, value, null);

        // Assert
        var rating = _context.Ratings.FirstOrDefault(r => r.MovieId == _testMovie.Id);
        Assert.That(rating, Is.Not.Null);
        Assert.That(rating.Value, Is.EqualTo(5));
    }

    [Test]
    public async Task AddBookRating_NegativeValue_ShouldNotAddRating()
    {
        // Arrange
        var invalidValue = -5;

        // Act
        var result = await _controller.AddBookRating(_testBook.Id, invalidValue, null);

        // Assert
        var rating = _context.Ratings.FirstOrDefault(r => r.BookId == _testBook.Id);
        Assert.That(rating, Is.Null);
    }

    [Test]
    public async Task AddMovieRating_ValueSix_ShouldNotAddRating()
    {
        // Arrange
        var invalidValue = 6;

        // Act
        var result = await _controller.AddMovieRating(_testMovie.Id, invalidValue, null);

        // Assert
        var rating = _context.Ratings.FirstOrDefault(r => r.MovieId == _testMovie.Id);
        Assert.That(rating, Is.Null, "Оценка с 6 не трябва да се добавя");
    }
}

// Помощен клас за TempData
public class NullTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context)
    {
        return new Dictionary<string, object>();
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        // Не прави нищо
    }
}
