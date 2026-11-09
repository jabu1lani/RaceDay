using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authenticated]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ISessionService _sessionService;

        public ProfileController(RaceDayDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = _sessionService.GetUserId()!.Value;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            return Ok(new ProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role!.RoleName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }
        [HttpPost("image")]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile file, [FromServices] BlobStorageService blobStorage)
        {
            var userId = _sessionService.GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            try
            {
                // Upload to Azure via BlobStorageService
                string url = await blobStorage.UploadAsync(file, "profile-pics");

                user.ProfileImageUrl = url;
                await _context.SaveChangesAsync();

                return Ok(new { imageUrl = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Azure upload failed", details = ex.Message });
            }
        }
    }
}