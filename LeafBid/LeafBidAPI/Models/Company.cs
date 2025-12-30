namespace LeafBidAPI.Models;

/// <summary>
/// Represents a company in the system.
/// </summary>
public class Company
{
    /// <summary>
    /// Unique identifier for the company.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the company.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    public required string Street { get; set; }

    /// <summary>
    /// City of residence
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// House number (123)
    /// </summary>
    public required string HouseNumber { get; set; }

    /// <summary>
    /// Suffix for the house number 123(A)
    /// </summary>
    public required string HouseNumberSuffix { get; set; }

    /// <summary>
    /// Postal code (1234AB)
    /// </summary>
    public required string PostalCode { get; set; }

    /// <summary>
    /// Country Code (NL)
    /// </summary>
    public required string CountryCode { get; set; }

    /// <summary>
    /// Users associated with the company.
    /// </summary>
    public ICollection<User> Users { get; set; } = new List<User>();
}