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
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ISessionService _sessionService;

        public EventsController(RaceDayDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // Both roles can view all events
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    EventType = e.EventType,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                    Categories = e.Categories.Select(c => new CategoryDto
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        EntryFee = c.EntryFee,
                        MaxParticipants = c.MaxParticipants
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var e = await _context.Events
                .Include(x => x.Organiser)
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.EventId == id);

            if (e == null) return NotFound(new { message = "Event not found." });

            return Ok(new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser!.FirstName + " " + e.Organiser.LastName,
                Categories = e.Categories.Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    EntryFee = c.EntryFee,
                    MaxParticipants = c.MaxParticipants
                }).ToList()
            });
        }

        // Organiser only: View their own events
        [HttpGet("mine")]
        [OrganiserOnly]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = _sessionService.GetUserId()!.Value;
            var events = await _context.Events
                .Where(e => e.OrganiserId == userId)
                .Include(e => e.Categories)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    EventType = e.EventType,
                    OrganiserId = e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);
        }

        // Organiser only: Create event
        [HttpPost]
        [OrganiserOnly]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;

            var newEvent = new Event
            {
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                OrganiserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, new
            {
                message = "Event created successfully.",
                eventId = newEvent.EventId
            });
        }

        // Organiser only: Update own event
        [HttpPut("{id}")]
        [OrganiserOnly]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null) return NotFound(new { message = "Event not found." });
            if (evt.OrganiserId != userId) return Forbid();

            evt.Name = request.Name;
            evt.Description = request.Description;
            evt.EventDate = request.EventDate;
            evt.Location = request.Location;
            evt.DistanceKm = request.DistanceKm;
            evt.EventType = request.EventType;
            evt.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully." });
        }

        // Organiser only: Delete own event
        [HttpDelete("{id}")]
        [OrganiserOnly]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null) return NotFound(new { message = "Event not found." });
            if (evt.OrganiserId != userId) return Forbid();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}