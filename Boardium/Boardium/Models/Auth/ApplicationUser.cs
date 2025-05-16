using Microsoft.AspNetCore.Identity;

namespace Boardium.Models.Auth;

public class ApplicationUser:IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string AddressLine { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }

    public ICollection<Rental.Rental> Rentals { get; set; } = new List<Rental.Rental>();
}