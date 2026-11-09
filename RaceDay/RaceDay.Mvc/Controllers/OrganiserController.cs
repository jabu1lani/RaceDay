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
        // GET: Show the form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        // POST: Send the data to the API
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
        // --- EDIT EVENT ---
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            var ev = await _api.ReadAsync<EventDto>(response);
            if (ev == null) return NotFound();

            var model = new UpdateEventRequest
            {
                Name = ev.Name,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                DistanceKm = ev.DistanceKm,
                EventType = ev.EventType
            };
            ViewBag.EventId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, UpdateEventRequest request)
        {
            if (!ModelState.IsValid) { ViewBag.EventId = id; return View(request); }

            var response = await _api.SendAsync(HttpMethod.Put, $"api/events/{id}", request);
            if (!response.IsSuccessStatusCode) { ModelState.AddModelError("", "Failed to update."); ViewBag.EventId = id; return View(request); }

            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Events));
        }

        // --- DELETE EVENT ---
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
            TempData["Success"] = "Event deleted.";
            return RedirectToAction(nameof(Events));
        }

        // --- MANAGE CATEGORIES ---
        [HttpGet]
        public async Task<IActionResult> Categories(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser") return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
            var categories = await _api.ReadAsync<List<CategoryDto>>(response) ?? new List<CategoryDto>();

            ViewBag.EventId = id;
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser") return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return RedirectToAction(nameof(Categories), new { id = eventId });

            await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", request);
            TempData["Success"] = "Category added successfully.";
            return RedirectToAction(nameof(Categories), new { id = eventId });
        }

        // --- VIEW ENROLMENTS & CAPTURE RESULTS ---
        [HttpGet]
        public async Task<IActionResult> Enrolments(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser") return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, $"api/enrolments/event/{id}");
            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new List<EnrolmentDto>();

            ViewBag.EventId = id;
            return View(enrolments);
        }

        [HttpPost]
        public async Task<IActionResult> AddResult(int eventId, CreateResultRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser") return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Post, "api/results", request);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to capture result. It may already exist.";
            }
            else
            {
                TempData["Success"] = "Result captured successfully.";
            }

            return RedirectToAction(nameof(Enrolments), new { id = eventId });
        }
    }
}