using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Dtos;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Gets the profile of the logged-in user.</summary>
        /// <response code="200">Returns the profile.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            return Ok(new { user.UserId, user.FirstName, user.LastName, user.Email, user.Role });
        }

        /// <summary>Updates the profile of the logged-in user.</summary>
        /// <response code="200">Profile updated.</response>
        /// <response code="400">Email already used by someone else.</response>
        /// <response code="401">Not logged in.</response>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != userId))
                return BadRequest("Email is already used by another account.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });
        }
    }
}