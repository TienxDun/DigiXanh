using Microsoft.AspNetCore.Identity;

namespace DigiXanh.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}