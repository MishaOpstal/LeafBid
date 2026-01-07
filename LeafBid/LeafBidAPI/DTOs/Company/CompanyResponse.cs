namespace LeafBidAPI.DTOs.Company;

public class CompanyResponse
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string HouseNumber { get; set; }
    public required string HouseNumberSuffix { get; set; }
    public required string PostalCode { get; set; }
    public required string CountryCode { get; set; }
}