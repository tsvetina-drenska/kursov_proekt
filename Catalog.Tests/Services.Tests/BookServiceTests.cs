using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using catalog.Data;
using catalog.Entities;
using catalog.Services;

namespace Catalog.Tests.Services.Tests
{
    //Тестове, които проверяват дали BookService работи правилно, използвайки временна база данни.
    [TestFixture]
    public class BookServiceTests
    {
        // Временна база данни в паметта за тестовете.
        private ApplicationDbContext _context;

        // Услугата под тест.
        private BookService _bookService;

        // Изпълнява се преди всеки тест: инициализира InMemory DB(настройва временната база) и инстанция на BookService.
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _bookService = new BookService(_context);
        }

        // Изпълнява се след всеки тест: изчиства и освобождава контекста.
        [TearDown]
        public void TearDown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _context = null!;
            _bookService = null!;
        }

        // Тест: GetAll трябва да върне всички книги налични в базата.
        [Test]
        public void GetAll_ReturnsAllBooks()
        {
            //  добавяме две книги в контекста.
            var b1 = new Book { Title = "Book A", Author = "Author A", Year = 2000 };
            var b2 = new Book { Title = "Book B", Author = "Author B", Year = 2001 };
            _context.Books.AddRange(b1, b2);
            _context.SaveChanges();

            //извикваме GetAll.
            var result = _bookService.GetAll();

            //  връща се списък с точно две книги и техните заглавия съвпадат.
            Assert.That(result, Is.InstanceOf<List<Book>>());
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(b => b.Title), Does.Contain("Book A"));
            Assert.That(result.Select(b => b.Title), Does.Contain("Book B"));
        }

        // Тест: GetById с валиден id трябва да върне съответната книга.
        [Test]
        public void GetById_ExistingId_ReturnsBook()
        {
            // Arrange: добавяме книга и запазваме нейния id.
            var book = new Book { Title = "FindMe", Author = "Finder", Year = 2010 };
            _context.Books.Add(book);
            _context.SaveChanges();
            var id = book.Id;

            // Act: извикваме GetById.
            var result = _bookService.GetById(id);

            // Assert: резултатът е книга с очакваното заглавие и id.
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.Title, Is.EqualTo("FindMe"));
        }

        // Тест: GetById с невалиден id трябва да върне null.
        [Test]
        public void GetById_NonExistingId_ReturnsNull()
        {
            // Act: извикваме GetById с id, което не съществува.
            var result = _bookService.GetById(999);

            // Assert: резултатът трябва да е null.
            Assert.That(result, Is.Null);
        }

        // Тест: Add трябва да запише нова книга и да зададе CreatedAt.
        [Test]
        public void Add_ValidBook_SavesAndSetsCreatedAt()
        {
            // Arrange: нова книга без зададен CreatedAt.
            var book = new Book { Title = "New Book", Author = "New Author", Year = 2022 };

            // Act: извикваме Add.
            _bookService.Add(book);

            // Assert: книгата е записана в контекста и CreatedAt е зададен.
            var saved = _context.Books.SingleOrDefault(b => b.Title == "New Book");
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }

        // Тест: Update трябва да промени полета на съществуваща книга.
        [Test]
        public void Update_ExistingBook_UpdatesFields()
        {
            // Arrange: добавяме книга и променяме нейното заглавие.
            var book = new Book { Title = "Old Title", Author = "A", Year = 1999 };
            _context.Books.Add(book);
            _context.SaveChanges();

            book.Title = "Updated Title";

            // Act: извикваме Update.
            _bookService.Update(book);

            // Assert: презареждаме от контекста и проверяваме промяната.
            var updated = _context.Books.Find(book.Id);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
        }

        // Тест: Delete трябва да премахне съществуваща книга.
        [Test]
        public void Delete_ExistingBook_RemovesBook()
        {
            // Arrange: добавяме книга.
            var book = new Book { Title = "ToDelete", Author = "X", Year = 2005 };
            _context.Books.Add(book);
            _context.SaveChanges();
            var id = book.Id;

                // Act: извикваме Delete.
            _bookService.Delete(id);

            // Assert: книгата вече не съществува в базата.
            var deleted = _context.Books.Find(id);
            Assert.That(deleted, Is.Null);
        }

        // Тест: Delete с невалиден id не прави нищо.
        [Test]
        public void Delete_NonExistingId_DoesNothing()
        {
            // Arrange: добавяме една книга.
            var book = new Book { Title = "KeepMe", Author = "Y", Year = 2011 };
            _context.Books.Add(book);
            _context.SaveChanges();

            // Act: опитваме да изтрием несъществуващ id.
            _bookService.Delete(999);

            // Assert: броят на книгите остава 1.
            Assert.That(_context.Books.Count(), Is.EqualTo(1));
        }
    }
}
