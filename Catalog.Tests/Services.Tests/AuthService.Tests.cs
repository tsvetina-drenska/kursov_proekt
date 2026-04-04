using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using catalog.Data;
using catalog.Entities;
using catalog.Services;

namespace Catalog.Tests.Services.Tests
{
    // Тестове, които проверяват дали AuthService работи правилно, използвайки временна база данни.
    [TestFixture]
    public class AuthServiceTests
    {
        private ApplicationDbContext _context;
        private AuthService _authService;

        // Стартира преди всеки тест: инициализира InMemory DB и инстанция на AuthService.
        [SetUp]
        public void SetUp()
        {
            // Уникално име за базата да предпази състоянието между тестовете.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _authService = new AuthService(_context);
        }

        // Изпълнява се след всеки тест: изчиства и освобождава контекста.
        [TearDown]
        public void TearDown()
        {
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
            _context = null!;
            _authService = null!;
        }

        // Тест: Правилен логин дава потребителски данни.
        [Test]
        public void Login_ValidCredentials_ReturnsUser()
        {
            //  регистрираме потребител чрез услугата (Register ще хешира паролата).
            var plainPassword = "Secret123!";
            var user = new User { Username = "ivan", Email = "ivan@test.com", PasswordHash = plainPassword };
            _authService.Register(user);

            // Опит да се влезе в системата, като се използва същата парола в нормален вид.
            var result = _authService.Login("ivan", plainPassword);

            // трябва да върне User и Username да съвпада.
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Username, Is.EqualTo("ivan"));
        }

        // тест Login с невалидна парола връща null.
        [Test]
        public void Login_InvalidCredentials_ReturnsNull()
        {
            // регистрираме потребител.
            var plainPassword = "Password1";
            var user = new User { Username = "maria", Email = "maria@test.com", PasswordHash = plainPassword };
            _authService.Register(user);

            //  опит за логин с грешна парола.
            var result = _authService.Login("maria", "WrongPassword");

            //  резултатът трябва да е null.
            Assert.That(result, Is.Null);
        }

        // Тест дали регистрацията добавя потребител с защитена парола и дата.
        [Test]
        public void Register_ValidUser_SavesHashedPasswordAndCreatedAt()
        {
            // Създаваме нов потребител с обикновена парола, която регистрацията после ще защити.
            var plainPassword = "MyPass!";
            var newUser = new User { Username = "georgi", Email = "georgi@test.com", PasswordHash = plainPassword };

            // регистрираме потребителя.
            _authService.Register(newUser);

            //в контекста съществува записан потребител с изолиран username.
            var saved = _context.Users.SingleOrDefault(u => u.Username == "georgi");
            Assert.That(saved, Is.Not.Null);

            //Паролата в базата трябва да е защитена и да не е същата като обикновената.
            Assert.That(saved!.PasswordHash, Is.Not.EqualTo(plainPassword));

            // CreatedAt трябва да е зададен (не е default).(да има стойност, а не да е празен)
            Assert.That(saved.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        }

        // GetByUsername връща потребителя, ако съществува.
        [Test]
        public void GetByUsername_ReturnsUser_WhenExists()
        {
            //регистрираме потребител.
            var user = new User { Username = "stela", Email = "stela@test.com", PasswordHash = "pwd" };
            _authService.Register(user);

            //извикваме GetByUsername.
            var found = _authService.GetByUsername("stela");

            // Assert: намереният потребител не е null и потребителското име съвпада.
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Username, Is.EqualTo("stela"));
        }
    }
}
