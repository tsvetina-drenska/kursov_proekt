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

    // ========== EDIT TESTS ==========

    [Test]
    public void Edit_Get_ExistingId_ReturnsViewWithBook()
    {
        // Arrange
        var expectedBook = _testBooks[0];
        _mockBookService.Setup(s => s.GetById(1))
            .Returns(expectedBook);

        // Act
        var result = _controller.Edit(1);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<Book>());

        var model = viewResult.Model as Book;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Id, Is.EqualTo(1));
        Assert.That(model.Title, Is.EqualTo("Book 1"));
    }

    [Test]
    public void Edit_Get_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockBookService.Setup(s => s.GetById(999))
            .Returns((Book?)null);

        // Act
        var result = _controller.Edit(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void Edit_Post_ValidModel_RedirectsToIndex()
    {
        // Arrange
        var updatedBook = new Book
        {
            Id = 1,
            Title = "Updated Book",
            Author = "Updated Author",
            Year = 2023,
            Description = "Updated Description"
        };

        var existingBook = _testBooks[0];
        _mockBookService.Setup(s => s.GetById(1))
            .Returns(existingBook);
        _mockBookService.Setup(s => s.Update(It.IsAny<Book>()))
            .Callback<Book>(b =>
            {
                existingBook.Title = b.Title;
                existingBook.Author = b.Author;
                existingBook.Year = b.Year;
                existingBook.Description = b.Description;
            });

        // Act
        var result = _controller.Edit(1, updatedBook);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        Assert.That(existingBook.Title, Is.EqualTo("Updated Book"));
        Assert.That(existingBook.Author, Is.EqualTo("Updated Author"));

    }

    [Test]
    public void Edit_Post_IdMismatch_ReturnsNotFound()
    {
        // Arrange
        var book = new Book { Id = 2, Title = "Mismatched Book" };

        // Act
        var result = _controller.Edit(1, book); // ID 1 срещу ID 2

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        _mockBookService.Verify(s => s.Update(It.IsAny<Book>()), Times.Never);
    }

    [Test]
    public void Edit_Post_InvalidModel_ReturnsSameView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Заглавието е задължително");
        var invalidBook = new Book { Id = 1, Author = "Author", Year = 2023 };

        // Act
        var result = _controller.Edit(1, invalidBook);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.EqualTo(invalidBook));
        _mockBookService.Verify(s => s.Update(It.IsAny<Book>()), Times.Never);
    }

    // ========== DELETE TESTS ==========

    [Test]
    public void Delete_Get_ExistingId_ReturnsViewWithBook()
    {
        // Arrange
        var expectedBook = _testBooks[0];
        _mockBookService.Setup(s => s.GetById(1))
            .Returns(expectedBook);

        // Act
        var result = _controller.Delete(1);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);

        var model = viewResult.Model as Book;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Id, Is.EqualTo(1));
        Assert.That(model.Title, Is.EqualTo("Book 1"));
    }

    [Test]
    public void Delete_Get_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockBookService.Setup(s => s.GetById(999))
            .Returns((Book?)null);

        // Act
        var result = _controller.Delete(999);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void DeleteConfirmed_ValidId_RedirectsToIndex()
    {
        // Act
        var result = _controller.DeleteConfirmed(1);

        // Assert
        _mockBookService.Verify(s => s.Delete(1), Times.Once);

        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
    }

}