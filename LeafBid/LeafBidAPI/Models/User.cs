using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace LeafBidAPI.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Last Login
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// Company Id
    /// </summary>
    public int? CompanyId { get; set; }

    [JsonIgnore] public Company? Company { get; set; }
}