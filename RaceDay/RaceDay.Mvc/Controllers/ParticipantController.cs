using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;
        public ParticipantController(RaceDayApiClient api) => _api = api;

        // GET: Participant/Index (with Filtering)
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            // Fetch all public events from API
            var response = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            // Apply filtering if a search term was provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events.Where(e =>
                    e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Pass the search term back to the view so the input stays populated
            ViewBag.CurrentSearch = search;

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

        // GET: Participant/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            ViewBag.Name = HttpContext.Session.GetString("DisplayName");
            ViewBag.Email = HttpContext.Session.GetString("Email");
            return View();
        }
        // POST: Participant/Profile (Handles the image upload)
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePic)
        {
            if (profilePic == null || profilePic.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Profile));
            }

            // Send file to the API
            var response = await _api.UploadFileAsync("api/profile/image", profilePic);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Profile picture updated!";
            }
            else
            {
                // Read the exact error from the API to help us debug
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to upload. API says: {error}";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}