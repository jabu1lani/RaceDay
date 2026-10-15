namespace RaceDay.Api.Services
{
    public interface ISessionService
    {
        void SetUserSession(int userId, string role, string email);
        int? GetUserId();
        string? GetUserRole();
        string? GetUserEmail();
        bool IsAuthenticated();
        bool IsOrganiser();
        bool IsParticipant();
        void ClearSession();
    }

    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session!;

        public void SetUserSession(int userId, string role, string email)
        {
            Session.SetInt32("UserId", userId);
            Session.SetString("Role", role);
            Session.SetString("Email", email);
        }

        public int? GetUserId() => Session.GetInt32("UserId");
        public string? GetUserRole() => Session.GetString("Role");
        public string? GetUserEmail() => Session.GetString("Email");
        public bool IsAuthenticated() => GetUserId().HasValue;
        public bool IsOrganiser() => GetUserRole() == "Organiser";
        public bool IsParticipant() => GetUserRole() == "Participant";
        public void ClearSession() => Session.Clear();
    }
}