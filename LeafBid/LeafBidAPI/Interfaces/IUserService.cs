using System.Security.Claims;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;

namespace LeafBidAPI.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    Task<List<User>> GetUsers();

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user with the specified ID.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    Task<User> GetUserById(string id);

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="userData">The data for the new user, including password and confirmation.</param>
    /// <returns>The created user entity.</returns>
    /// <exception cref="PasswordMismatchException">
    /// Thrown when the provided password and password confirmation do not match.
    /// </exception>
    /// <exception cref="UserCreationFailedException">
    /// Thrown when user creation or role assignment fails in the identity system.
    /// </exception>
    Task<User> RegisterUser(CreateUserDto userData);

    /// <summary>
    /// Update an existing user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to update.</param>
    /// <param name="updatedUser">The updated user data (username, email, password).</param>
    /// <returns>The updated user entity.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    /// <exception cref="UserUpdateFailedException">
    /// Thrown when updating the username, email, or password fails in the identity system.
    /// </exception>
    Task<User> UpdateUser(string id, UpdateUserDto updatedUser);
    
    /// <summary>
    /// Update the currently logged-in user.
    /// </summary>
    /// <param name="loggedInUser">The claims principal of the currently authenticated user.</param>
    /// <param name="updatedUser">The updated user data.</param>
    /// <returns>The updated user entity.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the current user cannot be resolved from the principal.
    /// </exception>
    Task<User> UpdateUser(ClaimsPrincipal loggedInUser, UpdateUserDto updatedUser);

    /// <summary>
    /// Login a user with email and password.
    /// </summary>
    /// <param name="loginData">The login data containing email, password, and remember-me flag.</param>
    /// <returns>The authenticated user entity.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified email.
    /// </exception>
    /// <exception cref="UnauthorizedException">
    /// Thrown when the password is invalid or sign-in fails.
    /// </exception>
    Task<User> LoginUser(LoginUserDto loginData);
    
    /// <summary>
    /// Verify a user's email address.
    /// </summary>
    /// <param name="user">The user to verify.</param>
    /// <returns>The authenticated user entity.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found.
    /// </exception>
    Task<User> VerifyUser(User user);

    /// <summary>
    /// Logout the currently logged-in user.
    /// </summary>
    /// <param name="loggedInUser">The claims principal of the currently authenticated user.</param>
    /// <returns><c>true</c> if logout completed successfully.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the current user cannot be resolved from the principal.
    /// </exception>
    Task<bool> LogoutUser(ClaimsPrincipal loggedInUser);

    /// <summary>
    /// Retrieve data for the currently logged-in user.
    /// </summary>
    /// <param name="loggedInUser">The claims principal of the currently authenticated user.</param>
    /// <returns>
    /// A <see cref="LoggedInUserResponse"/> containing the login state
    /// and user data (including roles).
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when the current user cannot be resolved from the principal.
    /// </exception>
    Task<LoggedInUserResponse> GetLoggedInUser(ClaimsPrincipal loggedInUser);

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>
    /// <c>true</c> if the user was deleted successfully;
    /// otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    Task<bool> DeleteUser(string id);

    /// <summary>
    /// Map a <see cref="User"/> entity to a <see cref="UserResponse"/> DTO.
    /// </summary>
    /// <param name="user">The user entity to map.</param>
    /// <param name="roles">The list of role names assigned to the user.</param>
    /// <returns>A populated <see cref="UserResponse"/>.</returns>
    UserResponse CreateUserResponse(User user, IList<string> roles);
}