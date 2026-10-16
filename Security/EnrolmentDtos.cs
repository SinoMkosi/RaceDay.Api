using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class EnrolDto
    {
        [Required] public int CategoryId { get; set; }
    }
}