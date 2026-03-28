using Moq;
using Microsoft.AspNetCore.Mvc;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;

namespace Catalog.Tests.Controllers.Tests;

[TestFixture]
public class MovieControllerTests
{
    private Mock<IMovieService> _mockMovieService;
    private MovieController _controller;
    private List<Movie> _testMovies;


    [SetUp]
    public void SetUp()
    {
        _mockMovieService = new Mock<IMovieService>();
        _controller = new MovieController(_mockMovieService.Object);

        _testMovies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Movie 1", Director = "Director 1", Year = 2020 },
            new Movie { Id = 2, Title = "Movie 2", Director = "Director 2", Year = 2021 }
        };
    }


    [TearDown]
    public void TearDown()
    {
        _mockMovieService = null;
        if (_controller != null)
        {
            _controller.Dispose();
            _controller = null;
        }
        _testMovies = null;
    }

    // ТЕСТ 1: Index трябва да върне View с всички книги
    [Test]
    public void Index_ReturnsViewWithAllBooks()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetAll())
            .Returns(_testMovies);

        // Act
        var result = _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult.Model, Is.InstanceOf<List<Movie>>());
        var model = viewResult.Model as List<Movie>;
        Assert.That(model.Count, Is.EqualTo(2));
    }

    [Test]
    public void Details_ExistingId_ReturnsViewWithMovie()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetById(1))
            .Returns(_testMovies.First(m => m.Id == 1));

        // Act
        var result = _controller.Details(1);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult.Model as Movie;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Title, Is.EqualTo("Movie 1"));
    }

    [Test]
    public void Details_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetById(999))
            .Returns((Movie?)null);

        // Act
        var result = _controller.Details(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Create_Post_ValidMovie_RedirectsToIndex()
    {
        // Arrange
        var newMovie = new Movie { Title = "New Movie", Director = "New Director", Year = 2023 };
        _mockMovieService.Setup(s => s.Add(It.IsAny<Movie>()))
            .Callback<Movie>(m => _testMovies.Add(m));
        _mockMovieService.Setup(s => s.GetAll())
            .Returns(_testMovies);

        // Act
        var result = _controller.Create(newMovie);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

        // Проверка дали е добавено
        Assert.That(_testMovies.Count, Is.EqualTo(3));
        _mockMovieService.Verify(s => s.Add(It.Is<Movie>(x => x.Title == "New Movie")), Times.Once);
    }

    [Test]
    public void Create_Post_InvalidModel_ReturnsSameView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Заглавието е задължително");
        var invalidMovie = new Movie { Director = "Director", Year = 2023 };

        // Act
        var result = _controller.Create(invalidMovie);

        // Assert
        _mockMovieService.Verify(s => s.Add(It.IsAny<Movie>()), Times.Never);

        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.EqualTo(invalidMovie));
    }

    [Test]
    public void Delete_Get_ExistingId_ReturnsViewWithMovie()
    {
        // Arrange
        _mockMovieService.Setup(s => s.GetById(1))
            .Returns(_testMovies.First(m => m.Id == 1));

        // Act
        var result = _controller.Delete(1);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult.Model as Movie;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Title, Is.EqualTo("Movie 1"));
    }

    [Test]
    public void DeleteConfirmed_ValidId_RedirectsToIndex()
    {
        // Act
        var result = _controller.DeleteConfirmed(1);

        // Assert
        _mockMovieService.Verify(s => s.Delete(1), Times.Once);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
    }

}