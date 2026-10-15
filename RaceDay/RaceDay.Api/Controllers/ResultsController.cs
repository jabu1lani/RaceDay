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
    [Route("api/results")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly ISessionService _sessionService;

        public ResultsController(RaceDayDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        // Organiser only: Record result
        [HttpPost]
        [OrganiserOnly]
        public async Task<IActionResult> CreateResult([FromBody] CreateResultRequest request)
        {
            var userId = _sessionService.GetUserId()!.Value;

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event!.OrganiserId != userId) return Forbid();

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == request.EnrolmentId))
                return Conflict(new { message = "Result already exists for this enrolment." });

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                OverallPosition = request.OverallPosition,
                CategoryPosition = request.CategoryPosition,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateResult), new { message = "Result recorded.", resultId = result.ResultId });
        }

        // Participant only: View own results
        [HttpGet("mine")]
        [ParticipantOnly]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = _sessionService.GetUserId()!.Value;

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.ParticipantId == userId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = r.Enrolment.Event!.Name,
                    CategoryName = r.Enrolment.Category!.Name,
                    FinishTime = r.FinishTime,
                    OverallPosition = r.OverallPosition,
                    CategoryPosition = r.CategoryPosition,
                    RecordedAt = r.RecordedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        // Organiser only: View all results for an event
        [HttpGet("event/{eventId}")]
        [OrganiserOnly]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = _sessionService.GetUserId()!.Value;
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (evt == null) return NotFound();
            if (evt.OrganiserId != userId) return Forbid();

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.EventId == eventId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment!.Participant!.FirstName + " " + r.Enrolment.Participant.LastName,
                    EventName = evt.Name,
                    CategoryName = r.Enrolment.Category!.Name,
                    FinishTime = r.FinishTime,
                    OverallPosition = r.OverallPosition,
                    CategoryPosition = r.CategoryPosition,
                    RecordedAt = r.RecordedAt
                })
                .OrderBy(r => r.OverallPosition)
                .ToListAsync();

            return Ok(results);
        }
    }
}