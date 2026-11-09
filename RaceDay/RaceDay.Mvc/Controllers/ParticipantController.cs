using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;
        public ParticipantController(RaceDayApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            // Fetch all public events
            var response = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            return View(events);
        }
        // GET: Show event details and categories
        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            // 1. Get the event from the API
            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            var ev = await _api.ReadAsync<EventDto>(response);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // POST: Enrol in the event
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            // 2. Tell the API to enrol this user
            var request = new CreateEnrolmentRequest { EventId = eventId, CategoryId = categoryId };
            var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not enrol. You might already be registered.";
                return RedirectToAction("EventDetails", new { id = eventId });
            }

            TempData["Success"] = "Successfully enrolled!";
            return RedirectToAction("EventDetails", new { id = eventId });
        }
        // GET: My Events (Participant's Enrolments)
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new List<EnrolmentDto>();

            return View(enrolments);
        }

        // GET: My Results
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, "api/results/mine");
            var results = await _api.ReadAsync<List<ResultDto>>(response) ?? new List<ResultDto>();

            return View(results);
        }

        // GET: Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            // Note: For now, we just show the session data. 
            // Later, you can call the API to get the full profile.
            ViewBag.Name = HttpContext.Session.GetString("DisplayName");
            ViewBag.Email = HttpContext.Session.GetString("Email");

            return View();
        }
    }
}