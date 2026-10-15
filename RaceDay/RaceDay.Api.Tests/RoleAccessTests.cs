using RaceDay.Api.Filters;
using RaceDay.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class RoleAccessTests
    {
        private AuthorizationFilterContext CreateContext(ISessionService session)
        {
            var services = new ServiceCollection();
            services.AddSingleton(session);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }

        [Fact]
        public void Unauthenticated_AccessProtectedEndpoint_Returns401()
        {
            var session = new Mock<ISessionService>();
            session.Setup(s => s.IsAuthenticated()).Returns(false);

            var attribute = new OrganiserOnlyAttribute();
            var context = CreateContext(session.Object);

            attribute.OnAuthorization(context);

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public void Participant_AccessOrganiserEndpoint_Returns403()
        {
            var session = new Mock<ISessionService>();
            session.Setup(s => s.IsAuthenticated()).Returns(true);
            session.Setup(s => s.GetUserRole()).Returns("Participant");

            var attribute = new OrganiserOnlyAttribute();
            var context = CreateContext(session.Object);

            attribute.OnAuthorization(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void Organiser_AccessOrganiserEndpoint_Allowed()
        {
            var session = new Mock<ISessionService>();
            session.Setup(s => s.IsAuthenticated()).Returns(true);
            session.Setup(s => s.GetUserRole()).Returns("Organiser");

            var attribute = new OrganiserOnlyAttribute();
            var context = CreateContext(session.Object);

            attribute.OnAuthorization(context);

            Assert.Null(context.Result); // No result = allowed
        }
    }
}