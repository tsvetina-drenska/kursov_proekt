using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Tests.Services.Tests
{
    internal class MovieServiceTest
    {
        public MovieServiceTest() { }        // In-memory EF Core контекст за тестовете.
        private ApplicationDbContext _context;

        // Услугата под тест.
        private MovieService _movieService;

        // Изпълнява се преди всеки тест: създава нов InMemory DB и инстанция на MovieService.
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
        // In-memory EF Core контекст за тестовете.
        private ApplicationDbContext _context;

        // Услугата под тест.
        private MovieService _movieService;

        // Стартира преди всеки тест: инициализира InMemory DB и инстанция на MovieService.
        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _movieService = new MovieService(_context);
        }

        // Изпълнява се след всеки тест: изчиства и освобождава контекста.
        [TearDown]
        public void TearDown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _context = null!;
            _movieService = null!;
        }

        // Тест: GetAll трябва да върне всички филми налични в базата.
        [Test]
        public void GetAll_ReturnsAllMovies()
        {
            // Arrange: добавяме два филма в контекста.
            var m1 = new Movie { Title = "Movie A", Director = "Director A", Year = 2000 };
            var m2 = new Movie { Title = "Movie B", Director = "Director B", Year = 2001 };
            _context.Movies.AddRange(m1, m2);
            _context.SaveChanges();

            // Act: извикваме GetAll.
            var result = _movieService.GetAll();

            // Assert: връща се списък с точно две филма и техните заглавия съвпадат.
            Assert.That(result, Is.InstanceOf<List<Movie>>());
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Select(m => m.Title), Does.Contain("Movie A"));
            Assert.That(result.Select(m => m.Title), Does.Contain("Movie B"));
        }

        // Тест: GetById с валиден id трябва да върне съответния филм.
        [Test]
        public void GetById_ExistingId_ReturnsMovie()
        {
            // Arrange: добавяме филм и запазваме неговия id.
            var movie = new Movie { Title = "FindMe", Director = "Finder", Year = 2010 };
            _context.Movies.Add(movie);
            _context.SaveChanges();
            var id = movie.Id;

            // Act: извикваме GetById.
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

        // Тест: Add трябва да запише нов филм и да зададе CreatedAt.
        [Test]
        public void Add_ValidMovie_SavesAndSetsCreatedAt()
        {
            // Arrange: нов филм без зададен CreatedAt.
            var movie = new Movie { Title = "New Movie", Director = "New Director", Year = 2022 };
    
    }
}
