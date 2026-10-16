using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class UpdateProfileDto
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    }
}