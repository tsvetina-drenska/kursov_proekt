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

}