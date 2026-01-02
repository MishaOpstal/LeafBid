namespace LeafBidAPI.DTOs.User;

public class LoginUserDto
{
    /// <summary>
    /// Data required to login a user
    /// </summary>

    public required string Email { get; set; } // Can also be the username

    public required string Password { get; set; }
    public bool Remember { get; set; } = false;
}