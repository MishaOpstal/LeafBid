using System.Security.Claims;
using LeafBidAPI.Data;
using LeafBidAPI.DTOs.User;
using LeafBidAPI.Exceptions;
using LeafBidAPI.Interfaces;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Services;

public class UserService(
    ApplicationDbContext context,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IRoleService roleService) : IUserService
{
    public async Task<List<User>> GetUsers()
    {
        return await context.Users.ToListAsync();
    }
    
    public async Task<User> GetUserById(string id)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return user;
    }
    
    public async Task<User> RegisterUser(CreateUserDto userData)
    {
        User user = new()
        {
            UserName = userData.UserName,
            Email = userData.Email
        };

        if (userData.Password != userData.PasswordConfirmation)
        {
            throw new PasswordMismatchException("Passwords do not match");
        }

        IdentityResult result = await userManager.CreateAsync(user, userData.Password);
        
        if (!result.Succeeded)
        {
            throw new UserCreationFailedException("User creation failed, error: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (userData.Roles != null && !Array.Empty<string>().Equals(userData.Roles))
        {
            result = await userManager.AddToRolesAsync(user, userData.Roles);
        }

        if (!result.Succeeded)
        {
            throw new UserCreationFailedException("User creation failed");
        }

        return user;
    }
    
    public async Task<User> UpdateUser(string id, UpdateUserDto updatedUser)
    {
        User? user = context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!string.IsNullOrEmpty(updatedUser.UserName))
        {
            IdentityResult updateUserNameResult = await userManager.SetUserNameAsync(user, updatedUser.UserName);

            if (!updateUserNameResult.Succeeded)
            {
                throw new UserUpdateFailedException("Failed to update username");
            }
        }

        if (!string.IsNullOrEmpty(updatedUser.Email))
        {
            IdentityResult updateEmailResult = await userManager.SetEmailAsync(user, updatedUser.Email);

            if (!updateEmailResult.Succeeded)
            {
                throw new UserUpdateFailedException("Failed to update email");
            }
        }

        if (!string.IsNullOrEmpty(updatedUser.Password))
        {
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult resetResult = await userManager.ResetPasswordAsync(user, token, updatedUser.Password);

            if (!resetResult.Succeeded)
            {
                throw new UserUpdateFailedException("Failed to update password");
            }
        }

        return user;
    }
    
    public async Task<User> UpdateUser(ClaimsPrincipal loggedInUser, UpdateUserDto updatedUser)
    {
        User? user = await userManager.GetUserAsync(loggedInUser);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return await UpdateUser(user.Id, updatedUser);
    }
    
    public async Task<User> LoginUser(LoginUserDto loginData)
    {
        // Check for email, then username if email wasn't found.
        User? user = await userManager.FindByEmailAsync(loginData.Email)
            ?? await userManager.FindByNameAsync(loginData.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        SignInResult result = await signInManager.PasswordSignInAsync(
            user,
            loginData.Password,
            loginData.Remember,
            false
        );

        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        user.LastLogin = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return user;
    }
    
    public async Task<User> VerifyUser(User user)
    {
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
        return user;
    }
    
    public async Task<bool> LogoutUser(ClaimsPrincipal loggedInUser)
    {
        User? user = await userManager.GetUserAsync(loggedInUser);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        await signInManager.SignOutAsync();
        return true;
    }
    
    public async Task<LoggedInUserResponse> GetLoggedInUser(ClaimsPrincipal loggedInUser)
    {
        User? user = await userManager.GetUserAsync(loggedInUser);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        IList<string> roles = await roleService.GetRolesForUser(user);
        UserResponse userResponse = CreateUserResponse(user, roles);

        LoggedInUserResponse loggedInUserResponse = new()
        {
            LoggedIn = true,
            UserData = userResponse
        };

        return loggedInUserResponse;
    }
    
    public async Task<bool> DeleteUser(string id)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        IdentityResult result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }
    
    public UserResponse CreateUserResponse(User user, IList<string> roles)
    {
        UserResponse userResponse = new()
        {
            LastLogin = user.LastLogin,
            Id = user.Id,
            AccessFailedCount = user.AccessFailedCount,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            NormalizedEmail = user.NormalizedEmail ?? string.Empty,
            NormalizedUserName = user.NormalizedUserName ?? string.Empty,
            Roles = roles
        };

        return userResponse;
    }
}
