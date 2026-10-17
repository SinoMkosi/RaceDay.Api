namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }
}