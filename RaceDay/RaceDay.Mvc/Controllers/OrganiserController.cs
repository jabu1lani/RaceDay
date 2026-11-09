using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;
        public OrganiserController(RaceDayApiClient api) => _api = api;

        // 1. Show the form
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new CreateEventRequest());
        }

        // 2. Handle the form submission
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest model, IFormFile? bannerFile)
        {
            if (!ModelState.IsValid) return View(model);

            // A. Send the event data to the API
            var response = await _api.SendAsync(HttpMethod.Post, "api/events", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create event in the API.");
                return View(model);
            }

            // B. Read the newly created event ID from the API response
            var createdEvent = await _api.ReadAsync<EventDto>(response);
            if (createdEvent == null) return View(model);

            // C. If a banner file was uploaded, send it to the API upload endpoint
            if (bannerFile != null && bannerFile.Length > 0)
            {
                // The Part 2 API endpoint we created earlier
                string uploadUrl = $"api/events/{createdEvent.EventId}/banner";

                // Call the multipart upload method in your API Client
                await _api.UploadFileAsync(uploadUrl, bannerFile);
            }

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction("Events"); // Redirect back to event list
        }
    }
}