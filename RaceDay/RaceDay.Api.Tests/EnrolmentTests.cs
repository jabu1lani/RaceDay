using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using RaceDay.Contracts;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
    {
        [Fact]
        public async Task Participant_EnrolInEvent_ReturnsCreated()
        {
            using var context = TestFixture.CreateInMemoryContext();
            context.Users.Add(new AppUser { UserId = 1, Email = "p@test.com", FirstName = "P", LastName = "T", PasswordHash = "x", RoleId = 2 });
            context.Events.Add(new Event { EventId = 1, Name = "Race", OrganiserId = 99, EventDate = DateTime.UtcNow.AddDays(10), Location = "X", EventType = "Run", DistanceKm = 10 });
            context.Categories.Add(new Category { CategoryId = 1, Name = "10km", EventId = 1, EntryFee = 100, MaxParticipants = 100 });
            await context.SaveChangesAsync();

            var session = new FakeSessionService();
            session.SetUserSession(1, "Participant", "p@test.com");

            var controller = new EnrolmentsController(context, session);
            var result = await controller.CreateEnrolment(new CreateEnrolmentRequest
            {
                EventId = 1,
                CategoryId = 1
            });

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Participant_DuplicateEnrolment_ReturnsConflict()
        {
            using var context = TestFixture.CreateInMemoryContext();
            context.Users.Add(new AppUser { UserId = 1, Email = "p@test.com", FirstName = "P", LastName = "T", PasswordHash = "x", RoleId = 2 });
            context.Events.Add(new Event { EventId = 1, Name = "Race", OrganiserId = 99, EventDate = DateTime.UtcNow.AddDays(10), Location = "X", EventType = "Run", DistanceKm = 10 });
            context.Categories.Add(new Category { CategoryId = 1, Name = "10km", EventId = 1, EntryFee = 100, MaxParticipants = 100 });
            context.Enrolments.Add(new Enrolment { EnrolmentId = 1, ParticipantId = 1, EventId = 1, CategoryId = 1, Status = "Pending" });
            await context.SaveChangesAsync();

            var session = new FakeSessionService();
            session.SetUserSession(1, "Participant", "p@test.com");

            var controller = new EnrolmentsController(context, session);
            var result = await controller.CreateEnrolment(new CreateEnrolmentRequest { EventId = 1, CategoryId = 1 });

            Assert.IsType<ConflictObjectResult>(result);
        }
    }
}