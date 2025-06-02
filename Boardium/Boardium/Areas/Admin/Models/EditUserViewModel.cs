namespace Boardium.Areas.Admin.Models;

public class EditUserViewModel
{
    public string Id { get; set; }
    public string Email { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    public List<string?> Roles { get; set; } = new();
    public string? SelectedRole { get; set; }
}