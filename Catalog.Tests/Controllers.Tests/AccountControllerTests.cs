using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using catalog.Controllers;
using catalog.Entities;
using catalog.Services;
using NUnit.Framework;

namespace Catalog.Tests.ControllersTests;

// Unit tests for AccountController covering Register, Login, Logout and Profile behaviours.
[TestFixture]
public class AccountControllerTests
{
    // Mock of the authentication service used by the controller.
    private Mock<IAuthService> _mockAuthService;

    // Controller instance under test.
    private AccountController _controller;

    // In-memory list of users used as test data / fake datastore.
    private List<User> _testUsers;

    // Runs before each test: create mocks, controller and seed test data.
    [SetUp]
    public void SetUp()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AccountController(_mockAuthService.Object);

        // Seed test users for scenarios that require existing users.
        _testUsers = new List<User>
        {
            new User { Id = 1, Username = "testuser", Email = "test@test.com", PasswordHash = "hashed123" },
            new User { Id = 2, Username = "john", Email = "john@test.com", PasswordHash = "hashed456" }
        };
    }

    // Runs after each test: dispose controller and clear references to allow GC and avoid cross-test pollution.
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
    // Test that GET /Account/Register returns the registration view.
    [Test]
    public void Register_Get_ReturnsView()
    {
        // Act: call the Register (GET) action.
        var result = _controller.Register();

        // Assert: the result is a view so registration page is returned.
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    // Test that posting a valid new user triggers register and redirects to Login.
    [Test]
    public void Register_Post_ValidUser_RedirectsToLogin()
    {
        // Arrange: create a new user that does not exist yet.
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };

        // Setup mock to return null for existing user check (user doesn't exist).
        _mockAuthService.Setup(s => s.GetByUsername("newuser"))
            .Returns((User?)null);

        // Capture Register calls by adding the created user to our in-memory list.
        _mockAuthService.Setup(s => s.Register(It.IsAny<User>()))
            .Callback<User>(u => _testUsers.Add(u));

        // Act: perform the POST Register with matching confirm password.
        var result = _controller.Register(newUser, "password123");

        // Assert: expect a redirect to the Login action and that Register was called once.
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
        _mockAuthService.Verify(s => s.Register(It.Is<User>(x => x.Username == "newuser")), Times.Once);
    }

    // Test that posting with non-matching passwords returns the view and sets ModelState error.
    [Test]
    public void Register_Post_PasswordMismatch_ReturnsViewWithError()
    {
        // Arrange: create a user where confirm password will not match.
        var newUser = new User { Username = "newuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act: call Register with a different confirm password.
        var result = _controller.Register(newUser, "wrongpassword");

        // Assert: should return the view with the same model and ModelState should be invalid.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.Model, Is.EqualTo(newUser));
        Assert.That(_controller.ModelState.IsValid, Is.False);

        // Ensure Register on the service was not called due to validation failure.
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // Test that attempting to register with an existing username returns the view with error.
    [Test]
    public void Register_Post_ExistingUsername_ReturnsViewWithError()
    {
        // Arrange: configure mock to return an existing user when checking username.
        var existingUser = _testUsers[0];
        _mockAuthService.Setup(s => s.GetByUsername("testuser"))
            .Returns(existingUser);

        var newUser = new User { Username = "testuser", Email = "new@test.com", PasswordHash = "password123" };

        // Act: attempt to register with an existing username.
        var result = _controller.Register(newUser, "password123");

        // Assert: should return view and ModelState should be invalid; Register should not be called.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // Test that registering with empty username triggers validation and returns the view with error.
    [Test]
    public void Register_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Arrange: invalid user missing username.
        var invalidUser = new User { Username = "", Email = "test@test.com", PasswordHash = "pass123" };

        // Act: call Register with invalid model.
        var result = _controller.Register(invalidUser, "pass123");

        // Assert: view is returned and ModelState is invalid; Register should not be invoked.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        _mockAuthService.Verify(s => s.Register(It.IsAny<User>()), Times.Never);
    }

    // ========== LOGIN TESTS ==========
    // Test that GET /Account/Login returns the login view.
    [Test]
    public void Login_Get_ReturnsView()
    {
        // Act: call the Login (GET) action.
        var result = _controller.Login();

        // Assert: the action returns a view.
        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    // Test that posting invalid credentials results in showing the login view with an error message.
    [Test]
    public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
    {
        // Arrange: configure mock to return null for wrong credentials.
        _mockAuthService.Setup(s => s.Login("testuser", "wrongpassword"))
            .Returns((User?)null);

        // Act: call Login POST with wrong password.
        var result = await _controller.Login("testuser", "wrongpassword");

        // Assert: expect view returned and ViewData["Error"] set to the expected message.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
        _mockAuthService.Verify(s => s.Login("testuser", "wrongpassword"), Times.Once);
    }

    // Test that logging in with a non-existent user returns the login view with an error.
    [Test]
    public async Task Login_Post_NonExistingUser_ReturnsViewWithError()
    {
        // Arrange: mock returns null when user does not exist.
        _mockAuthService.Setup(s => s.Login("nonexistent", "password"))
            .Returns((User?)null);

        // Act: attempt to login.
        var result = await _controller.Login("nonexistent", "password");

        // Assert: view returned and error message set.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Невалидно потребителско име или парола"));
    }

    // Test that empty username input returns the login view with an input error and does not call the service.
    [Test]
    public async Task Login_Post_EmptyUsername_ReturnsViewWithError()
    {
        // Act: call Login with empty username.
        var result = await _controller.Login("", "password");

        // Assert: view returned, appropriate error message set, and Login service was not called.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // Test that empty password input returns the login view with an input error and does not call the service.
    [Test]
    public async Task Login_Post_EmptyPassword_ReturnsViewWithError()
    {
        // Act: call Login with empty password.
        var result = await _controller.Login("testuser", "");

        // Assert: view returned, error message set, and Login service was not called.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(_controller.ViewData["Error"], Is.EqualTo("Моля, въведете потребителско име и парола"));
        _mockAuthService.Verify(s => s.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    // ========== PROFILE TESTS ==========
    // Test that Profile returns the user's data when the user is authenticated.
    [Test]
    public void Profile_WhenAuthenticated_ReturnsViewWithUser()
    {
        // Arrange: create authenticated principal with claims for the test user.
        var user = _testUsers[0];
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // Attach the principal to the controller's HttpContext so User is populated.
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Mock the service to return the user when searched by username.
        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns(user);

        // Act: call Profile action.
        var result = _controller.Profile();

        // Assert: view returned containing the expected user model.
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        var model = viewResult.Model as User;
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Username, Is.EqualTo("testuser"));
    }

    // Test that Profile redirects to Login when no user is authenticated.
    [Test]
    public void Profile_WhenNotAuthenticated_RedirectsToLogin()
    {
        // Arrange: set an empty HttpContext with no authenticated user.
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act: call Profile.
        var result = _controller.Profile();

        // Assert: expect redirect to the Login action.
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult, Is.Not.Null);
        Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
    }

    // Test that Profile returns NotFound when the authenticated user cannot be found in the data store.
    [Test]
    public void Profile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange: authenticate a principal for an existing username.
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

        // Mock the service to return null (user not found).
        _mockAuthService.Setup(s => s.GetByUsername(user.Username))
            .Returns((User?)null);

        // Act: call Profile.
        var result = _controller.Profile();

        // Assert: expect a 404 NotFound result when user is missing.
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
}
                                                                                                                                                