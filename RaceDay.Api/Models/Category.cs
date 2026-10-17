namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Under 20", "10km"
        public int EventId { get; set; }
    }
}
