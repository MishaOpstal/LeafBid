using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using LeafBidAPI.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[Produces("application/json")]
public class UserController(IUserService userService, IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    [HttpGet]
    [Authorize(Policy = PolicyTypes.Users.ViewOthers)]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        List<User> users = await userService.GetUsers();

        List<UserResponse> userResponses = new(users.Count);

        foreach (User user in users)
        {
            IList<string> roles = await roleService.GetRolesForUser(user);
            UserResponse response = userService.CreateUserResponse(user, roles);
            userResponses.Add(response);
        }

        return Ok(userResponses);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The requested user.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = PolicyTypes.Users.ViewOthers)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        try
        {
            User user = await userService.GetUserById(id);
            UserResponse userResponse = userService.CreateUserResponse(
                user,
                await roleService.GetRolesForUser(user)
            );
            return Ok(userResponse);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="userData">The user registration data.</param>
    /// <returns>The created user.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> RegisterUser([FromBody] CreateUserDto userData)
    {
        try
        {
            User createdUser = await userService.RegisterUser(userData);
            UserResponse createdUserResponse = userService.CreateUserResponse(
                createdUser,
                await roleService.GetRolesForUser(createdUser)
            );

            // Set the email verified to true
            await userService.VerifyUser(createdUser);

            return CreatedAtAction(
                actionName: nameof(GetUser),
                routeValues: new { id = createdUserResponse.Id, version = "2.0" },
                value: createdUserResponse
            );
        }
        catch (PasswordMismatchException e)
        {
            return BadRequest(e.Message);
        }
        catch (UserCreationFailedException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Log in a user.
    /// </summary>
    /// <param name="login">The login credentials.</param>
    /// <returns>The logged-in user.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> LoginUser([FromBody] LoginUserDto login)
    {
        try
        {
            User user = await userService.LoginUser(login);
            UserResponse userResponse = userService.CreateUserResponse(
                user,
                await roleService.GetRolesForUser(user)
            );

            return Ok(userResponse);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedException e)
        {
            return Unauthorized(e.Message);
        }
    }

    /// <summary>
    /// Log out the current user.
    /// </summary>
    /// <returns>No content if logout succeeded.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> LogoutUser()
    {
        try
        {
            bool ok = await userService.LogoutUser(User);
            return ok ? NoContent() : Problem("Logout failed.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get the currently logged-in user.
    /// </summary>
    /// <returns>The currently logged-in user.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(LoggedInUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoggedInUserResponse>> LoggedInUser()
    {
        try
        {
            LoggedInUserResponse me = await userService.GetLoggedInUser(User);
            return Ok(me);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Update a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="updatedUser">The updated user data.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        string id,
        [FromBody] UpdateUserDto updatedUser)
    {
        try
        {
            User updated = await userService.UpdateUser(id, updatedUser);
            UserResponse userResponse = userService.CreateUserResponse(
                updated,
                await roleService.GetRolesForUser(updated)
            );

            return Ok(userResponse);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UserUpdateFailedException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Update the current user.
    /// </summary>
    /// <param name="updatedUser">The updated user data.</param>
    /// <returns>The updated user.</returns>
    [HttpPut]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser([FromBody] UpdateUserDto updatedUser)
    {
        try
        {
            User updated = await userService.UpdateUser(User, updatedUser);
            UserResponse userResponse = userService.CreateUserResponse(
                updated,
                await roleService.GetRolesForUser(updated)
            );
            return Ok(userResponse);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Delete a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>No content if deletion succeeded.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = PolicyTypes.Users.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            bool ok = await userService.DeleteUser(id);
            return ok ? NoContent() : Problem("Delete failed.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}