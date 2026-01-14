using LeafBidAPI.Data;
using LeafBidAPI.DTOs.Company;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPI.Controllers.v2;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("2.0")]
[AllowAnonymous]
[Produces("application/json")]
public class CompanyController(ApplicationDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Get all companies.
    /// </summary>
    /// <returns>A list of all companies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Auction>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Company>>> GetCompanies()
    {
        List<Company> companies = await dbContext.Companies.ToListAsync();
        return Ok(companies);
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    /// <returns>an ok result</returns>
    [HttpPut("CreateCompany")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Auction), StatusCodes.Status200OK)]
    public async Task<ActionResult<Company>> CreateCompany( [FromBody] CreateCompanyRequest cCreateR)
    {
        Company company = new()
        {
            Name = cCreateR.Name,
            Street = cCreateR.Street,
            City = cCreateR.City,
            CountryCode = cCreateR.CountryCode,
            HouseNumber = cCreateR.HouseNumber,
            PostalCode = cCreateR.PostalCode,
            HouseNumberSuffix = cCreateR.HouseNumberSuffix               
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(); 
            
        return Ok();
    }
}