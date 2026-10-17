using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class CategoryDto
    {
        [Required] public string Name { get; set; } = string.Empty; // e.g. "Under 20", "10km"
    }
}