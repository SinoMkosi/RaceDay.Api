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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Gets all categories for an event. Open to Organisers and Participants.</summary>
        /// <response code="200">Returns the categories.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound("Event not found.");

            return Ok(await _db.Categories.Where(c => c.EventId == eventId).ToListAsync());
        }

        /// <summary>Adds a category to your own event. Organisers only.</summary>
        /// <response code="201">Category created.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpPost("events/{eventId}/categories")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(int eventId, CategoryDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");

            if (ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only add categories to your own events.");

            var category = new Category { Name = dto.Name, EventId = eventId };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return StatusCode(201, category);
        }

        /// <summary>Deletes a category from your own event. Organisers only.</summary>
        /// <response code="200">Category deleted.</response>
        /// <response code="400">Category already has enrolments.</response>
        /// <response code="403">Not an Organiser, or not your event.</response>
        /// <response code="404">Category not found.</response>
        [HttpDelete("categories/{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var ev = await _db.Events.FindAsync(category.EventId);
            if (ev == null || ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
                return StatusCode(403, "You can only delete categories from your own events.");

            if (await _db.Enrolments.AnyAsync(e => e.CategoryId == id))
                return BadRequest("This category already has enrolments and cannot be deleted.");

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Category deleted." });
        }
    }
}