using System.Security.Claims;
using LeafBidAPI.Data;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Services;
using LeafBidAPITest.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LeafBidAPITest.Services;

public sealed class UserServiceTest
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly UserService _userService;

    public UserServiceTest()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _userManagerMock = DummyUsers.CreateUserManagerMock();

        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        Mock<IUserClaimsPrincipalFactory<User>> claimsFactoryMock = new();

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            httpContextAccessorMock.Object,
            claimsFactoryMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<User>>()
        );

        _roleServiceMock = new Mock<IRoleService>();

        _userService = new UserService(
            _context,
            _signInManagerMock.Object,
            _userManagerMock.Object,
            _roleServiceMock.Object
        );
    }

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        // Arrange
        User user1 = DummyUsers.CreateUser("1", "Alice", "alice@example.com");
        User user2 = DummyUsers.CreateUser("2", "Bob", "bob@example.com");

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        List<User> result = await _userService.GetUsers();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == "1");
        Assert.Contains(result, u => u.Id == "2");
    }

    [Theory]
    [InlineData("missing-1")]
    [InlineData("missing-2")]
    public async Task GetUserById_ThrowsNotFound_WhenUserDoesNotExist(string id)
    {
        // Arrange: no users added

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetUserById(id));
    }

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenUserExists()
    {
        // Arrange
        User user = DummyUsers.CreateUser("42", "Existing", "existing@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        User result = await _userService.GetUserById("42");

        // Assert
        Assert.Equal("42", result.Id);
        Assert.Equal("Existing", result.UserName);
        Assert.Equal("existing@example.com", result.Email);
    }

    [Fact]
    public async Task RegisterUser_ThrowsPasswordMismatch_WhenPasswordsDoNotMatch()
    {
        // Arrange
        CreateUserDto dto = new()
        {
            UserName = "NewUser",
            Email = "new@example.com",
            Password = "Password123!",
            PasswordConfirmation = "DifferentPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PasswordMismatchException>(() => _userService.RegisterUser(dto));

        _userManagerMock.Verify(
            m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RegisterUser_ThrowsUserCreationFailed_WhenCreateAsyncFails()
    {
        // Arrange
        CreateUserDto dto = new()
        {
            UserName = "NewUser",
            Email = "new@example.com",
            Password = "Password123!",
            PasswordConfirmation = "Password123!"
        };

        IdentityResult failed = IdentityResult.Failed(new IdentityError { Description = "create failed" });

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(failed);

        // Act & Assert
        await Assert.ThrowsAsync<UserCreationFailedException>(() => _userService.RegisterUser(dto));
    }

    [Fact]
    public async Task RegisterUser_ThrowsUserCreationFailed_WhenAddToRolesFails()
    {
        // Arrange
        CreateUserDto dto = new()
        {
            UserName = "NewUser",
            Email = "new@example.com",
            Password = "Password123!",
            PasswordConfirmation = "Password123!",
            Roles = new[] { "Buyer" }
        };

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRolesAsync(It.IsAny<User>(), dto.Roles!))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "roles failed" }));

        // Act & Assert
        await Assert.ThrowsAsync<UserCreationFailedException>(() => _userService.RegisterUser(dto));
    }

    [Fact]
    public async Task RegisterUser_Succeeds_AndAssignsRoles_WhenRolesProvided()
    {
        // Arrange
        string[] roles = { "Buyer", "Provider" };

        CreateUserDto dto = new()
        {
            UserName = "NewUser",
            Email = "new@example.com",
            Password = "Password123!",
            PasswordConfirmation = "Password123!",
            Roles = roles
        };

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRolesAsync(It.IsAny<User>(), dto.Roles!))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        User result = await _userService.RegisterUser(dto);

        // Assert
        Assert.Equal(dto.UserName, result.UserName);
        Assert.Equal(dto.Email, result.Email);

        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), dto.Password), Times.Once);
        _userManagerMock.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), dto.Roles!), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ThrowsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        UpdateUserDto dto = new()
        {
            UserName = "UpdatedName"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUser("missing-id", dto));
    }

    [Fact]
    public async Task UpdateUser_ThrowsUserUpdateFailed_WhenSetUserNameFails()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "OldName", "old@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        UpdateUserDto dto = new()
        {
            UserName = "NewName"
        };

        _userManagerMock
            .Setup(m => m.SetUserNameAsync(user, dto.UserName))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "username failed" }));

        // Act & Assert
        await Assert.ThrowsAsync<UserUpdateFailedException>(() => _userService.UpdateUser("1", dto));
    }

    [Fact]
    public async Task UpdateUser_ThrowsUserUpdateFailed_WhenSetEmailFails()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "OldName", "old@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        UpdateUserDto dto = new()
        {
            Email = "new@example.com"
        };

        _userManagerMock
            .Setup(m => m.SetEmailAsync(user, dto.Email))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "email failed" }));

        // Act & Assert
        await Assert.ThrowsAsync<UserUpdateFailedException>(() => _userService.UpdateUser("1", dto));
    }

    [Fact]
    public async Task UpdateUser_ThrowsUserUpdateFailed_WhenResetPasswordFails()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "OldName", "old@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        UpdateUserDto dto = new()
        {
            Password = "NewPassword123!"
        };

        const string token = "reset-token";

        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(token);

        _userManagerMock
            .Setup(m => m.ResetPasswordAsync(user, token, dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "password failed" }));

        // Act & Assert
        await Assert.ThrowsAsync<UserUpdateFailedException>(() => _userService.UpdateUser("1", dto));
    }

    [Fact]
    public async Task UpdateUser_LoggedIn_ThrowsNotFound_WhenUserIsNull()
    {
        // Arrange
        ClaimsPrincipal principal = new();

        _userManagerMock
            .Setup(m => m.GetUserAsync(principal))
            .ReturnsAsync((User?)null);

        UpdateUserDto dto = new()
        {
            UserName = "NewName"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUser(principal, dto));
    }

    [Fact]
    public async Task LoginUser_ThrowsNotFound_WhenUserNotFound()
    {
        // Arrange
        LoginUserDto dto = new()
        {
            Email = "missing@example.com",
            Password = "Password123!"
        };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.LoginUser(dto));
    }

    [Fact]
    public async Task LoginUser_ThrowsUnauthorized_WhenSignInFails()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");

        LoginUserDto dto = new()
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.Remember, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _userService.LoginUser(dto));
    }

    [Fact]
    public async Task LoginUser_Succeeds_AndUpdatesLastLogin()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");

        LoginUserDto dto = new()
        {
            Email = "user@example.com",
            Password = "CorrectPassword",
            Remember = true
        };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.Remember, false))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        User result = await _userService.LoginUser(dto);

        // Assert
        Assert.Equal(user, result);
        Assert.NotNull(result.LastLogin);
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task LogoutUser_ThrowsNotFound_WhenUserIsNull()
    {
        // Arrange
        ClaimsPrincipal principal = new();

        _userManagerMock
            .Setup(m => m.GetUserAsync(principal))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.LogoutUser(principal));
    }

    [Fact]
    public async Task LogoutUser_SignsOutAndReturnsTrue()
    {
        // Arrange
        ClaimsPrincipal principal = new();
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");

        _userManagerMock
            .Setup(m => m.GetUserAsync(principal))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.SignOutAsync())
            .Returns(Task.CompletedTask);

        // Act
        bool result = await _userService.LogoutUser(principal);

        // Assert
        Assert.True(result);
        _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task GetLoggedInUser_ThrowsNotFound_WhenUserIsNull()
    {
        // Arrange
        ClaimsPrincipal principal = new();

        _userManagerMock
            .Setup(m => m.GetUserAsync(principal))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.GetLoggedInUser(principal));
    }

    [Fact]
    public async Task GetLoggedInUser_ReturnsLoggedInUserResponse_WithRoles()
    {
        // Arrange
        ClaimsPrincipal principal = new();
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");

        IList<string> roles = new List<string> { "Buyer", "Provider" };

        _userManagerMock
            .Setup(m => m.GetUserAsync(principal))
            .ReturnsAsync(user);

        _roleServiceMock
            .Setup(r => r.GetRolesForUser(user))
            .ReturnsAsync(roles);

        // Act
        LoggedInUserResponse result = await _userService.GetLoggedInUser(principal);

        // Assert
        Assert.True(result.LoggedIn);
        Assert.NotNull(result.UserData);
        Assert.Equal(user.Id, result.UserData.Id);
        Assert.Equal(roles, result.UserData.Roles);
    }

    [Fact]
    public async Task DeleteUser_ThrowsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        const string missingId = "missing-id";

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _userService.DeleteUser(missingId));
    }

    [Fact]
    public async Task DeleteUser_ReturnsTrue_WhenDeletionSucceeds()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        bool result = await _userService.DeleteUser("1");

        // Assert
        Assert.True(result);
        _userManagerMock.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenDeletionFails()
    {
        // Arrange
        User user = DummyUsers.CreateUser("1", "User", "user@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userManagerMock
            .Setup(m => m.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        bool result = await _userService.DeleteUser("1");

        // Assert
        Assert.False(result);
    }
}