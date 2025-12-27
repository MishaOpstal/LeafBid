namespace LeafBidAPI.DTOs.User;

public class LoggedInUserResponse
{
    /// <summary>
    /// Data that gets send back to the front-end
    /// </summary>
    public required bool LoggedIn { get; set; }

    public UserResponse? UserData { get; set; }
}