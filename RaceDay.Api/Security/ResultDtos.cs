using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class ResultDto
    {
        [Required] public int EnrolmentId { get; set; }
        [Required] public TimeSpan FinishTime { get; set; } // e.g. "00:45:30"
        [Range(1, 100000)] public int Position { get; set; }
    }
}