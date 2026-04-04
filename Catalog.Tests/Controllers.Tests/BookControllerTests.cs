using Moq;
using Microsoft.AspNetCore.Mvc;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;

namespace Catalog.Tests.Controllers.Tests;

[TestFixture]
public class BookControllerTests
{
    private Mock<IBookService> _mockBookService;
    private BookController _controller;
    private List<Book> _testBooks;

    
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

    [Test]
    public void Details_ExistingId_ReturnsViewWithBook()
    {
        // Arrange
        _mockBookService.Setup(s => s.GetById(1))
            .Returns(_testBooks.First(b => b.Id == 1));

        // Act
        var result = _controller.Details(1);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult.Model as Book;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Title, Is.EqualTo("Book 1"));
    }

    [Test]
    public void Details_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = _controller.Details(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void Create_Post_ValidBook_RedirectsToIndex()
    {
        // Arrange
        var newBook = new Book { Title = "New Book", Author = "New Author", Year = 2023 };
        _mockBookService.Setup(s => s.Add(It.IsAny<Book>()))
            .Callback<Book>(b => _testBooks.Add(b));
        _mockBookService.Setup(s => s.GetAll())
            .Returns(_testBooks);

        // Act
        var result = _controller.Create(newBook);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));

        // Проверка дали е добавено
        Assert.That(_testBooks.Count, Is.EqualTo(3));
        _mockBookService.Verify(s => s.Add(It.Is<Book>(x => x.Title == "New Book")), Times.Once);
    }
}