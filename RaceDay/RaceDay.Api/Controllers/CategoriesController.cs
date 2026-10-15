using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ISessionService _sessionService;

        public CategoriesController(RaceDayDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    EntryFee = c.EntryFee,
                    MaxParticipants = c.MaxParticipants,
                    EventId = c.EventId
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("events/{eventId}/categories")]
        [OrganiserOnly]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            if (evt == null) return NotFound(new { message = "Event not found." });
            if (evt.OrganiserId != userId) return Forbid();

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                EntryFee = request.EntryFee,
                MaxParticipants = request.MaxParticipants,
                EventId = eventId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { eventId = eventId }, new
            {
                message = "Category created successfully.",
                categoryId = category.CategoryId
            });
        }

        [HttpPut("categories/{id}")]
        [OrganiserOnly]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != userId) return Forbid();

            category.Name = request.Name;
            category.Description = request.Description;
            category.EntryFee = request.EntryFee;
            category.MaxParticipants = request.MaxParticipants;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated." });
        }

        [HttpDelete("categories/{id}")]
        [OrganiserOnly]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != userId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted." });
        }
    }
}