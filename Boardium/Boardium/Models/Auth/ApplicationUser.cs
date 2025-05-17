using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Boardium.Models.Auth;

public class ApplicationUser:IdentityUser
{
    [MaxLength(64)]
    [Required]
    public string FirstName { get; set; }
    [MaxLength(64)]
    [Required]
    public string LastName { get; set; }
    [MaxLength(64)]
    public string AddressLine { get; set; }
    [MaxLength(32)]
    public string City { get; set; }
    [MaxLength(16)]
    public string PostalCode { get; set; }

    public ICollection<Rental.Rental> Rentals { get; set; } = new List<Rental.Rental>();
}