using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using catalog.Models;
using NUnit.Framework;

namespace Catalog.Tests.ControllersTests;

[TestFixture]
public class AccountControllerTests
{
    private Mock<IAuthService> _mockAuthService;
    private AccountController _controller;
    private List<User> _testUsers;

    [SetUp]
    public void SetUp()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AccountController(_mockAuthService.Object);



        _testUsers = new List<User>
        {
            new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hashed123" },
            new User { Id = 2, Username = "john", Email = "john@test.com", PasswordHash = "hashed456" }
        };
    }

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

    [Test]
    public void Register_Get_ReturnsView()
    {
        // Act
        var result = _controller.Register();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void Register_Post_ValidUser_RedirectsToLogin()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };
        _mockAuthService.Setup(s => s.GetByUsername("newuser"))
            .Returns((User?)null);
        _mockAuthService.Setup(s => s.Register(It.IsAny<User>()))
            .Callback<User>(u => _testUsers.Add(u));

        // Act
        var result = _controller.Register(newUser, "password123");

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
        _mockAuthService.Verify(s => s.Register(It.Is<User>(x => x.Username == "newuser")), Times.Once);
    }

    [Test]
    public void Register_Post_PasswordMismatch_ReturnsViewWithError()
    {
        // Arrange
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act
        var result = _controller.Register(newUser, "wrongpassword");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.EqualTo(newUser));
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void Register_Post_ExistingUsername_ReturnsViewWithError()
    {
        // Arrange
        var existingUser = _testUsers[0];
        _mockAuthService.Setup(s => s.GetByUsername("testuser"))
            .Returns(existingUser);

        var newUser = new User { Username = "testuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act
        var result = _controller.Register(newUser, "password123");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void Register_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Arrange
        var invalidUser = new User { Username = "", Email = "test@test.com", PasswordHash = "pass123" };

        // Act
        var result = _controller.Register(invalidUser, "pass123");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // ========== LOGIN TESTS ==========

    [Test]
    public void Login_Get_ReturnsView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }


    [Test]
    public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
    {
        // Arrange
        _mockAuthService.Setup(s => s.Login("testuser", "wrongpassword"))
            .Returns((User?)null);

        // Act
        var result = await _controller.Login("testuser", "wrongpassword");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
        _mockAuthService.Verify(s => s.Login("testuser", "wrongpassword"), Times.Once);
    }

    [Test]
    public async Task Login_Post_NonExistingUser_ReturnsViewWithError()
    {
        // Arrange
        _mockAuthService.Setup(s => s.Login("nonexistent", "password"))
            .Returns((User?)null);

        // Act
        var result = await _controller.Login("nonexistent", "password");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
    }

    [Test]
    public async Task Login_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Act
        var result = await _controller.Login("", "password");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Login_Post_EmptyPassword_ReturnsViewWithError()
    {
        // Act
        var result = await _controller.Login("testuser", "");

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    // ========== PROFILE TESTS ==========

    [Test]
    public void Profile_WhenAuthenticated_ReturnsViewWithUser()
    {
        // Arrange
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

        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns(user);

        // Act
        var result = _controller.Profile();

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        var model = viewResult.Model as UserDto;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public void Profile_WhenNotAuthenticated_RedirectsToLogin()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act
        var result = _controller.Profile();

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
    }

    [Test]
    public void Profile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
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

        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns((User?)null);

        // Act
        var result = _controller.Profile();

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
