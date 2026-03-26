using Moq;
using Microsoft.AspNetCore.Mvc;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;

namespace Catalog.Tests.ControllersTests;

[TestFixture]
public class BookControllerTests
{
    private Mock<IBookService> _mockBookService;
    private BookController _controller;
    private List<Book> _testBooks;

    // [SetUp] се изпълнява преди всеки тест (като Initialize)
    [SetUp]
    public void SetUp()
    {
        _mockBookService = new Mock<IBookService>();
        _controller = new BookController(_mockBookService.Object);

        _testBooks = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", Year = 2020 },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", Year = 2021 }
        };
    }

    // [TearDown] се изпълнява след всеки тест (като Cleanup)
    [TearDown]
    public void TearDown()
    {
        _mockBookService = null;
        if (_controller != null)
        {
            _controller.Dispose();
            _controller = null;
        }
        _testBooks = null;
    }

    // ТЕСТ 1: Index трябва да върне View с всички книги
    [Test]
    public void Index_ReturnsViewWithAllBooks()
    {
        // Arrange
        _mockBookService.Setup(s => s.GetAll())
            .Returns(_testBooks);

        // Act
        var result = _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult.Model, Is.InstanceOf<List<Book>>());
        var model = viewResult.Model as List<Book>;
        Assert.That(model.Count, Is.EqualTo(2));
    }

}