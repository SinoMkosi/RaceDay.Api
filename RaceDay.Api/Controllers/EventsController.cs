using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EventsController(AppDbContext db)
        {
            _db = db;
        }

        private static bool IsValidType(string type) =>
            type == "Run" || type == "Walk" || type == "Cycle";

        /// <summary>Gets all events. Open to Organisers and Participants.</summary>
        /// <response code="200">Returns the list of events.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _db.Events.ToListAsync());
        }

        /// <summary>Gets one event by its id. Open to Organisers and Participants.</summary>
        /// <response code="200">Returns the event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        /// <summary>Creates an event. Organisers only.</summary>
        /// <response code="201">Event created.</response>
        /// <response code="400">Invalid data or event type.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(EventDto dto)
        {
            if (!IsValidType(dto.EventType))
                return BadRequest("EventType must be 'Run', 'Walk' or 'Cycle'.");

            var ev = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
        }

        /// <summary>Updates your own event. Organisers only.</summary>
        /// <response code="200">Event updated.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, EventDto dto)
        {
            if (!IsValidType(dto.EventType))
                return BadRequest("EventType must be 'Run', 'Walk' or 'Cycle'.");

            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only change your own events.");

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.Date = dto.Date;
            ev.Location = dto.Location;
            ev.DistanceKm = dto.DistanceKm;
            ev.EventType = dto.EventType;
            await _db.SaveChangesAsync();

            return Ok(ev);
        }

        /// <summary>Deletes your own event. Organisers only.</summary>
        /// <response code="200">Event deleted.</response>
        /// <response code="400">Event already has enrolments.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only delete your own events.");

            if (await _db.Enrolments.AnyAsync(e => e.EventId == id))
                return BadRequest("This event already has enrolments and cannot be deleted.");

            var categories = _db.Categories.Where(c => c.EventId == id);
            _db.Categories.RemoveRange(categories);
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Event deleted." });
        }
    }
}
