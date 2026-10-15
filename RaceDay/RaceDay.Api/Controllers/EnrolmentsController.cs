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
    [Route("api/enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ISessionService _sessionService;

        public EnrolmentsController(RaceDayDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // Participant only: Enrol in an event
        [HttpPost]
        [ParticipantOnly]
        public async Task<IActionResult> CreateEnrolment([FromBody] CreateEnrolmentRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == request.EventId);
            if (evt == null) return NotFound(new { message = "Event not found." });

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId && c.EventId == request.EventId);
            if (category == null) return BadRequest(new { message = "Category not found for this event." });

            // Check if already enrolled
            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == userId
                            && e.EventId == request.EventId
                            && e.Status != "Cancelled");

            if (alreadyEnrolled)
                return Conflict(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantId = userId,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                Status = "Pending",
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateEnrolment), new
            {
                message = "Enrolment created successfully.",
                enrolmentId = enrolment.EnrolmentId
            });
        }

        // Participant only: View own enrolments
        [HttpGet("mine")]
        [ParticipantOnly]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = _sessionService.GetUserId()!.Value;
            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == userId)
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName,
                    EventId = e.EventId,
                    EventName = e.Event!.Name,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    Status = e.Status,
                    EnrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // Organiser only: View all enrolments for their event
        [HttpGet("event/{eventId}")]
        [OrganiserOnly]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return NotFound(new { message = "Event not found." });
            if (evt.OrganiserId != userId) return Forbid();

            var enrolments = await _context.Enrolments
                .Where(e => e.EventId == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant!.FirstName + " " + e.Participant.LastName,
                    EventId = e.EventId,
                    EventName = evt.Name,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    Status = e.Status,
                    EnrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // Organiser only: Update enrolment status
        [HttpPut("{id}/status")]
        [OrganiserOnly]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null) return NotFound();
            if (enrolment.Event!.OrganiserId != userId) return Forbid();

            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
                return BadRequest(new { message = "Invalid status. Use Pending, Confirmed or Cancelled." });

            enrolment.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated." });
        }
    }
}