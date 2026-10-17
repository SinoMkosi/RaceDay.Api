using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class EventDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required] public string Description { get; set; } = string.Empty;
        [Required] public DateTime Date { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        [Range(0.1, 1000)] public double DistanceKm { get; set; }
        [Required] public string EventType { get; set; } = string.Empty; // "Run", "Walk" or "Cycle"
    }
}