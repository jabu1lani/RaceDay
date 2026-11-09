using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;
        public OrganiserController(RaceDayApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            // 1. Check if logged in and is an Organiser
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            // 2. Fetch events from API
            var response = await _api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            return View(events);
        }
        // 1. GET: Show the form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // 2. POST: Send the data to the API
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(request);

            // Send to API
            var response = await _api.SendAsync(HttpMethod.Post, "api/events", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", "Failed to create event. " + error);
                return View(request);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }
        // GET: Organiser/Events (Manage Events)
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await _api.ReadAsync<List<EventDto>>(response) ?? new List<EventDto>();

            return View(events);
        }
    }
}