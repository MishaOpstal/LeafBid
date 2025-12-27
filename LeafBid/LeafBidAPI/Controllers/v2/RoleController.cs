using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeafBidAPI.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
// [Authorize]
[AllowAnonymous]
[Produces("application/json")]
public class RoleController(IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Get all roles.
    /// </summary>
    /// <returns>A list of all roles.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<IdentityRole>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IdentityRole>>> GetRoles()
    {
        List<IdentityRole> roles = await roleService.GetRoles();
        return Ok(roles);
    }

    /// <summary>
    /// Check whether a user has a given role.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleName">The role name to check.</param>
    /// <returns><c>true</c> if the user has the role; otherwise <c>false</c>.</returns>
    [HttpGet("users/{userId}/has")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> GetUserHasRole(
        [FromRoute] string userId,
        [FromQuery] string roleName)
    {
        try
        {
            bool hasRole = await roleService.GetUserHasRole(userId, roleName);
            return Ok(hasRole);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Get all users associated with a given role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A list of users that have the given role.</returns>
    [HttpGet("{roleName}/users")]
    [ProducesResponseType(typeof(IList<User>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<User>>> GetUsersByRole([FromRoute] string roleName)
    {
        IList<User> users = await roleService.GetUsersByRole(roleName);
        return Ok(users);
    }

    /// <summary>
    /// Assign roles to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleNames">The roles to assign.</param>
    /// <returns>No content if assignment succeeded.</returns>
    [HttpPost("users/{userId}/roles")]
    [ProducesResponseType(typeof(IList<IdentityRole>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoles(
        [FromRoute] string userId,
        [FromBody] string[] roleNames)
    {
        try
        {
            bool ok = await roleService.AssignRoles(userId, roleNames);
            return ok ? NoContent() : Problem("Assign roles failed.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    /// <summary>
    /// Revoke roles from a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="roleNames">The roles to revoke.</param>
    /// <returns>No content if revocation succeeded.</returns>
    [HttpDelete("users/{userId}/roles")]
    [ProducesResponseType(typeof(IList<IdentityRole>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeRoles(
        [FromRoute] string userId,
        [FromBody] string[] roleNames)
    {
        try
        {
            bool ok = await roleService.RevokeRoles(userId, roleNames);
            return ok ? NoContent() : Problem("Revoke roles failed.");
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}