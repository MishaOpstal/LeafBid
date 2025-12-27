using LeafBidAPI.Exceptions;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Interfaces;

public interface IRoleService
{
    /// <summary>
    /// Get all defined roles.
    /// </summary>
    /// <returns>A list of all roles.</returns>
    Task<List<IdentityRole>> GetRoles();

    /// <summary>
    /// Get all role names assigned to a specific user.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <returns>A list of role names assigned to the user.</returns>
    Task<IList<string>> GetRolesForUser(User user);

    /// <summary>
    /// Check whether a user has a given role.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="roleName">The role name to check for.</param>
    /// <returns>
    /// <c>true</c> if the user has the specified role;
    /// otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    Task<bool> GetUserHasRole(string userId, string roleName);

    /// <summary>
    /// Get all users associated with a given role.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    /// <returns>A list of users that have the specified role.</returns>
    Task<IList<User>> GetUsersByRole(string roleName);

    /// <summary>
    /// Assign roles to a user.
    /// </summary>
    /// <param name="userId">The ID of the user to assign roles to.</param>
    /// <param name="roleNames">The roles to assign.</param>
    /// <returns>
    /// <c>true</c> if the roles were successfully assigned;
    /// otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    Task<bool> AssignRoles(string userId, string[] roleNames);

    /// <summary>
    /// Revoke roles from a user.
    /// </summary>
    /// <param name="userId">The ID of the user to revoke roles from.</param>
    /// <param name="roleNames">The roles to revoke.</param>
    /// <returns>
    /// <c>true</c> if the roles were successfully revoked;
    /// otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// Thrown when no user is found with the specified ID.
    /// </exception>
    Task<bool> RevokeRoles(string userId, string[] roleNames);
}