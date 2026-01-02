using System.Text;
using Bogus;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Data.seeders;

public class UserSeeder(
    ApplicationDbContext dbContext,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IHostEnvironment env
) : ISeeder
{
    private const string LogRelativePath = "logs/seededUsers.log";

    public async Task SeedAsync()
    {
        string logPath = EnsureAndGetLogPath();
        await LogHeaderAsync(logPath);

        Faker faker = new();
        int[] companyIds = await dbContext.Companies.Select(c => c.Id).ToArrayAsync();

        IdentityRole[] roles = roleManager.Roles.Where(r => r.Name != null).ToArray();

        foreach (IdentityRole role in roles)
        {
            string roleName = role.Name!;

            for (int i = 0; i < 5; i++)
            {
                string firstName = faker.Person.FirstName;
                string lastName = faker.Person.LastName;
                
                string email = faker.Internet.Email(firstName, lastName);
                string userName = faker.Internet.UserName(firstName, lastName);
                string password = GenerateCompliantPassword(faker);
                int? companyId = companyIds.Length == 0 ? null : faker.Random.ArrayElement(companyIds);

                (bool created, string? error) = await SeedUserAsync(
                    userManager,
                    email,
                    userName,
                    password,
                    roleName,
                    companyId
                );

                if (created)
                {
                    await LogLineAsync(
                        logPath,
                        $"[{DateTime.UtcNow:HH:mm:ss}] USER CREATED | Role: {roleName} | Username: {userName} | Email: {email} | Password: {password}"
                    );
                }
                else
                {
                    await LogLineAsync(
                        logPath,
                        $"[{DateTime.UtcNow:HH:mm:ss}] USER CREATE FAILED | Role: {roleName} | Username: {userName} | Email: {email} | Reason: {error ?? "Unknown"}"
                    );
                }
            }
        }
    }

    private static async Task<(bool Created, string? Error)> SeedUserAsync(
        UserManager<User> userManager,
        string email,
        string userName,
        string password,
        string role,
        int? companyId)
    {
        if (await userManager.FindByEmailAsync(email) != null)
        {
            return (false, "Email already exists");
        }

        User user = new()
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            CompanyId = companyId
        };

        IdentityResult createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            string errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
            return (false, errors);
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            string errors = string.Join(" | ", roleResult.Errors.Select(e => e.Description));
            return (false, $"Failed to add role: {errors}");
        }

        return (true, null);
    }

    private static string GenerateCompliantPassword(Faker faker)
    {
        string tail = faker.Random.String2(12, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
        return $"Aa1!{tail}";
    }

    private string EnsureAndGetLogPath()
    {
        string logPath = Path.Combine(env.ContentRootPath, LogRelativePath);
        string? directory = Path.GetDirectoryName(logPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(logPath))
        {
            File.WriteAllText(logPath, string.Empty);
        }

        return logPath;
    }

    private static async Task LogHeaderAsync(string logPath)
    {
        StringBuilder sb = new();

        if (new FileInfo(logPath).Length > 0)
        {
            sb.AppendLine();
        }

        DateTime timestamp = DateTime.UtcNow;

        sb.AppendLine("========================================");
        sb.AppendLine($"SEEDING USERS AT {timestamp:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("========================================");

        await File.AppendAllTextAsync(logPath, sb.ToString());
    }

    private static Task LogLineAsync(string logPath, string line)
    {
        return File.AppendAllTextAsync(logPath, line + Environment.NewLine);
    }
}
