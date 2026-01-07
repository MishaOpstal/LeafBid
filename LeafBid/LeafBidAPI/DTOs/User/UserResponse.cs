namespace LeafBidAPI.DTOs.User;

public class UserResponse
{
    public DateTime? LastLogin { get; set; }
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required bool EmailConfirmed { get; set; }
    public int? CompanyId { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public required bool LockoutEnabled { get; set; }
    public required int AccessFailedCount { get; set; }
    public IList<string>? Roles { get; set; }
}