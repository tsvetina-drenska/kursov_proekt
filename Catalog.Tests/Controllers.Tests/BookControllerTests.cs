using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers.Tests;

public class BookControllerTests
{
    private readonly Mock<IBookService> _mockBookService;
    private readonly BookController _controller;
    private readonly List<Book> _testBooks;

    public BookControllerTests()
    {
        _mockBookService = new Mock<IBookService>();
        _controller = new BookController();

        _testBooks = new List<Book>
        {
            new Book { Id = 1, Title = "Book 1", Author = "Author 1", Year = 2020 },
            new Book { Id = 2, Title = "Book 2", Author = "Author 2", Year = 2021 }
        };
    }

}