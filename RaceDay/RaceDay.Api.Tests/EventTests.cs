using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventTests
    {
        [Fact]
        public async Task Organiser_CreateEvent_ReturnsCreated()
        {
            using var context = TestFixture.CreateInMemoryContext();
            var session = new FakeSessionService();
            session.SetUserSession(1, "Organiser", "org@raceday.com");

            var controller = new EventsController(context, session);
            var result = await controller.CreateEvent(new CreateEventRequest
            {
                Name = "Test Race",
                Description = "Test",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                DistanceKm = 10,
                EventType = "Run"
            });

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOk()
        {
            using var context = TestFixture.CreateInMemoryContext();
            var session = new FakeSessionService();
            var controller = new EventsController(context, session);

            var result = await controller.GetAllEvents();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Organiser_UpdateOthersEvent_ReturnsForbid()
        {
            using var context = TestFixture.CreateInMemoryContext();
            context.Users.Add(new AppUser { UserId = 1, Email = "org1@test.com", FirstName = "Org1", LastName = "One", PasswordHash = "x", RoleId = 1 });
            context.Users.Add(new AppUser { UserId = 2, Email = "org2@test.com", FirstName = "Org2", LastName = "Two", PasswordHash = "x", RoleId = 1 });
            context.Events.Add(new Event { EventId = 1, Name = "Existing", OrganiserId = 1, EventDate = DateTime.UtcNow, Location = "X", EventType = "Run", DistanceKm = 5 });
            await context.SaveChangesAsync();

            var session = new FakeSessionService();
            session.SetUserSession(2, "Organiser", "org2@test.com");

            var controller = new EventsController(context, session);
            var result = await controller.UpdateEvent(1, new UpdateEventRequest
            {
                Name = "Hacked",
                Description = "",
                EventDate = DateTime.UtcNow,
                Location = "Y",
                DistanceKm = 10,
                EventType = "Cycle"
            });

            Assert.IsType<ForbidResult>(result);
        }
    }
}