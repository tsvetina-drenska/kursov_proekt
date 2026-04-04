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
    // Юнит тестове за MovieService, използващи InMemory EF Core контекст.(Тестове, които проверяват дали MovieService работи правилно, използвайки временна база данни.)
    [TestFixture]
    public class MovieServiceTests
    {
        // In-memory EF Core контекст за тестовете.(Временна база данни в паметта за тестовете.)
        private ApplicationDbContext _context;

        // Услугата под тест.
        private MovieService _movieService;

        // Изпълнява се преди всеки тест: инициализира InMemory DB(Временна база данни в паметта.) и инстанция на MovieService.
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _movieService = new MovieService(_context);
        }

        // Изпълнява се след всеки тест: изтрива базата и освобождава ресурсите.
        [TearDown]
        public void TearDown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _context = null!;
            _movieService = null!;
        }

        //GetAll трябва да върне всички филми налични в базата.
        [Test]
        public void GetAll_ReturnsAllMovies()
        {
            // Arrange: добавяме два филма в контекста.
            var m1 = new Movie { Title = "Movie A", Director = "Director A", Year = 2000 };
            var m2 = new Movie { Title = "Movie B", Director = "Director B", Year = 2001 };
            _context.Movies.AddRange(m1, m2);
            _context.SaveChanges();

            // извикваме GetAll.
            var result = _movieService.GetAll();

            //връща се списък с точно две филма и техните заглавия съвпадат.
            Assert.That(result, Is.InstanceOf<List<Movie>>());
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(m => m.Title), Does.Contain("Movie A"));
            Assert.That(result.Select(m => m.Title), Does.Contain("Movie B"));
        }

        // Тест: GetById с валиден id трябва да върне съответния филм.
        [Test]
        public void GetById_ExistingId_ReturnsMovie()
        {
            //добавяме филм и запазваме неговия id.
            var movie = new Movie { Title = "FindMe", Director = "Finder", Year = 2010 };
            _context.Movies.Add(movie);
            _context.SaveChanges();
            var id = movie.Id;

            // извикваме GetById.
            var result = _movieService.GetById(id);

            // Assert: резултатът е филм с очакваното заглавие и id.
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.Title, Is.EqualTo("FindMe"));
        }

        // Тест: GetById с невалиден id трябва да върне null.
        [Test]
        public void GetById_NonExistingId_ReturnsNull()
        {
            // Act: извикваме GetById с id, което не съществува.
            var result = _movieService.GetById(999);

            // Assert: резултатът трябва да е null.
            Assert.That(result, Is.Null);
        }

        // Тест: Add трябва да запише нов филм и да зададе CreatedAt(времето на създаване.).
        [Test]
        public void Add_ValidMovie_SavesAndSetsCreatedAt()
        {
            // Arrange: нов филм без зададен CreatedAt.
            var movie = new Movie { Title = "New Movie", Director = "New Director", Year = 2022 };

            // Act: извикваме Add.
            _movieService.Add(movie);

            // Assert: филмът е записан в контекста и CreatedAt е зададен.
            var saved = _context.Movies.SingleOrDefault(m => m.Title == "New Movie");
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }

        // Тест: Update трябва да промени полета на съществуващ филм.
        [Test]
        public void Update_ExistingMovie_UpdatesFields()
        {
            // Arrange: добавяме филм и променяме неговото заглавие.
            var movie = new Movie { Title = "Old Title", Director = "A", Year = 1999 };
            _context.Movies.Add(movie);
            _context.SaveChanges();

            movie.Title = "Updated Title";

            // Act: извикваме Update.
            _movieService.Update(movie);

            // Assert: презареждаме от контекста и проверяваме промяната.
            var updated = _context.Movies.Find(movie.Id);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
        }

        // Тест: Delete трябва да премахне съществуващ филм.
        [Test]
        public void Delete_ExistingMovie_RemovesMovie()
        {
            // Arrange: добавяме филм.
            var movie = new Movie { Title = "ToDelete", Director = "X", Year = 2005 };
            _context.Movies.Add(movie);
            _context.SaveChanges();
            var id = movie.Id;

            // Act: извикваме Delete.
            _movieService.Delete(id);

            // Assert: филмът вече не съществува в базата.
            var deleted = _context.Movies.Find(id);
            Assert.That(deleted, Is.Null);
        }

        // Тест: Delete с невалиден id не прави нищо.
        [Test]
        public void Delete_NonExistingId_DoesNothing()
        {
            // Arrange: добавяме един филм.
            var movie = new Movie { Title = "KeepMe", Director = "Y", Year = 2011 };
            _context.Movies.Add(movie);
            _context.SaveChanges();

            // Act: опитваме да изтрием несъществуващ id.
            _movieService.Delete(999);

            // Assert: броят на филмите остава 1.
            Assert.That(_context.Movies.Count(), Is.EqualTo(1));
        }
    }
}