using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LeafBidAPITest.Helpers;

public static class DummyUsers
{
    /// <summary>
    /// Create a basic UserManager mock without any role configuration.
    /// You can further configure methods in your tests as needed.
    /// </summary>
    public static Mock<UserManager<User>> CreateUserManagerMock()
    {
        Mock<IUserStore<User>> store = new();

        Mock<UserManager<User>> userManagerMock = new(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );

        return userManagerMock;
    }

    /// <summary>
    /// Convenience wrapper that returns the concrete UserManager instance.
    /// Prefer using CreateUserManagerMock() when you need to configure behavior.
    /// </summary>
    public static UserManager<User> CreateFakeUserManager()
    {
        return CreateUserManagerMock().Object;
    }

    /// <summary>
    /// Create a UserManager mock with roles configured per user.
    /// The key is the User instance, the value is the list of roles for that user.
    /// </summary>
    public static Mock<UserManager<User>> CreateUserManagerMockWithRoles(
        IDictionary<User, IList<string>> userRoles)
    {
        Mock<IUserRoleStore<User>> store = new();

        foreach ((User user, IList<string> roles) in userRoles)
        {
            store
                .Setup(s => s.GetRolesAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(roles);
        }

        Mock<UserManager<User>> userManagerMock = new(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<User>>(),
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<User>>>()
        );

        userManagerMock
            .Setup(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        return userManagerMock;
    }

    /// <summary>
    /// Convenience wrapper: create a UserManager where the given user has the specified roles.
    /// For example: CreateUserManagerMockForSingleUserWithRoles(user, new[] { "Buyer", "Provider" }).
    /// </summary>
    public static Mock<UserManager<User>> CreateUserManagerMockForSingleUserWithRoles(
        User user,
        IList<string> roles)
    {
        IDictionary<User, IList<string>> map =
            new Dictionary<User, IList<string>> { { user, roles } };

        return CreateUserManagerMockWithRoles(map);
    }

    /// <summary>
    /// Helper to create a simple user for tests.
    /// </summary>
    public static User CreateUser(
        string id,
        string userName,
        string email)
    {
        User user = new()
        {
            Id = id,
            UserName = userName,
            Email = email
        };

        return user;
    }
}
