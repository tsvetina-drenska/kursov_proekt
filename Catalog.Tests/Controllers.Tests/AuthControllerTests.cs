using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;


namespace Catalog.Tests.Controllers.Tests;

// Юнит тестове за "AuthController" (използваме реалния `AccountController`),
// покриващи сценариите Login, Logout и Register.
// Използва Moq за мокване на `IAuthService` и NUnit за асерции.
[TestFixture]
public class AuthControllerTests
{
    // Mock на IAuthService, чрез който контролираме поведението на сервиза.
    private Mock<IAuthService> _mockAuthService;

    // Контролерът под тест.
    private AccountController _controller;

    // Няколко фиктивни потребителя за различни сценарии.
    private List<User> _testUsers;

    // Изпълнява се преди всеки тест: инициализира mock-овете, контролера и тестовите данни.
    [SetUp]
    public void SetUp()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AccountController(_mockAuthService.Object);

        // Seed с примерни потребители за сценариите "съществуващ потребител".
        _testUsers = new List<User>
        {
            new User { Id = 1, Username = "alice", Email = "alice@test.com", PasswordHash = "hash1" },
            new User { Id = 2, Username = "bob", Email = "bob@test.com", PasswordHash = "hash2" }
        };
    }

    // Изпълнява се след всеки тест: освобождава ресурсите и нулира референциите.
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

    // Тест: Login с валидни креденшъли трябва да пренасочи към Home/Index.
    [Test]
    public async Task Login_ValidCredentials_RedirectsToHomeIndex()
    {
        // Arrange: мокваме успешен Login, връщащ потребителен обект.
        var user = new User { Id = 10, Username = "alice", Email = "alice@test.com" };
        _mockAuthService.Setup(s => s.Login("alice", "correct"))
            .Returns(user);

        // Мокваме IAuthenticationService, за да позволим SignInAsync да се извика без грешки.
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        // Прикрепяме mock-а към RequestServices на HttpContext.
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authServiceMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act: извикваме Login (POST) с валидни данни.
        var result = await _controller.Login("alice", "correct");

        // Assert: очакваме RedirectToActionResult към Home/Index.
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirect = result as RedirectToActionResult;
        Assert.That(redirect.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Home"));

        // Проверяваме, че Login в сервиза е извикан веднъж.
        _mockAuthService.Verify(s => s.Login("alice", "correct"), Times.Once);

        // Проверяваме, че SignInAsync е извикан веднъж.
        authServiceMock.Verify(a => a.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()), Times.Once);
    }

    // Тест: Login с невалидни креденшъли трябва да върне View с грешка.
    [Test]
    public async Task Login_InvalidCredentials_ReturnsViewWithError()
    {
        // Arrange: mock Login връща null за невалидни креденшъли.
        _mockAuthService.Setup(s => s.Login("alice", "bad"))
            .Returns((User?)null);

        // Act: извикваме Login (POST) с невалидни данни.
        var result = await _controller.Login("alice", "bad");

        // Assert: резултатът трябва да е ViewResult и ViewData/ ViewBag съдържа съобщение за грешка.
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;

        // Контролерът използва ViewBag.Error в кода — проверяваме ViewData["Error"].
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));

        // Login сервизът трябва да е извикан веднъж.
        _mockAuthService.Verify(s => s.Login("alice", "bad"), Times.Once);
    }

    // Тест: Logout трябва да извика SignOutAsync и да пренасочи към Home/Index.
    [Test]
    public async Task Logout_RedirectsToHomeIndex()
    {
        // Arrange: мокваме IAuthenticationService.SignOutAsync и прикрепяме към RequestServices.
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(authServiceMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act: извикваме Logout (POST).
        var result = await _controller.Logout();

        // Assert: пренасочване към Home/Index.
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirect = result as RedirectToActionResult;
        Assert.That(redirect.ActionName, Is.EqualTo("Index"));
        Assert.That(redirect.ControllerName, Is.EqualTo("Home"));

        // Проверяваме, че SignOutAsync е извикан веднъж.
        authServiceMock.Verify(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Once);
        
    }


    // Тест: Register със съществуващо потребителско име трябва да върне View с грешка в ModelState.
    [Test]
    public void Register_ExistingUser_ReturnsViewWithError()
    {
        // Arrange: съществуващ потребител с username "alice".
        var existing = _testUsers[0];
        _mockAuthService.Setup(s => s.GetByUsername("alice"))
            .Returns(existing);

        var newUser = new User { Username = "alice", Email = "new@test.com", PasswordHash = "pwd" };

        // Act: опит за регистрация със съществуващо потребителско име.
    }
}