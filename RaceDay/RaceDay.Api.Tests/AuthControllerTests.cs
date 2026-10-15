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
    public class AuthControllerTests
    {
        private RaceDayDbContext GetContext() => TestFixture.CreateInMemoryContext();

        [Fact]
        public async Task Register_ValidUser_ReturnsCreated()
        {
            // Arrange
            using var context = GetContext();
            var passwordService = new PasswordService();
            var sessionService = new FakeSessionService();
            var controller = new AuthController(context, passwordService, sessionService);

            var request = new RegisterRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.com",
                Password = "Password123!",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Single(await context.Users.ToListAsync());
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            // Arrange
            using var context = GetContext();
            var passwordService = new PasswordService();
            var sessionService = new FakeSessionService();
            var controller = new AuthController(context, passwordService, sessionService);

            context.Users.Add(new AppUser
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@raceday.com",
                PasswordHash = passwordService.HashPassword("Pass123!"),
                RoleId = 2
            });
            await context.SaveChangesAsync();

            var request = new RegisterRequest
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@raceday.com",
                Password = "Pass123!",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            using var context = GetContext();
            var passwordService = new PasswordService();
            var sessionService = new FakeSessionService();
            var controller = new AuthController(context, passwordService, sessionService);

            context.Users.Add(new AppUser
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.com",
                PasswordHash = passwordService.HashPassword("Pass123!"),
                RoleId = 2
            });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Login(new LoginRequest
            {
                Email = "test@raceday.com",
                Password = "Pass123!"
            });

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            // Arrange
            using var context = GetContext();
            var passwordService = new PasswordService();
            var sessionService = new FakeSessionService();
            var controller = new AuthController(context, passwordService, sessionService);

            context.Users.Add(new AppUser
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@raceday.com",
                PasswordHash = passwordService.HashPassword("CorrectPass!"),
                RoleId = 2
            });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Login(new LoginRequest
            {
                Email = "test@raceday.com",
                Password = "WrongPass!"
            });

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }

    public class FakeSessionService : ISessionService
    {
        private int? _userId;
        private string? _role;
        private string? _email;

        public void SetUserSession(int userId, string role, string email)
        { _userId = userId; _role = role; _email = email; }

        public int? GetUserId() => _userId;
        public string? GetUserRole() => _role;
        public string? GetUserEmail() => _email;
        public bool IsAuthenticated() => _userId.HasValue;
        public bool IsOrganiser() => _role == "Organiser";
        public bool IsParticipant() => _role == "Participant";
        public void ClearSession() { _userId = null; _role = null; _email = null; }
    }
}