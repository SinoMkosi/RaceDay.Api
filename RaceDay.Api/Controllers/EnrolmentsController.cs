using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [SessionAuthorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EnrolmentsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Enrols the logged-in Participant in an event with a chosen category.</summary>
        /// <response code="201">Enrolment created.</response>
        /// <response code="400">Category does not belong to this event, or already enrolled.</response>
        /// <response code="403">Not a Participant.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("events/{eventId}/enrol")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(int eventId, EnrolDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound("Event not found.");

            if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId))
                return BadRequest("That category does not belong to this event.");

            if (await _db.Enrolments.AnyAsync(e => e.UserId == userId && e.EventId == eventId))
                return BadRequest("You are already enrolled in this event.");

            var enrolment = new Enrolment
            {
                UserId = userId,
                EventId = eventId,
                CategoryId = dto.CategoryId
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            return StatusCode(201, enrolment);
        }

        /// <summary>Gets all enrolments for your own event. Organisers only.</summary>
        /// <response code="200">Returns the enrolments.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{eventId}/enrolments")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only view enrolments for your own events.");

            var list = await (
                from en in _db.Enrolments
                join u in _db.Users on en.UserId equals u.UserId
                join c in _db.Categories on en.CategoryId equals c.CategoryId
                where en.EventId == eventId
                select new
                {
                    en.EnrolmentId,
                    en.UserId,
                    ParticipantName = u.FirstName + " " + u.LastName,
                    en.CategoryId,
                    CategoryName = c.Name,
                    en.EnrolledOn
                }).ToListAsync();

            return Ok(list);
        }

        /// <summary>Gets the logged-in Participant's own enrolments.</summary>
        /// <response code="200">Returns your enrolments.</response>
        /// <response code="403">Not a Participant.</response>
        [HttpGet("enrolments/mine")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMine()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return Ok(await _db.Enrolments.Where(e => e.UserId == userId).ToListAsync());
        }
    }
}
