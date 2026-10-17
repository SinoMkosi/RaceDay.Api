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
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ResultsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Captures a finish time and position for a Participant. Organisers only.</summary>
        /// <response code="201">Result captured.</response>
        /// <response code="400">Result already exists for this enrolment.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Enrolment not found.</response>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(ResultDto dto)
        {
            var enrolment = await _db.Enrolments.FindAsync(dto.EnrolmentId);
            if (enrolment == null) return NotFound("Enrolment not found.");

            var ev = await _db.Events.FindAsync(enrolment.EventId);
            if (ev == null || ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only capture results for your own events.");

            if (await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
                return BadRequest("A result already exists for this enrolment. Use PUT to change it.");

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return StatusCode(201, result);
        }

        /// <summary>Updates a result for your own event. Organisers only.</summary>
        /// <response code="200">Result updated.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Result not found.</response>
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, ResultDto dto)
        {
            var result = await _db.Results.FindAsync(id);
            if (result == null) return NotFound();

            var enrolment = await _db.Enrolments.FindAsync(result.EnrolmentId);
            var ev = enrolment == null ? null : await _db.Events.FindAsync(enrolment.EventId);
            if (ev == null || ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only change results for your own events.");

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            await _db.SaveChangesAsync();

            return Ok(result);
        }

        /// <summary>Gets all results for your own event. Organisers only.</summary>
        /// <response code="200">Returns the results.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("event/{eventId}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only view results for your own events.");

            var list = await (
                from r in _db.Results
                join en in _db.Enrolments on r.EnrolmentId equals en.EnrolmentId
                join u in _db.Users on en.UserId equals u.UserId
                where en.EventId == eventId
                orderby r.Position
                select new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    ParticipantName = u.FirstName + " " + u.LastName,
                    r.FinishTime,
                    r.Position
                }).ToListAsync();

            return Ok(list);
        }

        /// <summary>Gets the logged-in Participant's own results.</summary>
        /// <response code="200">Returns your results.</response>
        /// <response code="403">Not a Participant.</response>
        [HttpGet("mine")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMine()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var list = await (
                from r in _db.Results
                join en in _db.Enrolments on r.EnrolmentId equals en.EnrolmentId
                join ev in _db.Events on en.EventId equals ev.EventId
                where en.UserId == userId
                select new
                {
                    r.ResultId,
                    EventName = ev.Name,
                    r.FinishTime,
                    r.Position
                }).ToListAsync();

            return Ok(list);
        }
    }
}