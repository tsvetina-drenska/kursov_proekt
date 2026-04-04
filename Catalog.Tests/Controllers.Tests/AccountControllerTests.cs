using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;

namespace Catalog.Tests.ControllersTests;

// Юнит тестове за `AccountController`, покриващи поведението на `Register`, `Login`, `Logout` и `Profile`.
[TestFixture]
public class AccountControllerTests
{
    // Mock на сервиза за автентикация, използван от контролера.
    private Mock<IAuthService> _mockAuthService;

    // Инстанция на контролера, която се тества.
    private AccountController _controller;

    // Списък с потребители в паметта, използван като тестови данни / фалшиво хранилище.
    private List<User> _testUsers;

    // Изпълнява се преди всеки тест: създава mock-ове, контролера и подготвя тестовите данни.
    [SetUp]
    public void SetUp()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AccountController(_mockAuthService.Object);



        // Зареждаме фиктивни потребители за сценарии които изискват съществуващи записи.
        _testUsers = new List<User>
        {
            new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hashed123" },
            new User { Id = 2, Username = "john", Email = "john@test.com", PasswordHash = "hashed456" }
        };
    }

    // Изпълнява се след всеки тест: освобождава ресурсите и нулира референциите, за да се избегне замърсяване между тестовете.
    [TearDown]
    public void TearDown()
    {
        _mockAuthService = null;
        if (_controller != null)
        {
            _controller.Dispose();
            _controller = null;
        }
        _testUsers = null;
    }

    // ========== REGISTER TESTS ==========

    // Тест: GET /Account/Register трябва да върне изгледа за регистрация.
    [Test]
    public void Register_Get_ReturnsView()
    {
        // Act: извикваме Register (GET).
        var result = _controller.Register();

        // Assert: резултатът е ViewResult (страницата за регистрация се връща).
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    // Тест: POST /Account/Register с валидни данни трябва да извика Register в сервиза и да пренасочи към Login.
    [Test]
    public void Register_Post_ValidUser_RedirectsToLogin()
    {
        // Arrange: нов потребител, който не съществува в системата.
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };

        // Mock: няма съществуващ потребител с това потребителско име.
        _mockAuthService.Setup(s => s.GetByUsername("newuser"))
            .Returns((User?)null);

        // Mock: при Register добавяме потребителя в нашия in-memory списък.
        _mockAuthService.Setup(s => s.Register(It.IsAny<User>()))
            .Callback<User>(u => _testUsers.Add(u));

        // Act: извикваме POST Register с потвърждаване на парола.
        var result = _controller.Register(newUser, "password123");

        // Assert: очакваме пренасочване към Login и метода Register да е извикан веднъж.
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
        _mockAuthService.Verify(s => s.Register(It.Is<User>(x => x.Username == "newuser")), Times.Once);
    }

    // Тест: POST Register, когато паролите не съвпадат, трябва да върне изгледа с грешка в ModelState.
    [Test]
    public void Register_Post_PasswordMismatch_ReturnsViewWithError()
    {
        // Arrange: потребител, чиито пароли няма да съвпаднат.
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act: извикваме Register с различна потвърдителна парола.
        var result = _controller.Register(newUser, "wrongpassword");

        // Assert: връща се ViewResult с модела и ModelState е невалиден.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.EqualTo(newUser));
        Assert.That(_controller.ModelState.IsValid, Is.False);

        // Уверяваме се, че Register на сървиса не е извикан.
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // Тест: POST Register с вече съществуващо потребителско име трябва да върне изгледа с грешка.
    [Test]
    public void Register_Post_ExistingUsername_ReturnsViewWithError()
    {
        // Arrange: конфигурираме mock-а да връща вече съществуващ потребител.
        var existingUser = _testUsers[0];
        _mockAuthService.Setup(s => s.GetByUsername("testuser"))
            .Returns(existingUser);

        var newUser = new User { Username = "testuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act: опит за регистрация с потребителско име, което вече съществува.
        var result = _controller.Register(newUser, "password123");

        // Assert: връща се изглед и ModelState е невалиден; Register не трябва да се извика.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // Тест: POST Register с празно потребителско име трябва да върне изгледа с валидационна грешка.
    [Test]
    public void Register_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Arrange: невалиден потребител без потребителско име.
        var invalidUser = new User { Username = "", Email = "test@test.com", PasswordHash = "pass123" };

        // Act: извикваме Register с невалидния модел.
        var result = _controller.Register(invalidUser, "pass123");

        // Assert: връща се ViewResult и ModelState е невалиден; Register не трябва да се извика.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // ========== LOGIN TESTS ==========

    // Тест: GET /Account/Login трябва да върне изгледа за вход.
    [Test]
    public void Login_Get_ReturnsView()
    {
        // Act: извикваме Login (GET).
        var result = _controller.Login();

        // Assert: резултатът е ViewResult.
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    // Тест: POST Login с грешна парола трябва да покаже изгледа с грешка за невалидни креденшъли.
    [Test]
    public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
    {
        // Arrange: mock да връща null за неправилни данни.
        _mockAuthService.Setup(s => s.Login("testuser", "wrongpassword"))
            .Returns((User?)null);

        // Act: извикваме POST Login с грешна парола.
        var result = await _controller.Login("testuser", "wrongpassword");

        // Assert: връща се ViewResult и във ViewData["Error"] е зададено съобщение за невалидни данни.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
        _mockAuthService.Verify(s => s.Login("testuser", "wrongpassword"), Times.Once);
    }

    // Тест: POST Login с не-съществуващ потребител трябва да върне изгледа с грешка.
    [Test]
    public async Task Login_Post_NonExistingUser_ReturnsViewWithError()
    {
        // Arrange: mock връща null за несъществуващ потребител.
        _mockAuthService.Setup(s => s.Login("nonexistent", "password"))
            .Returns((User?)null);

        // Act: опит за логин.
        var result = await _controller.Login("nonexistent", "password");

        // Assert: връща се ViewResult и съобщение за грешни креденшъли.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
    }

    // Тест: POST Login с празно потребителско име трябва да върне изгледа с грешка и да не извиква сервиза.
    [Test]
    public async Task Login_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Act: извикваме Login с празно потребителско име.
        var result = await _controller.Login("", "password");

        // Assert: връща се ViewResult, зададено е съобщение за липсващи входни данни и Login сервиса не е извикан.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // Тест: POST Login с празна парола трябва да върне изгледа с грешка и да не извиква сервиза.
    [Test]
    public async Task Login_Post_EmptyPassword_ReturnsViewWithError()
    {
        // Act: извикваме Login с празна парола.
        var result = await _controller.Login("testuser", "");

        // Assert: връща се ViewResult, зададено е съобщение за липсващи входни данни и Login сервиса не е извикан.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    // ========== PROFILE TESTS ==========

    // Тест: Profile връща изглед с данните на потребителя, когато потребителят е автентикиран.
    [Test]
    public void Profile_WhenAuthenticated_ReturnsViewWithUser()
    {
        // Arrange: създаваме автентикиран principal с необходимите claims за тестовия потребител.
        var user = _testUsers[0];
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Прикачваме principal към HttpContext на контролера, за да бъде попълнен User.
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Mock: при търсене по потребителско име се връща тестовият потребител.
        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns(user);

        // Act: извикваме Profile.
        var result = _controller.Profile();

        // Assert: ViewResult с модела на потребителя се връща и съдържа очакваното потребителско име.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        var model = viewResult.Model as User;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Username, Is.EqualTo("testuser"));
    }

    // Тест: Profile пренасочва към Login, когато няма автентикиран потребител.
    [Test]
    public void Profile_WhenNotAuthenticated_RedirectsToLogin()
    {
        // Arrange: задаваме празен HttpContext без автентикиран потребител.
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act: извикваме Profile.
        var result = _controller.Profile();

        // Assert: очаква се RedirectToAction към Login.
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
    }

    // Тест: Profile връща NotFound, когато автентикираният потребител не е намерен в хранилището.
    [Test]
    public void Profile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange: автентикираме principal за съществуващо потребителско име.
        var user = _testUsers[0];
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Mock: при търсене по потребителско име връщаме null (потребителят не е намерен).
        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns((User?)null);

        // Act: извикваме Profile.
        var result = _controller.Profile();

        // Assert: очакваме NotFoundResult когато потребителят липсва.
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
                                