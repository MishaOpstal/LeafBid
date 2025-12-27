namespace LeafBidAPI.DTOs.User;

public class UpdateUserDto
{
    /// <summary>
    /// Data required to update a user
    /// </summary>
    public string? UserName { get; set; }

    public string? Email { get; set; }
    public string? Password { get; set; }
}