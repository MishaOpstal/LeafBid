using LeafBidAPI.Data;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class RoleService(
    ApplicationDbContext context,
    UserManager<User> userManager) : IRoleService
{
    public async Task<List<IdentityRole>> GetRoles()
    {
        return await context.Roles.ToListAsync();
    }

    public async Task<IList<string>> GetRolesForUser(User user)
    {
        return await userManager.GetRolesAsync(user);
    }

    public async Task<bool> GetUserHasRole(string userId, string roleName)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        return roles.Contains(roleName);
    }

    public async Task<IList<User>> GetUsersByRole(string roleName)
    {
        return await userManager.GetUsersInRoleAsync(roleName);
    }

    public async Task<bool> AssignRoles(string userId, string[] roleNames)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        IdentityResult result = await userManager.AddToRolesAsync(user, roleNames);
        return result.Succeeded;
    }

    public async Task<bool> RevokeRoles(string userId, string[] roleNames)
    {
        User? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        IdentityResult result = await userManager.RemoveFromRolesAsync(user, roleNames);
        return result.Succeeded;
    }
}